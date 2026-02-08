using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Models
{
    public class ServicioDescargaResultado
    {
        public string MesAnio { get; set; }
        public string TipoDocumento { get; set; }
        public string Estado { get; set; }
    }

    public class ServicioCargaResultado
    {
        public string Estado { get; set; }
        public DateTime? FechaHoraCarga { get; set; }
        public int ClavesNuevasCargadas { get; set; }
    }

    public class ServicioClienteResultado
    {
        public string Usuario { get; set; }
        public string NombreCliente { get; set; }
        public List<ServicioDescargaResultado> Descargas { get; set; } = new List<ServicioDescargaResultado>();
        public ServicioCargaResultado Carga { get; set; }
    }

    public class ServicioConfEjecucion
    {
        public bool ServicioActivo { get; set; }
        public bool CargaAutomatica { get; set; }
        public int FrecuenciaMinutos { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public HashSet<DayOfWeek> DiasActivos { get; set; } = new HashSet<DayOfWeek>();
    }

}
