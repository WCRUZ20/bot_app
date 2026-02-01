using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation.Models
{
    public class TipoComprobante
    {
        public string Value { get; set; }          // value del <option>
        public string Nombre { get; set; }          // Nombre lógico
        public string PrefijoArchivo { get; set; }  // Para el nombre del archivo
    }
}
