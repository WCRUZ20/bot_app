using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation.Models
{
    public class ParametrosConsulta
    {
        public string Anio { get; set; }
        public string Mes { get; set; }
        public string Dia { get; set; }
        public TipoComprobante Tipo { get; set; }
    }
}
