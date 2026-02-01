using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using botapp.Models;

namespace botapp.Helpers
{
    public static class ConfigManager
    {
        public static void GuardarConfiguracion(ConfigData config)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configuration.AppSettings.Settings;

            settings["OrigenDatos"].Value = config.OrigenDatos ?? "";
            settings["RutaArchivo"].Value = config.RutaArchivo ?? "";
            settings["Servidor"].Value = config.Servidor ?? "";
            settings["Usuario"].Value = config.Usuario ?? "";
            settings["Contrasena"].Value = config.Contrasena ?? "";
            settings["NombreBD"].Value = config.NombreBD ?? "";
            settings["EstaConectado"].Value = config.EstaConectado.ToString() ?? "N";
            settings["SRI_LoginUrl"].Value = config.SriUrl ?? "";
            settings["EDOC_Endpoint"].Value = config.EdocUrl ?? "";
            settings["Headless"].Value = config.Headless.ToString();
            settings["usuario"].Value = config.usuario ?? "";
            settings["clave"].Value = config.clave ?? "";
            settings["savecredentials"].Value = config.savecredentials.ToString() ?? "N";

            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


        public static ConfigData CargarConfiguracion()
        {
            var settings = ConfigurationManager.AppSettings;

            return new ConfigData
            {
                OrigenDatos = settings["OrigenDatos"],
                RutaArchivo = settings["RutaArchivo"],
                Servidor = settings["Servidor"],
                Usuario = settings["Usuario"],
                Contrasena = settings["Contrasena"],
                NombreBD = settings["NombreBD"],
                EstaConectado = bool.TryParse(settings["EstaConectado"], out var conectado) && conectado,
                SriUrl = settings["SRI_LoginUrl"],
                EdocUrl = settings["EDOC_Endpoint"],
                Headless = bool.TryParse(settings["Headless"], out var Y) && Y,
                RutaDirectorio = settings["RutaDirectorio"],
                usuario = settings["usuario"],
                clave = settings["clave"],
                savecredentials = bool.TryParse(settings["savecredentials"], out var svcrd) && svcrd
            };
        }
    }
}
