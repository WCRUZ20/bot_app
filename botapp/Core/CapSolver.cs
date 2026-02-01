using botapp.Helpers;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    class CapSolver
    {
        public readonly string _apiKey;
        private readonly int _timeoutSeconds;
        private readonly int _pollIntervalSeconds;
        //private readonly Action<string> _logger;
        private readonly string _logFilePath;

        private string statusJson;

        public CapSolver(string apiKey, string logFilePath, int timeoutSeconds = 120, int pollIntervalSeconds = 5)
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
        /// Detecta si aparece el desafío, lo envía a CapSolver, inyecta el token, ejecuta el callback y opcionalmente re-submitea.
        /// Retorna true si se resolvió exitosamente y el proceso continuó.
        /// </summary>
        /// <param name="page">Página de Playwright donde está el botón.</param>
        /// <param name="pageUrl">URL actual de la página (para el sitekey).</param>
        /// <param name="submitSelector">Selector del botón de submit (para clic inicial y/o resubmit).</param>
        /// <param name="formSelector">Selector del formulario a re-submitear si es necesario (opcional).</param>
        /// <param name="alreadyClicked">Si true, no hace clic inicial (ya se activó el CAPTCHA).</param>
        /// <returns>True si resuelto y procesado, false en caso de error o no presente.</returns>
        //public async Task<bool> SolveOnButtonClickAsync(IPage page, string pageUrl, string submitSelector = null, string formSelector = null, bool alreadyClicked = false)
        //{
        //    if (string.IsNullOrEmpty(pageUrl)) pageUrl = page.Url;

        //    try
        //    {
        //        //_logger($"🔍 Detectando reCAPTCHA {(alreadyClicked ? "(ya clicado)" : "antes de clic")} en {submitSelector ?? "formulario"}...");

        //        // Paso 1: Si no ya clicado, verificar si ya hay reCAPTCHA visible
        //        bool alreadyPresent = await IsRecaptchaPresentAsync(page);
        //        if (alreadyPresent && !alreadyClicked)
        //        {
        //            //_logger("⚠ reCAPTCHA ya presente antes del clic. Procediendo a resolver.");
        //        }

        //        // Paso 2: Si no ya clicado, hacer clic en el botón que activa el CAPTCHA
        //        if (!alreadyClicked)
        //        {
        //            if (string.IsNullOrEmpty(submitSelector))
        //                throw new ArgumentException("submitSelector requerido si !alreadyClicked");
        //            //_logger($"🖱️ Haciendo clic en {submitSelector}...");
        //            await page.ClickAsync(submitSelector);
        //            await page.WaitForTimeoutAsync(3000); // Pausa para que cargue el iframe del desafío
        //        }

        //        // Paso 3: Detectar si apareció el desafío
        //        bool captchaTriggered = await IsRecaptchaPresentAsync(page);
        //        if (!captchaTriggered)
        //        {
        //            //_logger("ℹ️ No se detectó reCAPTCHA. Continuando sin resolver.");
        //            return true; // No hay CAPTCHA
        //        }

        //        //_logger("🔒 reCAPTCHA v2 detectado (desafío de imágenes). Enviando a CapSolver para resolución...");

        //        // Paso 4: Extraer sitekey del elemento reCAPTCHA
        //        string sitekey = await ExtractSiteKeyAsync(page);
        //        if (string.IsNullOrEmpty(sitekey))
        //        {
        //            throw new Exception("No se pudo extraer el sitekey de reCAPTCHA.");
        //        }
        //        //_logger($"🔑 Sitekey extraído: {sitekey}");

        //        // Paso 5: Enviar tarea a CapSolver y obtener token
        //        string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl);
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            throw new Exception("No se recibió token válido de CapSolver.");
        //        }
        //        //_logger("✅ Token recibido de CapSolver.");

        //        // Paso 6: Inyectar el token y ejecutar callback para cerrar el desafío
        //        await InjectTokenAndCallbackAsync(page, token);
        //        //_logger("📝 Token inyectado y callback ejecutado.");
        //        await ExecuteRcBuscarAsync(page);
        //        //_logger("📝 Token inyectado y callback ejecutado.");

        //        // Paso 7: Pausa para que la página procese
        //        await page.WaitForTimeoutAsync(3000);

        //        // Paso 8: Opcional: Re-submitear si es necesario
        //        bool submitted = await ResubmitFormAsync(page, submitSelector, formSelector);
        //        if (!submitted)
        //        {
        //            //_logger("⚠ No se pudo re-submitear, pero callback debería haberlo manejado.");
        //        }
        //        else
        //        {
        //            //_logger("📤 Formulario re-submiteado exitosamente.");
        //        }

        //        // Paso 9: Verificar que el CAPTCHA ya no esté presente
        //        bool stillPresent = await IsRecaptchaPresentAsync(page);
        //        if (stillPresent)
        //        {
        //            //_logger("⚠ reCAPTCHA aún visible tras callback (verificar logs de consola del browser).");
        //        }
        //        else
        //        {
        //            //_logger("✅ Desafío de reCAPTCHA cerrado exitosamente.");
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger($"❌ Error en resolución de reCAPTCHA: {ex.Message}");
        //        return false;
        //    }
        //}

        //public async Task<bool> SolveOnButtonClickAsync(IPage page, string pageUrl, string submitSelector = null, string formSelector = null, bool alreadyClicked = false)
        //{
        //    if (string.IsNullOrEmpty(pageUrl)) pageUrl = page.Url;

        //    try
        //    {
        //        _logger($"🔍 Detectando tipo de reCAPTCHA en {pageUrl}...");

        //        // Determinar si es v2, v3 invisible o v3 visible
        //        bool isV3 = true;
        //        bool isV3Visible = isV3 ? await IsRecaptchaV3VisiblePresentAsync(page) : false;

        //        if (isV3)
        //        {
        //            if (isV3Visible)
        //            {
        //                _logger("🔍 Detectado reCAPTCHA v3 Enterprise VISIBLE. Usando método específico...");
        //                return await SolveRecaptchaV3VisibleAsync(page, pageUrl, submitSelector);
        //            }
        //            else
        //            {
        //                _logger("🔍 Detectado reCAPTCHA v3 Enterprise INVISIBLE. Usando método específico...");
        //                //return await SolveRecaptchaV3Async(page, pageUrl);
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            _logger("🔍 Detectado reCAPTCHA v2. Usando método original...");
        //            //return await SolveRecaptchaV2Async(page, pageUrl, submitSelector, formSelector, alreadyClicked);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger($"❌ Error en detección/resolución de reCAPTCHA: {ex.Message}");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Detecta si reCAPTCHA está presente en la página (busca iframe o div.g-recaptcha).
        /// </summary>
        //private async Task<bool> IsRecaptchaPresentAsync(IPage page)
        //{
        //    bool iframePresent = await page.Locator("iframe[src*='recaptcha']").CountAsync() > 0;
        //    bool divPresent = await page.Locator("div.g-recaptcha").CountAsync() > 0;
        //    return iframePresent || divPresent;
        //}

        /// <summary>
        /// Detecta si reCAPTCHA Enterprise está presente en la página.
        /// </summary>
        private async Task<bool> IsRecaptchaPresentAsync(IPage page)
        {
            // Verificar elementos visibles
            bool hasSitekeyElement = await page.Locator("[data-sitekey]").CountAsync() > 0;

            // Verificar scripts de reCAPTCHA Enterprise
            bool hasEnterpriseScript = await page.EvaluateAsync<bool>(@"
                () => {
                    const scripts = document.getElementsByTagName('script');
                    for (const script of scripts) {
                        if (script.src && script.src.includes('recaptcha/enterprise.js')) {
                            return true;
                        }
                        if (script.innerText && script.innerText.includes('grecaptcha.enterprise')) {
                            return true;
                        }
                    }
                    return false;
                }
            ");

            // Verificar si grecaptcha.enterprise está disponible
            bool hasGrecaptchaEnterprise = await page.EvaluateAsync<bool>(@"
                () => {
                    return !!(window.grecaptcha && window.grecaptcha.enterprise);
                }
            ");

            return hasSitekeyElement || hasEnterpriseScript || hasGrecaptchaEnterprise;
        }

        /// <summary>
        /// Extrae el sitekey del atributo data-sitekey.
        /// </summary>
        //private async Task<string> ExtractSiteKeyAsync(IPage page)
        //{
        //    return await page.EvaluateAsync<string>(@"
        //        () => {
        //            const recaptchaElement = document.querySelector('div[data-sitekey]');
        //            return recaptchaElement ? recaptchaElement.getAttribute('data-sitekey') : '';
        //        }"
        //    );
        //}

        /// <summary>
        /// Extrae el sitekey de reCAPTCHA directamente desde la página,
        /// sin depender de que el iframe esté visible.
        /// </summary>
        /// comentado 06/01/2026
        private async Task<string> ExtractSiteKeyAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(@"
                () => {

                    // 1️⃣ div.g-recaptcha
                    const div1 = document.querySelector('.g-recaptcha[data-sitekey]');
                    if (div1) return div1.getAttribute('data-sitekey');

                    // 2️⃣ cualquier elemento con data-sitekey
                    const any = document.querySelector('[data-sitekey]');
                    if (any) return any.getAttribute('data-sitekey');

                    // 3️⃣ buscar en scripts inline (grecaptcha.render)
                    for (const script of document.scripts) {
                        if (!script.innerText) continue;

                        const match = script.innerText.match(/sitekey\s*:\s*['""]([^'""]+)['""]/);
                        if (match && match[1]) return match[1];
                    }

                    // 4️⃣ buscar en iframes (aunque no estén visibles)
                    const iframe = document.querySelector('iframe[src*=""recaptcha""]');
                    if (iframe) {
                        const url = new URL(iframe.src);
                        return url.searchParams.get('k');
                    }

                    return '';
                }
            ");
        }

        /// <summary>
        /// Extrae el sitekey específicamente para reCAPTCHA Enterprise.
        /// </summary>
        //private async Task<string> ExtractSiteKeyAsync(IPage page)
        //{
        //    return await page.EvaluateAsync<string>(@"
        //        () => {
        //            console.log('Buscando sitekey reCAPTCHA Enterprise...');

        //            // 1️⃣ Buscar elementos con data-sitekey
        //            const sitekeyElements = document.querySelectorAll('[data-sitekey]');
        //            for (const element of sitekeyElements) {
        //                const sitekey = element.getAttribute('data-sitekey');
        //                if (sitekey && sitekey.trim().length > 0) {
        //                    console.log('✅ Sitekey encontrado en elemento:', sitekey);
        //                    return sitekey;
        //                }
        //            }

        //            // 2️⃣ Buscar en scripts (grecaptcha.enterprise.render)
        //            const scripts = document.getElementsByTagName('script');
        //            for (const script of scripts) {
        //                if (script.innerText && script.innerText.includes('grecaptcha.enterprise.render')) {
        //                    const match = script.innerText.match(/sitekey\s*:\s*['""]([^'""]+)['""]/);
        //                    if (match && match[1]) {
        //                        console.log('✅ Sitekey encontrado en script:', match[1]);
        //                        return match[1];
        //                    }
        //                }
        //            }

        //            // 3️⃣ Buscar en window.grecaptcha si ya está inicializado
        //            if (window.grecaptcha && window.grecaptcha.enterprise && window.___grecaptcha_cfg) {
        //                try {
        //                    const clients = window.___grecaptcha_cfg.clients;
        //                    for (const clientKey in clients) {
        //                        const client = clients[clientKey];
        //                        if (client && client.sitekey) {
        //                            console.log('✅ Sitekey encontrado en configuración:', client.sitekey);
        //                            return client.sitekey;
        //                        }
        //                    }
        //                } catch (e) {
        //                    console.warn('Error buscando sitekey en configuración:', e);
        //                }
        //            }

        //            console.warn('❌ No se encontró sitekey');
        //            return '';
        //        }
        //    ");
        //}

        /// <summary>
        /// Envía la tarea a CapSolver y hace polling hasta obtener el token.
        /// </summary>
        //private async Task<string> SubmitAndPollForSolutionAsync(string sitekey, string pageUrl)
        //{
        //    HttpClient httpClient = null;
        //    try
        //    {
        //        httpClient = new HttpClient();

        //        // Paso 1: Crear tarea en CapSolver
        //        var requestPayload = new
        //        {
        //            clientKey = _apiKey,
        //            task = new
        //            {
        //                //type = "ReCaptchaV2TaskProxyless",
        //                type = "ReCaptchaV3EnterpriseTaskProxyless",
        //                websiteURL = pageUrl,
        //                websiteKey = sitekey,
        //                pageAction = "consulta_cel_recibidos"
        //            }
        //        };

        //        var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload), System.Text.Encoding.UTF8, "application/json");
        //        string submitUrl = "https://api.capsolver.com/createTask";
        //        var submitResponse = await httpClient.PostAsync(submitUrl, content);
        //        submitResponse.EnsureSuccessStatusCode();
        //        string submitResponseBody = await submitResponse.Content.ReadAsStringAsync();
        //        var submitJson = JObject.Parse(submitResponseBody);

        //        if (submitJson["errorId"]?.Value<int>() != 0)
        //        {
        //            throw new Exception($"Error al enviar a CapSolver: {submitJson["errorDescription"]?.ToString() ?? "Desconocido"}");
        //        }

        //        string taskId = submitJson["taskId"]?.ToString();
        //        if (string.IsNullOrEmpty(taskId))
        //        {
        //            throw new Exception("No se recibió taskId de CapSolver.");
        //        }
        //        //_logger($"📤 Tarea enviada a CapSolver. ID: {taskId}. Esperando solución...");

        //        // Paso 2: Polling para obtener la solución
        //        int maxAttempts = _timeoutSeconds / _pollIntervalSeconds;
        //        for (int attempt = 0; attempt < maxAttempts; attempt++)
        //        {
        //            await Task.Delay(_pollIntervalSeconds * 1000);

        //            var pollPayload = new
        //            {
        //                clientKey = _apiKey,
        //                taskId = taskId
        //            };

        //            var pollContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(pollPayload), System.Text.Encoding.UTF8, "application/json");
        //            string pollUrl = "https://api.capsolver.com/getTaskResult";
        //            var pollResponse = await httpClient.PostAsync(pollUrl, pollContent);
        //            pollResponse.EnsureSuccessStatusCode();
        //            string pollResponseBody = await pollResponse.Content.ReadAsStringAsync();
        //            var pollJson = JObject.Parse(pollResponseBody);

        //            if (pollJson["errorId"]?.Value<int>() != 0)
        //            {
        //                throw new Exception($"Error en polling de CapSolver: {pollJson["errorDescription"]?.ToString() ?? "Desconocido"}");
        //            }

        //            string status = pollJson["status"]?.ToString();
        //            if (status == "ready")
        //            {
        //                string token = pollJson["solution"]?["gRecaptchaResponse"]?.ToString();
        //                if (!string.IsNullOrEmpty(token))
        //                {
        //                    return token;
        //                }
        //                throw new Exception("Token no encontrado en la respuesta de CapSolver.");
        //            }
        //            else if (status == "failed" || status == "error")
        //            {
        //                throw new Exception($"CapSolver falló: {pollJson["errorDescription"]?.ToString() ?? "Estado fallido"}");
        //            }

        //            //_logger($"⏳ Polling intento {attempt + 1}/{maxAttempts}... (Estado: {status})");
        //        }

        //        throw new Exception($"Timeout en polling de CapSolver ({_timeoutSeconds}s).");
        //    }
        //    finally
        //    {
        //        if (httpClient != null)
        //        {
        //            httpClient.Dispose();
        //        }
        //    }
        //}


        /// <summary>
        /// Envía la tarea a CapSolver y hace polling hasta obtener el token.
        /// Para reCAPTCHA v3 Enterprise VISIBLE.
        /// </summary>
        //private async Task<string> SubmitAndPollForSolutionAsync(string sitekey, string pageUrl, string action)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        var createPayload = new
        //        {
        //            clientKey = _apiKey,
        //            task = new
        //            {
        //                type = "ReCaptchaV3EnterpriseTaskProxyless",
        //                websiteURL = pageUrl,
        //                websiteKey = sitekey,
        //                pageAction = action
        //            }
        //        };

        //        var createContent = new StringContent(
        //            Newtonsoft.Json.JsonConvert.SerializeObject(createPayload),
        //            Encoding.UTF8,
        //            "application/json");

        //        var createResp = await httpClient.PostAsync(
        //            "https://api.capsolver.com/createTask",
        //            createContent);

        //        var createJson = JObject.Parse(await createResp.Content.ReadAsStringAsync());

        //        if (createJson["errorId"]?.Value<int>() != 0)
        //            throw new Exception(createJson["errorDescription"]?.ToString());

        //        var taskId = createJson["taskId"]?.ToString();
        //        if (string.IsNullOrEmpty(taskId))
        //            throw new Exception("CapSolver no devolvió taskId");

        //        for (int i = 0; i < _timeoutSeconds / _pollIntervalSeconds; i++)
        //        {
        //            await Task.Delay(_pollIntervalSeconds * 1000);

        //            var pollPayload = new
        //            {
        //                clientKey = _apiKey,
        //                taskId = taskId
        //            };

        //            var pollContent = new StringContent(
        //                Newtonsoft.Json.JsonConvert.SerializeObject(pollPayload),
        //                Encoding.UTF8,
        //                "application/json");

        //            var pollResp = await httpClient.PostAsync(
        //                "https://api.capsolver.com/getTaskResult",
        //                pollContent);

        //            var pollJson = JObject.Parse(await pollResp.Content.ReadAsStringAsync());

        //            if (pollJson["status"]?.ToString() == "ready")
        //            {
        //                return pollJson["solution"]?["gRecaptchaResponse"]?.ToString();
        //            }
        //        }

        //        throw new TimeoutException("Timeout esperando token de CapSolver");
        //    }
        //}

        /// <summary>
        /// Inyecta el token en el campo g-recaptcha-response, dispara eventos y ejecuta el callback.
        /// </summary>
        //private async Task InjectTokenAndCallbackAsync(IPage page, string token)
        //{
        //    await page.EvaluateAsync(@"
        //        (token) => {
        //            let responseField = document.getElementById('g-recaptcha-response') || 
        //                                document.querySelector('textarea[name=""g-recaptcha-response""]') ||
        //                                document.querySelector('[name=""g-recaptcha-response""]');

        //            if (responseField) {
        //                responseField.value = token;
        //                if (responseField.tagName.toUpperCase() === 'DIV') {
        //                    responseField.innerHTML = token;
        //                }

        //                const changeEvent = new Event('change', { bubbles: true });
        //                const inputEvent = new Event('input', { bubbles: true });
        //                responseField.dispatchEvent(changeEvent);
        //                responseField.dispatchEvent(inputEvent);

        //                console.log('Token inyectado y eventos disparados');
        //            } else {
        //                console.error('No se encontró campo g-recaptcha-response');
        //                return;
        //            }

        //            const recaptchaDiv = document.querySelector('.g-recaptcha');
        //            if (recaptchaDiv) {
        //                const callbackName = recaptchaDiv.getAttribute('data-callback');
        //                if (callbackName && window[callbackName]) {
        //                    window[callbackName](token);
        //                    console.log('Callback ejecutado: ' + callbackName);
        //                } else {
        //                    console.warn('No se encontró callback válido en data-callback');
        //                }
        //            } else {
        //                console.error('No se encontró div.g-recaptcha');
        //            }
        //        }"
        //        , token
        //    );
        //}

        /// <summary>
        /// Inyecta el token de reCAPTCHA Enterprise y ejecuta el callback específico.
        /// </summary>
        /// <summary>
        /// Inyecta el token de reCAPTCHA v3 Enterprise VISIBLE y ejecuta el callback.
        /// Para v3 visible, el flujo es similar a v2 pero sin iframe de desafío.
        /// </summary>
        private async Task InjectRecaptchaV3TokenAsync(IPage page, string token)
        {
            await page.EvaluateAsync(@"
                (token) => {
                    if (!window.grecaptcha || !grecaptcha.enterprise) {
                        console.warn('grecaptcha.enterprise no disponible');
                        return;
                    }

                    const originalExecute = grecaptcha.enterprise.execute;

                    grecaptcha.enterprise.execute = function () {
                        console.log('Interceptando execute(), devolviendo token');
                        return Promise.resolve(token);
                    };

                    console.log('Token v3 Enterprise inyectado correctamente');
                }
            ", token);
        }

        public async Task<bool> SolveRecaptchaV3EnterpriseWithVerificationAsync(IPage page, string pageUrl, string submitSelector = null, string action = "consulta_cel_recibidos")
        {
            try
            {
                _logger($"🔍 Iniciando resolución de reCAPTCHA v3 Enterprise para {pageUrl}");

                // Paso 1: Verificar si hay reCAPTCHA Enterprise
                bool hasEnterprise = await IsRecaptchaV3EnterprisePresentAsync(page);
                if (!hasEnterprise)
                {
                    _logger("ℹ️ No se detectó reCAPTCHA Enterprise. Continuando...");
                    return true;
                }

                _logger("🔒 reCAPTCHA v3 Enterprise detectado.");

                // Paso 2: Extraer sitekey
                string sitekey = await ExtractSiteKeyAsync(page);
                if (string.IsNullOrEmpty(sitekey))
                {
                    throw new Exception("No se pudo extraer el sitekey de reCAPTCHA Enterprise.");
                }
                _logger($"🔑 Sitekey extraído: {sitekey}");

                // Paso 3: Obtener token de CapSolver
                _logger($"🔄 Enviando solicitud a CapSolver con action: {action}");
                string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, action);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("No se recibió token válido de CapSolver.");
                }
                _logger($"✅ Token recibido de CapSolver");

                // Paso 4: Inyectar el token
                _logger("📝 Inyectando token...");
                await InjectRecaptchaV3EnterpriseTokenAsync(page, token);

                // Paso 5: Esperar a que se ejecute automáticamente
                _logger("⏳ Esperando ejecución automático del reCAPTCHA...");
                await page.WaitForTimeoutAsync(2000);

                // Paso 6: Hacer clic si hay selector
                if (!string.IsNullOrEmpty(submitSelector))
                {
                    _logger($"🖱️ Haciendo clic en botón: {submitSelector}");
                    await page.ClickAsync(submitSelector);
                }

                // Paso 7: Esperar procesamiento del servidor
                _logger("⏳ Esperando respuesta del servidor...");
                await page.WaitForTimeoutAsync(3000);

                // Paso 9: Verificar errores visuales en la página
                var pageStatus = await VerifyPageStatusAsync(page);
                _logger($"📋 Estado de la página:");
                _logger($"   - Tiene error de CAPTCHA: {pageStatus.HasCaptchaError}");
                _logger($"   - Tiene error general: {pageStatus.HasError}");
                _logger($"   - Token en página: {pageStatus.TokenPresent}");
                _logger($"   - Tabla de resultados: {pageStatus.HasResultsTable}");

                // Paso 10: Determinar si fue exitoso
                if (pageStatus.HasCaptchaError)
                {
                    _logger("❌ TOKEN RECHAZADO: La página muestra error de CAPTCHA");
                    return false;
                }

                if (pageStatus.HasResultsTable || !pageStatus.HasError)
                {
                    _logger("✅ TOKEN ACEPTADO: La página avanzó correctamente");
                    return true;
                }

                _logger("⚠️ Estado indeterminado. El token puede haber sido aceptado.");
                return true;
            }
            catch (Exception ex)
            {
                _logger($"❌ Error en resolución: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Método específico para reCAPTCHA v3 visible.
        /// Para v3 visible, puede requerir clic en el botón para activar el badge.
        /// </summary>
        //public async Task<bool> SolveRecaptchaV3VisibleAsync(IPage page, string pageUrl, string submitSelector = null, string action = "consulta_cel_recibidos")
        //{
        //    try
        //    {
        //        _logger($"🔍 Detectando reCAPTCHA v3 visible en {pageUrl}...");

        //        // Paso 1: Verificar si hay reCAPTCHA v3 en la página
        //        bool hasRecaptchaV3 = await IsRecaptchaV3VisiblePresentAsync(page);
        //        if (!hasRecaptchaV3)
        //        {
        //            _logger("ℹ️ No se detectó reCAPTCHA v3 visible. Continuando sin resolver.");
        //            return true;
        //        }

        //        _logger("🔒 reCAPTCHA v3 Enterprise (visible) detectado. Procediendo...");

        //        // Paso 2: Para v3 visible, puede ser necesario hacer clic en un botón primero
        //        if (!string.IsNullOrEmpty(submitSelector))
        //        {
        //            _logger($"🖱️ Haciendo clic en botón {submitSelector} para activar reCAPTCHA v3...");
        //            await page.ClickAsync(submitSelector);
        //            await page.WaitForTimeoutAsync(2000);
        //        }

        //        // Paso 3: Extraer sitekey
        //        string sitekey = await ExtractSiteKeyAsync(page);
        //        if (string.IsNullOrEmpty(sitekey))
        //        {
        //            throw new Exception("No se pudo extraer el sitekey de reCAPTCHA v3.");
        //        }
        //        _logger($"🔑 Sitekey extraído: {sitekey}");

        //        // Paso 4: Obtener token de CapSolver
        //        string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, action);
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            throw new Exception("No se recibió token válido de CapSolver para v3.");
        //        }
        //        _logger("✅ Token reCAPTCHA v3 recibido.");

        //        // Paso 5: Inyectar token y ejecutar callback
        //        //bool callbackExecuted = await InjectRecaptchaV3TokenAsync(page, token);
        //        //if (!callbackExecuted)
        //        //{
        //        //    _logger("⚠ Callback no se ejecutó automáticamente. Intentando rcBuscar...");
        //        //}

        //        await InjectRecaptchaV3TokenAsync(page, token);

        //        // Paso 6: Ejecutar rcBuscar si existe
        //        //await ExecuteRcBuscarAsync(page);

        //        // Paso 7: Pausa para procesamiento
        //        await page.WaitForTimeoutAsync(3000);

        //        //if (!string.IsNullOrEmpty(submitSelector))
        //        //{
        //        //    await page.ClickAsync(submitSelector);
        //        //}

        //        // Paso 8: Verificar si el badge de reCAPTCHA desapareció
        //        bool badgeStillVisible = await page.EvaluateAsync<bool>(@"
        //            () => {
        //                const badge = document.querySelector('.grecaptcha-badge');
        //                return badge && badge.offsetParent !== null;
        //            }
        //        ");

        //        if (badgeStillVisible)
        //        {
        //            _logger("⚠ Badge de reCAPTCHA v3 aún visible. Puede necesitar acción adicional.");
        //        }
        //        else
        //        {
        //            _logger("✅ Badge de reCAPTCHA v3 desaparecido (resuelto).");
        //        }

        //        _logger("✅ Proceso de reCAPTCHA v3 visible completado.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger($"❌ Error en resolución de reCAPTCHA v3 visible: {ex.Message}");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Detecta específicamente reCAPTCHA v3 visible.
        /// </summary>
        private async Task<bool> IsRecaptchaV3VisiblePresentAsync(IPage page)
        {
            // Primero verificar si es v3
            bool isV3 = await page.EvaluateAsync<bool>(@"
                () => {
                    // Verificar scripts de reCAPTCHA Enterprise v3
                    const scripts = document.getElementsByTagName('script');
                    for (const script of scripts) {
                        if (script.src && (script.src.includes('recaptcha/enterprise.js') || 
                                           script.src.includes('recaptcha/api.js'))) {
                            return true;
                        }
                    }
                    return !!(window.grecaptcha && window.grecaptcha.enterprise);
                }
            ");

            if (!isV3) return false;

            // Verificar si es visible (tiene badge)
            return await page.EvaluateAsync<bool>(@"
                () => {
                    // El badge de reCAPTCHA v3 visible
                    const badge = document.querySelector('.grecaptcha-badge');
                    if (badge && badge.offsetParent !== null) {
                        return true;
                    }
            
                    // Elementos con data-sitekey (pueden ser visibles o invisibles)
                    const sitekeyElements = document.querySelectorAll('[data-sitekey]');
                    for (const element of sitekeyElements) {
                        // Si el elemento es visible o tiene tamaño
                        const rect = element.getBoundingClientRect();
                        if (rect.width > 0 && rect.height > 0) {
                            return true;
                        }
                    }
            
                    return false;
                }
            ");
        }

        /// <summary>
        /// Método principal para resolver reCAPTCHA v3 Enterprise en el SRI.
        /// </summary>
        //public async Task<bool> SolveRecaptchaV3EnterpriseAsync(IPage page, string pageUrl, string submitSelector = null, string expectedAction = "consulta")
        //{
        //    try
        //    {
        //        _logger($"🔍 Iniciando resolución de reCAPTCHA v3 Enterprise para {pageUrl}");

        //        // Paso 1: Verificar si hay reCAPTCHA Enterprise
        //        bool hasEnterprise = await IsRecaptchaV3EnterprisePresentAsync(page);
        //        if (!hasEnterprise)
        //        {
        //            _logger("ℹ️ No se detectó reCAPTCHA Enterprise. Continuando...");
        //            return true;
        //        }

        //        _logger("🔒 reCAPTCHA v3 Enterprise detectado. Procediendo...");

        //        // Paso 2: Extraer parámetros
        //        //var (sitekey, action) = await ExtractEnterpriseParamsAsync(page);
        //        var sitekey = await ExtractSiteKeyAsync(page);
        //        var action = "consulta_cel_recibidos";
        //        if (string.IsNullOrEmpty(sitekey))
        //        {
        //            throw new Exception("No se pudo extraer el sitekey de reCAPTCHA Enterprise.");
        //        }

        //        _logger($"🔑 Sitekey extraído: {sitekey}");
        //        _logger($"🎯 Action detectado: {action}");

        //        // Usar la acción detectada o la esperada
        //        string finalAction = !string.IsNullOrEmpty(action) ? action : expectedAction;

        //        // Paso 3: Obtener token de CapSolver
        //        _logger($"🔄 Enviando solicitud a CapSolver para reCAPTCHA v3 Enterprise...");
        //        string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, finalAction);

        //        if (string.IsNullOrEmpty(token))
        //        {
        //            throw new Exception("No se recibió token válido de CapSolver.");
        //        }

        //        _logger($"✅ Token recibido: {token.Substring(0, 50)}...");

        //        // Paso 4: Inyectar el token
        //        await InjectRecaptchaV3EnterpriseTokenAsync(page, token);

        //        // Paso 5: Si hay selector de botón, hacer clic
        //        if (!string.IsNullOrEmpty(submitSelector))
        //        {
        //            _logger($"🖱️ Ejecutando acción en botón: {submitSelector}");
        //            await page.ClickAsync(submitSelector);
        //            await page.WaitForTimeoutAsync(3000);
        //        }

        //        // Paso 6: Verificar si se resolvió
        //        //bool captchaResuelto = await VerifyCaptchaResolvedAsync(page);

        //        //if (captchaResuelto)
        //        //{
        //        //    _logger("✅ reCAPTCHA v3 Enterprise resuelto exitosamente.");
        //        //}
        //        //else
        //        //{
        //        //    _logger("⚠️ reCAPTCHA puede no haberse resuelto completamente.");
        //        //}

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger($"❌ Error en resolución de reCAPTCHA v3 Enterprise: {ex.Message}");
        //        return false;
        //    }
        //}

        //public async Task<bool> SolveRecaptchaV3EnterpriseAsync(IPage page, string pageUrl, string submitSelector, string action = "consulta_cel_recibidos")
        //{
        //    try
        //    {
        //        _logger($"🔍 Iniciando resolución de reCAPTCHA v3 Enterprise INVISIBLE para {pageUrl}");

        //        // Paso 1: Verificar si hay reCAPTCHA Enterprise
        //        bool hasEnterprise = await IsRecaptchaV3EnterprisePresentAsync(page);
        //        if (!hasEnterprise)
        //        {
        //            _logger("ℹ️ No se detectó reCAPTCHA Enterprise. Continuando...");
        //            return true;
        //        }

        //        _logger("🔒 reCAPTCHA v3 Enterprise (invisible) detectado.");

        //        // Paso 2: Extraer sitekey
        //        string sitekey = await ExtractSiteKeyAsync(page);
        //        if (string.IsNullOrEmpty(sitekey))
        //        {
        //            throw new Exception("No se pudo extraer el sitekey de reCAPTCHA Enterprise.");
        //        }
        //        _logger($"🔑 Sitekey extraído: {sitekey}");

        //        // Paso 3: Obtener token de CapSolver PRIMERO (antes de hacer clic)
        //        _logger($"🔄 Enviando solicitud a CapSolver...");
        //        string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, action);
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            throw new Exception("No se recibió token válido de CapSolver.");
        //        }
        //        _logger($"✅ Token recibido de CapSolver");

        //        // Paso 4: Inyectar el token en la página (ANTES de hacer clic)
        //        _logger("📝 Inyectando token en grecaptcha.enterprise...");
        //        await InjectRecaptchaV3EnterpriseTokenAsync(page, token);

        //        // Paso 5: Esperar a que grecaptcha esté listo
        //        await page.WaitForTimeoutAsync(1000);

        //        // Paso 6: Hacer clic en el botón para disparar el flujo
        //        _logger($"🖱️ Haciendo clic en botón: {submitSelector}");
        //        await page.ClickAsync(submitSelector);

        //        // Paso 7: Esperar a que se procese
        //        await page.WaitForTimeoutAsync(3000);

        //        _logger("✅ Proceso de reCAPTCHA v3 Enterprise completado.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger($"❌ Error en resolución de reCAPTCHA v3 Enterprise: {ex.Message}");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Inyecta el token en reCAPTCHA v3 Enterprise y ejecuta callbacks.
        /// </summary>
        //private async Task InjectRecaptchaV3EnterpriseTokenAsync(IPage page, string token)
        //{
        //    await page.EvaluateAsync(@"(
        //        token) => {
        //        console.log('Injecting reCAPTCHA v3 Enterprise token...');

        //        // Método 1: Sobrescribir grecaptcha.enterprise.execute
        //        if (window.grecaptcha && window.grecaptcha.enterprise) {
        //            const originalExecute = window.grecaptcha.enterprise.execute;

        //            window.grecaptcha.enterprise.execute = function(sitekey, options) {
        //                console.log('Interceptando grecaptcha.enterprise.execute, devolviendo token...');
        //                return Promise.resolve(token);
        //            };

        //            // También manejar la versión con callback
        //            window.grecaptcha.enterprise.ready = function(callback) {
        //                console.log('grecaptcha.enterprise.ready llamado');
        //                if (callback) callback();
        //            };
        //        }

        //        // Método 2: Buscar textarea oculto de respuesta
        //        const responseFields = document.querySelectorAll('textarea[name=""g-recaptcha-response""], input[name=""g-recaptcha-response""]');
        //        for (const field of responseFields) {
        //            field.value = token;
        //            field.dispatchEvent(new Event('change', { bubbles: true }));
        //            field.dispatchEvent(new Event('input', { bubbles: true }));
        //            console.log('Token inyectado en campo:', field);
        //        }

        //        // Método 3: Disparar eventos globales
        //        document.dispatchEvent(new CustomEvent('recaptcha-token', { 
        //            detail: { token: token } 
        //        }));

        //        // Método 4: Si hay una función de callback global, ejecutarla
        //        if (window.recaptchaCallback) {
        //            window.recaptchaCallback(token);
        //        }

        //        // Método 5: Para el SRI específicamente
        //        const sriForm = document.querySelector('form[id*=""frmPrincipal""]');
        //        if (sriForm) {
        //            // Crear campo oculto si no existe
        //            let hiddenField = sriForm.querySelector('input[name=""g-recaptcha-response-enterprise""]');
        //            if (!hiddenField) {
        //                hiddenField = document.createElement('input');
        //                hiddenField.type = 'hidden';
        //                hiddenField.name = 'g-recaptcha-response-enterprise';
        //                hiddenField.id = 'g-recaptcha-response-enterprise';
        //                sriForm.appendChild(hiddenField);
        //            }
        //            hiddenField.value = token;
        //        }

        //        console.log('Token inyectado exitosamente.');
        //    }", token);

        //    await page.WaitForTimeoutAsync(2000);
        //}

        /// <summary>
        /// Inyecta el token en reCAPTCHA v3 Enterprise.
        /// </summary>
        private async Task InjectRecaptchaV3EnterpriseTokenAsync(IPage page, string token, string submitSelector)
        {
            await page.EvaluateAsync(@"(args) => {
                const token = args.token;
                const submitSelector = args.submitSelector;
                console.log('=== INYECCIÓN reCAPTCHA v3 Enterprise ===');
                console.log('Token:', token.substring(0, 50) + '...');
                console.log('Selector botón:', submitSelector);
                console.log('Argumentos recibidos:', JSON.stringify(args));

                // ========== VERIFICAR GRECAPTCHA ==========
                if (!window.grecaptcha || !window.grecaptcha.enterprise) {
                    console.error('❌ grecaptcha.enterprise NO DISPONIBLE');
                    return;
                }
                console.log('✅ grecaptcha.enterprise disponible');

                // ========== GUARDAR FUNCIÓN ORIGINAL ==========
                const originalExecute = window.grecaptcha.enterprise.execute;
                console.log('✅ Execute original guardado');

                // ========== ALMACENAR TOKEN ==========
                window.__captchaToken = token;
                console.log('✅ Token almacenado en window.__captchaToken');

                // ========== SOBRESCRIBIR EXECUTE() ==========
                window.grecaptcha.enterprise.execute = function(options) {
                    console.log('🎯 execute() INTERCEPTADO');
                    console.log('Opciones:', JSON.stringify(options));
            
                    const tokenPromise = Promise.resolve(token);
            
                    tokenPromise.then((resolvedToken) => {
                        console.log('✅ Token listo para inyectar');
                
                        // INYECTAR EN TODOS LOS CAMPOS POSIBLES
                        const fields = [
                            'g-recaptcha-response',
                            'g-recaptcha-response-enterprise',
                            '__grecaptcha_token'
                        ];
                
                        for (const fieldName of fields) {
                            // Buscar textarea
                            let field = document.querySelector('textarea[name=""' + fieldName + '""]');
                            if (field) {
                                field.value = resolvedToken;
                                field.dispatchEvent(new Event('change', { bubbles: true }));
                                field.dispatchEvent(new Event('input', { bubbles: true }));
                                console.log('✅ Token inyectado en textarea:', fieldName);
                            }
                    
                            // Buscar input
                            field = document.querySelector('input[name=""' + fieldName + '""]');
                            if (field) {
                                field.value = resolvedToken;
                                field.dispatchEvent(new Event('change', { bubbles: true }));
                                field.dispatchEvent(new Event('input', { bubbles: true }));
                                console.log('✅ Token inyectado en input:', fieldName);
                            }
                        }

                        // EJECUTAR CALLBACK
                        if (window.onSubmit && typeof window.onSubmit === 'function') {
                            console.log('🔔 EJECUTANDO onSubmit()');
                            try {
                                window.onSubmit();
                                console.log('✅ onSubmit() ejecutado');
                            } catch (e) {
                                console.error('❌ Error en onSubmit():', e.message);
                            }
                        } else {
                            console.warn('⚠️ onSubmit NO ENCONTRADO, buscando rcBuscar...');
                    
                            if (window.rcBuscar && typeof window.rcBuscar === 'function') {
                                console.log('🔔 EJECUTANDO rcBuscar()');
                                try {
                                    window.rcBuscar();
                                    console.log('✅ rcBuscar() ejecutado');
                                } catch (e) {
                                    console.error('❌ Error en rcBuscar():', e.message);
                                }
                            }
                        }
                    }).catch((err) => {
                        console.error('❌ Error en Promise:', err);
                    });

                    return tokenPromise;
                };

                console.log('✅ execute() sobrescrito correctamente');

                // PRE-INYECTAR EN CAMPOS VISIBLES
                const allFields = document.querySelectorAll(
                    'textarea[name*=""recaptcha""], ' +
                    'input[name*=""recaptcha""], ' +
                    '[name*=""g-recaptcha""]'
                );
        
                console.log('🔍 Campos encontrados:', allFields.length);
                for (const field of allFields) {
                    field.value = token;
                    console.log('✅ Pre-inyectado en:', field.name || field.id || 'sin nombre');
                }

                console.log('=== INYECCIÓN COMPLETADA ===');

            }", new { token = token, submitSelector = submitSelector });

            _logger("✅ Inyección ejecutada en la página");
        }

        /// <summary>
        /// Verifica el estado actual de la página (errores, tablas, etc)
        /// </summary>
        private async Task<PageStatus> VerifyPageStatusAsync(IPage page)
        {
            try
            {
                var statusJson = await page.EvaluateAsync<string>(@"
                    () => {
                        const result = {
                            hasCaptchaError: false,
                            hasError: false,
                            tokenPresent: false,
                            hasResultsTable: false,
                            errorMessage: '',
                            pageTitle: document.title,
                            bodyText: document.body.innerText.substring(0, 500)
                        };

                        // ========== BUSCAR ERRORES DE CAPTCHA ==========
                        const pageText = document.body.innerText.toLowerCase();
                
                        const captchaErrors = [
                            'captcha incorrecta',
                            'captcha inválid',
                            'recaptcha failed',
                            'robot verification failed',
                            'verificación de robot',
                            'failed captcha'
                        ];

                        for (const error of captchaErrors) {
                            if (pageText.includes(error)) {
                                result.hasCaptchaError = true;
                                result.errorMessage = error;
                                break;
                            }
                        }

                        // ========== BUSCAR ERRORES GENERALES ==========
                        const errorElements = document.querySelectorAll(
                            '[class*=""error""], ' +
                            '[class*=""danger""], ' +
                            '[class*=""alert""], ' +
                            '.ui-messages-error, ' +
                            '.error-message'
                        );
                
                        if (errorElements.length > 0) {
                            result.hasError = true;
                            result.errorMessage = errorElements[0].innerText.substring(0, 100);
                        }

                        // ========== VERIFICAR TOKEN EN PÁGINA ==========
                        const tokenField = document.querySelector('textarea[name=""g-recaptcha-response""]') ||
                                         document.querySelector('input[name=""g-recaptcha-response""]');
                
                        if (tokenField && tokenField.value && tokenField.value.length > 50) {
                            result.tokenPresent = true;
                        }

                        // ========== BUSCAR TABLA DE RESULTADOS ==========
                        const resultTables = [
                            '#frmPrincipal\\:tablaCompRecibidos',
                            '[id*=""tabla""]',
                            '[id*=""results""]',
                            '[id*=""listado""]'
                        ];

                        for (const selector of resultTables) {
                            const table = document.querySelector(selector);
                            if (table && table.offsetParent !== null) {
                                result.hasResultsTable = true;
                                break;
                            }
                        }

                        // ========== BUSCAR INDICADORES DE ÉXITO ==========
                        if (pageText.includes('comprobante') || pageText.includes('resultado')) {
                            result.hasResultsTable = true;
                        }

                        return JSON.stringify(result);
                    }
                ");

                var statusDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(statusJson);

                bool hasCaptchaError = false;
                bool hasError = false;
                bool tokenPresent = false;
                bool hasResultsTable = false;

                if (statusDict.ContainsKey("hasCaptchaError"))
                {
                    bool.TryParse(statusDict["hasCaptchaError"].ToString(), out hasCaptchaError);
                }

                if (statusDict.ContainsKey("hasError"))
                {
                    bool.TryParse(statusDict["hasError"].ToString(), out hasError);
                }

                if (statusDict.ContainsKey("tokenPresent"))
                {
                    bool.TryParse(statusDict["tokenPresent"].ToString(), out tokenPresent);
                }

                if (statusDict.ContainsKey("hasResultsTable"))
                {
                    bool.TryParse(statusDict["hasResultsTable"].ToString(), out hasResultsTable);
                }

                string responseBody = "";
                if (statusDict.ContainsKey("bodyText"))
                {
                    responseBody = statusDict["bodyText"].ToString();
                }

                return new PageStatus
                {
                    HasCaptchaError = hasCaptchaError,
                    HasError = hasError,
                    TokenPresent = tokenPresent,
                    HasResultsTable = hasResultsTable,
                    ResponseBody = responseBody
                };
            }
            catch (Exception ex)
            {
                _logger($"Error verificando estado de página: {ex.Message}");
                return new PageStatus { HasError = true };
            }
        }

        /// <summary>
        /// Inyecta el token en reCAPTCHA v3 Enterprise
        /// </summary>
        private async Task InjectRecaptchaV3EnterpriseTokenAsync(IPage page, string token)
        {
            await page.EvaluateAsync(@"(args) => {
                const token = args.token;
                console.log('=== INYECCIÓN reCAPTCHA v3 Enterprise ===');

                const waitForGrecaptcha = () => {
                    return new Promise((resolve) => {
                        const checkInterval = setInterval(() => {
                            if (window.grecaptcha && window.grecaptcha.enterprise) {
                                clearInterval(checkInterval);
                                console.log('✅ grecaptcha.enterprise disponible');
                                resolve();
                            }
                        }, 100);
                
                        setTimeout(() => {
                            clearInterval(checkInterval);
                            console.warn('⚠️ Timeout esperando grecaptcha');
                            resolve();
                        }, 10000);
                    });
                };

                const setupRenderInterceptor = () => {
                    if (!window.grecaptcha || !window.grecaptcha.enterprise) {
                        setTimeout(setupRenderInterceptor, 100);
                        return;
                    }

                    const originalRender = window.grecaptcha.enterprise.render;
                    window.grecaptcha.enterprise.render = function(element, options) {
                        console.log('🎯 render() interceptado');
                
                        const originalCallback = options.callback;
                
                        options.callback = function() {
                            console.log('✅ render() callback - inyectando token');
                    
                            const responseField = document.querySelector('textarea[name=""g-recaptcha-response""]') ||
                                                document.querySelector('input[name=""g-recaptcha-response""]');
                            if (responseField) {
                                responseField.value = token;
                                responseField.dispatchEvent(new Event('change', { bubbles: true }));
                            }
                    
                            if (originalCallback && typeof originalCallback === 'function') {
                                originalCallback(token);
                            }
                        };
                
                        return originalRender.call(this, element, options);
                    };
                };

                const setupExecuteInterceptor = () => {
                    if (!window.grecaptcha || !window.grecaptcha.enterprise) {
                        setTimeout(setupExecuteInterceptor, 100);
                        return;
                    }

                    const originalExecute = window.grecaptcha.enterprise.execute;
                    window.grecaptcha.enterprise.execute = function(options) {
                        console.log('🎯 execute() interceptado');
                
                        const tokenPromise = Promise.resolve(token);
                
                        tokenPromise.then((resolvedToken) => {
                            console.log('✅ execute() - token resuelto');
                    
                            const responseField = document.querySelector('textarea[name=""g-recaptcha-response""]') ||
                                                document.querySelector('input[name=""g-recaptcha-response""]');
                            if (responseField) {
                                responseField.value = resolvedToken;
                                responseField.dispatchEvent(new Event('change', { bubbles: true }));
                            }
                    
                            if (window.onSubmit && typeof window.onSubmit === 'function') {
                                console.log('✅ Ejecutando onSubmit()');
                                window.onSubmit();
                            }
                        });
                
                        return tokenPromise;
                    };
                };

                waitForGrecaptcha().then(() => {
                    setupRenderInterceptor();
                    setupExecuteInterceptor();
                    window.__captchaToken = token;
                    console.log('=== INYECCIÓN COMPLETADA ===');
                });

            }", new { token = token });
        }

        public async Task<bool> SolveOnButtonClickAsync(IPage page, string pageUrl, string submitSelector = null, string formSelector = null, bool alreadyClicked = false)
        {
            if (string.IsNullOrEmpty(pageUrl)) pageUrl = page.Url;

            try
            {
                _logger($"🔍 Analizando página: {pageUrl}");

                // Detectar tipo específico de CAPTCHA
                bool isEnterprise = await IsRecaptchaV3EnterprisePresentAsync(page);

                if (isEnterprise)
                {
                    _logger("🔍 Detectado reCAPTCHA v3 Enterprise. Usando método específico...");

                    // Para el SRI, la acción suele ser 'consulta_cel_recibidos' o similar
                    // pero extraeremos la acción real de la página
                    return await SolveRecaptchaV3EnterpriseWithVerificationAsync(
                        page,
                        pageUrl,
                        submitSelector,
                        "consulta_cel_recibidos");
                }

                // Si no es Enterprise, verificar si es reCAPTCHA v2
                bool isRecaptchaV2 = await page.EvaluateAsync<bool>(@"
                    () => {
                        // Buscar iframe de reCAPTCHA v2
                        const iframe = document.querySelector('iframe[src*=""recaptcha""]');
                        if (iframe) return true;
                
                        // Buscar div.g-recaptcha
                        const recaptchaDiv = document.querySelector('.g-recaptcha');
                        if (recaptchaDiv) return true;
                
                        return false;
                    }
                ");

                if (isRecaptchaV2)
                {
                    _logger("🔍 Detectado reCAPTCHA v2. Usando método v2...");
                    // Aquí llamarías a tu método existente para v2
                    // return await SolveRecaptchaV2Async(page, pageUrl, submitSelector, formSelector, alreadyClicked);
                    return false;
                }

                _logger("ℹ️ No se detectó CAPTCHA conocido. Continuando...");
                return true;
            }
            catch (Exception ex)
            {
                _logger($"❌ Error en detección/resolución de CAPTCHA: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía la tarea a CapSolver para reCAPTCHA v3 Enterprise y hace polling.
        /// </summary>
        private async Task<string> SubmitAndPollForSolutionAsync(string sitekey, string pageUrl, string action = "submit", bool isEnterprise = true)
        {
            using (var httpClient = new HttpClient())
            {
                // Configurar timeout
                httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

                var createPayload = new
                {
                    clientKey = _apiKey,
                    task = new
                    {
                        type = "ReCaptchaV3EnterpriseTaskProxyless",
                        websiteURL = pageUrl,
                        websiteKey = sitekey,
                        pageAction = action,
                        // Opcional: especificar mínimo score si se conoce
                        // minScore = 0.7
                    }
                };

                _logger($"📤 Enviando solicitud a CapSolver: {JsonConvert.SerializeObject(createPayload)}");

                var createContent = new StringContent(
                    JsonConvert.SerializeObject(createPayload),
                    Encoding.UTF8,
                    "application/json");

                var createResp = await httpClient.PostAsync(
                    "https://api.capsolver.com/createTask",
                    createContent);

                var createJson = JObject.Parse(await createResp.Content.ReadAsStringAsync());

                if (createJson["errorId"]?.Value<int>() != 0)
                {
                    string errorDesc = createJson["errorDescription"]?.ToString() ?? "Error desconocido";
                    throw new Exception($"Error CapSolver: {errorDesc}");
                }

                var taskId = createJson["taskId"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                {
                    throw new Exception("CapSolver no devolvió taskId");
                }

                _logger($"📋 Tarea creada con ID: {taskId}. Esperando solución...");

                // Polling
                int maxAttempts = _timeoutSeconds / _pollIntervalSeconds;
                for (int i = 0; i < maxAttempts; i++)
                {
                    await Task.Delay(_pollIntervalSeconds * 1000);

                    var pollPayload = new
                    {
                        clientKey = _apiKey,
                        taskId = taskId
                    };

                    var pollContent = new StringContent(
                        JsonConvert.SerializeObject(pollPayload),
                        Encoding.UTF8,
                        "application/json");

                    var pollResp = await httpClient.PostAsync(
                        "https://api.capsolver.com/getTaskResult",
                        pollContent);

                    var pollJson = JObject.Parse(await pollResp.Content.ReadAsStringAsync());

                    var status = pollJson["status"]?.ToString();

                    if (status == "ready")
                    {
                        var solution = pollJson["solution"];
                        if (solution != null)
                        {
                            _logger("✅ Solución recibida de CapSolver");
                            return solution["gRecaptchaResponse"]?.ToString();
                        }
                    }
                    else if (status == "failed")
                    {
                        string error = pollJson["errorDescription"]?.ToString() ?? "Error desconocido";
                        throw new Exception($"CapSolver falló: {error}");
                    }

                    _logger($"⏳ Polling {i + 1}/{maxAttempts} - Estado: {status}");
                }

                throw new TimeoutException($"Timeout esperando solución de CapSolver ({_timeoutSeconds}s)");
            }
        }

        /// <summary>
        /// Verifica si el reCAPTCHA fue resuelto.
        /// </summary>
        private async Task<bool> VerifyCaptchaResolvedAsync(IPage page)
        {
            return await page.EvaluateAsync<bool>(@"
                () => {
                    // Verificar si hay mensajes de error de CAPTCHA
                    const errorMessages = document.body.innerText;
                    const captchaErrors = [
                        'captcha incorrecto',
                        'captcha inválido',
                        'recaptcha failed',
                        'robot verification failed',
                        'verificación de robot'
                    ];
            
                    for (const error of captchaErrors) {
                        if (errorMessages.toLowerCase().includes(error)) {
                            return false;
                        }
                    }
            
                    // Verificar si hay elementos de CAPTCHA visibles
                    const captchaBadge = document.querySelector('.grecaptcha-badge');
                    if (captchaBadge && captchaBadge.offsetParent !== null) {
                        // Si el badge aún es visible, verificar si está oculto por estilo
                        const style = window.getComputedStyle(captchaBadge);
                        if (style.visibility !== 'hidden' && style.display !== 'none') {
                            return false;
                        }
                    }
            
                    // Verificar si la página procedió (tabla de resultados visible)
                    const resultTable = document.querySelector('#frmPrincipal\\\\:tablaCompRecibidos');
                    if (resultTable && resultTable.offsetParent !== null) {
                        return true;
                    }
            
                    // Verificar si el botón de descarga está habilitado
                    const downloadLink = document.querySelector('#frmPrincipal\\\\:lnkTxtlistado');
                    if (downloadLink && downloadLink.offsetParent !== null) {
                        return true;
                    }
            
                    return false;
                }
            ");
        }

        /// <summary>
        /// Detecta específicamente reCAPTCHA v3 Enterprise en el sitio del SRI.
        /// </summary>
        private async Task<bool> IsRecaptchaV3EnterprisePresentAsync(IPage page)
        {
            return await page.EvaluateAsync<bool>(@"
                () => {
                    // 1. Verificar scripts de reCAPTCHA Enterprise
                    const scripts = document.getElementsByTagName('script');
                    for (const script of scripts) {
                        if (script.src && script.src.includes('recaptcha/enterprise.js')) {
                            return true;
                        }
                        if (script.innerText && script.innerText.includes('grecaptcha.enterprise')) {
                            return true;
                        }
                    }
            
                    // 2. Verificar si grecaptcha.enterprise está disponible
                    if (window.grecaptcha && window.grecaptcha.enterprise) {
                        return true;
                    }
            
                    // 3. Verificar elementos con data-sitekey (común en Enterprise)
                    const sitekeyElements = document.querySelectorAll('[data-sitekey]');
                    if (sitekeyElements.length > 0) {
                        return true;
                    }
            
                    // 4. Verificar botones con onclick que ejecuten grecaptcha
                    const buttons = document.querySelectorAll('button, input[type=""submit""]');
                    for (const btn of buttons) {
                        const onclick = btn.getAttribute('onclick') || '';
                        if (onclick.includes('grecaptcha') || onclick.includes('recaptcha')) {
                            return true;
                        }
                    }
            
                    return false;
                }
            ");
        }

        /// <summary>
        /// Extrae el sitekey y action específicos para reCAPTCHA v3 Enterprise del SRI.
        /// </summary>
        private async Task<(string sitekey, string action)> ExtractEnterpriseParamsAsync(IPage page)
        {
            var result = await page.EvaluateAsync<JObject>(@"
                () => {
                    const params = { sitekey: '', action: 'submit' };
            
                    // 1. Buscar sitekey en elementos con data-sitekey
                    const sitekeyElements = document.querySelectorAll('[data-sitekey]');
                    for (const element of sitekeyElements) {
                        const sitekey = element.getAttribute('data-sitekey');
                        if (sitekey && sitekey.trim().length > 10) {
                            params.sitekey = sitekey;
                            break;
                        }
                    }
            
                    // 2. Si no se encontró, buscar en scripts
                    if (!params.sitekey) {
                        const scripts = document.getElementsByTagName('script');
                        for (const script of scripts) {
                            if (script.innerText && script.innerText.includes('grecaptcha.enterprise')) {
                                // Buscar sitekey en el script
                                const sitekeyMatch = script.innerText.match(/sitekey\s*['""]([^'""]+)['""]/);
                                if (sitekeyMatch && sitekeyMatch[1]) {
                                    params.sitekey = sitekeyMatch[1];
                                }
                        
                                // Buscar acción en el script
                                const actionMatch = script.innerText.match(/action\s*['""]([^'""]+)['""]/);
                                if (actionMatch && actionMatch[1]) {
                                    params.action = actionMatch[1];
                                }
                            }
                        }
                    }
            
                    // 3. Si aún no hay sitekey, buscar en window.grecaptcha
                    if (!params.sitekey && window.grecaptcha && window.grecaptcha.enterprise) {
                        try {
                            if (window.___grecaptcha_cfg && window.___grecaptcha_cfg.clients) {
                                const clients = window.___grecaptcha_cfg.clients;
                                for (const key in clients) {
                                    if (clients[key].sitekey) {
                                        params.sitekey = clients[key].sitekey;
                                        break;
                                    }
                                }
                            }
                        } catch (e) {
                            console.warn('Error reading grecaptcha config:', e);
                        }
                    }
            
                    // 4. Para el SRI, la acción suele ser específica
                    // Intentar determinar la acción del contexto
                    const form = document.querySelector('form');
                    if (form) {
                        const formId = form.id || form.name || '';
                        if (formId.includes('login') || formId.includes('ingresar')) {
                            params.action = 'login';
                        } else if (formId.includes('consulta') || formId.includes('buscar')) {
                            params.action = 'consulta';
                        } else if (formId.includes('descarga') || formId.includes('descargar')) {
                            params.action = 'download';
                        }
                    }
            
                    return params;
                }
            ");

            return (result["sitekey"]?.ToString(), result["action"]?.ToString());
        }

        /// <summary>
        /// Detecta si es reCAPTCHA v3 Enterprise INVISIBLE (sin badge visible).
        /// </summary>
        private async Task<bool> IsRecaptchaV3InvisibleAsync(IPage page)
        {
            return await page.EvaluateAsync<bool>(@"
        () => {
            // Verificar si hay scripts de reCAPTCHA Enterprise
            const scripts = document.getElementsByTagName('script');
            let hasEnterpriseScript = false;
            for (const script of scripts) {
                if (script.src && script.src.includes('recaptcha/enterprise.js')) {
                    hasEnterpriseScript = true;
                    break;
                }
            }
            
            if (!hasEnterpriseScript) return false;
            
            // Verificar si grecaptcha.enterprise está disponible
            if (!window.grecaptcha || !window.grecaptcha.enterprise) {
                return false;
            }
            
            // Para v3 invisible, NO debe haber badge visible
            const badge = document.querySelector('.grecaptcha-badge');
            if (badge && badge.offsetParent !== null) {
                // Hay badge visible, no es invisible puro
                return false;
            }
            
            // Buscar elementos que usen grecaptcha.enterprise.execute
            // común en formularios SRI
            const scriptsText = Array.from(scripts)
                .filter(s => s.innerText)
                .map(s => s.innerText);
            
            for (const text of scriptsText) {
                if (text.includes('grecaptcha.enterprise.execute') || 
                    text.includes('recaptcha.execute')) {
                    return true;
                }
            }
            
            // Verificar botones con onclick que llamen a recaptcha
            const buttons = document.querySelectorAll('button, input[type=""submit""]');
            for (const btn of buttons) {
                const onclick = btn.getAttribute('onclick') || '';
                if (onclick.includes('grecaptcha') || onclick.includes('recaptcha.execute')) {
                    return true;
                }
            }
            
            return false;
        }
    ");
        }

        /// <summary>
        /// Resuelve reCAPTCHA v3 Enterprise INVISIBLE específico para el SRI.
        /// </summary>
        //public async Task<bool> SolveRecaptchaV3EnterpriseAsync(IPage page, string pageUrl, string submitSelector, string action = "consulta_cel_recibidos")
        //{
        //    try
        //    {
        //        _logger($"🔍 Iniciando resolución de reCAPTCHA v3 Enterprise INVISIBLE para {pageUrl}");

        //        // Paso 1: Verificar si hay reCAPTCHA Enterprise
        //        bool hasEnterprise = await IsRecaptchaV3EnterprisePresentAsync(page);
        //        if (!hasEnterprise)
        //        {
        //            _logger("ℹ️ No se detectó reCAPTCHA Enterprise. Continuando...");
        //            return true;
        //        }

        //        _logger("🔒 reCAPTCHA v3 Enterprise (invisible) detectado.");

        //        // Paso 2: Extraer sitekey
        //        string sitekey = await ExtractSiteKeyAsync(page);
        //        if (string.IsNullOrEmpty(sitekey))
        //        {
        //            throw new Exception("No se pudo extraer el sitekey de reCAPTCHA Enterprise.");
        //        }
        //        _logger($"🔑 Sitekey extraído: {sitekey}");

        //        // Paso 3: Obtener token de CapSolver PRIMERO (antes de hacer clic)
        //        _logger($"🔄 Enviando solicitud a CapSolver...");
        //        string token = await SubmitAndPollForSolutionAsync(sitekey, pageUrl, action);
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            throw new Exception("No se recibió token válido de CapSolver.");
        //        }
        //        _logger($"✅ Token recibido de CapSolver");

        //        // Paso 4: Inyectar el token en la página (ANTES de hacer clic)
        //        _logger("📝 Inyectando token en grecaptcha.enterprise...");
        //        await InjectRecaptchaV3EnterpriseTokenAsync(page, token);

        //        // Paso 5: Esperar a que grecaptcha esté listo
        //        await page.WaitForTimeoutAsync(1000);

        //        // Paso 6: Hacer clic en el botón para disparar el flujo
        //        _logger($"🖱️ Haciendo clic en botón: {submitSelector}");
        //        await page.ClickAsync(submitSelector);

        //        // Paso 7: Esperar a que se procese
        //        await page.WaitForTimeoutAsync(3000);

        //        _logger("✅ Proceso de reCAPTCHA v3 Enterprise completado.");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger($"❌ Error en resolución de reCAPTCHA v3 Enterprise: {ex.Message}");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Estructura para almacenar el estado de la página
        /// </summary>
        private class PageStatus
        {
            public bool HasCaptchaError { get; set; }
            public bool HasError { get; set; }
            public bool TokenPresent { get; set; }
            public bool HasResultsTable { get; set; }
            public string ResponseBody { get; set; }
        }
    } 
}
