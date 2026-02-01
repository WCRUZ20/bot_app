using botapp.Helpers;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    class RecaptchaV2Solver
    {
        private readonly string _apiKey;
        private readonly int _timeoutSeconds;
        private readonly int _pollIntervalSeconds;
        //private readonly Action<string> _logger;
        private readonly string _logFilePath;

        public RecaptchaV2Solver(string apiKey, string logFilePath, int timeoutSeconds = 120, int pollIntervalSeconds = 5)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _timeoutSeconds = Math.Max(30, timeoutSeconds); // Mínimo 30s para dar tiempo al solver
            _pollIntervalSeconds = Math.Max(1, pollIntervalSeconds);
            //_logger = logger ?? (msg => Console.WriteLine(msg));
        }

        private void _logger(string message)
        {
            LoggerHelper.Log(_logFilePath, message);
        }

        /// <summary>
        /// Resuelve reCAPTCHA v2 de forma independiente. Opcional: hace clic en el botón para activarlo.
        /// Detecta si aparece el desafío, lo envía a 2Captcha, inyecta el token, ejecuta el callback y opcionalmente re-submitea.
        /// Retorna true si se resolvió exitosamente y el proceso continuó.
        /// </summary>
        /// <param name="page">Página de Playwright donde está el botón.</param>
        /// <param name="pageUrl">URL actual de la página (para el sitekey).</param>
        /// <param name="submitSelector">Selector del botón de submit (para clic inicial y/o resubmit).</param>
        /// <param name="formSelector">Selector del formulario a re-submitear si es necesario (opcional).</param>
        /// <param name="alreadyClicked">Si true, no hace clic inicial (ya se activó el CAPTCHA).</param>
        /// <returns>True si resuelto y procesado, false en caso de error o no presente.</returns>
        public async Task<bool> SolveOnButtonClickAsync(IPage page, string pageUrl, string submitSelector = null, string formSelector = null, bool alreadyClicked = false)
        {
            if (string.IsNullOrEmpty(pageUrl)) pageUrl = page.Url;

            try
            {
                _logger($"🔍 Detectando reCAPTCHA {(alreadyClicked ? "(ya clicado)" : "antes de clic")} en {submitSelector ?? "formulario"}...");

                // Paso 1: Si no ya clicado, verificar si ya hay reCAPTCHA visible
                bool alreadyPresent = await IsRecaptchaPresentAsync(page);
                if (alreadyPresent && !alreadyClicked)
                {
                    _logger("⚠ reCAPTCHA ya presente antes del clic. Procediendo a resolver.");
                }

                // Paso 2: Si no ya clicado, hacer clic en el botón que activa el CAPTCHA
                if (!alreadyClicked)
                {
                    if (string.IsNullOrEmpty(submitSelector))
                        throw new ArgumentException("submitSelector requerido si !alreadyClicked");
                    _logger($"🖱️ Haciendo clic en {submitSelector}...");
                    await page.ClickAsync(submitSelector);
                    await page.WaitForTimeoutAsync(3000); // Pausa más larga para que cargue el iframe del desafío de imágenes
                }

                // Paso 3: Detectar si apareció el desafío (buscar iframe de recaptcha o div de challenge)
                bool captchaTriggered = await IsRecaptchaPresentAsync(page);
                if (!captchaTriggered)
                {
                    _logger("ℹ️ No se detectó reCAPTCHA. Continuando sin resolver.");
                    return true; // No hay CAPTCHA
                }

                _logger("🔒 reCAPTCHA v2 detectado (desafío de imágenes). Enviando a 2Captcha para resolución...");

                // Paso 4: Extraer sitekey del elemento reCAPTCHA
                string sitekey = await ExtractSiteKeyAsync(page);
                if (string.IsNullOrEmpty(sitekey))
                {
                    throw new Exception("No se pudo extraer el sitekey de reCAPTCHA.");
                }
                _logger($"🔑 Sitekey extraído: {sitekey}");

                // Paso 5: Enviar tarea a 2Captcha y obtener token (usa invisible=0 para v2 visible con desafío)
                string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, invisible: 0);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("No se recibió token válido de 2Captcha.");
                }
                _logger("✅ Token recibido de 2Captcha.");

                // Paso 6: Inyectar el token y ejecutar callback para cerrar el desafío
                await InjectTokenAndCallbackAsync(page, token);
                _logger("📝 Token inyectado y callback ejecutado.");

                // Paso 7: Pausa para que la página procese (el callback debería cerrar el desafío y submitear)
                await page.WaitForTimeoutAsync(3000);

                // Paso 8: Opcional: Si aún no se submiteó, re-submitear
                bool submitted = await ResubmitFormAsync(page, submitSelector, formSelector);
                if (!submitted)
                {
                    _logger("⚠ No se pudo re-submitear, pero callback debería haberlo manejado.");
                }
                else
                {
                    _logger("📤 Formulario re-submiteado exitosamente.");
                }

                // Paso 9: Verificar que el CAPTCHA ya no esté presente
                bool stillPresent = await IsRecaptchaPresentAsync(page);
                if (stillPresent)
                {
                    _logger("⚠ reCAPTCHA aún visible tras callback (verificar logs de consola del browser).");
                }
                else
                {
                    _logger("✅ Desafío de reCAPTCHA cerrado exitosamente.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger($"❌ Error en resolución de reCAPTCHA: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detecta si reCAPTCHA está presente en la página (busca iframe o div.g-recaptcha).
        /// </summary>
        private async Task<bool> IsRecaptchaPresentAsync(IPage page)
        {
            // Buscar iframe de recaptcha (incluyendo desafío de imágenes)
            bool iframePresent = await page.Locator("iframe[src*='recaptcha']").CountAsync() > 0;
            // O buscar el contenedor
            bool divPresent = await page.Locator("div.g-recaptcha").CountAsync() > 0;
            return iframePresent || divPresent;
        }

        /// <summary>
        /// Extrae el sitekey del atributo data-sitekey.
        /// </summary>
        private async Task<string> ExtractSiteKeyAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(@"
                () => {
                    const recaptchaElement = document.querySelector('div[data-sitekey]');
                    return recaptchaElement ? recaptchaElement.getAttribute('data-sitekey') : '';
                }
            ");
        }

        /// <summary>
        /// Envía la tarea a 2Captcha y hace polling hasta obtener el token.
        /// </summary>
        private async Task<string> SubmitAndPollForSolutionAsync(string sitekey, string pageUrl, int invisible = 0)
        {
            HttpClient httpClient = null;
            try
            {
                httpClient = new HttpClient();

                // Submit tarea (invisible=0 para v2 checkbox con desafío de imágenes)
                string submitUrl = string.Format("http://2captcha.com/in.php?key={0}&method=userrecaptcha&googlekey={1}&pageurl={2}&invisible={3}&json=1",
                    _apiKey, Uri.EscapeDataString(sitekey), Uri.EscapeDataString(pageUrl), invisible);
                string submitResponse = await httpClient.GetStringAsync(submitUrl);
                var submitJson = JObject.Parse(submitResponse);

                if (submitJson["status"]?.Value<int>() != 1)
                {
                    throw new Exception(string.Format("Error al enviar a 2Captcha: {0}", submitJson["request"]?.ToString() ?? "Desconocido"));
                }

                string taskId = submitJson["request"].ToString();
                _logger(string.Format("📤 Tarea enviada a 2Captcha. ID: {0}. Esperando solución...", taskId));

                // Polling (más intentos para desafíos de imágenes, que toman ~20-40s)
                int maxAttempts = _timeoutSeconds / _pollIntervalSeconds;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    await Task.Delay(_pollIntervalSeconds * 1000);

                    string pollUrl = string.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}&json=1", _apiKey, taskId);
                    string pollResponse = await httpClient.GetStringAsync(pollUrl);
                    var pollJson = JObject.Parse(pollResponse);

                    if (pollJson["status"]?.Value<int>() == 1)
                    {
                        return pollJson["request"].ToString(); // Token
                    }

                    string status = pollJson["request"]?.ToString() ?? "";
                    if (status != "CAPCHA_NOT_READY")
                    {
                        throw new Exception(string.Format("Error en polling de 2Captcha: {0}", status));
                    }

                    _logger(string.Format("⏳ Polling intento {0}/{1}... (Estado: {2})", attempt + 1, maxAttempts, status));
                }

                throw new Exception(string.Format("Timeout en polling de 2Captcha ({0}s).", _timeoutSeconds));
            }
            finally
            {
                if (httpClient != null)
                {
                    httpClient.Dispose();
                }
            }
        }

        /// <summary>
        /// Inyecta el token en el campo g-recaptcha-response, dispara eventos y ejecuta el callback para cerrar el desafío.
        /// </summary>
        private async Task InjectTokenAndCallbackAsync(IPage page, string token)
        {
            await page.EvaluateAsync(@"
                (token) => {
                    // Buscar el textarea o div para g-recaptcha-response
                    let responseField = document.getElementById('g-recaptcha-response') || 
                                        document.querySelector('textarea[name=""g-recaptcha-response""]') ||
                                        document.querySelector('[name=""g-recaptcha-response""]');
                    
                    if (responseField) {
                        responseField.value = token;
                        if (responseField.tagName.toUpperCase() === 'DIV') {
                            responseField.innerHTML = token; // Para divs en algunos casos
                        }
                        
                        // Disparar eventos de cambio y input para notificar a la página
                        const changeEvent = new Event('change', { bubbles: true });
                        const inputEvent = new Event('input', { bubbles: true });
                        responseField.dispatchEvent(changeEvent);
                        responseField.dispatchEvent(inputEvent);
                        
                        console.log('Token inyectado y eventos disparados');
                    } else {
                        console.error('No se encontró campo g-recaptcha-response');
                        return; // Salir si no hay campo
                    }
                    
                    // Obtener y ejecutar el callback del div g-recaptcha para cerrar el desafío
                    const recaptchaDiv = document.querySelector('.g-recaptcha');
                    if (recaptchaDiv) {
                        const callbackName = recaptchaDiv.getAttribute('data-callback');
                        if (callbackName && window[callbackName]) {
                            window[callbackName](token);
                            console.log('Callback ejecutado: ' + callbackName);
                        } else {
                            console.warn('No se encontró callback válido en data-callback');
                        }
                    } else {
                        console.error('No se encontró div.g-recaptcha');
                    }
                }
            ", token);
        }

        /// <summary>
        /// Re-submitea el formulario o re-clica el botón para continuar el proceso (opcional post-callback).
        /// </summary>
        private async Task<bool> ResubmitFormAsync(IPage page, string buttonSelector, string formSelector)
        {
            try
            {
                // Si hay selector de form, submitearlo directamente
                if (!string.IsNullOrEmpty(formSelector))
                {
                    await page.EvaluateAsync(formSelector, "form => { if (form) form.submit(); }");
                    return true;
                }

                // Sino, re-clic en el botón (solo si proporcionado)
                if (!string.IsNullOrEmpty(buttonSelector))
                {
                    await page.ClickAsync(buttonSelector);
                    return true;
                }

                return false; // No resubmit si no hay selectores
            }
            catch
            {
                return false;
            }
        }
    }
}
