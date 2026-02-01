using botapp.Helpers;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    class CapSolverV3Enterprise
    {
        private readonly string _apiKey;
        private readonly int _timeoutSeconds;
        private readonly int _pollIntervalSeconds;
        private readonly string _logFilePath;

        public CapSolverV3Enterprise(string apiKey, string logFilePath, int timeoutSeconds = 120, int pollIntervalSeconds = 5)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _timeoutSeconds = Math.Max(30, timeoutSeconds);
            _pollIntervalSeconds = Math.Max(1, pollIntervalSeconds);
        }

        private void _logger(string message)
        {
            LoggerHelper.Log(_logFilePath, message);
        }

        /// <summary>
        /// Método principal con la misma firma que el original para mantener compatibilidad.
        /// </summary>
        public async Task<bool> SolveOnButtonClickAsync(
            IPage page,
            string pageUrl,
            string submitSelector = null,
            string formSelector = null,
            bool alreadyClicked = false)
        {
            return await SolveRecaptchaV3EnterpriseAsync(
                page,
                pageUrl,
                submitSelector,
                formSelector,
                alreadyClicked,
                "consulta_cel_recibidos");
        }

        /// <summary>
        /// Resuelve reCAPTCHA v3 Enterprise invisible.
        /// </summary>
        public async Task<bool> SolveRecaptchaV3EnterpriseAsync(
            IPage page,
            string pageUrl,
            string submitSelector = null,
            string formSelector = null,
            bool alreadyClicked = false,
            string action = "consulta_cel_recibidos")
        {
            try
            {
                _logger($"🔍 Iniciando resolución reCAPTCHA v3 Enterprise (acción: {action})");

                // 1. Verificar si ya está resuelto o no es necesario
                bool captchaPresent = await IsRecaptchaPresentAsync(page);
                if (!captchaPresent)
                {
                    _logger("ℹ️ No se detectó reCAPTCHA. Continuando...");
                    return true;
                }

                // 2. Hacer clic en el botón si es necesario
                if (!alreadyClicked && !string.IsNullOrEmpty(submitSelector))
                {
                    _logger($"🖱️ Activando reCAPTCHA con clic en: {submitSelector}");
                    await page.ClickAsync(submitSelector);
                    await page.WaitForTimeoutAsync(2000);
                }

                // 3. Extraer sitekey
                string sitekey = await ExtractSiteKeyAsync(page);
                if (string.IsNullOrEmpty(sitekey))
                {
                    _logger("⚠️ No se pudo extraer sitekey. Intentando método alternativo...");
                    sitekey = await FindSiteKeyInScriptsAsync(page);

                    if (string.IsNullOrEmpty(sitekey))
                    {
                        throw new Exception("No se pudo encontrar el sitekey de reCAPTCHA");
                    }
                }
                _logger($"🔑 Sitekey: {sitekey}");

                // 4. Obtener token de CapSolver
                _logger("📤 Solicitando token a CapSolver...");
                string token = await GetTokenFromCapSolverAsync(sitekey, pageUrl, action);

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("No se recibió token de CapSolver");
                }
                _logger($"✅ Token recibido ({token.Substring(0, Math.Min(40, token.Length))}...)");

                // 5. Inyectar token y ejecutar callback
                _logger("🔄 Inyectando token y ejecutando callback...");
                bool injected = await InjectTokenAsync(page, token);

                if (!injected)
                {
                    _logger("⚠️ No se pudo inyectar token. Intentando método directo...");
                    await ExecuteDirectMethodAsync(page);
                }

                // 6. Re-submitear si es necesario
                if (!string.IsNullOrEmpty(formSelector))
                {
                    _logger("📤 Re-submiteando formulario...");
                    await page.EvaluateAsync($"(sel) => {{ const f = document.querySelector(sel); if (f) f.submit(); }}", formSelector);
                }
                else if (!string.IsNullOrEmpty(submitSelector))
                {
                    await page.ClickAsync(submitSelector);
                }

                await page.WaitForTimeoutAsync(2000);
                _logger("✅ reCAPTCHA v3 Enterprise resuelto exitosamente.");
                return true;
            }
            catch (Exception ex)
            {
                _logger($"❌ Error resolviendo reCAPTCHA v3 Enterprise: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> IsRecaptchaPresentAsync(IPage page)
        {
            return await page.EvaluateAsync<bool>(@"
                () => {
                    // Verificar múltiples indicadores de reCAPTCHA
                    if (window.grecaptcha && window.grecaptcha.enterprise) return true;
                    if (document.querySelector('[data-sitekey]')) return true;
                    if (typeof window.cargarRecaptcha === 'function') return true;
                    
                    const scripts = document.getElementsByTagName('script');
                    for (const script of scripts) {
                        if (script.src && script.src.includes('recaptcha/enterprise.js')) return true;
                        if (script.innerText && script.innerText.includes('grecaptcha.enterprise')) return true;
                    }
                    
                    return false;
                }
            ");
        }

        private async Task<string> ExtractSiteKeyAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(@"
                () => {
                    // Buscar en elementos con data-sitekey
                    const element = document.querySelector('[data-sitekey]');
                    if (element) return element.getAttribute('data-sitekey');
                    
                    // Buscar en scripts
                    const scripts = document.getElementsByTagName('script');
                    for (const script of scripts) {
                        if (!script.innerText) continue;
                        
                        // Buscar patrones comunes
                        const patterns = [
                            /sitekey\s*:\s*['""]([^'""]+)['""]/,
                            /['""]sitekey['""]\s*:\s*['""]([^'""]+)['""]/,
                            /data-sitekey\s*=\s*['""]([^'""]+)['""]/
                        ];
                        
                        for (const pattern of patterns) {
                            const match = script.innerText.match(pattern);
                            if (match && match[1]) return match[1];
                        }
                    }
                    
                    return '';
                }
            ");
        }

        private async Task<string> FindSiteKeyInScriptsAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(@"
                () => {
                    // Buscar específicamente en función cargarRecaptcha
                    if (typeof window.cargarRecaptcha === 'function') {
                        try {
                            const funcStr = window.cargarRecaptcha.toString();
                            const match = funcStr.match(/sitekey\s*:\s*['""]([^'""]+)['""]/);
                            if (match && match[1]) return match[1];
                        } catch(e) {}
                    }
                    
                    // Buscar en window si hay algún objeto con sitekey
                    if (window.___grecaptcha_cfg) {
                        try {
                            for (const key in window.___grecaptcha_cfg.clients) {
                                const client = window.___grecaptcha_cfg.clients[key];
                                if (client && client.sitekey) return client.sitekey;
                            }
                        } catch(e) {}
                    }
                    
                    return '';
                }
            ");
        }

        private async Task<string> GetTokenFromCapSolverAsync(string sitekey, string pageUrl, string action)
        {
            using (var httpClient = new HttpClient())
            {
                // Crear payload para CapSolver
                var payload = new
                {
                    clientKey = _apiKey,
                    task = new
                    {
                        type = "ReCaptchaV3EnterpriseTaskProxyless",
                        websiteURL = pageUrl,
                        websiteKey = sitekey,
                        pageAction = action,
                        minScore = 0.3,
                        enterprisePayload = new
                        {
                            s = "ENTERPRISE"
                        }
                    }
                };

                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                // Enviar tarea
                var response = await httpClient.PostAsync("https://api.capsolver.com/createTask", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseBody);

                if (json["errorId"]?.Value<int>() != 0)
                    throw new Exception($"CapSolver error: {json["errorDescription"]}");

                string taskId = json["taskId"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                    throw new Exception("No taskId received");

                // Polling para obtener resultado
                for (int i = 0; i < _timeoutSeconds / _pollIntervalSeconds; i++)
                {
                    await Task.Delay(_pollIntervalSeconds * 1000);

                    var pollData = new { clientKey = _apiKey, taskId = taskId };
                    var pollContent = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(pollData),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var pollResponse = await httpClient.PostAsync("https://api.capsolver.com/getTaskResult", pollContent);
                    var pollBody = await pollResponse.Content.ReadAsStringAsync();
                    var pollJson = JObject.Parse(pollBody);

                    if (pollJson["status"]?.ToString() == "ready")
                    {
                        return pollJson["solution"]?["token"]?.ToString()
                             ?? pollJson["solution"]?["gRecaptchaResponse"]?.ToString();
                    }
                    else if (pollJson["status"]?.ToString() == "failed")
                    {
                        throw new Exception($"CapSolver failed: {pollJson["errorDescription"]}");
                    }
                }

                throw new TimeoutException("Timeout waiting for CapSolver");
            }
        }

        private async Task<bool> InjectTokenAsync(IPage page, string token)
        {
            return await page.EvaluateAsync<bool>(@"
                (token) => {
                    try {
                        // 1. Crear campo de respuesta
                        let field = document.getElementById('g-recaptcha-response');
                        if (!field) {
                            field = document.createElement('textarea');
                            field.id = 'g-recaptcha-response';
                            field.name = 'g-recaptcha-response';
                            field.style.display = 'none';
                            document.body.appendChild(field);
                        }
                        field.value = token;
                        
                        // 2. Disparar eventos
                        field.dispatchEvent(new Event('change', { bubbles: true }));
                        field.dispatchEvent(new Event('input', { bubbles: true }));
                        
                        // 3. Ejecutar callbacks en orden
                        let executed = false;
                        
                        // Primero: executeRecaptcha si existe
                        if (typeof window.executeRecaptcha === 'function') {
                            try {
                                window.executeRecaptcha('consulta_cel_recibidos');
                                executed = true;
                            } catch(e) {}
                        }
                        
                        // Segundo: onSubmit si existe
                        if (!executed && typeof window.onSubmit === 'function') {
                            try {
                                window.onSubmit();
                                executed = true;
                            } catch(e) {}
                        }
                        
                        // Tercero: rcBuscar directamente
                        if (!executed && typeof window.rcBuscar === 'function') {
                            window.rcBuscar();
                            executed = true;
                        }
                        
                        // Cuarto: grecaptcha.enterprise.execute
                        if (!executed && window.grecaptcha && window.grecaptcha.enterprise) {
                            try {
                                window.grecaptcha.enterprise.execute();
                                executed = true;
                            } catch(e) {}
                        }
                        
                        console.log('✅ Token inyectado y callbacks ejecutados');
                        return executed;
                    } catch(e) {
                        console.error('Error inyectando token:', e);
                        return false;
                    }
                }
            ", token);
        }

        private async Task ExecuteDirectMethodAsync(IPage page)
        {
            await page.EvaluateAsync(@"
                () => {
                    // Método directo: ejecutar todo lo posible
                    if (typeof window.rcBuscar === 'function') {
                        window.rcBuscar();
                        console.log('✅ rcBuscar ejecutado directamente');
                    }
                    
                    // También disparar evento de formulario
                    const forms = document.querySelectorAll('form');
                    forms.forEach(form => {
                        const event = new Event('submit', { bubbles: true });
                        form.dispatchEvent(event);
                    });
                }
            ");
        }
    }
}