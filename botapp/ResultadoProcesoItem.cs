using botapp.Automation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp
{
    public enum EstadoResultado
    {
        Exitoso,
        Fallido,
        SinDatos
    }

    public class ResultadoProcesoItem
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Anio { get; set; }
        public string Mes { get; set; }
        public string Documento { get; set; }
        public EstadoResultado Estado { get; set; }
        public string CiAdicional { get; set; }
        public string Password { get; set; }
        public ParametrosConsulta Parametros { get; set; }
    }
}
