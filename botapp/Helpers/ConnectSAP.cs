using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Helpers
{
    public class ConnectSAP
    {
        public SAPbobsCOM.Company _oCompany;
        //private string _rutalog = ConfigurationManager.AppSettings["rutalog"];

        public ConnectSAP(ref SAPbobsCOM.Company oCompany)
        {
            this._oCompany = oCompany;
        }

        public bool ConectSAP(string compania, string usuario, string clave)
        {
            try
            {
                long ErrCode;
                string ErrMsg = "";

                // NO crear un nuevo Company, usar el que ya viene
                // _oCompany = new SAPbobsCOM.Company();  <-- eliminar

                // Configuración básica
                _oCompany.DbServerType = (BoDataServerTypes)Enum.Parse(typeof(BoDataServerTypes),
                    ConfigurationManager.AppSettings["DevServerType"], true);

                _oCompany.UseTrusted = ConfigurationManager.AppSettings["UseTrusted"] == "Y";
                _oCompany.CompanyDB = compania;
                _oCompany.UserName = usuario;
                _oCompany.Password = clave;

                int sapVersion = int.Parse(ConfigurationManager.AppSettings["SAP_VERSION"]);
                if (sapVersion < 10)
                {
                    _oCompany.Server = ConfigurationManager.AppSettings["DevServer"];
                    _oCompany.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
                }
                else
                {
                    _oCompany.Server = ConfigurationManager.AppSettings["DevServer"];
                }

                Log("******INFORMACION DE CONEXION*******");
                Log($"DevServerType: {ConfigurationManager.AppSettings["DevServerType"]}");
                Log($"UseTrusted: {ConfigurationManager.AppSettings["UseTrusted"]}");
                Log($"CompanyDB: {compania}");
                Log($"UserName: {usuario}");
                Log($"Password: {clave}");
                Log($"SAP_VERSION: {ConfigurationManager.AppSettings["SAP_VERSION"]}");
                Log($"DevServer: {ConfigurationManager.AppSettings["DevServer"]}");
                Log($"LicenseServer: {ConfigurationManager.AppSettings["LicenseServer"]}");
                Log("******INFORMACION DE CONEXION*******");

                if (_oCompany.Connected)
                {
                    Log("Ya estaba conectado a SAP");
                    return true;
                }

                ErrCode = _oCompany.Connect();

                if (ErrCode != 0)
                {
                    _oCompany.GetLastError(out int errorCode, out string errorMessage);
                    Log($"ERROR: {errorCode} - {errorMessage}");
                    return false;
                }
                else
                {
                    Log("Conectado a SAP");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex}");
                return false;
            }
        }


        private void Log(string message)
        {
            string logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "conexionSAP.txt");//_rutalog;
            string folderPath = Path.GetDirectoryName(logPath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }
    }
}
