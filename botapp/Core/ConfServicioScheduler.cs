using botapp.Helpers;
using botapp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace botapp.Core
{
    public class ConfServicioScheduler : IDisposable
    {
        private readonly string _supabaseUrl;
        private readonly string _apiKey;
        private Timer _timer;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public ConfServicioScheduler(string supabaseUrl, string apiKey)
        {
            _supabaseUrl = supabaseUrl;
            _apiKey = apiKey;
        }

        public void StartOrUpdate(ServicioConfEjecucion conf)
        {
            Stop();
            if (!conf.ServicioActivo)
                return;

            var intervalo = Math.Max(conf.FrecuenciaMinutos, 1);
            _timer = new Timer(async _ => await Tick(conf), null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalo));
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private async Task Tick(ServicioConfEjecucion conf)
        {
            if (!_gate.Wait(0))
                return;

            try
            {
                if (!DebeEjecutar(conf, DateTime.Now))
                    return;

                var helper = new SupabaseDbHelper(_supabaseUrl, _apiKey);
                DataTable clientes = await helper.GetClientesBotAsync();
                if (clientes == null || clientes.Rows.Count == 0)
                    return;

                var descargaService = new ServicioDescargaDocumentos();
                var resultados = await descargaService.EjecutarAsync(clientes);

                if (conf.CargaAutomatica)
                {
                    var cargaService = new ServicioCargaDocumentos(helper);
                    var cargas = await cargaService.EjecutarAsync(clientes);
                    foreach (var cliente in resultados)
                    {
                        ServicioCargaResultado carga;
                        if (cargas.TryGetValue(cliente.Usuario, out carga))
                            cliente.Carga = carga;
                    }
                }

                GenerarReportePdfPorCliente(resultados);
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "confServicio.log");
                LoggerHelper.Log(logPath, $"❌ Error en servicio programado: {ex.Message}");
            }
            finally
            {
                _gate.Release();
            }
        }

        private static bool DebeEjecutar(ServicioConfEjecucion conf, DateTime ahora)
        {
            if (!conf.DiasActivos.Contains(ahora.DayOfWeek))
                return false;

            TimeSpan actual = ahora.TimeOfDay;
            if (conf.HoraInicio <= conf.HoraFin)
            {
                return actual >= conf.HoraInicio && actual <= conf.HoraFin;
            }

            return actual >= conf.HoraInicio || actual <= conf.HoraFin;
        }

        private static void GenerarReportePdfPorCliente(IEnumerable<ServicioClienteResultado> resultados)
        {
            string reportesDir = Utils.ObtenerRutaDescargaPersonalizada("BOT_REPORTES");

            foreach (var cliente in resultados)
            {
                string nombreSeguro = string.Join("_", (cliente.NombreCliente ?? cliente.Usuario ?? "cliente")
                    .Split(Path.GetInvalidFileNameChars()));
                string ruta = Path.Combine(reportesDir, $"ReporteServicio_{nombreSeguro}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                using (var stream = new FileStream(ruta, FileMode.Create, FileAccess.Write))
                {
                    var doc = new Document(PageSize.A4, 30, 30, 30, 30);
                    PdfWriter.GetInstance(doc, stream);
                    doc.Open();

                    var titulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var subtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                    var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    doc.Add(new Paragraph($"Reporte servicio - {cliente.NombreCliente} ({cliente.Usuario})", titulo));
                    doc.Add(new Paragraph($"Fecha generación: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", normal));
                    doc.Add(new Paragraph(" "));

                    doc.Add(new Paragraph("Sección Descarga", subtitulo));
                    PdfPTable tDesc = new PdfPTable(3) { WidthPercentage = 100 };
                    tDesc.AddCell("Mes-Año");
                    tDesc.AddCell("Tipo documento");
                    tDesc.AddCell("Estado");
                    foreach (var item in cliente.Descargas)
                    {
                        tDesc.AddCell(item.MesAnio);
                        tDesc.AddCell(item.TipoDocumento);
                        tDesc.AddCell(item.Estado);
                    }
                    doc.Add(tDesc);
                    doc.Add(new Paragraph(" "));

                    doc.Add(new Paragraph("Sección Carga", subtitulo));
                    PdfPTable tCarga = new PdfPTable(3) { WidthPercentage = 100 };
                    tCarga.AddCell("Estado carga");
                    tCarga.AddCell("Fecha y hora");
                    tCarga.AddCell("Claves nuevas");
                    tCarga.AddCell(cliente.Carga != null ? cliente.Carga.Estado : "No ejecutada");
                    tCarga.AddCell(cliente.Carga != null && cliente.Carga.FechaHoraCarga.HasValue
                        ? cliente.Carga.FechaHoraCarga.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : "-");
                    tCarga.AddCell(cliente.Carga != null ? cliente.Carga.ClavesNuevasCargadas.ToString() : "0");
                    doc.Add(tCarga);

                    doc.Close();
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _gate.Dispose();
        }
    }
}
