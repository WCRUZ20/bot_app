using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Models
{
    class ClienteBot
    {
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string Adicional { get; set; }
        public string Clave { get; set; }
        public string ClaveEdoc { get; set; }
        public string Dias { get; set; }
        public string MesesAnte { get; set; }
        public bool Activo { get; set; }
        public bool Carga { get; set; }
        public bool Descarga { get; set; }
        public bool Factura { get; set; }
        public bool NotaCredito { get; set; }
        public bool Retencion { get; set; }
        public bool LiquidacionCompra { get; set; }
        public bool NotaDebito { get; set; }
        public string orden { get; set; }
        public bool ConsultarMesActual { get; set; }
        public int HiloAsignado { get; set; }
    }
}
