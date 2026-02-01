using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation.Models
{
    public static class CatalogoComprobantes
    {
        public static readonly List<TipoComprobante> Todos = new List<TipoComprobante>
        {
            new TipoComprobante { Value = "1", Nombre = "Factura", PrefijoArchivo = "FACTURAS" },
            new TipoComprobante { Value = "2", Nombre = "Liquidación", PrefijoArchivo = "LIQUIDACIONES" },
            new TipoComprobante { Value = "3", Nombre = "NotaCredito", PrefijoArchivo = "NOTAS_CREDITO" },
            new TipoComprobante { Value = "4", Nombre = "NotaDebito", PrefijoArchivo = "NOTAS_DEBITO" },
            new TipoComprobante { Value = "6", Nombre = "Retencion", PrefijoArchivo = "RETENCIONES" }
        };

        public static TipoComprobante ObtenerPorValue(string value)
        {
            return Todos.FirstOrDefault(t => t.Value == value);
        }
    }
}
