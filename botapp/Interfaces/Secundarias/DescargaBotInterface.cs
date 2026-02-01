using botapp.Automation;
using botapp.Automation.Models;
using botapp.Core;
using botapp.Helpers;
using botapp.Licensing;
using botapp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using DesktopSpeechSynthesizer = System.Speech.Synthesis.SpeechSynthesizer;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class DescargaBotInterface : UserControl
    {

        private bool humanInteraction;

        private object _captchaSolver;
        private RecaptchaV2Solver _recaptchaSolver;
        private int reintentos;
        private ResumenProceso resumenProceso;

        private readonly string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";

        private DataTable dt;
        private int? last_id_trans;
        private string _logPath;
        private int _orden;
        private string sol_provider = "";
        private bool writepermission = false;
        private string _usuarioActual = "";
        private string apiKeySolver = "";

        //variables para cancelacion de proceso
        private bool _procesoEnEjecucion = false;
        private CancellationTokenSource _cts;

        //variables de constructor 
        private string _apiKey;

        private Dictionary<string, ResumenCliente> _resumenPorCliente = new Dictionary<string, ResumenCliente>();

        private System.Windows.Forms.Timer timer;
        private int step = 10; // velocidad de apertura

        private bool abriendo = true;

        const string CAPTCHA_ERROR_TEXT = "Captcha incorrecta";
        private bool _procesoEnCurso;
        private CancellationTokenSource _cancellationTokenSource;
        private DateTime? _ultimaValidacionLicencia;
        private LicenseValidationResult _estadoLicencia;
        private const int MinutosCacheLicencia = 10;
        private string url;
        private const string DownloadFolderName = "Carga_Botcito";
        private const string LogFolderName = "BOT_LOG";
        private string downloadPath = "";

        private readonly object _lockResultados = new object();
        private List<ResultadoProcesoItem> _resultados = new List<ResultadoProcesoItem>();
        private readonly object _lockResumen = new object();

        private enum ResultadoConsulta
        {
            Descargado,
            SinDatos
        }

        private sealed class ClienteProcesable
        {
            public string Usuario { get; set; }
            public string Nombre { get; set; }
            public string CiAdicional { get; set; }
            public string Password { get; set; }
            public List<DateTime> Periodos { get; set; }
            public List<TipoComprobante> Tipos { get; set; }
        }

        public DescargaBotInterface(string usuarioActual)
        {
            InitializeComponent();

            _usuarioActual = usuarioActual;
            ModificaLblableWrite();

            CrearAnimacion();

            lblHIni2.Text = "";
            lblHFin2.Text = "";
            //lblDur2.Text = "";

            try
            {
                if (grdDescarga.Rows.Count > 10)
                {
                    nudHilos.Value = grdDescarga.Rows.Count / 4;
                }
                else { nudHilos.Value = grdDescarga.Rows.Count / 2; }
            }
            catch
            {

            }
            finally { nudHilos.Value = 10; }
            
            var apiKey2cp = ConfigurationManager.AppSettings["TwoCaptchaApiKey"] ?? "";
            var capSolverApiKey = ConfigurationManager.AppSettings["CapSolverApiKey"] ?? "";
            int timeout = int.TryParse(ConfigurationManager.AppSettings["TwoCaptchaTimeoutSeconds"], out var t) ? t : 240;
            int poll = int.TryParse(ConfigurationManager.AppSettings["TwoCaptchaPollSeconds"], out var p) ? p : 5;
            int cycles = int.TryParse(ConfigurationManager.AppSettings["MaxCaptchaCycles"], out var c) ? c : 5;
            bool enterprise = string.Equals(ConfigurationManager.AppSettings["UseRecaptchaEnterprise"], "true", StringComparison.OrdinalIgnoreCase);
            bool hcaptcha = string.Equals(ConfigurationManager.AppSettings["UseHCaptcha"], "true", StringComparison.OrdinalIgnoreCase);
            bool useTwoCaptcha = string.Equals(ConfigurationManager.AppSettings["2captcha"], "true", StringComparison.OrdinalIgnoreCase);
            bool useCapSolver = string.Equals(ConfigurationManager.AppSettings["Capsolver"], "true", StringComparison.OrdinalIgnoreCase);
            string logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoDescarga.log");
            //RutaDirectorio

            url = ConfigurationManager.AppSettings["SRI_LoginUrl"] ?? "";

            _recaptchaSolver = new RecaptchaV2Solver(
                ConfigurationManager.AppSettings["TwoCaptchaApiKey"],
                logPath,
                timeoutSeconds: int.Parse(ConfigurationManager.AppSettings["TwoCaptchaTimeoutSeconds"] ?? "240"),
                pollIntervalSeconds: int.Parse(ConfigurationManager.AppSettings["TwoCaptchaPollSeconds"] ?? "5")
            );

            if (useCapSolver && !string.IsNullOrWhiteSpace(capSolverApiKey))
            {
                _captchaSolver = new CapSolver(
                    capSolverApiKey,
                    logPath,
                    timeoutSeconds: timeout,
                    pollIntervalSeconds: poll
                );
                MostrarSaldoCaptcha();
            }
            else if (useTwoCaptcha && !string.IsNullOrWhiteSpace(apiKey2cp))
            {
                _captchaSolver = new RecaptchaV2Solver(
                    apiKey2cp,
                    logPath,
                    timeoutSeconds: timeout,
                    pollIntervalSeconds: poll
                );
                MostrarSaldoCaptcha();
            }
            else
            {
                _captchaSolver = null;
            }

            reintentos = int.Parse(dudReintentos.Text);
            chkOculto.Checked = string.Equals(ConfigurationManager.AppSettings["Headless"], "true", StringComparison.OrdinalIgnoreCase);
            #region
            grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdDescarga.AllowUserToAddRows = false;
            grdDescarga.ReadOnly = false;
            grdDescarga.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grdDescarga.MultiSelect = false;
            grdDescarga.Enabled = true;
            grdDescarga.TabStop = false;
            grdDescarga.BackgroundColor = Color.WhiteSmoke;

            grdDescarga.DataError += (s, e) => { e.Cancel = true; };
            grdDescarga.EnableHeadersVisualStyles = false;
            grdDescarga.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
            grdDescarga.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdDescarga.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 7F, FontStyle.Bold);
            grdDescarga.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 7F);
            grdDescarga.RowTemplate.Height = 20;
            grdDescarga.GridColor = Color.DarkGray;
            grdDescarga.BorderStyle = BorderStyle.None;
            grdDescarga.RowHeadersVisible = false;
            grdDescarga.ClearSelection();
            grdDescarga.CurrentCell = null;
            grdDescarga.DefaultCellStyle.SelectionBackColor = grdDescarga.DefaultCellStyle.BackColor;
            grdDescarga.DefaultCellStyle.SelectionForeColor = grdDescarga.DefaultCellStyle.ForeColor;
            grdDescarga.CellContentClick += grdDescarga_CellContentClick;

            //Modificaciones visuales 
            grdDescarga.BackgroundColor = Color.White;//Color.FromArgb(245, 246, 250); // gris claro
            grdDescarga.BorderStyle = BorderStyle.None;
            grdDescarga.GridColor = Color.FromArgb(209, 213, 219); // líneas suaves

            // Encabezados
            grdDescarga.EnableHeadersVisualStyles = false;
            grdDescarga.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 47); // azul grisáceo oscuro
            grdDescarga.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdDescarga.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 7F, FontStyle.Bold);

            // Celdas
            grdDescarga.DefaultCellStyle.BackColor = Color.White;
            grdDescarga.DefaultCellStyle.ForeColor = Color.FromArgb(46, 46, 46);
            grdDescarga.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 255); // azul muy suave
            grdDescarga.DefaultCellStyle.SelectionForeColor = Color.Black;
            grdDescarga.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdDescarga.ClearSelection();
            grdDescarga.CurrentCell = null;
            //grdDescarga.Enabled = false;
            #endregion

            grdDescarga.DataBindingComplete += (s, e) =>
            {
                try
                {
                    if (grdDescarga.Columns.Contains("orden"))
                    {
                        grdDescarga.Columns["orden"].Visible = true;
                        grdDescarga.Columns["orden"].ReadOnly = true;
                        grdDescarga.Columns["orden"].HeaderText = "Orden";
                    }

                    grdDescarga.Columns["usuario"].Visible = true;
                    grdDescarga.Columns["usuario"].ReadOnly = true;
                    grdDescarga.Columns["usuario"].HeaderText = "Identi.";

                    grdDescarga.Columns["NombUsuario"].Visible = true;
                    grdDescarga.Columns["NombUsuario"].ReadOnly = true;
                    grdDescarga.Columns["NombUsuario"].HeaderText = "Empresa";

                    grdDescarga.Columns["dias"].Visible = true;
                    grdDescarga.Columns["dias"].ReadOnly = true;
                    grdDescarga.Columns["dias"].HeaderText = "Días máx";

                    grdDescarga.Columns["meses_ante"].Visible = true;
                    grdDescarga.Columns["meses_ante"].ReadOnly = true;
                    grdDescarga.Columns["meses_ante"].HeaderText = "Meses";

                    grdDescarga.Columns["ci_adicional"].Visible = false;
                    grdDescarga.Columns["ci_adicional"].ReadOnly = true;
                    grdDescarga.Columns["ci_adicional"].HeaderText = "Adicional";

                    grdDescarga.Columns["clave"].Visible = false;
                    grdDescarga.Columns["clave"].ReadOnly = true;
                    grdDescarga.Columns["clave_ws"].Visible = false;

                    grdDescarga.Columns["descarga"].Visible = true;
                    grdDescarga.Columns["descarga"].ReadOnly = true;
                    grdDescarga.Columns["descarga"].HeaderText = "Descarga";

                    grdDescarga.Columns["factura"].Visible = true;
                    grdDescarga.Columns["factura"].ReadOnly = true;
                    grdDescarga.Columns["factura"].HeaderText = "Factura";

                    grdDescarga.Columns["notacredito"].Visible = true;
                    grdDescarga.Columns["notacredito"].ReadOnly = true;
                    grdDescarga.Columns["notacredito"].HeaderText = "Nota Crédito";

                    grdDescarga.Columns["retencion"].Visible = true;
                    grdDescarga.Columns["retencion"].ReadOnly = true;
                    grdDescarga.Columns["retencion"].HeaderText = "Retención";

                    grdDescarga.Columns["liquidacioncompra"].Visible = false;
                    grdDescarga.Columns["liquidacioncompra"].ReadOnly = true;
                    grdDescarga.Columns["liquidacioncompra"].HeaderText = "Liq. Compra";

                    grdDescarga.Columns["notadebito"].Visible = false;
                    grdDescarga.Columns["notadebito"].ReadOnly = true;
                    grdDescarga.Columns["notadebito"].HeaderText = "Nota Débito";

                    if (grdDescarga.Columns.Contains("consultar_mes_actual"))
                    {
                        grdDescarga.Columns["consultar_mes_actual"].ReadOnly = true;
                        grdDescarga.Columns["consultar_mes_actual"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdDescarga.Columns["consultar_mes_actual"].HeaderText = "Mes Actual";
                        grdDescarga.Columns["consultar_mes_actual"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        grdDescarga.Columns["consultar_mes_actual"].Width = 80;
                    }

                    if (grdDescarga.Columns.Contains("Activo"))
                    {
                        grdDescarga.Columns["Activo"].ReadOnly = true;
                        grdDescarga.Columns["Activo"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdDescarga.Columns["Activo"].HeaderText = "Activo";
                        grdDescarga.Columns["Activo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdDescarga.Columns.Contains("carga"))
                    {
                        grdDescarga.Columns["carga"].ReadOnly = true;
                        grdDescarga.Columns["carga"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdDescarga.Columns["carga"].HeaderText = "Carga";
                        grdDescarga.Columns["carga"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (!grdDescarga.Columns.Contains("Hilo"))
                    {
                        grdDescarga.Columns.Add("Hilo", "Hilo");
                    }

                    if (!grdDescarga.Columns.Contains("Procesado"))
                    {
                        var columnaProcesado = new DataGridViewTextBoxColumn
                        {
                            Name = "Procesado",
                            HeaderText = "Procesado",
                            ReadOnly = true,
                            SortMode = DataGridViewColumnSortMode.NotSortable
                        };
                        grdDescarga.Columns.Add(columnaProcesado);
                    }

                    grdDescarga.Columns["Procesado"].ReadOnly = true;
                    grdDescarga.Columns["Procesado"].HeaderText = "Procesado";
                    grdDescarga.Columns["Procesado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    if (grdDescarga.Columns.Contains("Directorio"))
                    {
                        // Ocultar la columna original vinculada al DataTable
                        grdDescarga.Columns["Directorio"].Visible = false;

                        // Verificar si ya existe la columna link personalizada
                        if (!grdDescarga.Columns.Contains("DirectorioLink"))
                        {
                            var linkColumn = new DataGridViewLinkColumn
                            {
                                Name = "DirectorioLink",
                                HeaderText = "Directorio",
                                Text = "📂",
                                UseColumnTextForLinkValue = true,
                                LinkColor = Color.Blue,
                                ActiveLinkColor = Color.DarkBlue,
                                VisitedLinkColor = Color.Purple,
                                Width = 80,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
                            };

                            linkColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            grdDescarga.Columns.Add(linkColumn);
                            grdDescarga.Columns["DirectorioLink"].DisplayIndex = grdDescarga.Columns.Count - 1;
                            grdDescarga.Columns["Directorio"].DisplayIndex = grdDescarga.Columns.Count - 1;
                        }
                    }

                    if (grdDescarga.Columns.Contains("orden"))
                    {
                        grdDescarga.Sort(grdDescarga.Columns["orden"], ListSortDirection.Ascending);
                    }

                    grdDescarga.Columns["carga"].Visible = false;

                    //if (grdDescarga.Columns.Contains("Claves cargadas"))
                    //{
                    //    DataColumn col = new DataColumn("Claves cargadas", typeof(int));
                    //    dt.Columns.Add(col);
                    //    //col.SetOrdinal(0); // mueve la columna al índice 0
                    //}

                    //grdDescarga.Columns["Claves cargadas"].ReadOnly = true;
                    grdDescarga.Columns["DirectorioLink"].DisplayIndex = grdDescarga.Columns.Count - 1;
                    grdDescarga.Columns["Hilo"].DisplayIndex = grdDescarga.Columns.Count - 2;

                }
                catch { }

                //grdDescarga.AutoResizeColumns();
                grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grdDescarga.ClearSelection();
                grdDescarga.CurrentCell = null;
            };

            grdDescarga.SelectionChanged += (s, e) =>
            {
                if (grdDescarga.SelectedRows.Count > 0)
                {
                    var row = grdDescarga.SelectedRows[0];
                    if (row.DataBoundItem is DataRowView rowView)
                    {
                        string usuario = rowView["usuario"]?.ToString();
                        if (!string.IsNullOrEmpty(usuario))
                        {
                            MostrarResumenEnRichTextBox(usuario);
                        }
                    }
                }
            };

            _ = CargarClientes_Desc();

            grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var basePath = ConfigurationManager.AppSettings["RutaDirectorio"];
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                downloadPath = Path.Combine(basePath, DownloadFolderName);
            }
        }

        public void ActualizarUCDescarga()
        {
            MostrarSaldoCaptcha();
            _ = CargarClientes_Desc();
            ModificaLblableWrite();
            CrearAnimacion();

            try
            {
                if (grdDescarga.Columns.Contains("orden") && grdDescarga.Rows.Count > 0)
                {
                    grdDescarga.Sort(grdDescarga.Columns["orden"], ListSortDirection.Ascending);
                    grdDescarga.ClearSelection();
                    grdDescarga.CurrentCell = null;
                }
            }
            catch (Exception ex)
            {
                // Log opcional si necesitas debug
                // LoggerHelper.Log(_logPath, $"Error al ordenar grid: {ex.Message}");
            }
        }

        private async void ModificaLblableWrite()
        {
            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
            bool validuser = await helper.TienePermisoEscrituraAsync(_usuarioActual);
            string logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoDescarga.log");

            if (validuser)
            {
                lblableWrite.Text = "Permiso escritura: Sí";
            }
            else if (!validuser)
            {
                lblableWrite.Text = "Permiso escritura: No";
            }

            var apiKey2cp = ConfigurationManager.AppSettings["TwoCaptchaApiKey"] ?? "";
            var capSolverApiKey = ConfigurationManager.AppSettings["CapSolverApiKey"] ?? "";
            int timeout = int.TryParse(ConfigurationManager.AppSettings["TwoCaptchaTimeoutSeconds"], out var t) ? t : 240;
            int poll = int.TryParse(ConfigurationManager.AppSettings["TwoCaptchaPollSeconds"], out var p) ? p : 5;
            int cycles = int.TryParse(ConfigurationManager.AppSettings["MaxCaptchaCycles"], out var c) ? c : 5;
            bool enterprise = string.Equals(ConfigurationManager.AppSettings["UseRecaptchaEnterprise"], "true", StringComparison.OrdinalIgnoreCase);
            bool hcaptcha = string.Equals(ConfigurationManager.AppSettings["UseHCaptcha"], "true", StringComparison.OrdinalIgnoreCase);
            bool useTwoCaptcha = string.Equals(ConfigurationManager.AppSettings["2captcha"], "true", StringComparison.OrdinalIgnoreCase);
            bool useCapSolver = string.Equals(ConfigurationManager.AppSettings["Capsolver"], "true", StringComparison.OrdinalIgnoreCase);
            string _logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoDescarga.log");

            _recaptchaSolver = new RecaptchaV2Solver(
                ConfigurationManager.AppSettings["TwoCaptchaApiKey"],
                _logPath,
                timeoutSeconds: int.Parse(ConfigurationManager.AppSettings["TwoCaptchaTimeoutSeconds"] ?? "240"),
                pollIntervalSeconds: int.Parse(ConfigurationManager.AppSettings["TwoCaptchaPollSeconds"] ?? "5")
            );

            if (useCapSolver && !string.IsNullOrWhiteSpace(capSolverApiKey))
            {
                _captchaSolver = new CapSolver(
                    capSolverApiKey,
                    _logPath,
                    timeoutSeconds: timeout,
                    pollIntervalSeconds: poll
                );
                MostrarSaldoCaptcha();
            }
            else if (useTwoCaptcha && !string.IsNullOrWhiteSpace(apiKey2cp))
            {
                _captchaSolver = new RecaptchaV2Solver(
                    apiKey2cp,
                    _logPath,
                    timeoutSeconds: timeout,
                    pollIntervalSeconds: poll
                );
                MostrarSaldoCaptcha();
            }
            else
            {
                _captchaSolver = null;
            }

            reintentos = int.Parse(dudReintentos.Text);
            chkOculto.Checked = string.Equals(ConfigurationManager.AppSettings["Headless"], "true", StringComparison.OrdinalIgnoreCase);
            grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void grdDescarga_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                grdDescarga.Columns[e.ColumnIndex].Name == "DirectorioLink")  // 🔹 Cambiar a "DirectorioLink"
            {
                DataRowView rowView = grdDescarga.Rows[e.RowIndex].DataBoundItem as DataRowView;

                if (rowView != null)
                {
                    string ruta = rowView["Directorio"]?.ToString();  // 🔹 Sigue leyendo de "Directorio" del DataTable

                    if (!string.IsNullOrWhiteSpace(ruta))
                    {
                        if (!Directory.Exists(ruta))
                        {
                            try
                            {
                                Directory.CreateDirectory(ruta);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error al crear el directorio: {ex.Message}",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        System.Diagnostics.Process.Start("explorer.exe", ruta);
                    }
                    else
                    {
                        MessageBox.Show("Ruta de directorio no disponible.",
                            "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private async Task CargarClientes_Desc()
        {
            try
            {
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                dt = await helper.GetClientesBotActivosDescAsync();

                if (dt != null)
                {
                    if (dt.Columns.Contains("Activo"))
                    {
                        dt.Columns.Add("ActivoBool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["Activo"].ToString().ToUpper();
                            row["ActivoBool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("Activo");
                        dt.Columns["ActivoBool"].ColumnName = "Activo";
                    }

                    if (dt.Columns.Contains("carga"))
                    {
                        dt.Columns.Add("cargabool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["carga"].ToString().ToUpper();
                            row["cargabool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("carga");
                        dt.Columns["cargabool"].ColumnName = "carga";
                    }

                    if (dt.Columns.Contains("descarga"))
                    {
                        dt.Columns.Add("descargabool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["descarga"].ToString().ToUpper();
                            row["descargabool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("descarga");
                        dt.Columns["descargabool"].ColumnName = "descarga";
                    }

                    if (dt.Columns.Contains("factura"))
                    {
                        dt.Columns.Add("facturabool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["factura"].ToString().ToUpper();
                            row["facturabool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("factura");
                        dt.Columns["facturabool"].ColumnName = "factura";
                    }

                    if (dt.Columns.Contains("notacredito"))
                    {
                        dt.Columns.Add("notacreditobool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["notacredito"].ToString().ToUpper();
                            row["notacreditobool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("notacredito");
                        dt.Columns["notacreditobool"].ColumnName = "notacredito";
                    }

                    if (dt.Columns.Contains("liquidacioncompra"))
                    {
                        dt.Columns.Add("liquidacioncomprabool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["liquidacioncompra"].ToString().ToUpper();
                            row["liquidacioncomprabool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("liquidacioncompra");
                        dt.Columns["liquidacioncomprabool"].ColumnName = "liquidacioncompra";
                    }

                    if (dt.Columns.Contains("retencion"))
                    {
                        dt.Columns.Add("retencionbool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["retencion"].ToString().ToUpper();
                            row["retencionbool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("retencion");
                        dt.Columns["retencionbool"].ColumnName = "retencion";
                    }

                    if (dt.Columns.Contains("notadebito"))
                    {
                        dt.Columns.Add("notadebitobool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["notadebito"].ToString().ToUpper();
                            row["notadebitobool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("notadebito");
                        dt.Columns["notadebitobool"].ColumnName = "notadebito";
                    }

                    if (dt.Columns.Contains("consultar_mes_actual"))
                    {
                        dt.Columns.Add("consultar_mes_actual_bool", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            string val = row["consultar_mes_actual"].ToString().ToUpper();
                            row["consultar_mes_actual_bool"] = (val == "Y" || val == "TRUE" || val == "1");
                        }
                        dt.Columns.Remove("consultar_mes_actual");
                        dt.Columns["consultar_mes_actual_bool"].ColumnName = "consultar_mes_actual";
                    }
                    else
                    {
                        // Si no existe en BD, agregar columna por defecto (false)
                        dt.Columns.Add("consultar_mes_actual", typeof(bool));
                        foreach (DataRow row in dt.Rows)
                        {
                            row["consultar_mes_actual"] = false;
                        }
                    }

                    //if (!dt.Columns.Contains("Procesado"))
                    //{
                    //    DataColumn col = new DataColumn("Procesado", typeof(string));
                    //    dt.Columns.Add(col);
                    //    //col.SetOrdinal(0); // mueve la columna al índice 0
                    //}

                    //foreach (DataRow row in dt.Rows)
                    //    row["Procesado"] = String.Empty;

                    if (!dt.Columns.Contains("Directorio"))
                    {
                        DataColumn col = new DataColumn("Directorio", typeof(string));
                        dt.Columns.Add(col);
                        //grdDescarga.Columns["DirectorioLink"].DisplayIndex = grdDescarga.Columns.Count - 1;
                        //grdDescarga.Columns["Directorio"].DisplayIndex = grdDescarga.Columns.Count - 1;
                    }

                    // 🔹 IMPORTANTE: Genera la ruta aquí para cada fila
                    foreach (DataRow row in dt.Rows)
                    {
                        string usuario = row["usuario"].ToString();
                        string nombreUsuario = row["NombUsuario"].ToString();
                        string carpeta = $"{usuario} - {nombreUsuario}";
                        string path = Helpers.Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                        row["Directorio"] = path;  // Guarda la ruta completa
                    }

                    grdDescarga.DataSource = dt;

                    if (dt.Columns.Contains("orden"))
                    {
                        dt.Columns.Add("orden_int", typeof(int));

                        foreach (DataRow row in dt.Rows)
                        {
                            int val;
                            if (int.TryParse(row["orden"].ToString(), out val))
                                row["orden_int"] = val;
                            else
                                row["orden_int"] = 0;
                        }

                        dt.Columns.Remove("orden");
                        dt.Columns["orden_int"].ColumnName = "orden";
                        grdDescarga.Columns["orden"].DisplayIndex = 0;
                    }

                    grdDescarga.ClearSelection();
                    grdDescarga.CurrentCell = null;

                    grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                else
                {
                    //MessageBox.Show("No se encontraron clientes.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error cargando clientes: " + ex.Message,
                //"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            grdDescarga.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private async void btnDescarga_Click(object sender, EventArgs e)
        {
            tblLayout.Visible = true;
            pictureBox1.Image = Properties.Resources.robot_move;
            tblLayout.ColumnStyles[0].Width = 50;
            tblLayout.ColumnStyles[1].Width = 0;
            tblLayout.ColumnStyles[2].Width = 50;

            abriendo = true;
            timer.Start();

            DateTime horaInicio = DateTime.Now;
            lblHIni2.Text = horaInicio.ToString("HH:mm:ss");

            if (_procesoEnCurso)
            {
                SolicitarCancelacionProceso();
                return;
            }

            var basePath = ConfigurationManager.AppSettings["RutaDirectorio"];
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                downloadPath = Path.Combine(basePath, DownloadFolderName);
            }

            url = ConfigurationManager.AppSettings["SRI_LoginUrl"] ?? "";

            await EjecutarProcesoManualAsync();

            pictureBox1.Image = Properties.Resources.robot_no_move;
            tblLayout.ColumnStyles[0].Width = 0;
            tblLayout.ColumnStyles[1].Width = 100;
            tblLayout.ColumnStyles[2].Width = 0;

            abriendo = false;
            timer.Start();

            tblLayout.Visible = false;

            DateTime horaFin = DateTime.Now;
            lblHFin2.Text = horaFin.ToString("HH:mm:ss");
        }

        private async Task EjecutarProcesoManualAsync()
        {
            await EjecutarProcesoAsync(true);
        }

        private async Task EjecutarProcesoAsync(bool mostrarMensajes)
        {
            if (_procesoEnCurso)
                return;

            _procesoEnCurso = true;
            _cancellationTokenSource = new CancellationTokenSource();
            ActualizarEstadoBotonProceso(true);
            //ReiniciarResultados();
            LimpiarResumenProceso();

            try
            {
                if (!await ValidarLicenciaAsync(mostrarMensajes))
                {
                    return;
                }

                var dt = grdDescarga.DataSource as DataTable;
                if (dt == null)
                    throw new InvalidOperationException("No hay clientes cargados para procesar.");

                var clientes = ConstruirClientesProcesables(dt);
                var tareas = Enumerable.Range(0, (int) nudHilos.Value)
                    .Select(indice => ProcesarClientesAsync(clientes, indice, _cancellationTokenSource.Token))
                    .ToList();

                await Task.WhenAll(tareas);

                if (mostrarMensajes)
                {
                    ReproducirMensajeFinal(ExistenFallosEnResumen());
                    MessageBox.Show("Proceso finalizado correctamente", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //MostrarResultadosFinales();
            }
            catch (OperationCanceledException)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show("Proceso cancelado por el usuario.", "Cancelado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show(ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ejecución automática fallida: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                ActualizarEstadoBotonProceso(false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _procesoEnCurso = false;
            }
        }

        private void LimpiarResumenProceso()
        {
            lock (_lockResumen)
            {
                _resumenPorCliente.Clear();
            }
        }

        private bool ExistenFallosEnResumen()
        {
            lock (_lockResumen)
            {
                return _resumenPorCliente.Values.Any(resumen =>
                    resumen.Fallidas.Any(detalle => !detalle.Error.Contains("No existen datos")));
            }
        }

        private void ReproducirMensajeFinal(bool huboFallos)
        {
            string mensaje = huboFallos
                ? "The download process has finished and needs to be reviewed."
                : "The download process has completed successfully.";

            Task.Run(async () =>
            {
                var speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
                var speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
                var speechVoice = Environment.GetEnvironmentVariable("AZURE_SPEECH_VOICE");
                const string idiomaDefecto = "es-ES";
                const string vozFemeninaDefecto = "es-ES-ElviraNeural";

                if (!string.IsNullOrWhiteSpace(speechKey) && !string.IsNullOrWhiteSpace(speechRegion))
                {
                    var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                    speechConfig.SpeechSynthesisLanguage = idiomaDefecto;
                    speechConfig.SpeechSynthesisVoiceName = vozFemeninaDefecto;

                    if (!string.IsNullOrWhiteSpace(speechVoice))
                    {
                        speechConfig.SpeechSynthesisVoiceName = speechVoice;
                    }

                    using (var sintetizadorAzure = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(speechConfig))
                    {
                        await sintetizadorAzure.SpeakTextAsync(mensaje).ConfigureAwait(false);
                        return;
                    }
                }

                using (var sintetizadorLocal = new DesktopSpeechSynthesizer())
                {
                    SeleccionarVozLocalFemenina(sintetizadorLocal, new CultureInfo(idiomaDefecto));
                    sintetizadorLocal.SetOutputToDefaultAudioDevice();
                    sintetizadorLocal.Speak(mensaje);
                }
            });
        }

        private static void SeleccionarVozLocalFemenina(DesktopSpeechSynthesizer sintetizador, CultureInfo cultura)
        {
            var voces = sintetizador
                .GetInstalledVoices()
                .Where(voz => voz.Enabled)
                .ToList();

            var vozCultura = voces.FirstOrDefault(voz =>
                voz.VoiceInfo.Gender == System.Speech.Synthesis.VoiceGender.Female &&
                Equals(voz.VoiceInfo.Culture, cultura));

            var vozFemenina = vozCultura ?? voces.FirstOrDefault(voz =>
                voz.VoiceInfo.Gender == System.Speech.Synthesis.VoiceGender.Female);

            if (vozFemenina != null)
            {
                sintetizador.SelectVoice(vozFemenina.VoiceInfo.Name);
            }
        }

        private async Task ProcesarClientesAsync(IReadOnlyList<ClienteProcesable> clientes, int indiceHilo, CancellationToken cancellationToken)
        {
            for (int i = indiceHilo; i < clientes.Count; i += (int) nudHilos.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var cliente = clientes[i];
                var estadosCliente = new List<EstadoResultado>();
                var descargasExitosas = new List<DescargaDetalle>();
                var descargasFallidas = new List<DescargaDetalle>();
                var tiposFiltrados = cliente.Tipos
                    .Select(tipo => new KeyValuePair<int, string>(ObtenerClaveTipo(tipo), tipo.Nombre ?? tipo.Value))
                    .ToList();
                var mesesConsulta = cliente.Periodos
                    .Select(periodo => (periodo.Month, periodo.Year))
                    .ToList();
                string rutaLogCliente = ObtenerRutaLogCliente(cliente.Usuario, cliente.Nombre);


                try
                {
                    foreach (var periodo in cliente.Periodos)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        foreach (var tipo in cliente.Tipos)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var parametros = new ParametrosConsulta
                            {
                                Anio = periodo.Year.ToString(),
                                Mes = periodo.Month.ToString(),
                                Dia = "0",
                                Tipo = tipo
                            };

                            EstadoResultado estado;
                            try
                            {
                                estado = await EjecutarProcesoConReintentosAsync(url, false, cliente.Usuario, cliente.CiAdicional, cliente.Password, cliente.Nombre, parametros, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                estado = EstadoResultado.Fallido;
                                LogCliente(cliente.Usuario, cliente.Nombre, $"❌ Error inesperado: {ex.Message}");
                            }

                            RegistrarResultado(cliente, parametros, estado);
                            estadosCliente.Add(estado);
                            RegistrarResumenDetalle(cliente, periodo, tipo, estado, descargasExitosas, descargasFallidas);
                        }
                    }
                }
                finally
                {
                    GenerarResumenCliente(cliente.Usuario, cliente.Nombre, descargasExitosas, descargasFallidas, tiposFiltrados, mesesConsulta, rutaLogCliente);
                    var resumenEstado = CalcularEstadoCliente(estadosCliente);
                    ActualizarEstadoCliente(cliente.Usuario, resumenEstado.Texto, resumenEstado.ColorFondo);
                }
            }
        }

        private void RegistrarResumenDetalle(ClienteProcesable cliente, DateTime periodo, TipoComprobante tipo, EstadoResultado estado, List<DescargaDetalle> exitosas, List<DescargaDetalle> fallidas)
        {
            var detalle = new DescargaDetalle
            {
                Cliente = cliente.Nombre,
                TipoDocumento = tipo.Nombre ?? tipo.Value,
                Mes = periodo.Month,
                Año = periodo.Year,
                Exitoso = estado == EstadoResultado.Exitoso
            };

            if (estado == EstadoResultado.Exitoso)
            {
                exitosas.Add(detalle);
                return;
            }

            detalle.Error = estado == EstadoResultado.SinDatos ? "No existen datos" : "Error de descarga";
            fallidas.Add(detalle);
        }

        private int ObtenerClaveTipo(TipoComprobante tipo)
        {
            return int.TryParse(tipo.Value, out var clave) ? clave : 0;
        }

        private string ObtenerRutaLogCliente(string usuario, string nombre)
        {
            string logDirectory = PrepararRutaLog(downloadPath);
            string sanitizedNombre = SanitizarNombreArchivo(nombre);
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            string logFile = $"{usuario}_{sanitizedNombre}_{fecha}.log";
            return Path.Combine(logDirectory, logFile);
        }

        private void RegistrarResultado(ClienteProcesable cliente, ParametrosConsulta parametros, EstadoResultado estado)
        {
            var item = new ResultadoProcesoItem
            {
                Usuario = cliente.Usuario,
                Nombre = cliente.Nombre,
                CiAdicional = cliente.CiAdicional,
                Password = cliente.Password,
                Anio = parametros.Anio,
                Mes = parametros.Mes,
                Documento = parametros.Tipo?.Nombre ?? parametros.Tipo?.Value,
                Estado = estado,
                Parametros = parametros
            };

            lock (_lockResultados)
            {
                _resultados.Add(item);
            }
        }

        private async Task<EstadoResultado> EjecutarProcesoConReintentosAsync(string pageUrl, bool headless, string usuario, string ciAdicional, string password, string nombre, ParametrosConsulta parametros, CancellationToken cancellationToken)
        {
            Exception lastEx = null;

            for (int intento = 1; intento <= dudReintentos.Value; intento++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var resultado = await EjecutarProcesoAsync_per_client(pageUrl, headless, usuario, ciAdicional, password, nombre, parametros, cancellationToken);

                    if (resultado == ResultadoConsulta.SinDatos)
                    {
                        LogCliente(usuario, nombre, $"¨Sin datos {intento}/{dudReintentos.Value} ({parametros.Anio}-{parametros.Mes}, {parametros.Tipo?.Value})");
                        return EstadoResultado.SinDatos; // OK: no hay datos, no es error
                    }

                    LogCliente(usuario, nombre, $"¨Descarga exitosa {intento}/{dudReintentos.Value} ({parametros.Anio}-{parametros.Mes}, {parametros.Tipo?.Value})");
                    return EstadoResultado.Exitoso; // OK: descargó
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastEx = ex;

                    LogCliente(usuario, nombre, $"Falló intento {intento}/{dudReintentos.Value} ({parametros.Anio}-{parametros.Mes}, {parametros.Tipo?.Value}): {ex.Message}");
                }
            }

            LogCliente(usuario, nombre, $"❌ Falló la combinación luego de {dudReintentos.Value} intentos: {parametros.Anio}-{parametros.Mes} Tipo={parametros.Tipo?.Value}. Error: {lastEx?.Message}");

            return EstadoResultado.Fallido;
        }

        private async Task<ResultadoConsulta> EjecutarProcesoAsync_per_client(string pageUrl, bool headless, string usuario, string ciAdicional, string password, string nombre, ParametrosConsulta parametros, CancellationToken cancellationToken)
        {
            PlaywrightManager manager = null;
            BrowserSession session = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var config = new BrowserConfig
                {
                    Url = pageUrl,
                    Headless = headless
                };

                manager = new PlaywrightManager();
                await manager.InitializeAsync(config);

                session = new BrowserSession(manager.Browser, config);
                await session.StartAsync();

                var actions = new PageActions(session.Page);

                // 1️⃣ LOGIN
                await IniciarSesionAsync(session, actions, usuario, ciAdicional, password, nombre);

                cancellationToken.ThrowIfCancellationRequested();

                // 2️⃣ CONSULTA + DESCARGA
                return await EjecutarConsultaYDescargaAsync(session, actions, parametros, usuario, nombre);
            }
            finally
            {
                // 3️⃣ CERRAR SESIÓN (intento)
                if (session != null)
                {
                    try
                    {
                        await CerrarSesionAsync(session);
                    }
                    catch
                    {
                        // no romper el cierre general
                    }

                    await session.DisposeAsync();
                }

                if (manager != null)
                    await manager.DisposeAsync();
            }
        }

        private async Task IniciarSesionAsync(BrowserSession session, PageActions actions, string usuario, string ciAdicional, string password, string _nombre)
        {
            await session.Page.WaitForSelectorAsync("#usuario", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible
            });

            await actions.SetTextAsync("#usuario", usuario);
            await actions.SetTextAsync("#ciAdicional", ciAdicional);
            await actions.SetTextAsync("#password", password);

            await actions.ClickAsync("#kc-login");

            // Esperas seguras SRI
            await session.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            bool loginExitoso = await WaitHelper.ExistsAsync(
                session.Page,
                "a[tooltip='Cerrar sesión']",
                25000
            );

            if (!loginExitoso)
                throw new Exception("❌ No se detectó el botón Cerrar sesión (login fallido)");
        }

        private async Task CerrarSesionAsync(BrowserSession session)
        {
            bool existeCerrarSesion = await WaitHelper.ExistsAsync(
                session.Page,
                "a[tooltip='Cerrar sesión']",
                5000
            );

            if (!existeCerrarSesion)
                return;

            await session.Page.ClickAsync("a[tooltip='Cerrar sesión']");

            await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task<ResultadoConsulta> EjecutarConsultaYDescargaAsync(BrowserSession session, PageActions actions, ParametrosConsulta parametros, string usuario, string nombreUsuario)
        {
            // SETEAR COMBOS
            await actions.SelectAsync("#frmPrincipal\\:ano", parametros.Anio);
            await actions.SelectAsync("#frmPrincipal\\:mes", parametros.Mes);
            await actions.SelectAsync("#frmPrincipal\\:dia", parametros.Dia);
            await actions.SelectAsync("#frmPrincipal\\:cmbTipoComprobante", parametros.Tipo.Value);

            // CONSULTAR
            await actions.ClickAsync("#frmPrincipal\\:btnBuscar");
            // CONSULTAR (con detección y recuperación de "captcha incorrecta")
            //await ConsultarConRecuperacionCaptchaAsync(session.Page, actions, "#frmPrincipal\\:btnBuscar");

            bool captchaincorrecta = await WaitHelper.ExistsAsync(
                session.Page,
                "text=Captcha incorrecta",
                3000
            );

            if (captchaincorrecta)
                await ConsultarConRecuperacionCaptchaAsync(session.Page, actions, "#frmPrincipal\\:btnBuscar");

            bool sinDatos = await WaitHelper.ExistsAsync(
                session.Page,
                "text=No existen datos",
                3000
            );

            if (sinDatos)
                return ResultadoConsulta.SinDatos;

            bool tablaCargada = await WaitHelper.ExistsAsync(
                session.Page,
                "#frmPrincipal\\:tablaCompRecibidos",
                10000
            );

            if (!tablaCargada)
                throw new Exception("❌ No se cargó la tabla de comprobantes electrónicos");

            string rutaUsuario = PrepararRutaDescarga(downloadPath, usuario, nombreUsuario);

            // DESCARGA CONTROLADA
            var download = await session.Page.RunAndWaitForDownloadAsync(async () =>
            {
                await actions.ClickAsync("#frmPrincipal\\:lnkTxtlistado");
            });

            string extension = Path.GetExtension(download.SuggestedFilename);

            string nombreArchivo =
                $"{parametros.Tipo.PrefijoArchivo}_" +
                $"{parametros.Mes.PadLeft(2, '0')}_" +
                $"{parametros.Anio}{extension}";

            string rutaFinal = Path.Combine(rutaUsuario, nombreArchivo);

            await download.SaveAsAsync(rutaFinal);
            return ResultadoConsulta.Descargado;
        }
        private string PrepararRutaDescarga(string basePath, string usuario, string _nombre)
        {
            string userFolder = Path.Combine(basePath, $"{usuario} - {_nombre}");

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            return userFolder;
        }

        private async Task ConsultarConRecuperacionCaptchaAsync(IPage page, PageActions actions, string btnBuscarSelector)
        {
            // 1) Intento normal
            await actions.ClickAsync(btnBuscarSelector);

            // 2) Espera corta para ver si aparece "captcha incorrecta"
            //    (importante: que el DOM tenga tiempo de pintar el mensaje)
            await page.WaitForTimeoutAsync(6000);

            // 3) Si detecta mensaje de captcha incorrecta, forzar estrategias
            bool captchaIncorrecta = await ExisteCaptchaIncorrectaAsync(page, 6000);
            if (!captchaIncorrecta)
                return;

            // Estrategias más efectivas: intentos cortos con esperas, sin “romper” el flujo
            var estrategias = ConstruirEstrategiasForzadas(btnBuscarSelector);

            for (int i = 0; i < estrategias.Count; i++)
            {
                // Pequeña espera entre estrategias (deja respirar a PrimeFaces/AJAX)
                await page.WaitForTimeoutAsync(6000);

                await estrategias[i](page);

                // Esperar a que el request/DOM reaccione
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForTimeoutAsync(9000);

                // Si ya no está el mensaje, salir
                captchaIncorrecta = await ExisteCaptchaIncorrectaAsync(page, 1200);
                if (!captchaIncorrecta)
                    return;
            }

            // Si llega aquí, sigue con captcha incorrecta:
            // NO lanzamos excepción aquí; dejamos que tu lógica global de reintentos maneje el fallo si aplica
        }

        private async Task<bool> ExisteCaptchaIncorrectaAsync(IPage page, int timeoutMs)
        {
            // Buscamos texto que “contenga” captcha incorrecta (case-insensitive)
            // Espera hasta timeoutMs para capturar el render del mensaje
            try
            {
                // Preferimos locator con regex por robustez (a veces hay espacios o tildes)
                var locator = page.Locator("text=/captcha\\s+incorrecta/i");
                await locator.First.WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = timeoutMs,
                    State = WaitForSelectorState.Visible
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<Func<IPage, Task>> ConstruirEstrategiasForzadas(string btnBuscarSelector)
        {
            return new List<Func<IPage, Task>>
            {
                // 1) Scroll + focus + click Playwright
                async (page) =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });
                        el.focus();
                    }", btnBuscarSelector);

                    await page.ClickAsync(btnBuscarSelector, new PageClickOptions { Force = true, Timeout = 5000 });
                },

                // 2) Click con JS directo
                async (page) =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });
                        el.click();
                    }", btnBuscarSelector);
                },

                // 3) Disparar MouseEvents (mousedown/up/click) para forzar listeners
                async (page) =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });

                        const fire = (type) => el.dispatchEvent(new MouseEvent(type, { bubbles: true, cancelable: true, view: window }));
                        fire('mousedown');
                        fire('mouseup');
                        fire('click');
                    }", btnBuscarSelector);
                },

                // 4) Enter sobre el botón (a veces dispara submit/PrimeFaces)
                async (page) =>
                {
                    await page.FocusAsync(btnBuscarSelector);
                    await page.Keyboard.PressAsync("Enter");
                },

                // 5) Submit del formulario más cercano
                async (page) =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;

                        const form = el.closest('form');
                        if (!form) return;

                        // Si existe requestSubmit, es lo más “real”
                        if (typeof form.requestSubmit === 'function') {
                            form.requestSubmit();
                        } else {
                            form.submit();
                        }
                    }", btnBuscarSelector);
                },

                // 6) Intento PrimeFaces.ab si el onclick lo contiene
                async (page) =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;

                        const onclick = el.getAttribute('onclick') || '';
                        if (onclick.includes('PrimeFaces.ab')) {
                            try { eval(onclick); } catch (e) {}
                        } else {
                            // fallback: click
                            el.click();
                        }
                    }", btnBuscarSelector);
                },
            };
        }

        private void LogCliente(string usuario, string nombre, string mensaje)
        {
            string logDirectory = PrepararRutaLog(downloadPath);
            string sanitizedNombre = SanitizarNombreArchivo(nombre);
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            string logFile = $"{usuario}_{sanitizedNombre}_{fecha}.log";
            string rutaLog = Path.Combine(logDirectory, logFile);
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}{Environment.NewLine}";
            File.AppendAllText(rutaLog, entry, Encoding.UTF8);
        }

        private string PrepararRutaLog(string basePath)
        {
            string logFolder = Path.Combine(basePath, LogFolderName);

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            return logFolder;
        }

        private string SanitizarNombreArchivo(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "sin_nombre";

            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(nombre.Length);

            foreach (char ch in nombre)
            {
                builder.Append(invalidChars.Contains(ch) ? '_' : ch);
            }

            return builder.ToString().Trim();
        }

        private List<ClienteProcesable> ConstruirClientesProcesables(DataTable dt)
        {
            var clientes = new List<ClienteProcesable>();

            foreach (DataRow row in dt.Rows)
            {
                if (!EsValorYN(row, "Activo"))
                    continue;

                clientes.Add(new ClienteProcesable
                {
                    Usuario = row["Usuario"]?.ToString(),
                    Nombre = row["NombUsuario"]?.ToString(),
                    CiAdicional = row["ci_Adicional"]?.ToString(),
                    Password = row["clave"]?.ToString(),
                    Periodos = ConstruirPeriodosConsulta(row).ToList(),
                    Tipos = ObtenerTiposHabilitados(row).ToList()
                });
            }

            return clientes;
        }

        private bool EsValorYN(DataRow row, string columnName)
        {
            var value = row[columnName]?.ToString();
            return string.Equals(value?.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);
        }

        private int ObtenerEntero(DataRow row, string columnName)
        {
            if (int.TryParse(row[columnName]?.ToString(), out int valor))
                return valor;

            return 0;
        }

        private IEnumerable<DateTime> ConstruirPeriodosConsulta(DataRow row)
        {
            DateTime hoy = DateTime.Today;
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var periodos = new List<DateTime>();

            if (EsValorYN(row, "consultar_mes_actual"))
                periodos.Add(inicioMes);

            int mesesAnteriores = ObtenerEntero(row, "meses_ante");
            int diasPermitidos = ObtenerEntero(row, "dias");

            if (mesesAnteriores > 0 && (hoy.Day <= diasPermitidos || diasPermitidos == 0))
            {
                for (int i = 1; i <= mesesAnteriores; i++)
                    periodos.Add(inicioMes.AddMonths(-i));
            }

            return periodos;
        }

        private IEnumerable<TipoComprobante> ObtenerTiposHabilitados(DataRow row)
        {
            var tipos = new Dictionary<string, string>
            {
                { "factura", "1" },
                { "liquidacioncompra", "2" },
                { "notacredito", "3" },
                { "notadebito", "4" },
                { "retencion", "6" }
            };

            foreach (var tipo in tipos)
            {
                if (!EsValorYN(row, tipo.Key))
                    continue;

                var encontrado = CatalogoComprobantes.ObtenerPorValue(tipo.Value);
                if (encontrado != null)
                    yield return encontrado;
            }
        }

        private void SolicitarCancelacionProceso()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                return;

            _cancellationTokenSource.Cancel();
        }

        private void ActualizarEstadoBotonProceso(bool enEjecucion)
        {
            if (enEjecucion)
            {
                btnDescarga.BackColor = Color.Red;
                btnDescarga.Text = "CANCELAR";
                btnDescarga.Enabled = true;
                return;
            }

            btnDescarga.BackColor = Color.FromArgb(34, 197, 94);
            btnDescarga.Text = "DESCARGAR 🔽";
            btnDescarga.Enabled = true;
        }

        private async Task<bool> ValidarLicenciaAsync(bool mostrarMensajes)
        {
            if (_ultimaValidacionLicencia.HasValue
                && _estadoLicencia != null
                && (DateTime.Now - _ultimaValidacionLicencia.Value).TotalMinutes < MinutosCacheLicencia)
            {
                return _estadoLicencia.IsValid && _estadoLicencia.IsActive;
            }

            var _supabaseUrl = supabaseUrl;
            var supabaseAnonKey = apiKey;
            var userCode = ConfigurationManager.AppSettings["usuario"];
            var password = ConfigurationManager.AppSettings["clave"];

            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(password))
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show("Configura Supabase y la licencia en Configuración antes de ejecutar.",
                        "Licencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                return false;
            }

            try
            {
                var client = new SupabaseLicenseClient(_supabaseUrl, supabaseAnonKey);
                _estadoLicencia = await client.ValidateAsync(userCode, password);
                _ultimaValidacionLicencia = DateTime.Now;

                if (!_estadoLicencia.IsValid || !_estadoLicencia.IsActive)
                {
                    if (mostrarMensajes)
                    {
                        MessageBox.Show(_estadoLicencia.Message ?? "Licencia inactiva.",
                            "Licencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show($"No se pudo validar la licencia: {ex.Message}",
                        "Licencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }

        private void GenerarResumenCliente(string usuario,string nombre,List<DescargaDetalle> exitosas,List<DescargaDetalle> fallidas,List<KeyValuePair<int, string>> tiposFiltrados,List<(int Mes, int Año)> mesesConsulta, string pathHilo)
        {
            // Guardar resumen en diccionario
            lock (_lockResumen)
            {
                _resumenPorCliente[usuario] = new ResumenCliente
                {
                    Usuario = usuario,
                    Nombre = nombre,
                    Exitosas = new List<DescargaDetalle>(exitosas),
                    Fallidas = new List<DescargaDetalle>(fallidas),
                    TiposFiltrados = new List<KeyValuePair<int, string>>(tiposFiltrados),
                    MesesConsulta = new List<(int Mes, int Año)>(mesesConsulta)
                };
            }

            // Log en archivo (mantener comportamiento original)
            LoggerHelper.Log(pathHilo, $"\n{'=',-60}");
            LoggerHelper.Log(pathHilo, $"📊 RESUMEN DE DESCARGA - {nombre}");
            LoggerHelper.Log(pathHilo, $"{'=',-60}");

            foreach (var tipo in tiposFiltrados)
            {
                LoggerHelper.Log(pathHilo, $"\n📄 {tipo.Value}:");

                foreach (var (mes, anio) in mesesConsulta)
                {
                    var exitosa = exitosas.FirstOrDefault(
                        x => x.TipoDocumento == tipo.Value && x.Mes == mes && x.Año == anio);
                    var fallida = fallidas.FirstOrDefault(
                        x => x.TipoDocumento == tipo.Value && x.Mes == mes && x.Año == anio);

                    if (exitosa != null)
                    {
                        LoggerHelper.Log(pathHilo, $"   ✅ {mes:00}/{anio} - Descargado");
                    }
                    else if (fallida != null)
                    {
                        string simbolo = fallida.Error.Contains("No existen datos") ? "⚪" : "❌";
                        LoggerHelper.Log(pathHilo, $"   {simbolo} {mes:00}/{anio} - {fallida.Error}");
                    }
                    else
                    {
                        LoggerHelper.Log(pathHilo, $"   ⚪ {mes:00}/{anio} - No procesado");
                    }
                }
            }

            int totalIntentos = tiposFiltrados.Count * mesesConsulta.Count;
            int totalExitosas = exitosas.Count;
            int totalFallidas = fallidas.Count(x => !x.Error.Contains("No existen datos"));
            int totalSinDatos = fallidas.Count(x => x.Error.Contains("No existen datos"));

            LoggerHelper.Log(pathHilo, $"\n📈 ESTADÍSTICAS:");
            LoggerHelper.Log(pathHilo, $"   Total intentos: {totalIntentos}");
            LoggerHelper.Log(pathHilo, $"   ✅ Exitosas: {totalExitosas}");
            LoggerHelper.Log(pathHilo, $"   ❌ Fallidas: {totalFallidas}");
            LoggerHelper.Log(pathHilo, $"   ⚪ Sin datos: {totalSinDatos}");
            LoggerHelper.Log(pathHilo, $"{'=',-60}\n");
        }

        private void MostrarResumenEnRichTextBox(string usuario)
        {
            richTextBox1.Clear();

            if (!_resumenPorCliente.ContainsKey(usuario))
            {
                richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Gray;
                richTextBox1.AppendText("No hay resumen disponible para este cliente.\n");
                richTextBox1.AppendText("El resumen se genera después de ejecutar el proceso de descarga.");
                return;
            }

            var resumen = _resumenPorCliente[usuario];

            // Título principal
            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 14F, FontStyle.Bold);
            richTextBox1.SelectionColor = Color.FromArgb(30, 30, 47);
            richTextBox1.AppendText($"📊 RESUMEN DE DESCARGA\n");

            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Bold);
            richTextBox1.SelectionColor = Color.FromArgb(59, 130, 246);
            richTextBox1.AppendText($"{resumen.Nombre}\n");

            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);
            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText($"RUC/CI: {resumen.Usuario}\n");

            // Línea separadora
            richTextBox1.SelectionColor = Color.LightGray;
            richTextBox1.AppendText(new string('─', 60) + "\n\n");

            // Estadísticas generales
            int totalIntentos = resumen.TiposFiltrados.Count * resumen.MesesConsulta.Count;
            int totalExitosas = resumen.Exitosas.Count;
            int totalFallidas = resumen.Fallidas.Count(x => !x.Error.Contains("No existen datos"));
            int totalSinDatos = resumen.Fallidas.Count(x => x.Error.Contains("No existen datos"));

            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Bold);
            richTextBox1.SelectionColor = Color.FromArgb(30, 30, 47);
            richTextBox1.AppendText("📈 ESTADÍSTICAS GENERALES\n");

            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.AppendText($"   Total de intentos: {totalIntentos}\n");

            richTextBox1.SelectionColor = Color.FromArgb(34, 197, 94);
            richTextBox1.AppendText($"   ✅ Descargas exitosas: {totalExitosas}\n");

            richTextBox1.SelectionColor = Color.FromArgb(239, 68, 68);
            richTextBox1.AppendText($"   ❌ Descargas fallidas: {totalFallidas}\n");

            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText($"   ⚪ Sin datos disponibles: {totalSinDatos}\n\n");

            // Línea separadora
            richTextBox1.SelectionColor = Color.LightGray;
            richTextBox1.AppendText(new string('─', 60) + "\n\n");

            // Detalle por tipo de documento
            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Bold);
            richTextBox1.SelectionColor = Color.FromArgb(30, 30, 47);
            richTextBox1.AppendText("📄 DETALLE POR TIPO DE DOCUMENTO\n\n");

            foreach (var tipo in resumen.TiposFiltrados)
            {
                richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold);
                richTextBox1.SelectionColor = Color.FromArgb(59, 130, 246);
                richTextBox1.AppendText($"▸ {tipo.Value}\n");

                foreach (var (mes, anio) in resumen.MesesConsulta)
                {
                    var exitosa = resumen.Exitosas.FirstOrDefault(
                        x => x.TipoDocumento == tipo.Value && x.Mes == mes && x.Año == anio);
                    var fallida = resumen.Fallidas.FirstOrDefault(
                        x => x.TipoDocumento == tipo.Value && x.Mes == mes && x.Año == anio);

                    richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);

                    if (exitosa != null)
                    {
                        richTextBox1.SelectionColor = Color.FromArgb(34, 197, 94);
                        richTextBox1.AppendText($"   ✅ {mes:00}/{anio} - Descargado exitosamente\n");
                    }
                    else if (fallida != null)
                    {
                        if (fallida.Error.Contains("No existen datos"))
                        {
                            richTextBox1.SelectionColor = Color.Gray;
                            richTextBox1.AppendText($"   ⚪ {mes:00}/{anio} - Sin datos disponibles\n");
                        }
                        else
                        {
                            richTextBox1.SelectionColor = Color.FromArgb(239, 68, 68);
                            richTextBox1.AppendText($"   ❌ {mes:00}/{anio} - Error: ");
                            richTextBox1.SelectionColor = Color.DarkRed;
                            richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 8F, FontStyle.Italic);
                            richTextBox1.AppendText($"{fallida.Error}\n");
                        }
                    }
                    else
                    {
                        richTextBox1.SelectionColor = Color.LightGray;
                        richTextBox1.AppendText($"   ⚪ {mes:00}/{anio} - No procesado\n");
                    }
                }

                richTextBox1.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.AppendText("\n");
            }

            // Scroll al inicio
            richTextBox1.SelectionStart = 0;
            richTextBox1.ScrollToCaret();
        }

        private async void MostrarSaldoCaptcha()
        {
            bool useTwoCaptcha = string.Equals(ConfigurationManager.AppSettings["2captcha"], "true", StringComparison.OrdinalIgnoreCase);
            bool useCapSolver = string.Equals(ConfigurationManager.AppSettings["Capsolver"], "true", StringComparison.OrdinalIgnoreCase);

            string apiKey = "";
            string servicio = "";

            if (useCapSolver)
            {
                apiKey = ConfigurationManager.AppSettings["CapSolverApiKey"] ?? "";
                apiKeySolver = ConfigurationManager.AppSettings["CapSolverApiKey"] ?? "";
                servicio = "CapSolver";
            }
            else if (useTwoCaptcha)
            {
                apiKey = ConfigurationManager.AppSettings["TwoCaptchaApiKey"] ?? "";
                apiKeySolver = ConfigurationManager.AppSettings["TwoCaptchaApiKey"] ?? "";
                servicio = "2Captcha";
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                lblSaldo.Text = "💰 Saldo CAPTCHA: No configurado";
                lblSaldo.ForeColor = Color.Gray;
                return;
            }

            lblSaldo.Text = "💰 Saldo CAPTCHA: Consultando...";
            lblSaldo.ForeColor = Color.Orange;

            try
            {
                string saldo = await ConsultarSaldoAsync(servicio, apiKey);
                lblSaldo.Text = $"💰 Saldo {servicio}: ${saldo}";
                lblSaldo.ForeColor = Color.Blue;
            }
            catch (Exception ex)
            {
                lblSaldo.Text = $"💰 Saldo {servicio}: Error";
                lblSaldo.ForeColor = Color.Red;
                //EscribirLog($"⚠ Error al consultar saldo {servicio}: {ex.Message}");
            }

            lblSolver.Text = $"API: {servicio}";
        }

        private async Task<string> ConsultarSaldoAsync(string servicio, string apiKey)
        {
            using (var httpClient = new HttpClient())
            {
                if (string.Equals(servicio, "2Captcha", StringComparison.OrdinalIgnoreCase))
                {
                    string url = $"http://2captcha.com/res.php?key={apiKey}&action=getbalance&json=1";
                    string response = await httpClient.GetStringAsync(url);

                    var jsonResponse = JObject.Parse(response);
                    int errorId = jsonResponse["status"]?.Value<int>() ?? -1;
                    if (errorId == 1)
                    {
                        double balance = jsonResponse["request"]?.Value<double>() ?? 0.0;
                        return balance.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        throw new Exception($"Error 2Captcha: {response}");
                    }
                }
                else if (string.Equals(servicio, "CapSolver", StringComparison.OrdinalIgnoreCase))
                {
                    string url = "https://api.capsolver.com/getBalance";
                    var requestBody = new { clientKey = apiKey };
                    string jsonRequest = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, content);
                    string responseText = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JObject.Parse(responseText);

                    int errorId = jsonResponse["errorId"]?.Value<int>() ?? -1;
                    if (errorId == 0)
                    {
                        double balance = jsonResponse["balance"]?.Value<double>() ?? 0.0;
                        return balance.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string error = jsonResponse["errorDescription"]?.ToString() ?? "Error desconocido";
                        throw new Exception($"Error CapSolver: {error}");
                    }
                }
                else
                {
                    throw new ArgumentException($"Servicio '{servicio}' no soportado.");
                }
            }
        }

        private void btnAbrirLog_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG");
                string logPath = Path.Combine(logDir, "procesoDescarga.log");

                if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
                {
                    string notepadppPath = ConfigurationManager.AppSettings["RutaDirNP"]; // ruta seleccionada con el OpenFileDialog

                    if (!string.IsNullOrEmpty(notepadppPath) && File.Exists(notepadppPath))
                    {
                        // 🔹 Abrir con Notepad++
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = notepadppPath,
                            Arguments = "\"" + logPath + "\"",
                            UseShellExecute = false
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    else
                    {
                        // 🔹 Si no se configuró Notepad++ o no existe, abrir con app predeterminada
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = logPath,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                }
                else
                {
                    MessageBox.Show("El log aún no ha sido generado.", "Abrir Log",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el log: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReporte_Click(object sender, EventArgs e)
        {
            try
            {
                // Ruta donde se guardará el reporte
                string rutaReporte = Path.Combine(
                    Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_REPORTES"),
                    $"Reporte_BOT_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                );

                GenerarReportePDF(rutaReporte);

                MessageBox.Show($"Reporte generado con éxito:\n{rutaReporte}",
                    "Reporte BOT", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(rutaReporte);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarReportePDF(string ruta)
        {
            Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(doc, new FileStream(ruta, FileMode.Create));
            doc.Open();

            // Encabezado
            var titulo = new Paragraph("📊 Reporte Final BOT")
            {
                Alignment = Element.ALIGN_CENTER
            };

            doc.Add(titulo);
            doc.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
            doc.Add(new Paragraph(" "));

            // Tabla resumen desde DataGridView
            PdfPTable tabla = new PdfPTable(grdDescarga.Columns.Count);
            tabla.WidthPercentage = 100;

            // Encabezados
            foreach (DataGridViewColumn col in grdDescarga.Columns)
            {
                if (!col.Visible) continue;
                tabla.AddCell(new PdfPCell(new Phrase(col.HeaderText))
                { BackgroundColor = BaseColor.LIGHT_GRAY });
            }

            // Filas
            foreach (DataGridViewRow row in grdDescarga.Rows)
            {
                if (row.IsNewRow) continue;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (!cell.OwningColumn.Visible) continue;

                    string valor = cell.Value?.ToString() ?? "";
                    BaseColor bgColor = BaseColor.WHITE;

                    if (cell.OwningColumn.Name == "Procesado")
                    {
                        if (valor == "OK" || valor == "REVISAR") bgColor = BaseColor.GREEN;
                        else if (valor == "FALLIDO") bgColor = BaseColor.RED;
                    }

                    tabla.AddCell(new PdfPCell(new Phrase(valor))
                    {
                        BackgroundColor = bgColor,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                }
            }

            doc.Add(tabla);
            doc.Close();
        }

        private void CrearAnimacion()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 30;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (abriendo)
            {
                // Animar apertura
                if (tblLayout.ColumnStyles[0].Width > 0)
                {
                    tblLayout.ColumnStyles[0].Width -= step;
                    tblLayout.ColumnStyles[2].Width -= step;
                    tblLayout.ColumnStyles[1].Width += step * 2;
                }
                else
                {
                    timer.Stop();
                }
            }
            else
            {
                // Animar cierre
                if (tblLayout.ColumnStyles[0].Width < 50)
                {
                    tblLayout.ColumnStyles[0].Width += step;
                    tblLayout.ColumnStyles[2].Width += step;
                    tblLayout.ColumnStyles[1].Width -= step * 2;
                }
                else
                {
                    timer.Stop();
                }
            }
        }

        private void ActualizarColorFilaCliente(string usuario, bool exitoso)
        {
            if (grdDescarga == null || grdDescarga.IsDisposed) return;

            if (grdDescarga.InvokeRequired)
            {
                try
                {
                    grdDescarga.BeginInvoke(new Action(() =>
                        ActualizarColorFilaCliente(usuario, exitoso)));
                }
                catch { /* Si el form ya se cerró, ignorar */ }
                return;
            }

            try
            {
                foreach (DataGridViewRow row in grdDescarga.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (row.DataBoundItem is DataRowView dataRowView &&
                        (dataRowView.Row.RowState == DataRowState.Deleted ||
                         dataRowView.Row.RowState == DataRowState.Detached))
                    {
                        continue;
                    }
                    var cellUsuario = row.Cells["usuario"];
                    if (cellUsuario?.Value == null) continue;

                    if (cellUsuario.Value.ToString() == usuario)
                    {
                        // Colores suaves
                        Color color = exitoso ? Color.FromArgb(0, 235, 92) : Color.FromArgb(255, 59, 59);

                        // Pintar todas las celdas de la fila
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.BackColor = color;
                        }

                        // NO modificar el Value de la celda vinculada, solo el estilo
                        if (grdDescarga.Columns.Contains("Procesado"))
                        {
                            var cellProcesado = row.Cells["Procesado"];

                            // Solo modificar estilos, NO el Value
                            cellProcesado.Style.ForeColor = exitoso ? Color.Green : Color.Red;
                            cellProcesado.Style.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);

                            // Si quieres mostrar texto, hazlo SIN modificar el DataTable
                            // La alternativa es modificar directamente el DataTable de forma thread-safe
                        }

                        grdDescarga.Refresh();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(_logPath, $"⚠️ Error al actualizar color de fila: {ex.Message}");
            }
        }

        private (string Texto, Color ColorFondo) CalcularEstadoCliente(IReadOnlyCollection<EstadoResultado> estados)
        {
            if (estados == null || estados.Count == 0)
            {
                return ("FALLIDO", Color.Red);
            }

            bool tieneExitoso = estados.Any(estado => estado == EstadoResultado.Exitoso || estado == EstadoResultado.SinDatos);
            bool tieneFallido = estados.Any(estado => estado == EstadoResultado.Fallido);

            if (!tieneExitoso && tieneFallido)
            {
                return ("FALLIDO", Color.Red);
            }

            if (tieneFallido)
            {
                return ("REVISAR", Color.Green);
            }

            return ("OK", Color.Green);
        }

        private void ActualizarEstadoCliente(string usuario, string estadoTexto, Color colorFondo)
        {
            if (grdDescarga == null || grdDescarga.IsDisposed) return;

            if (grdDescarga.InvokeRequired)
            {
                try
                {
                    grdDescarga.BeginInvoke(new Action(() =>
                        ActualizarEstadoCliente(usuario, estadoTexto, colorFondo)));
                }
                catch
                {
                    // Si el form ya se cerró, ignorar.
                }
                return;
            }

            try
            {
                foreach (DataGridViewRow row in grdDescarga.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (row.DataBoundItem is DataRowView dataRowView &&
                        (dataRowView.Row.RowState == DataRowState.Deleted ||
                         dataRowView.Row.RowState == DataRowState.Detached))
                    {
                        continue;
                    }
                    var cellUsuario = row.Cells["usuario"];
                    if (cellUsuario?.Value == null) continue;

                    if (cellUsuario.Value.ToString() == usuario)
                    {
                        if (!grdDescarga.Columns.Contains("Procesado"))
                        {
                            break;
                        }

                        var cellProcesado = row.Cells["Procesado"];
                        if (cellProcesado == null)
                        {
                            break;
                        }
                        cellProcesado.Value = estadoTexto;
                        cellProcesado.Style.BackColor = colorFondo;
                        cellProcesado.Style.ForeColor = Color.White;
                        cellProcesado.Style.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
                        cellProcesado.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                        grdDescarga.Refresh();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(_logPath, $"⚠️ Error al actualizar estado de fila: {ex.Message}");
            }
        }

    }

    public class DescargaDetalle
    {
        public string Cliente { get; set; }
        public string TipoDocumento { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }
        public bool Exitoso { get; set; }
        public string Error { get; set; }
    }

    public class ResumenCliente
    {
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public List<DescargaDetalle> Exitosas { get; set; }
        public List<DescargaDetalle> Fallidas { get; set; }
        public List<KeyValuePair<int, string>> TiposFiltrados { get; set; }
        public List<(int Mes, int Año)> MesesConsulta { get; set; }
    }
}