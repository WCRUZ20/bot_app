using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace botapp.Core
{
    public class ServicioDescargaDocumentos
    {
        private const int Reintentos = 3;
        private const int Hilos = 8;

        public Task<List<ServicioClienteResultado>> EjecutarAsync(DataTable clientes)
        {
            if (clientes == null)
                return Task.FromResult(new List<ServicioClienteResultado>());

            var resultados = new List<ServicioClienteResultado>();
            var sync = new object();

            var opciones = new ParallelOptions { MaxDegreeOfParallelism = Hilos };
            Parallel.ForEach(clientes.AsEnumerable(), opciones, row =>
            {
                if (!EsTrue(row["descarga"]))
                    return;

                var resultado = ProcesarCliente(row);
                lock (sync)
                {
                    resultados.Add(resultado);
                }
            });

            return Task.FromResult(resultados.OrderBy(x => x.NombreCliente).ToList());
        }

        private ServicioClienteResultado ProcesarCliente(DataRow row)
        {
            string usuario = row["usuario"].ToString();
            string nombre = row["NombUsuario"].ToString();
            int mesesAnte = TryInt(row.Table.Columns.Contains("meses_ante") ? row["meses_ante"] : null, 0);
            bool incluirMesActual = !row.Table.Columns.Contains("consultar_mes_actual") || EsTrue(row["consultar_mes_actual"]);

            var tipos = new List<string>();
            AgregarTipoSiAplica(tipos, row, "factura", "Factura");
            AgregarTipoSiAplica(tipos, row, "notacredito", "Nota de Crédito");
            AgregarTipoSiAplica(tipos, row, "retencion", "Retención");
            AgregarTipoSiAplica(tipos, row, "liquidacioncompra", "Liquidación de compra");
            AgregarTipoSiAplica(tipos, row, "notadebito", "Nota de Débito");

            var mesesConsulta = ConstruirMesesConsulta(mesesAnte, incluirMesActual);

            var resultadoCliente = new ServicioClienteResultado
            {
                Usuario = usuario,
                NombreCliente = nombre
            };

            foreach (var tipo in tipos)
            {
                foreach (var fecha in mesesConsulta)
                {
                    string estado = DeterminarEstadoDescargaConReintento(usuario, nombre, tipo, fecha);
                    resultadoCliente.Descargas.Add(new ServicioDescargaResultado
                    {
                        MesAnio = fecha.ToString("MM-yyyy"),
                        TipoDocumento = tipo,
                        Estado = estado
                    });
                }
            }

            return resultadoCliente;
        }

        private static string DeterminarEstadoDescargaConReintento(string usuario, string nombre, string tipo, DateTime fecha)
        {
            for (int intento = 1; intento <= Reintentos; intento++)
            {
                try
                {
                    string carpeta = Utils.ObtenerRutaDescargaPersonalizada($"{usuario} - {nombre}");
                    if (!Directory.Exists(carpeta))
                        return "Sin datos";

                    string mes = fecha.ToString("MM");
                    string anio = fecha.ToString("yyyy");
                    var archivos = Directory.GetFiles(carpeta, "*", SearchOption.AllDirectories)
                        .Where(x => Contiene(x, tipo) && Contiene(x, mes) && Contiene(x, anio))
                        .ToList();

                    if (archivos.Count > 0)
                        return "Exitoso";

                    return "Sin datos";
                }
                catch
                {
                    if (intento == Reintentos)
                        return "Fallido";
                    Thread.Sleep(250);
                }
            }

            return "Fallido";
        }

        private static bool Contiene(string valor, string texto)
        {
            return valor.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static List<DateTime> ConstruirMesesConsulta(int mesesAnte, bool incluirMesActual)
        {
            var salida = new List<DateTime>();
            var baseMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            int maxBack = Math.Max(mesesAnte, 0);
            int inicio = incluirMesActual ? 0 : 1;

            for (int i = inicio; i <= maxBack; i++)
            {
                salida.Add(baseMes.AddMonths(-i));
            }

            if (salida.Count == 0)
                salida.Add(baseMes);

            return salida;
        }

        private static void AgregarTipoSiAplica(List<string> tipos, DataRow row, string campo, string nombre)
        {
            if (row.Table.Columns.Contains(campo) && EsTrue(row[campo]))
                tipos.Add(nombre);
        }

        private static int TryInt(object valor, int porDefecto)
        {
            int parsed;
            return int.TryParse(valor == null ? string.Empty : valor.ToString(), out parsed) ? parsed : porDefecto;
        }

        private static bool EsTrue(object valor)
        {
            return string.Equals(valor == null ? string.Empty : valor.ToString(), "Y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor == null ? string.Empty : valor.ToString(), "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor == null ? string.Empty : valor.ToString(), "1", StringComparison.OrdinalIgnoreCase);
        }
    }
}

