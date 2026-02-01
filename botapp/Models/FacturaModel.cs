using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace botapp.Models
{
    [XmlRoot("factura")]
    public class FacturaModel
    {
        [XmlElement("infoTributaria")]
        public InfoTributaria InfoTributaria { get; set; }

        [XmlElement("infoFactura")]
        public InfoFactura InfoFactura { get; set; }

        [XmlArray("detalles")]
        [XmlArrayItem("detalle")]
        public List<Detalle> Detalles { get; set; }

        [XmlElement("infoAdicional")]
        public InfoAdicional InfoAdicional { get; set; }
    }

    public class InfoTributaria
    {
        [XmlElement("ambiente")]
        public string Ambiente { get; set; }

        [XmlElement("tipoEmision")]
        public string TipoEmision { get; set; }

        [XmlElement("razonSocial")]
        public string RazonSocial { get; set; }

        [XmlElement("nombreComercial")]
        public string NombreComercial { get; set; }

        [XmlElement("ruc")]
        public string Ruc { get; set; }

        [XmlElement("claveAcceso")]
        public string ClaveAcceso { get; set; }

        [XmlElement("codDoc")]
        public string CodDoc { get; set; }

        [XmlElement("estab")]
        public string Estab { get; set; }

        [XmlElement("ptoEmi")]
        public string PtoEmi { get; set; }

        [XmlElement("secuencial")]
        public string Secuencial { get; set; }

        [XmlElement("dirMatriz")]
        public string DirMatriz { get; set; }
    }

    public class InfoFactura
    {
        [XmlElement("fechaEmision")]
        public string FechaEmision { get; set; }

        [XmlElement("dirEstablecimiento")]
        public string DirEstablecimiento { get; set; }

        [XmlElement("contribuyenteEspecial")]
        public string ContribuyenteEspecial { get; set; }

        [XmlElement("obligadoContabilidad")]
        public string ObligadoContabilidad { get; set; }

        [XmlElement("tipoIdentificacionComprador")]
        public string TipoIdentificacionComprador { get; set; }

        [XmlElement("razonSocialComprador")]
        public string RazonSocialComprador { get; set; }

        [XmlElement("identificacionComprador")]
        public string IdentificacionComprador { get; set; }

        [XmlElement("direccionComprador")]
        public string DireccionComprador { get; set; }

        [XmlElement("totalSinImpuestos")]
        public decimal TotalSinImpuestos { get; set; }

        [XmlElement("totalDescuento")]
        public decimal TotalDescuento { get; set; }

        [XmlArray("totalConImpuestos")]
        [XmlArrayItem("totalImpuesto")]
        public List<TotalImpuesto> TotalConImpuestos { get; set; }

        [XmlElement("propina")]
        public decimal Propina { get; set; }

        [XmlElement("importeTotal")]
        public decimal ImporteTotal { get; set; }

        [XmlElement("moneda")]
        public string Moneda { get; set; }

        [XmlArray("pagos")]
        [XmlArrayItem("pago")]
        public List<Pago> Pagos { get; set; }
    }

    public class TotalImpuesto
    {
        [XmlElement("codigo")]
        public string Codigo { get; set; }

        [XmlElement("codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }

        [XmlElement("baseImponible")]
        public decimal BaseImponible { get; set; }

        [XmlElement("tarifa")]
        public decimal Tarifa { get; set; }

        [XmlElement("valor")]
        public decimal Valor { get; set; }
    }

    public class Pago
    {
        [XmlElement("formaPago")]
        public string FormaPago { get; set; }

        [XmlElement("total")]
        public decimal Total { get; set; }

        [XmlElement("plazo")]
        public string Plazo { get; set; }

        [XmlElement("unidadTiempo")]
        public string UnidadTiempo { get; set; }
    }

    public class Detalle
    {
        [XmlElement("codigoPrincipal")]
        public string CodigoPrincipal { get; set; }

        [XmlElement("codigoAuxiliar")]
        public string CodigoAuxiliar { get; set; }

        [XmlElement("descripcion")]
        public string Descripcion { get; set; }

        [XmlElement("cantidad")]
        public decimal Cantidad { get; set; }

        [XmlElement("precioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [XmlElement("descuento")]
        public decimal Descuento { get; set; }

        [XmlElement("precioTotalSinImpuesto")]
        public decimal PrecioTotalSinImpuesto { get; set; }

        [XmlArray("detallesAdicionales")]
        [XmlArrayItem("detAdicional")]
        public List<DetAdicional> DetallesAdicionales { get; set; }

        [XmlArray("impuestos")]
        [XmlArrayItem("impuesto")]
        public List<Impuesto> Impuestos { get; set; }
    }

    public class DetAdicional
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }

        [XmlAttribute("valor")]
        public string Valor { get; set; }
    }

    public class Impuesto
    {
        [XmlElement("codigo")]
        public string Codigo { get; set; }

        [XmlElement("codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }

        [XmlElement("tarifa")]
        public decimal Tarifa { get; set; }

        [XmlElement("baseImponible")]
        public decimal BaseImponible { get; set; }

        [XmlElement("valor")]
        public decimal Valor { get; set; }
    }

    public class InfoAdicional
    {
        [XmlElement("campoAdicional")]
        public List<CampoAdicional> CamposAdicionales { get; set; }
    }

    public class CampoAdicional
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }

        [XmlText]
        public string Valor { get; set; }
    }
}
