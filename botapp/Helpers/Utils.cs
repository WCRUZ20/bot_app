using botapp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Helpers
{
    public class Utils
    {
        public static string ObtenerRutaDescargaPersonalizada(string _subcarpeta)
        {
            var config = ConfigManager.CargarConfiguracion();
            string rutaDir = config.RutaDirectorio;
            var ruta = Path.Combine(rutaDir, "Carga_Botcito", _subcarpeta);
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
            return ruta;
        }

        public static string[] LeerClavesDesdeTXT(string _directorio)
        {
            var claves = new List<string>();

            if (!Directory.Exists(_directorio))
                return claves.ToArray();

            foreach (var archivo in Directory.GetFiles(_directorio, "*.txt"))
            {
                var lineas = File.ReadAllLines(archivo);
                for (int i = 1; i < lineas.Length; i++)
                {
                    var partes = lineas[i].Split('\t');
                    if (partes.Length >= 5 && partes[4].Trim().Length == 49)
                        claves.Add(partes[4].Trim());
                }
            }

            return claves.ToArray();
        }

        public static Dictionary<string, string> ObtenerFechasClavesEnviadas(string archivo)
        {
            var fechasPorClave = new Dictionary<string, string>(StringComparer.Ordinal);

            if (!File.Exists(archivo))
                return fechasPorClave;

            foreach (var linea in File.ReadAllLines(archivo, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                var partes = linea.Split(new[] { '\t' }, 2);
                var clave = partes[0].Trim();

                if (string.IsNullOrWhiteSpace(clave))
                    continue;

                string fecha = partes.Length > 1 ? partes[1].Trim() : string.Empty;

                if (!string.IsNullOrWhiteSpace(fecha))
                    fechasPorClave[clave] = fecha;
            }

            return fechasPorClave;
        }

        public static string ExtraerClaveEnviada(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return string.Empty;

            var partes = linea.Split(new[] { '\t' }, 2);
            return partes[0].Trim();
        }

    }
}
