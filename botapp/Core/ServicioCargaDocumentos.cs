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
    class ServicioCargaDocumentos
    {
        private readonly SupabaseDbHelper _helper;

        public ServicioCargaDocumentos(SupabaseDbHelper helper)
        {
            _helper = helper;
        }

        public async Task<Dictionary<string, ServicioCargaResultado>> EjecutarAsync(DataTable clientes)
        {
            var resultados = new Dictionary<string, ServicioCargaResultado>(StringComparer.OrdinalIgnoreCase);
            if (clientes == null) return resultados;

            string edocUrl = ConfigurationManager.AppSettings["EDOC_Endpoint"] ?? string.Empty;

            foreach (DataRow row in clientes.Rows)
            {
                if (!EsTrue(row["carga"]))
                    continue;

                string usuario = row["usuario"].ToString();
                string nombre = row["NombUsuario"].ToString();
                string claveWS = row["clave_ws"].ToString();
                string carpeta = $"{usuario} - {nombre}";
                string path = Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                string[] claves = Utils.LeerClavesDesdeTXT(path);

                string clavesFilePath = Path.Combine(
                    Utils.ObtenerRutaDescargaPersonalizada("CLAVES_ENVIADAS"),
                    $"claves_enviadas_{nombre}.txt");
                var fechasExistentes = Utils.ObtenerFechasClavesEnviadas(clavesFilePath);
                int nuevas = claves.Count(c => !fechasExistentes.ContainsKey(c));

                var resultadoWs = EnviarClavesWS(claveWS, claves, edocUrl);

                if (resultadoWs.exito)
                {
                    var fechaCarga = DateTime.Now;
                    var lineasSalida = new List<string>(claves.Length);
                    foreach (var clave in claves)
                    {
                        string fechaExistente;
                        if (!fechasExistentes.TryGetValue(clave, out fechaExistente) || string.IsNullOrWhiteSpace(fechaExistente))
                            fechaExistente = fechaCarga.ToString("yyyy-MM-dd HH:mm:ss");

                        lineasSalida.Add($"{clave}\t{fechaExistente}");
                    }

                    File.WriteAllLines(clavesFilePath, lineasSalida, Encoding.UTF8);
                }

                resultados[usuario] = new ServicioCargaResultado
                {
                    Estado = resultadoWs.exito ? "Exitoso" : "Fallido",
                    FechaHoraCarga = resultadoWs.exito ? (DateTime?)DateTime.Now : null,
                    ClavesNuevasCargadas = resultadoWs.exito ? nuevas : 0
                };
            }

            return resultados;
        }

        private static bool EsTrue(object valor)
        {
            return string.Equals(valor == null ? string.Empty : valor.ToString(), "Y", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(valor == null ? string.Empty : valor.ToString(), "true", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(valor == null ? string.Empty : valor.ToString(), "1", StringComparison.OrdinalIgnoreCase);
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
