using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Core
{
    class ProcesoCarga
    {
        private string _token;
        private string[] _claves;
        private string _wslink;

        public ProcesoCarga(string token, string[] claves, string wslink)
        {
            _token = token;
            _claves = claves;
            _wslink = wslink;
        }

        private (bool exito, string mensaje) EnviarClavesWS()
        {
            try
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 10485760,
                    ReaderQuotas = { MaxStringContentLength = 10485760 }
                };
                var endpoint = new EndpointAddress(_wslink);
                var client = new EdocServiceReference.WSRAD_KEY_CARGARClient(binding, endpoint);
                var arrayOfClaves = new EdocServiceReference.ArrayOfString();
                arrayOfClaves.AddRange(_claves);

                string mensajeSalida = "";
                bool resultado = client.CargarClavesAcceso(_token, arrayOfClaves, ref mensajeSalida);

                return (resultado, mensajeSalida);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
