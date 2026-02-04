using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    public static class BotBackgroundService
    {
        private static System.Threading.Timer _timer;
        private static bool _activo = false;

        private static string _supabaseUrl;
        private static string _apiKey;
        private static string _usuarioActual;
        private static DataTable _clientesCache;
        private static bool _writePermission;

        public static void Configure(string supabaseUrl, string apiKey, string usuarioActual)
        {
            _supabaseUrl = supabaseUrl;
            _apiKey = apiKey;
            _usuarioActual = usuarioActual;
        }

        public static void Start(int intervaloMin)
        {
            if (_activo) return;
            _activo = true;

            _timer = new System.Threading.Timer(async _ => await EjecutarProcesoCarga(),
                                                null,
                                                TimeSpan.Zero,
                                                TimeSpan.FromMinutes(intervaloMin));
        }

        public static void Stop()
        {
            _activo = false;
            _timer?.Dispose();
        }

        private static async Task EjecutarProcesoCarga()
        {
            string logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoCarga.log");
            try
            {
                var helper = new SupabaseDbHelper(_supabaseUrl, _apiKey);

                // permisos
                _writePermission = await helper.TienePermisoEscrituraAsync(_usuarioActual);


                
                //LoggerHelper.Init(logPath);
                LoggerHelper.Log(logPath, "🟢 [Servicio - Carga] Inicio de ejecución carga automática");

                // obtener clientes activos
                _clientesCache = await helper.GetClientesBotActivosAsync();
                if (_clientesCache == null || _clientesCache.Rows.Count == 0)
                {
                    LoggerHelper.Log(logPath, "⚠️ No se encontraron clientes activos para carga.");
                    return;
                }

                string edocUrl = ConfigurationManager.AppSettings["EDOC_Endpoint"].ToString();
                string sol_provider = "";

                if (ConfigurationManager.AppSettings["2captcha"] == "true")
                    sol_provider = "2Captcha";
                if (ConfigurationManager.AppSettings["Capsolver"] == "true")
                    sol_provider = "Capsolver";
                if (ConfigurationManager.AppSettings["HumanInteraction"] == "true")
                    sol_provider = "HumanInter";

                int? last_id_trans = await helper.GetMaxIdTransactionAsync();
                last_id_trans = (last_id_trans ?? 0) + 1;

                int orden = 0;

                foreach (DataRow row in _clientesCache.Rows)
                {
                    string usuario = row["usuario"].ToString();
                    string nombre = row["NombUsuario"].ToString();
                    string claveWS = row["clave_ws"].ToString();

                    string carpeta = $"{usuario} - {nombre}";
                    string path = Helpers.Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                    string[] claves = Helpers.Utils.LeerClavesDesdeTXT(path);

                    LoggerHelper.Log(logPath, $"[{nombre}] 📤 Enviando a EDOC ({claves.Length} claves)");
                    string clavesFilePath = Path.Combine(
                        Helpers.Utils.ObtenerRutaDescargaPersonalizada("CLAVES_ENVIADAS"),
                        $"claves_enviadas_{nombre}.txt");
                    var fechasExistentes = Helpers.Utils.ObtenerFechasClavesEnviadas(clavesFilePath);

                    var resultado = EnviarClavesWS(claveWS, claves, edocUrl);

                    string estado, observacion;
                    if (resultado.exito)
                    {
                        estado = "0";
                        observacion = "Carga exitosa";
                        string fechaCarga = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        var lineasSalida = new List<string>(claves.Length);

                        foreach (var clave in claves)
                        {
                            if (!fechasExistentes.TryGetValue(clave, out string fechaExistente) || string.IsNullOrWhiteSpace(fechaExistente))
                                fechaExistente = fechaCarga;

                            lineasSalida.Add($"{clave}\t{fechaExistente}");
                        }

                        File.WriteAllLines(clavesFilePath, lineasSalida, Encoding.UTF8);

                    }
                    else
                    {
                        estado = "1";
                        observacion = $"Fallo carga, {resultado.mensaje}";
                    }

                    // registrar transacción
                    var trans = new TransactionBot
                    {
                        id_transaction = last_id_trans.ToString(),
                        transaction_order = (orden++).ToString(),
                        resolution_provider = sol_provider,
                        id_cliente = usuario,
                        tipodocumento = "0",
                        mes_consulta = "0",
                        estado = estado,
                        observacion = observacion,
                        saldo = "0",
                        trans_type = "C"
                    };

                    if (_writePermission)
                        await helper.InsertarTransactionAsync(trans);

                    LoggerHelper.Log(logPath, $"[{nombre}] Resultado EDOC: {(resultado.exito ? "✅" : "❌")} - {resultado.mensaje}");
                }

                LoggerHelper.Log(logPath, "✅ [Servicio - Carga] Fin Ejecución carga automática");
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(logPath, $"❌ Error en carga automática: {ex.Message}");
            }
        }

        private static (bool exito, string mensaje) EnviarClavesWS(string token, string[] claves, string wslink)
        {
            try
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 10485760,
                    ReaderQuotas = { MaxStringContentLength = 10485760 }
                };
                var endpoint = new EndpointAddress(wslink);
                var client = new EdocServiceReference.WSRAD_KEY_CARGARClient(binding, endpoint);
                var arrayOfClaves = new EdocServiceReference.ArrayOfString();
                arrayOfClaves.AddRange(claves);

                string mensajeSalida = "";
                bool resultado = client.CargarClavesAcceso(token, arrayOfClaves, ref mensajeSalida);

                return (resultado, mensajeSalida);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
