using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Models
{
    public class ConfigData
    {
        public string OrigenDatos { get; set; }
        public string RutaArchivo { get; set; }
        public string Servidor { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string NombreBD { get; set; }
        public bool EstaConectado { get; set; }
        public string SriUrl { get; set; }
        public string EdocUrl { get; set; }
        public bool Headless { get; set; }
        public string RutaDirectorio { get; set; }
        public string usuario { get; set; }
        public string clave { get; set; }
        public bool savecredentials { get; set; }
    }
}
