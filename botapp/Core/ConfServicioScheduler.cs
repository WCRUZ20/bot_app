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
        private readonly object _stateLock = new object();
        private ServicioConfEjecucion _confActual;
        private DateTime? _ultimaEjecucionProgramada;

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

            lock (_stateLock)
            {
                _confActual = conf;
                _ultimaEjecucionProgramada = null;
            }

            _timer = new Timer(async _ => await Tick(), null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;

            lock (_stateLock)
            {
                _confActual = null;
                _ultimaEjecucionProgramada = null;
            }
        }

        private async Task Tick()
        {
            if (!_gate.Wait(0))
                return;

            try
            {
                ServicioConfEjecucion conf;
                DateTime? ultimaEjecucion;
                lock (_stateLock)
                {
                    conf = _confActual;
                    ultimaEjecucion = _ultimaEjecucionProgramada;
                }

                if (conf == null)
                    return;

                DateTime instanteProgramado;
                if (!DebeEjecutar(conf, DateTime.Now, out instanteProgramado))
                    return;

                if (ultimaEjecucion.HasValue && ultimaEjecucion.Value == instanteProgramado)
                    return;

                lock (_stateLock)
                {
                    _ultimaEjecucionProgramada = instanteProgramado;
                }

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

                GenerarReportePdfResumen(resultados);
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

        private static bool DebeEjecutar(ServicioConfEjecucion conf, DateTime ahora, out DateTime instanteProgramado)
        {
            instanteProgramado = DateTime.MinValue;

            if (conf.DiasActivos == null || conf.DiasActivos.Count == 0)
                return false;

            var ahoraMinuto = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, 0);
            var horaActual = ahora.TimeOfDay;
            DateTime ancla;
            DayOfWeek diaProgramado;

            if (conf.HoraInicio <= conf.HoraFin)
            {
                if (horaActual < conf.HoraInicio || horaActual > conf.HoraFin)
                    return false;

                diaProgramado = ahora.DayOfWeek;
                ancla = ahora.Date.Add(conf.HoraInicio);
            }
            else
            {
                if (horaActual >= conf.HoraInicio)
                {
                    diaProgramado = ahora.DayOfWeek;
                    ancla = ahora.Date.Add(conf.HoraInicio);
                }
                else if (horaActual <= conf.HoraFin)
                {
                    ancla = ahora.Date.AddDays(-1).Add(conf.HoraInicio);
                    diaProgramado = ancla.DayOfWeek;
                }
                else
                {
                    return false;
                }
            }

            if (!conf.DiasActivos.Contains(diaProgramado))
                return false;

            int frecuencia = Math.Max(conf.FrecuenciaMinutos, 1);
            int minutosDesdeInicio = (int)(ahoraMinuto - ancla).TotalMinutes;
            if (minutosDesdeInicio < 0)
                return false;

            if (minutosDesdeInicio % frecuencia != 0)
                return false;

            instanteProgramado = ahoraMinuto;
            return true;
        }

        private static void GenerarReportePdfResumen(IEnumerable<ServicioClienteResultado> resultados)
        {
            string reportesDir = Utils.ObtenerRutaDescargaPersonalizada("BOT_REPORTES");
            string ruta = Path.Combine(reportesDir, $"{DateTime.Now:yyyyMMdd_HHmmss}_resumenbot.pdf");

            using (var stream = new FileStream(ruta, FileMode.Create, FileAccess.Write))
            {
                var doc = new Document(PageSize.A4.Rotate(), 30, 30, 30, 30);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var titulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var subtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                var clientes = (resultados ?? Enumerable.Empty<ServicioClienteResultado>()).ToList();

                doc.Add(new Paragraph("Reporte resumen del servicio (todos los clientes)", titulo));
                doc.Add(new Paragraph($"Fecha generación: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", normal));
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph("Sección Descarga", subtitulo));
                PdfPTable tDesc = new PdfPTable(6) { WidthPercentage = 100 };
                tDesc.SetWidths(new float[] { 2.3f, 1.7f, 1.2f, 1.6f, 1.2f, 1.2f });
                tDesc.AddCell("Cliente");
                tDesc.AddCell("Usuario");
                tDesc.AddCell("Mes-Año");
                tDesc.AddCell("Tipo documento");
                tDesc.AddCell("Estado");
                tDesc.AddCell("Carga");

                foreach (var cliente in clientes)
                {
                    var descargas = cliente.Descargas != null && cliente.Descargas.Count > 0
                        ? cliente.Descargas
                        : new List<ServicioDescargaResultado>
                        {
                            new ServicioDescargaResultado
                            {
                                MesAnio = "-",
                                TipoDocumento = "-",
                                Estado = "Sin datos"
                            }
                        };

                    foreach (var item in descargas)
                    {
                        tDesc.AddCell(cliente.NombreCliente ?? "-");
                        tDesc.AddCell(cliente.Usuario ?? "-");
                        tDesc.AddCell(item.MesAnio ?? "-");
                        tDesc.AddCell(item.TipoDocumento ?? "-");
                        tDesc.AddCell(CrearCeldaEstado(item.Estado));
                        tDesc.AddCell(CrearCeldaEstado(cliente.Carga != null ? cliente.Carga.Estado : "Sin datos"));
                    }
                }

                doc.Add(tDesc);
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph("Sección Carga", subtitulo));
                PdfPTable tCarga = new PdfPTable(5) { WidthPercentage = 100 };
                tCarga.SetWidths(new float[] { 2.6f, 2f, 1.5f, 2f, 1.5f });
                tCarga.AddCell("Cliente");
                tCarga.AddCell("Usuario");
                tCarga.AddCell("Estado carga");
                tCarga.AddCell("Fecha y hora");
                tCarga.AddCell("Claves nuevas");

                foreach (var cliente in clientes)
                {
                    tCarga.AddCell(cliente.NombreCliente ?? "-");
                    tCarga.AddCell(cliente.Usuario ?? "-");
                    tCarga.AddCell(CrearCeldaEstado(cliente.Carga != null ? cliente.Carga.Estado : "Sin datos"));
                    tCarga.AddCell(cliente.Carga != null && cliente.Carga.FechaHoraCarga.HasValue
                        ? cliente.Carga.FechaHoraCarga.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : "-");
                    tCarga.AddCell(cliente.Carga != null ? cliente.Carga.ClavesNuevasCargadas.ToString() : "0");
                }

                doc.Add(tCarga);
                doc.Close();
            }
        }

        private static PdfPCell CrearCeldaEstado(string estado)
        {
            string valor = string.IsNullOrWhiteSpace(estado) ? "Sin datos" : estado.Trim();
            BaseColor color = BaseColor.LIGHT_GRAY;

            if (valor.Equals("Exitoso", StringComparison.OrdinalIgnoreCase))
                color = new BaseColor(198, 239, 206);
            else if (valor.Equals("Fallido", StringComparison.OrdinalIgnoreCase))
                color = new BaseColor(255, 199, 206);
            else if (valor.Equals("Sin datos", StringComparison.OrdinalIgnoreCase))
                color = new BaseColor(217, 217, 217);

            return new PdfPCell(new Phrase(valor))
            {
                BackgroundColor = color,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
        }

        public void Dispose()
        {
            Stop();
            _gate.Dispose();
        }
    }
}
