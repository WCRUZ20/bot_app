using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace botapp.Licensing
{
    public class SupabaseLicenseClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SupabaseLicenseClient(string baseUrl, string anonKey)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("El URL de Supabase es obligatorio.", nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(anonKey))
                throw new ArgumentException("La clave anon de Supabase es obligatoria.", nameof(anonKey));

            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", anonKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {anonKey}");
        }

        public async Task<LicenseValidationResult> ValidateAsync(string userCode, string password)
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(password))
            {
                return LicenseValidationResult.Invalid("Usuario o clave vacíos.");
            }

            var payload = new
            {
                p_user_code = userCode.Trim(),
                p_password = password
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/rest/v1/rpc/validate_user_bot")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return LicenseValidationResult.Invalid($"Error al validar licencia: {response.StatusCode}");
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    return LicenseValidationResult.Invalid("Respuesta vacía del servidor.");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<LicenseValidationResult>(json, options);

                return result ?? LicenseValidationResult.Invalid("No se pudo interpretar la respuesta.");
            }
        }
    }
}
