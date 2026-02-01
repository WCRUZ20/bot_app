using botapp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Helpers
{
    class SupabaseDbHelper
    {
        private readonly string _supabaseUrl;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public SupabaseDbHelper(string supabaseUrl, string apiKey)
        {
            _supabaseUrl = supabaseUrl.TrimEnd('/');
            _apiKey = apiKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", _apiKey);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Consulta mínima: pedimos solo 1 registro de la tabla user_bot
                string url = string.Format("{0}/rest/v1/user_bot?select=user_code&limit=1", _supabaseUrl);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true; // Hay conexión
                }

                return false; // No respondió con éxito
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en TestConnectionAsync: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Verifica si el usuario existe en la tabla user_bot con user_code y password.
        /// </summary>
        public async Task<bool> AuthenticateAsync(string userCode, string password)
        {
            try
            {
                // URL del endpoint REST de la tabla user_bot
                string url = string.Format(
                    "{0}/rest/v1/user_bot?user_code=eq.{1}&password=eq.{2}&select=*",
                    _supabaseUrl,
                    Uri.EscapeDataString(userCode),
                    Uri.EscapeDataString(password)
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string json = await response.Content.ReadAsStringAsync();

                // Si trae registros, autenticó correctamente
                return !string.IsNullOrWhiteSpace(json) && json != "[]";
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear con tu logger
                MessageBox.Show("Error en AuthenticateAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<(string UserCode, string ProfileImageUrl)> GetUserDataAsync(string userCode)
        {
            try
            {
                string url = string.Format(
                    "{0}/rest/v1/user_bot?user_code=eq.{1}&select=user_code,profile_image_url",
                    _supabaseUrl,
                    Uri.EscapeDataString(userCode)
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null);
                }

                string json = await response.Content.ReadAsStringAsync();

                // Ejemplo de respuesta: [{"user_code":"william","profile_image_url":"https://..."}]
                var data = Newtonsoft.Json.Linq.JArray.Parse(json);

                if (data.Count > 0)
                {
                    string userCodeResp = data[0]["user_code"]?.ToString();
                    string profileImageUrl = data[0]["profile_image_url"]?.ToString();
                    return (userCodeResp, profileImageUrl);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en GetUserDataAsync: " + ex.Message);
                return (null, null);
            }
        }

        public async Task<DataTable> GetClientesBotAsync()
        {
            try
            {
                // Endpoint REST: tabla Clientes_BOT
                string url = string.Format(
                    "{0}/rest/v1/Clientes_BOT?select=orden,usuario,NombUsuario,ci_adicional,clave,clave_ws,Activo,dias,meses_ante,consultar_mes_actual,carga,descarga,factura,notacredito,retencion,liquidacioncompra,notadebito",
                    _supabaseUrl
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error al obtener Clientes_BOT: " + response.StatusCode);
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                // Parsear JSON a JArray
                var array = Newtonsoft.Json.Linq.JArray.Parse(json);

                // Crear DataTable dinámico
                DataTable dt = new DataTable();

                if (array.Count > 0)
                {
                    // Crear columnas según el primer registro
                    foreach (var col in array[0].Children<JProperty>())
                    {
                        dt.Columns.Add(col.Name, typeof(string));
                    }

                    // Agregar filas
                    foreach (var item in array)
                    {
                        DataRow row = dt.NewRow();
                        foreach (var col in item.Children<JProperty>())
                        {
                            row[col.Name] = col.Value?.ToString();
                        }
                        dt.Rows.Add(row);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error en GetClientesBotAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<DataTable> GetClientesBotActivosAsync()
        {
            try
            {
                // Endpoint REST filtrando clientes Activo = 'Y' y carga = 'Y'
                string url = string.Format(
                    "{0}/rest/v1/Clientes_BOT?select=orden,usuario,NombUsuario,ci_adicional,clave,clave_ws,Activo,dias,meses_ante,carga,descarga,factura,notacredito,retencion,liquidacioncompra,notadebito&Activo=eq.Y&carga=eq.Y",
                    _supabaseUrl
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error al obtener Clientes_BOT activos: " + response.StatusCode);
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                var array = Newtonsoft.Json.Linq.JArray.Parse(json);

                DataTable dt = new DataTable();

                if (array.Count > 0)
                {
                    // Crear columnas según el primer registro
                    foreach (var col in array[0].Children<Newtonsoft.Json.Linq.JProperty>())
                    {
                        dt.Columns.Add(col.Name, typeof(string));
                    }

                    // Agregar filas
                    foreach (var item in array)
                    {
                        DataRow row = dt.NewRow();
                        foreach (var col in item.Children<Newtonsoft.Json.Linq.JProperty>())
                        {
                            row[col.Name] = col.Value?.ToString();
                        }
                        dt.Rows.Add(row);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error en GetClientesBotActivosAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<string> GetClaveWSEdocClientesAsync(string cliente)
        {
            try
            {
                // Endpoint REST filtrando clientes Activo = 'Y' y carga = 'Y'
                string url = string.Format(
                    "{0}/rest/v1/Clientes_BOT?select=clave_ws&NombUsuario=eq." + cliente,
                    _supabaseUrl
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                // Ejemplo de respuesta: [{"user_code":"william","profile_image_url":"https://..."}]
                var data = Newtonsoft.Json.Linq.JArray.Parse(json);

                if (data.Count > 0)
                {
                    string ciaalojamiento = data[0]["clave_ws"]?.ToString();
                    return ciaalojamiento;
                }

                return null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error en GetUserDataAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<DataTable> GetClientesBotActivosDescAsync()
        {
            try
            {
                // Endpoint REST filtrando clientes Activo = 'Y' y carga = 'Y' //"{0}/rest/v1/Clientes_BOT?select=orden,usuario,NombUsuario,ci_adicional,clave,clave_ws,Activo,dias,meses_ante,carga,descarga,factura,notacredito,retencion,liquidacioncompra,notadebito&Activo=eq.Y&descarga=eq.Y",
                string url = string.Format(
                    "{0}/rest/v1/Clientes_BOT?select=orden,usuario,NombUsuario,ci_adicional,clave,clave_ws,Activo,dias,meses_ante,consultar_mes_actual,carga,descarga,factura,notacredito,retencion,liquidacioncompra,notadebito&Activo=eq.Y&descarga=eq.Y",
                    _supabaseUrl
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error al obtener Clientes_BOT activos: " + response.StatusCode);
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                var array = Newtonsoft.Json.Linq.JArray.Parse(json);

                DataTable dt = new DataTable();

                if (array.Count > 0)
                {
                    // Crear columnas según el primer registro
                    foreach (var col in array[0].Children<Newtonsoft.Json.Linq.JProperty>())
                    {
                        dt.Columns.Add(col.Name, typeof(string));
                    }

                    // Agregar filas
                    foreach (var item in array)
                    {
                        DataRow row = dt.NewRow();
                        foreach (var col in item.Children<Newtonsoft.Json.Linq.JProperty>())
                        {
                            row[col.Name] = col.Value?.ToString();
                        }
                        dt.Rows.Add(row);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error en GetClientesBotActivosAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> GuardarClientesBotBatchAsync(List<ClienteBot> lista, bool actualiza = false)
        {
            try
            {
                foreach (var c in lista)
                {
                    if (actualiza) {
                        var payload = new
                        {
                            NombUsuario = c.Nombre,
                            ci_adicional = c.Adicional,
                            clave = c.Clave,
                            clave_ws = c.ClaveEdoc,
                            dias = c.Dias,
                            meses_ante = c.MesesAnte,
                            Activo = c.Activo ? "Y" : "N",
                            carga = c.Carga ? "Y" : "N",
                            descarga = c.Descarga ? "Y" : "N",
                            factura = c.Factura ? "Y" : "N",
                            notacredito = c.NotaCredito ? "Y" : "N",
                            retencion = c.Retencion ? "Y" : "N",
                            liquidacioncompra = c.LiquidacionCompra ? "Y" : "N",
                            notadebito = c.NotaDebito ? "Y" : "N",
                            orden = c.orden,
                            consultar_mes_actual = c.ConsultarMesActual ? "Y" : "N"
                        };

                        await PatchAsync("Clientes_BOT", c.Identificacion, payload);
                    }
                    else {
                        var payload = new
                        {
                            //NombUsuario = c.Nombre,
                            //ci_adicional = c.Adicional,
                            //clave = c.Clave,
                            //clave_ws = c.ClaveEdoc,
                            dias = c.Dias,
                            meses_ante = c.MesesAnte,
                            Activo = c.Activo ? "Y" : "N",
                            carga = c.Carga ? "Y" : "N",
                            descarga = c.Descarga ? "Y" : "N",
                            factura = c.Factura ? "Y" : "N",
                            notacredito = c.NotaCredito ? "Y" : "N",
                            retencion = c.Retencion ? "Y" : "N",
                            liquidacioncompra = c.LiquidacionCompra ? "Y" : "N",
                            notadebito = c.NotaDebito ? "Y" : "N",
                            orden = c.orden,
                            consultar_mes_actual = c.ConsultarMesActual ? "Y" : "N"
                        };

                        await PatchAsync("Clientes_BOT", c.Identificacion, payload);
                    }

                    
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        private async Task<string> PatchAsync(string tabla, string claveUsuario, object payload)
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/{tabla}?usuario=eq.{Uri.EscapeDataString(claveUsuario)}";

                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Remove("Prefer");
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                // Crear la petición PATCH manualmente
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error en PATCH a {tabla}: {response.StatusCode}\n{error}");
                    return null;
                }

                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en PatchAsync: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> EliminarClientesBatchAsync(List<string> usuarios)
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/Clientes_BOT?usuario=in.({string.Join(",", usuarios.Select(u => $"\"{u}\""))})";
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertarClienteAsync(ClienteBot cliente)
        {
            try
            {
                var payload = new
                {
                    usuario = cliente.Identificacion,
                    NombUsuario = cliente.Nombre,
                    ci_adicional = cliente.Adicional,
                    clave = cliente.Clave,
                    clave_ws = cliente.ClaveEdoc,
                    dias = cliente.Dias,
                    meses_ante = cliente.MesesAnte,
                    Activo = cliente.Activo ? "Y" : "N",
                    carga = cliente.Carga ? "Y" : "N",
                    descarga = cliente.Descarga ? "Y" : "N",
                    factura = cliente.Factura ? "Y" : "N",
                    notacredito = cliente.NotaCredito ? "Y" : "N",
                    retencion = cliente.Retencion ? "Y" : "N",
                    liquidacioncompra = cliente.LiquidacionCompra ? "Y" : "N",
                    notadebito = cliente.NotaDebito ? "Y" : "N",
                    orden = cliente.orden
                };

                string url = $"{_supabaseUrl}/rest/v1/Clientes_BOT";
                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Remove("Prefer");
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en InsertarClienteAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<List<ClienteBot>> GetClientesListAsync()
        {
            var dt = await GetClientesBotAsync();
            var lista = new List<ClienteBot>();

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new ClienteBot
                    {
                        Identificacion = row["usuario"]?.ToString(),
                        Nombre = row["NombUsuario"]?.ToString(),
                        Adicional = row["ci_adicional"]?.ToString()
                        // Puedes mapear más campos si los necesitas
                    });
                }
            }

            return lista;
        }

        public async Task<DataTable> GetTransaccionesBotAsync(string filtros = "", bool boton = false)
        {
            try
            {
                // Endpoint REST: tabla Clientes_BOT_Transaction
                string url = $"{_supabaseUrl}/rest/v1/Clientes_BOT_Transaction?select=*";

                // Agregar filtros si existen
                if (!string.IsNullOrEmpty(filtros))
                {
                    url += "&" + filtros;
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error al obtener Clientes_BOT_transaction: " + response.StatusCode);
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();

                // Parsear JSON a JArray
                var array = Newtonsoft.Json.Linq.JArray.Parse(json);

                // Crear DataTable dinámico
                DataTable dt = new DataTable();

                if (array.Count > 0)
                {
                    // Crear columnas según el primer registro
                    foreach (var col in array[0].Children<Newtonsoft.Json.Linq.JProperty>())
                    {
                        dt.Columns.Add(col.Name, typeof(string));
                    }

                    // Agregar filas
                    foreach (var item in array)
                    {
                        DataRow row = dt.NewRow();
                        foreach (var col in item.Children<Newtonsoft.Json.Linq.JProperty>())
                        {
                            row[col.Name] = col.Value?.ToString();
                        }
                        dt.Rows.Add(row);
                    }
                }
                else
                {
                    if(boton) MessageBox.Show("No hay registros",
                                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en GetTransaccionesBotAsync: " + ex.Message);
                return null;
            }
        }


        public async Task<int?> GetMaxIdTransactionAsync()
        {
            try
            {
                // Traer el último registro por id_transaction
                string url = string.Format(
                    "{0}/rest/v1/Clientes_BOT_Transaction?select=id_transaction&order=id_transaction.desc&limit=1",
                    _supabaseUrl
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Error al obtener máximo id_transaction: "
                                    + response.StatusCode + " - " + errorBody);
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                var array = Newtonsoft.Json.Linq.JArray.Parse(json);

                if (array.Count > 0)
                {
                    var value = array[0]["id_transaction"]?.ToString();
                    if (int.TryParse(value, out int maxId))
                    {
                        return maxId;
                    }
                }

                return null; // no hay registros
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error en GetMaxIdTransactionAsync: " + ex.Message);
                return null;
            }
        }


        public async Task<bool> InsertarTransactionAsync(TransactionBot transaction)
        {
            try
            {
                var payload = new
                {
                    id_transaction = int.Parse(transaction.id_transaction),
                    transaction_order = int.Parse(transaction.transaction_order),
                    resolution_provider = transaction.resolution_provider,
                    id_cliente = transaction.id_cliente,
                    tipodocumento = int.Parse(transaction.tipodocumento),
                    mes_consulta = int.Parse(transaction.mes_consulta),
                    estado = int.Parse(transaction.estado),
                    obervacion = transaction.observacion,
                    saldo = string.IsNullOrWhiteSpace(transaction.saldo)
                            ? (decimal?)null
                            : decimal.Parse(transaction.saldo, CultureInfo.InvariantCulture),
                    trans_type = transaction.trans_type
                };

                string url = $"{_supabaseUrl}/rest/v1/Clientes_BOT_Transaction";
                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Remove("Prefer");
                _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en InsertarTransactionAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> TienePermisoEscrituraAsync(string userCode)
        {
            try
            {
                string url = string.Format(
                    "{0}/rest/v1/user_bot?user_code=eq.{1}&select=ableWrite",
                    _supabaseUrl,
                    Uri.EscapeDataString(userCode)
                );

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return false; // no se pudo consultar
                }

                string json = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(json);

                if (data.Count > 0)
                {
                    string ableWriteValue = data[0]["ableWrite"]?.ToString();
                    return ableWriteValue == "Y" || ableWriteValue == "true" || ableWriteValue == "1";
                }

                return false; // usuario no encontrado
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en TienePermisoEscrituraAsync: " + ex.Message);
                return false;
            }
        }

    }
}
