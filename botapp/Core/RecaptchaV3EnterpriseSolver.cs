using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    public class RecaptchaV3EnterpriseSolver
    {
        private readonly string _apiKey;
        private readonly HttpClient _http;

        public RecaptchaV3EnterpriseSolver(string capSolverApiKey)
        {
            _apiKey = capSolverApiKey;
            _http = new HttpClient();
        }

        /// <summary>
        /// Resuelve reCAPTCHA v3 Enterprise invisible
        /// </summary>
        public async Task<string> ResolverAsync(
            string websiteUrl,
            string siteKey,
            string action)
        {
            var createTaskPayload = new
            {
                clientKey = _apiKey,
                task = new
                {
                    type = "ReCaptchaV3EnterpriseTaskProxyless",
                    websiteURL = websiteUrl,
                    websiteKey = siteKey,
                    pageAction = action,
                    minScore = 0.3
                }
            };

            var createResponse = await PostAsync(
                "https://api.capsolver.com/createTask",
                createTaskPayload
            );

            if ((int)createResponse.errorId != 0)
                throw new Exception(createResponse.errorDescription.ToString());

            string taskId = createResponse.taskId.ToString();

            while (true)
            {
                await Task.Delay(2000);

                var result = await PostAsync(
                    "https://api.capsolver.com/getTaskResult",
                    new { clientKey = _apiKey, taskId }
                );

                if (result.status == "ready")
                    return result.solution.gRecaptchaResponse;

                if (result.status != "processing")
                    throw new Exception("Estado inesperado: " + result.status);
            }
        }

        /// <summary>
        /// Inyecta el token donde PrimeFaces / JSF lo espera
        /// </summary>
        public async Task InyectarTokenJSFAsync(IPage page, string token)
        {
            await page.EvaluateAsync(@"(token) => {

                // Buscar dinámicamente el campo JSF del captcha
                let input = document.querySelector('input[name$=""captchaToken""]');

                if (!input) {
                    console.error('Campo captchaToken JSF no encontrado');
                    return;
                }

                input.value = token;
                console.log('Token reCAPTCHA inyectado correctamente en JSF');

            }", token);
        }

        private async Task<dynamic> PostAsync(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<dynamic>(responseText);
        }
    }
}
