using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation.Models
{
    public class ComprobanteConfig
    {
        public TipoComprobante Tipo { get; set; }
        public string ValueHtml { get; set; }
        public string PrefijoArchivo { get; set; }
    }
}
