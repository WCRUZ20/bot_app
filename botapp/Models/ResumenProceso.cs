using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Models
{
    public class ResumenProceso
    {
        public int TotalClientes { get; set; }
        public int ClientesProcesados { get; set; }
        public int ClientesExitosos { get; set; }
        public List<ResultadoDescarga> FallosDescarga { get; set; } = new List<ResultadoDescarga>();
        public List<ResultadoDescarga> DescargasExitosas { get; set; } = new List<ResultadoDescarga>();
        public List<ResultadoCarga> FallosCarga { get; set; } = new List<ResultadoCarga>();
        public List<ResultadoCarga> CargasExitosas { get; set; } = new List<ResultadoCarga>();
        public DateTime InicioEjecucion { get; set; }
        public DateTime FinEjecucion { get; set; }
    }

    public class ResultadoDescarga
    {
        public string Cliente { get; set; }
        public string TipoDocumento { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }
        public bool Exitoso { get; set; }
        public string Error { get; set; }
    }

    public class ResultadoCarga
    {
        public string Cliente { get; set; }
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public int CantidadClaves { get; set; }
    }
}
