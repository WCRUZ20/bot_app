using botapp.Core;
using botapp.Helpers;
using botapp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
using System.IO;
using System.Linq;
using System.Net.Http;
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

                    if (grdDescarga.Columns.Contains("Procesado"))
                    {
                        grdDescarga.Columns["Procesado"].ReadOnly = true;
                        grdDescarga.Columns["Procesado"].HeaderText = "Procesado";
                        grdDescarga.Columns["Procesado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

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

                    if (!dt.Columns.Contains("Procesado"))
                    {
                        DataColumn col = new DataColumn("Procesado", typeof(string));
                        dt.Columns.Add(col);
                        //col.SetOrdinal(0); // mueve la columna al índice 0
                    }

                    foreach (DataRow row in dt.Rows)
                        row["Procesado"] = String.Empty;

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
            reintentos = int.Parse(dudReintentos.Text);
            //txtDialogbot.Visible = true;
            tblLayout.Visible = true;
            pictureBox1.Image = Properties.Resources.robot_move;
            tblLayout.ColumnStyles[0].Width = 50;
            tblLayout.ColumnStyles[1].Width = 0;
            tblLayout.ColumnStyles[2].Width = 50;

            abriendo = true;
            timer.Start();

            if (!_procesoEnEjecucion)
            {
                _procesoEnEjecucion = true;
                btnDescarga.BackColor = Color.DarkRed;
                btnDescarga.ForeColor = Color.White;
                btnDescarga.FlatAppearance.MouseOverBackColor = Color.Red;
                btnDescarga.Text = "⬜ DETENER";

                _cts = new CancellationTokenSource();

                try
                {
                    var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                    writepermission = await helper.TienePermisoEscrituraAsync(_usuarioActual);

                    if (ConfigurationManager.AppSettings["2captcha"] == "true")
                        sol_provider = "2Captcha";
                    else if (ConfigurationManager.AppSettings["Capsolver"] == "true")
                        sol_provider = "Capsolver";
                    else if (ConfigurationManager.AppSettings["HumanInteraction"] == "true")
                        sol_provider = "HumanInter";

                    last_id_trans = await helper.GetMaxIdTransactionAsync();
                    last_id_trans = last_id_trans + 1;
                    _orden = 0;

                    decimal numAdvertencia = 0.50M;
                    decimal numBloqueo = 0.10M;
                    try
                    {
                        numAdvertencia = decimal.Parse(ConfigurationManager.AppSettings["numAdvertencia"]);
                        numBloqueo = decimal.Parse(ConfigurationManager.AppSettings["numBloqueo"]);
                    }
                    catch
                    {
                        // si hay error, se mantienen valores por defecto
                    }

                    if (sol_provider != "HumanInter")
                    {
                        try
                        {
                            string _saldoStr = await ConsultarSaldoAsync(sol_provider, apiKeySolver);
                            double _saldo = double.Parse(_saldoStr, System.Globalization.CultureInfo.InvariantCulture);

                            if (_saldo < (double)numBloqueo)
                            {
                                MessageBox.Show(
                                    $"Su saldo es muy bajo ({_saldoStr}).\nDebe recargar o cambiar de proveedor de resolución.",
                                    "Saldo insuficiente",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }
                            else if (_saldo < (double)numAdvertencia)
                            {
                                var result = MessageBox.Show(
                                    $"Su saldo es bajo ({_saldoStr}).\n¿Desea continuar con la descarga?",
                                    "Advertencia de saldo bajo",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.No)
                                    return;
                            }

                            await EjecutarProcesoAsync(_cts.Token);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al consultar saldo: {ex.Message}",
                                            "Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        await EjecutarProcesoAsync(_cts.Token);
                    }
                }
                finally
                {
                    _procesoEnEjecucion = false;
                    btnDescarga.Text = "DESCARGAR 🔽";
                    btnDescarga.BackColor = Color.FromArgb(34, 197, 94);
                    btnDescarga.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 163, 74);
                    btnDescarga.Enabled = true;
                    btnDescarga.ForeColor = Color.White;
                }
            }
            else
            {
                _cts?.Cancel();
                btnDescarga.Enabled = false;
                btnDescarga.BackColor = Color.DarkRed;
                btnDescarga.ForeColor = Color.White;
                btnDescarga.Text = "CANCELANDO...";
            }

            pictureBox1.Image = Properties.Resources.robot_no_move;
            tblLayout.ColumnStyles[0].Width = 0;
            tblLayout.ColumnStyles[1].Width = 100;
            tblLayout.ColumnStyles[2].Width = 0;

            abriendo = false;
            timer.Start();

            tblLayout.Visible = false;
            //txtDialogbot.Visible = false;
        }

        // agregar si no está:
        // using System.Collections.Concurrent;

        private async Task EjecutarProcesoAsync(CancellationToken token)
        {
            resumenProceso = new ResumenProceso { InicioEjecucion = DateTime.Now };

            var config = ConfigManager.CargarConfiguracion();
            string sriUrl = config.SriUrl;
            bool headless = config.Headless;

            // LOG global
            string logGlobalPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoDescarga.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logGlobalPath));
            LoggerHelper.Log(logGlobalPath, "🟢 Inicio de ejecución BOT (multi-hilo)");
            _logPath = logGlobalPath;

            DateTime horaInicio = DateTime.Now;
            lblHIni2.Text = horaInicio.ToString("HH:mm:ss");

            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
            var dtClientes = await helper.GetClientesBotActivosDescAsync();
            // Obtener clientes (mismo bloque que ya tienes)
            List<ClienteBot> clientes = new List<ClienteBot>();
            try
            {
                if (dtClientes != null)
                {
                    foreach (DataRow row in dtClientes.Rows)
                    {
                        clientes.Add(new ClienteBot
                        {
                            orden = row["orden"].ToString(),
                            Identificacion = row["usuario"].ToString(),
                            Nombre = row["NombUsuario"].ToString(),
                            Adicional = row["ci_adicional"].ToString(),
                            Clave = row["clave"].ToString(),
                            Dias = row["Dias"].ToString(),
                            MesesAnte = row["meses_ante"].ToString(),
                            ClaveEdoc = row["clave_ws"].ToString(),
                            Factura = row.Table.Columns.Contains("factura") && row["factura"].ToString() == "Y",
                            NotaCredito = row.Table.Columns.Contains("notacredito") && row["notacredito"].ToString() == "Y",
                            Retencion = row.Table.Columns.Contains("retencion") && row["retencion"].ToString() == "Y",
                            LiquidacionCompra = row.Table.Columns.Contains("liquidacioncompra") && row["liquidacioncompra"].ToString() == "Y",
                            NotaDebito = row.Table.Columns.Contains("notadebito") && row["notadebito"].ToString() == "Y",
                            ConsultarMesActual = row.Table.Columns.Contains("consultar_mes_actual") &&
                                (row["consultar_mes_actual"].ToString() == "Y" ||
                                 row["consultar_mes_actual"].ToString() == "True" ||
                                 row["consultar_mes_actual"].ToString() == "1")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(logGlobalPath, $"❌ Error al obtener clientes: {ex.Message}");
                return;
            }

            resumenProceso.TotalClientes = clientes.Count;
            if (clientes.Count == 0)
            {
                LoggerHelper.Log(logGlobalPath, "⚠️ No se encontraron clientes activos.");
                return;
            }

            // Estado thread-safe por cliente
            var estadoPorCliente = new ConcurrentDictionary<string, bool>(clientes.ToDictionary(c => c.Identificacion, c => false));

            // Número de hilos desde UI (nudHilos)
            int numHilos = 1;
            try { numHilos = Math.Max(1, (int)nudHilos.Value); } catch { numHilos = 1; }

            // Repartir clientes por hilo (balance simple)
            var grupos = clientes
                .Select((c, i) => new { Cliente = c, Index = i })
                .GroupBy(x => x.Index % numHilos)
                .Select(g => g.Select(x => x.Cliente).ToList())
                .ToList();

            // Asignar número de hilo a cada cliente
            for (int i = 0; i < grupos.Count; i++)
            {
                int hilo = i + 1;
                foreach (var cliente in grupos[i])
                {
                    cliente.HiloAsignado = hilo;
                }
            }

            // Asegurarnos de que dtClientes tenga columna "Hilo"
            if (dtClientes != null)
            {
                if (!dtClientes.Columns.Contains("Hilo"))
                {
                    dtClientes.Columns.Add("Hilo", typeof(int));
                    foreach (DataRow r in dtClientes.Rows) r["Hilo"] = 0;
                }

                // Actualizar valores en dtClientes (por si alguien usa ese DataTable)
                foreach (var cliente in clientes)
                {
                    // escape de comillas simples por seguridad al usar Select
                    string safeId = cliente.Identificacion.Replace("'", "''");
                    var filas = dtClientes.Select($"usuario = '{safeId}'");
                    foreach (var r in filas)
                    {
                        r["Hilo"] = cliente.HiloAsignado;
                    }
                }
            }

            // Actualizar la columna Hilo en la grilla (si ya está poblada)
            // Usamos Invoke para ejecutar en hilo UI
            if (grdDescarga != null && grdDescarga.Columns.Contains("Hilo"))
            {
                try
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            foreach (DataGridViewRow row in grdDescarga.Rows)
                            {
                                if (row.IsNewRow) continue;
                                var cellUsuario = row.Cells["usuario"];
                                if (cellUsuario?.Value == null) continue;
                                string id = cellUsuario.Value.ToString();
                                var cliente = clientes.FirstOrDefault(c => c.Identificacion == id);
                                if (cliente != null)
                                {
                                    row.Cells["Hilo"].Value = cliente.HiloAsignado;
                                }
                            }
                            grdDescarga.Refresh();
                        });
                    }
                    else
                    {
                        foreach (DataGridViewRow row in grdDescarga.Rows)
                        {
                            if (row.IsNewRow) continue;
                            var cellUsuario = row.Cells["usuario"];
                            if (cellUsuario?.Value == null) continue;
                            string id = cellUsuario.Value.ToString();
                            var cliente = clientes.FirstOrDefault(c => c.Identificacion == id);
                            if (cliente != null)
                            {
                                row.Cells["Hilo"].Value = cliente.HiloAsignado;
                            }
                        }
                        grdDescarga.Refresh();
                    }
                }
                catch { /* si algo falla no queremos abortar todo - puedes loguearlo */ }
            }

            var tareas = new List<Task>();

            for (int i = 0; i < grupos.Count; i++)
            {
                int hilo = i + 1;
                string logHiloPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), $"hilo_{hilo}.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logHiloPath));
                // marca inicial en log del hilo
                File.AppendAllText(logHiloPath, $"{DateTime.Now:s} Inicio hilo {hilo}{Environment.NewLine}");

                var clientesGrupo = grupos[i];

                tareas.Add(Task.Run(async () =>
                {
                    foreach (var cliente in clientesGrupo.OrderBy(c => int.Parse(c.orden)))
                    {
                        if (token.IsCancellationRequested)
                            return;

                        // 🔒 Bloquea el cliente desde el inicio
                        if (!estadoPorCliente.TryUpdate(cliente.Identificacion, false, false))
                            continue;

                        bool procesado = false;

                        for (int intento = 1; intento <= reintentos + 1 && !procesado; intento++)
                        {
                            LoggerHelper.Log(logGlobalPath,
                                $"[Hilo {hilo}] [{cliente.Nombre}] Intento {intento}");

                            try
                            {
                                using (var playwright = await Playwright.CreateAsync())
                                {
                                    var browser = await playwright.Chromium.LaunchAsync(
                                        new BrowserTypeLaunchOptions { Headless = headless });

                                    var context = await browser.NewContextAsync(
                                        new BrowserNewContextOptions { AcceptDownloads = true });

                                    var page = await context.NewPageAsync();

                                    try
                                    {
                                        bool loginOk = await IniciarSesionSRI(
                                            page, sriUrl,
                                            cliente.Identificacion,
                                            cliente.Nombre,
                                            cliente.Adicional,
                                            cliente.Clave,
                                            logHiloPath);

                                        if (!loginOk)
                                            throw new Exception("Login fallido");

                                        procesado = await DescargarComprobantesConSeguimiento(
                                            page,
                                            cliente.Identificacion,
                                            cliente.Nombre,
                                            int.Parse(cliente.Dias),
                                            int.Parse(cliente.MesesAnte),
                                            cliente.Factura,
                                            cliente.LiquidacionCompra,
                                            cliente.NotaCredito,
                                            cliente.NotaDebito,
                                            cliente.Retencion,
                                            cliente.ConsultarMesActual,
                                            token,
                                            logHiloPath);
                                    }
                                    finally
                                    {
                                        try { await page.CloseAsync(); } catch { }
                                        try { await context.CloseAsync(); } catch { }
                                        try { await browser.CloseAsync(); } catch { }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Log(logGlobalPath,
                                    $"[Hilo {hilo}] [{cliente.Nombre}] Error intento {intento}: {ex.Message}");
                            }
                        }

                        // ✅ Marcar resultado FINAL
                        estadoPorCliente[cliente.Identificacion] = procesado;
                        ActualizarColorFilaCliente(cliente.Identificacion, procesado);
                    }
                }, token));

            }

            // esperar todas las tareas
            await Task.WhenAll(tareas);

            LoggerHelper.Log(logGlobalPath, "✅ BOT finalizado con éxito (multi-hilo)");
            DateTime horaFin = DateTime.Now;
            lblHFin2.Text = horaFin.ToString("HH:mm:ss");
            MostrarSaldoCaptcha();
        }

        private async Task<bool> DescargarComprobantesConSeguimiento(
            IPage page,
            string usuario,
            string nombre,
            int dias,
            int meses_ante,
            bool factura,
            bool liquidacionCompra,
            bool notaCredito,
            bool notaDebito,
            bool retencion,
            bool consultarMesActual,
            CancellationToken token,
            string pathHilo,
            //IBrowserContext context, 
            //IBrowser browser, 
            //string ci, 
            //string clave_,
            int msEsperaEntreMeses = 5000,
            int msEsperaEntreTipos = 5000)
        {
            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
            LoggerHelper.Log(pathHilo, $"[{nombre}] Iniciando descarga de comprobantes...");

            var tipos = new Dictionary<int, string>
            {
                { 1, "Factura" },
                { 2, "LiquidacionCompra"},
                { 3, "NotaCredito" },
                { 4, "NotaDebito"},
                { 6, "Retencion" }
            };

            // 🔹 Filtrar según flags
            var tiposFiltrados = new List<KeyValuePair<int, string>>();
            if (factura) tiposFiltrados.Add(new KeyValuePair<int, string>(1, "Factura"));
            if (liquidacionCompra) tiposFiltrados.Add(new KeyValuePair<int, string>(2, "LiquidacionCompra"));
            if (notaCredito) tiposFiltrados.Add(new KeyValuePair<int, string>(3, "NotaCredito"));
            if (notaDebito) tiposFiltrados.Add(new KeyValuePair<int, string>(4, "NotaDebito"));
            if (retencion) tiposFiltrados.Add(new KeyValuePair<int, string>(6, "Retencion"));

            if (!tiposFiltrados.Any())
            {
                LoggerHelper.Log(pathHilo, $"⚠️ [{nombre}] No tiene tipos de comprobantes habilitados para descargar.");
                return false;
            }

            DateTime hoy = DateTime.Today;
            int diaActual = hoy.Day;

            // 🔹 Generar lista de meses dinámicamente
            var mesesConsulta = new List<(int Mes, int Año)>();

            if (consultarMesActual)
            {
                // Incluye el mes actual (i = 0)
                for (int i = 0; i <= meses_ante; i++)
                {
                    var fecha = hoy.AddMonths(-i);
                    mesesConsulta.Add((fecha.Month, fecha.Year));
                }
                LoggerHelper.Log(pathHilo, $"[{nombre}] 📅 Consultando mes actual + {meses_ante} meses anteriores");
            }
            else
            {
                // Solo meses anteriores (i = 1)
                for (int i = 1; i <= meses_ante; i++)
                {
                    var fecha = hoy.AddMonths(-i);
                    mesesConsulta.Add((fecha.Month, fecha.Year));
                }
                LoggerHelper.Log(pathHilo, $"[{nombre}] 📅 Consultando solo {meses_ante} meses anteriores (sin mes actual)");
            }

            bool algunDescargaExitosa = false;

            // 🔹 NUEVO: Diccionario para rastrear descargas pendientes
            var descargasPendientes = new Dictionary<string, DescargaDetalle>();
            var descargasExitosas = new List<DescargaDetalle>();
            var descargasFallidas = new List<DescargaDetalle>();

            // 🔹 Primera pasada: intentar descargar todo
            for (int t = 0; t < tiposFiltrados.Count; t++)
            {
                if (token.IsCancellationRequested) break;
                var tipo = tiposFiltrados[t];

                for (int m = 0; m < mesesConsulta.Count; m++)
                {
                    if (token.IsCancellationRequested) break;
                    int mes = mesesConsulta[m].Mes;
                    int anio = mesesConsulta[m].Año;
                    string clave = $"{tipo.Value}_{mes}_{anio}";

                    LoggerHelper.Log(pathHilo, $"[{nombre}] Consultando {tipo.Value} - {mes}/{anio}");
                    //txtDialogbot.Text = $"[{nombre}] Consultando {tipo.Value} - {mes}/{anio}";

                    var resultado = await IntentarDescargarDocumento(
                        page, helper, usuario, nombre, tipo, mes, anio,
                        msEsperaEntreMeses, token, pathHilo);
                    

                    if (resultado.Exitoso)
                    {
                        algunDescargaExitosa = true;
                        descargasExitosas.Add(resultado);
                    }
                    else if (!resultado.Error.Contains("No existen datos"))
                    {
                        // Solo agregar a pendientes si NO es "No existen datos"
                        descargasPendientes[clave] = resultado;
                        descargasFallidas.Add(resultado);
                    }
                    else
                    {
                        // Si es "No existen datos", registrar como fallida pero no reintentar
                        descargasFallidas.Add(resultado);
                    }

                    if (dias > 0 && diaActual > dias)
                    {
                        LoggerHelper.Log(pathHilo, $"[{nombre}] 🔁 Detenido por configuración de días límite ({dias})");
                        break;
                    }

                    // Pausa entre meses
                    if (m < mesesConsulta.Count - 1)
                    {
                        await page.ReloadAsync();
                        await EsperarAsync(msEsperaEntreMeses,
                            $"[{nombre}] ⏳ Esperando {msEsperaEntreMeses / 1000}s antes de pasar al siguiente mes…", pathHilo);
                    }
                }

                // Pausa entre tipos de documentos
                if (t < tiposFiltrados.Count - 1)
                {
                    await page.ReloadAsync();
                    await EsperarAsync(msEsperaEntreTipos,
                        $"[{nombre}] ⏳ Esperando {msEsperaEntreTipos / 1000}s antes de pasar al siguiente documento ({tiposFiltrados[t + 1].Value})…", pathHilo);
                }
            }

            // 🔹 NUEVO: Reintentos para documentos pendientes
            if (descargasPendientes.Any() && !token.IsCancellationRequested)
            {
                LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Reintentando {descargasPendientes.Count} documentos pendientes...");
                
                var clavesPendientes = descargasPendientes.Keys.ToList();
                foreach (var clave in clavesPendientes)
                {
                    if (token.IsCancellationRequested) break;

                    var detalle = descargasPendientes[clave];
                    var tipoKey = tipos.First(x => x.Value == detalle.TipoDocumento);

                    LoggerHelper.Log(pathHilo,
                        $"[{nombre}] 🔄 Reintentando {detalle.TipoDocumento} - {detalle.Mes}/{detalle.Año}");

                    var resultado = await IntentarDescargarDocumento(
                        page, helper, usuario, nombre,
                        new KeyValuePair<int, string>(tipoKey.Key, tipoKey.Value),
                        detalle.Mes, detalle.Año, msEsperaEntreMeses, token, pathHilo);
                    

                    if (resultado.Exitoso)
                    {
                        algunDescargaExitosa = true;
                        descargasExitosas.Add(resultado);
                        descargasPendientes.Remove(clave);

                        // Actualizar lista de fallidas
                        var fallidaOriginal = descargasFallidas.FirstOrDefault(
                            x => x.TipoDocumento == detalle.TipoDocumento &&
                                 x.Mes == detalle.Mes &&
                                 x.Año == detalle.Año);
                        if (fallidaOriginal != null)
                            descargasFallidas.Remove(fallidaOriginal);
                    }

                    await page.ReloadAsync();
                    await EsperarAsync(msEsperaEntreMeses,
                        $"[{nombre}] ⏳ Esperando {msEsperaEntreMeses / 1000}s antes del siguiente reintento…", pathHilo);
                }
            }

            // 🔹 NUEVO: Generar resumen para este cliente
            GenerarResumenCliente(usuario, nombre, descargasExitosas, descargasFallidas, tiposFiltrados, mesesConsulta, pathHilo);

            return algunDescargaExitosa;
        }

        private async Task<DescargaDetalle> IntentarDescargarDocumentoConSesionNuevaAsync(
    ClienteBot cliente,
    SupabaseDbHelper helper,
    string sriUrl,
    bool headless,
    KeyValuePair<int, string> tipo,
    int mes,
    int anio,
    CancellationToken token,
    string pathHilo)
{
    var detalle = new DescargaDetalle
    {
        Cliente = cliente.Nombre,
        TipoDocumento = tipo.Value,
        Mes = mes,
        Año = anio,
        Exitoso = false
    };

    if (token.IsCancellationRequested)
    {
        detalle.Error = "Cancelado por el usuario";
        return detalle;
    }

    using (var playwright = await Playwright.CreateAsync())
    {
        var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = headless });

        var context = await browser.NewContextAsync(
            new BrowserNewContextOptions { AcceptDownloads = true });

        var page = await context.NewPageAsync();

        try
        {
            // 1) LOGIN (ya lo tienes)
            bool loginOk = await IniciarSesionSRI(
                page,
                sriUrl,
                cliente.Identificacion,
                cliente.Nombre,
                cliente.Adicional,
                cliente.Clave,
                pathHilo);

            if (!loginOk)
            {
                detalle.Error = "Login fallido";
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ❌ {detalle.Error}");
                return detalle;
            }

            // 2) Set combos
            try
            {
                await page.SelectOptionAsync("#frmPrincipal\\:ano", anio.ToString());
                await page.SelectOptionAsync("#frmPrincipal\\:mes", mes.ToString());
                await page.SelectOptionAsync("#frmPrincipal\\:dia", "0");
                await page.SelectOptionAsync("#frmPrincipal\\:cmbTipoComprobante", tipo.Key.ToString());
            }
            catch (Exception e)
            {
                detalle.Error = $"Error al seleccionar filtros: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ⚠ {detalle.Error}");
                return detalle;
            }

            // 3) CONSULTAR + forzado (misma idea que Principal)
            //    En tu interface el selector del botón lo sacas de app.config (NombreBotonSRI)
            var botonConsultarSRI = ConfigurationManager.AppSettings["NombreBotonSRI"]?.ToString() ?? "#frmPrincipal\\:btnBuscar";

            // Aquí reusamos tu flujo de solver, pero el CLICK lo hacemos “blindado”
            // y con recuperación de "Captcha incorrecta" como en Principal.cs.
            bool ok = await ConsultarConRetoAsync(page, cliente.Nombre, 120000, humanInteraction, pathHilo, mes, anio, tipo.Key.ToString());
            if (!ok)
            {
                // igual que en tu código: no siempre retornas; pero aquí es mejor marcar error
                detalle.Error = "La consulta no devolvió resultados (timeout)";
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ⚠ {detalle.Error}");
            }

            // 4) “No existen datos” => NO reintentar
            try
            {
                var mensaje = await page.QuerySelectorAsync("#formMessages\\:messages");
                if (mensaje != null)
                {
                    var texto = await mensaje.InnerTextAsync();
                    if (texto.Contains("No existen datos"))
                    {
                        detalle.Error = "No existen datos para los parámetros ingresados";
                        LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ⚠ No existen datos para {mes}/{anio} - {tipo.Value}");
                        return detalle;
                    }
                }
            }
            catch { /* no romper */ }

            // 5) Esperar tabla (si no, igual intentamos descarga si el link aparece)
            try
            {
                var waitTable = new PageWaitForSelectorOptions { Timeout = 30000, State = WaitForSelectorState.Visible };
                await page.WaitForSelectorAsync("#frmPrincipal\\:tablaCompRecibidos", waitTable);
            }
            catch (Exception e)
            {
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ⚠ No se encontró la tabla: {e.Message}");
            }

            // 6) Descargar
            try
            {
                var waitLink = new PageWaitForSelectorOptions { Timeout = 20000, State = WaitForSelectorState.Visible };
                await page.WaitForSelectorAsync("#frmPrincipal\\:lnkTxtlistado", waitLink);

                IDownload download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    await page.ClickAsync("#frmPrincipal\\:lnkTxtlistado");
                });

                string carpeta = $"{cliente.Identificacion} - {cliente.Nombre}";
                string path = Helpers.Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                string filePath = Path.Combine(path, $"{cliente.Identificacion}_{tipo.Value}_{mes}_{anio}.txt");

                await download.SaveAsAsync(filePath);

                detalle.Exitoso = true;
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ✅ Archivo descargado: {Path.GetFileName(filePath)}");
                return detalle;
            }
            catch (Exception e)
            {
                detalle.Error = $"Error en descarga: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{cliente.Nombre}] ⚠ {detalle.Error}");
                return detalle;
            }
        }
        finally
        {
            // 7) Cerrar sesión + cerrar todo (tipo Principal)
            try { await CerrarSesion(page); } catch { } // ya tienes CerrarSesion(page) :contentReference[oaicite:4]{index=4}
            try { await page.CloseAsync(); } catch { }
            try { await context.CloseAsync(); } catch { }
            try { await browser.CloseAsync(); } catch { }
        }
    }
}

        private async Task<bool> EjecutarConSesionNuevaAsync(ClienteBot cliente, Func<IPage, Task<bool>> accion, bool headless, string sriUrl, string logHiloPath)
        {
            using (var playwright = await Playwright.CreateAsync())
            {
                var browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless });

                var context = await browser.NewContextAsync(
                    new BrowserNewContextOptions { AcceptDownloads = true });

                var page = await context.NewPageAsync();

                try
                {
                    bool loginOk = await IniciarSesionSRI(
                        page,
                        sriUrl,
                        cliente.Identificacion,
                        cliente.Nombre,
                        cliente.Adicional,
                        cliente.Clave,
                        logHiloPath);

                    if (!loginOk)
                        throw new Exception("Login fallido");

                    return await accion(page);
                }
                finally
                {
                    try { await page.CloseAsync(); } catch { }
                    try { await context.CloseAsync(); } catch { }
                    try { await browser.CloseAsync(); } catch { }
                }
            }
        }


        // 🔹 NUEVO: Método auxiliar para intentar descargar un documento
        private async Task<DescargaDetalle> IntentarDescargarDocumento(
            IPage page,
            SupabaseDbHelper helper,
            string usuario,
            string nombre,
            KeyValuePair<int, string> tipo,
            int mes,
            int anio,
            int msEspera,
            CancellationToken token,
            string pathHilo//,
            //IBrowserContext context, 
            //IBrowser browser, 
            //string ci, 
            //string clave
        )
        {
            var detalle = new DescargaDetalle
            {
                Cliente = nombre,
                TipoDocumento = tipo.Value,
                Mes = mes,
                Año = anio,
                Exitoso = false
            };
            await page.WaitForTimeoutAsync(2000);
            try
            {
                await page.SelectOptionAsync("#frmPrincipal\\:ano", anio.ToString());
                //await page.WaitForTimeoutAsync(2000);
                await page.SelectOptionAsync("#frmPrincipal\\:mes", mes.ToString());
                //await page.WaitForTimeoutAsync(2000);
                await page.SelectOptionAsync("#frmPrincipal\\:dia", "0");
                //await page.WaitForTimeoutAsync(2000);
                await page.SelectOptionAsync("#frmPrincipal\\:cmbTipoComprobante", tipo.Key.ToString());
            }
            catch (Exception e)
            {
                detalle.Error = $"Error al seleccionar filtros: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ {detalle.Error}");
                return detalle;
            }

            try
            {
                await page.WaitForTimeoutAsync(5000);
                var ok = await ConsultarConRetoAsync(page, nombre, 120000, humanInteraction, pathHilo, mes, anio, tipo.Key.ToString()/*, context, browser, usuario, ci, clave*/);
                //bool ok = await ConsultarDirectoSinBotoAsync(page, nombre, 120000, pathHilo, mes, anio, tipo.Key.ToString());
                await page.WaitForTimeoutAsync(20000);
                if (!ok)
                {
                    detalle.Error = "La consulta no devolvió resultados (timeout)";
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ {detalle.Error}");
                    //return detalle;
                }
            }
            catch (Exception e)
            {
                detalle.Error = $"Error ejecutando la consulta: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ {detalle.Error}");
                return detalle;
            }

            try
            {
                var mensaje = await page.QuerySelectorAsync("#formMessages\\:messages");
                if (mensaje != null)
                {
                    var texto = await mensaje.InnerTextAsync();
                    if (texto.Contains("No existen datos"))
                    {
                        detalle.Error = "No existen datos para los parámetros ingresados";
                        LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ No existen datos para {mes}/{anio} - {tipo.Value}");
                        return detalle; 
                    }
                }

                var waitTable = new PageWaitForSelectorOptions { Timeout = 30000, State = WaitForSelectorState.Visible };
                await page.WaitForSelectorAsync("#frmPrincipal\\:tablaCompRecibidos", waitTable);
            }
            catch (Exception e)
            {
                detalle.Error = $"No se encontró la tabla de comprobantes: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ Error inesperado buscando comprobantes: {e.Message}");
                //return detalle; //comentado para hacer la descarga de todos modos
            }

            try
            {
                var waitLink = new PageWaitForSelectorOptions { Timeout = 20000, State = WaitForSelectorState.Visible };
                await page.WaitForSelectorAsync("#frmPrincipal\\:lnkTxtlistado", waitLink);

                IDownload download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    await page.ClickAsync("#frmPrincipal\\:lnkTxtlistado");
                });

                string carpeta = $"{usuario} - {nombre}";
                string path = Helpers.Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                string filePath = Path.Combine(path, $"{usuario}_{tipo.Value}_{mes}_{anio}.txt");
                await download.SaveAsAsync(filePath);

                detalle.Exitoso = true;
                LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Archivo descargado: {Path.GetFileName(filePath)}");
                //txtDialogbot.Text = $"[{nombre}] ✅ Archivo descargado: {Path.GetFileName(filePath)}";

                string saldo = "0";
                if (writepermission) { saldo = await ConsultarSaldoAsync(sol_provider, apiKeySolver); }
                try { saldo = await ConsultarSaldoAsync(sol_provider, apiKeySolver); } catch { saldo = "0"; };

                var trans = new TransactionBot
                {
                    id_transaction = last_id_trans.ToString(),
                    transaction_order = (_orden++).ToString(),
                    resolution_provider = sol_provider,
                    id_cliente = usuario,
                    tipodocumento = tipo.Value.ToString(),
                    mes_consulta = mes.ToString(),
                    estado = "0",
                    observacion = $"[{nombre}] Archivo descargado",
                    saldo = saldo,
                    trans_type = "D"
                };

                if (writepermission) { await helper.InsertarTransactionAsync(trans); }
            }
            catch (Exception e)
            {
                detalle.Error = $"Error al descargar archivo: {e.Message}";
                LoggerHelper.Log(pathHilo, $"[{nombre}] {detalle.Error}");
                //txtDialogbot.Text = $"[{nombre}] Error al descargar archivo {tipo.Value}_{mes}_{anio}";

                string saldo = "0";
                if (writepermission) { saldo = await ConsultarSaldoAsync(sol_provider, apiKeySolver); }
                try { saldo = await ConsultarSaldoAsync(sol_provider, apiKeySolver); } catch { saldo = "0"; };

                var trans = new TransactionBot
                {
                    id_transaction = last_id_trans.ToString(),
                    transaction_order = (_orden++).ToString(),
                    resolution_provider = sol_provider,
                    id_cliente = usuario,
                    tipodocumento = tipo.Key.ToString(),
                    mes_consulta = mes.ToString(),
                    estado = "1",
                    observacion = detalle.Error,
                    saldo = saldo,
                    trans_type = "D"
                };

                if (writepermission) { await helper.InsertarTransactionAsync(trans); }
            }

            return detalle;
        }

        //private void ActualizarEstadoClienteEnGrid(string usuario, string estado, Color color)
        //{
        //    if (grdDescarga == null || grdDescarga.IsDisposed) return;

        //    if (grdDescarga.InvokeRequired)
        //    {
        //        try
        //        {
        //            grdDescarga.BeginInvoke(new Action(() =>
        //                ActualizarEstadoClienteEnGrid(usuario, estado, color)));
        //        }
        //        catch { /* Si el form ya se cerró, ignorar */ }
        //        return;
        //    }

        //    try
        //    {
        //        if (!grdDescarga.Columns.Contains("Procesado")) return;

        //        foreach (DataGridViewRow row in grdDescarga.Rows)
        //        {
        //            if (row.IsNewRow) continue;
        //            var cellUsuario = row.Cells["usuario"];
        //            if (cellUsuario?.Value == null) continue;

        //            if (cellUsuario.Value.ToString() == usuario)
        //            {
        //                var cell = row.Cells["Procesado"];
        //                cell.Value = estado;
        //                cell.Style.BackColor = color;
        //                cell.Style.ForeColor = Color.White;
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Opcional: registrar en log si algo falla
        //        LoggerHelper.Log(_logPath, $"⚠️ Error al actualizar grilla: {ex.Message}");
        //    }
        //}

        // 🔹 NUEVO: Método para generar resumen por cliente
        private void GenerarResumenCliente(
            string usuario,
            string nombre,
            List<DescargaDetalle> exitosas,
            List<DescargaDetalle> fallidas,
            List<KeyValuePair<int, string>> tiposFiltrados,
            List<(int Mes, int Año)> mesesConsulta,
            string pathHilo)
        {
            // Guardar resumen en diccionario
            _resumenPorCliente[usuario] = new ResumenCliente
            {
                Usuario = usuario,
                Nombre = nombre,
                Exitosas = new List<DescargaDetalle>(exitosas),
                Fallidas = new List<DescargaDetalle>(fallidas),
                TiposFiltrados = new List<KeyValuePair<int, string>>(tiposFiltrados),
                MesesConsulta = new List<(int Mes, int Año)>(mesesConsulta)
            };

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

        private async Task<bool> IniciarSesionSRI(IPage page, string url, string usuario, string nombre, string ci, string clave, string pathHilo)
        {
            try
            {
                await page.GotoAsync(url);

                try
                {
                    var cerrarBtn = page.Locator("a.boton-cerrar-legado");
                    if (await cerrarBtn.IsVisibleAsync())
                    {
                        await cerrarBtn.ClickAsync();
                        await page.WaitForTimeoutAsync(10000); // pequeña espera
                        LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Sesión previa cerrada.");
                    }
                }
                catch { /* ignoramos si no hay sesión */ }

                await page.GotoAsync(url);
                await page.WaitForTimeoutAsync(10000);

                await page.FillAsync("input[name='usuario']", usuario);
                await page.FillAsync("input[name='ciAdicional']", ci);
                await page.FillAsync("input[name='password']", clave);
                await page.ClickAsync("text=Ingresar");

                await page.WaitForTimeoutAsync(10000);

                return await page.Locator("a.boton-cerrar-legado").IsVisibleAsync();
            }
            catch { return false; }
        }

        private async Task CerrarSesion(IPage page)
        {
            try
            {
                var cerrar = page.Locator("a.boton-cerrar-legado");
                if (await cerrar.IsVisibleAsync()) await cerrar.ClickAsync();
            }
            catch { }
        }

        private async Task<bool> ClickConsultarAsync(IPage page, string boton)
        {
            // Intenta varios selectores posibles del botón Consultar
            string[] sels = {
                //"#frmPrincipal\\:btnConsultar",
                //"#btnRecaptcha",           // por compatibilidad con tu código actual
                //"text=Consultar",           // fallback por texto visible
                //"#frmPrincipal\\:btnBuscar",
                boton
            };

            foreach (var sel in sels)
            {
                try
                {
                    var e = await page.QuerySelectorAsync(sel);
                    if (e != null)
                    {
                        await page.ClickAsync(sel);
                        return true;
                    }
                }
                catch { /* sigue probando */ }
            }
            return false;
        }

        private async Task<bool> EsperarResultadosAsync(IPage page, int timeoutMs)
        {
            // 1) Intento: esperar quietud de red (puede no servir con AJAX, pero no estorba)
            try
            {
                var loadOpts = new PageWaitForLoadStateOptions { Timeout = timeoutMs };
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, loadOpts);
            }
            catch { /* AJAX quizá no dispara navegación */ }

            // 2) Esperar la tabla o el link de descarga
            try
            {
                var waitTable = new PageWaitForSelectorOptions { Timeout = timeoutMs };
                await page.WaitForSelectorAsync("#frmPrincipal\\:tablaCompRecibidos", waitTable);
                return true;
            }
            catch
            {
                try
                {
                    var waitLink = new PageWaitForSelectorOptions { Timeout = Math.Max(5000, timeoutMs / 2) };
                    await page.WaitForSelectorAsync("#frmPrincipal\\:lnkTxtlistado", waitLink);
                    return true;
                }
                catch { return false; }
            }
        }

        private async Task<bool> ConsultarConRetoAsync(IPage page, string nombre, int timeoutMs, bool humanInteraction, string pathHilo, int mes, int anio, string tipo/*, IBrowserContext context, IBrowser browser, string usuario, string ci, string clave*/)
        {
            var botonConsultarSRI = ConfigurationManager.AppSettings["NombreBotonSRI"].ToString() ?? "";
            // 1) Click en Consultar
            //await page.WaitForTimeoutAsync(13000);

            //comentado para que presione el boton consultar al cargar la pagina.
            //var clicked = await ClickConsultarAsync(page, botonConsultarSRI);
            //if (!clicked)
            //    throw new Exception("No se encontró el botón 'Consultar'.");

            bool respuestageneral = false;

            if (humanInteraction)
            {
                // MODO HUMANO: espera a que el usuario resuelva el reto
                Action<string> logger = msg => LoggerHelper.Log(_logPath, msg);
                await CaptchaGate.HandleIfOpenAsync(page, logger, 180000);
            }
            else
            {
                // MODO AUTOMÁTICO: Resuelve con el solver configurado
                if (_captchaSolver == null)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ No se configuró un solver de CAPTCHA válido.");
                    return false;
                }

                try
                {
                    bool resolved = false;
                    if (_captchaSolver is CapSolver capSolver)
                    {

                        await page.WaitForTimeoutAsync(5000);
                        await ClickConsultarAsync(page, botonConsultarSRI);

                        int maxIntentos = 1;
                        int intentos = 0;

                        while (intentos < maxIntentos)
                        {
                            //bool captchaIncorrecto = await page
                            //    .Locator($"text={CAPTCHA_ERROR_TEXT}")
                            //    .IsVisibleAsync();

                            //bool existe = false;

                            //if (await page.Locator("#frmPrincipal\\:tablaCompRecibidos").IsVisibleAsync())
                            //{
                            //    existe = true;
                            //}
                            //else
                            //{
                            //    // No existe o no está visible
                            //}

                            //var mensaje = await page.QuerySelectorAsync("#formMessages\\:messages");
                            //var texto = await mensaje.InnerTextAsync();

                            //if ((!captchaIncorrecto && existe) || texto.Contains("No existen datos")) break;

                            LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Intento {intentos + 1}/{maxIntentos}");

                            // USAR LA NUEVA FUNCIÓN EN LUGAR DE MÚLTIPLES INTENTOS
                            //bool clickExitoso = await ForceUltimateClick(page, botonConsultarSRI, CAPTCHA_ERROR_TEXT, pathHilo, nombre);
                            bool ejecutado = await ForceConsultarPrimeFacesAsync(
                                page,
                                botonConsultarSRI,
                                pathHilo,
                                nombre
                            );

                            await page.WaitForTimeoutAsync(20000);

                            if (ejecutado)
                            {
                                respuestageneral = ejecutado;
                                resolved = ejecutado;
                                break;
                            }
                                
                            //if ((!captchaIncorrecto && existe) || texto.Contains("No existen datos"))
                            //{

                            //}
                            //else { clickExitoso = false; }

                            #region
                            //if (!clickExitoso)
                            //{
                            //    try
                            //    {
                            //        if (page != null)
                            //        {
                            //            await page.CloseAsync();
                            //            page = null;
                            //        }

                            //        if (context != null)
                            //        {
                            //            await context.CloseAsync();
                            //            context = null;
                            //        }
                            //    }
                            //    catch { /* no bloquear */ }

                            //    // ÚLTIMO RECURSO: Recargar y empezar de nuevo
                            //    //LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Recargando página...");
                            //    //await page.ReloadAsync();

                            //    context = await browser.NewContextAsync(new BrowserNewContextOptions
                            //    {
                            //        AcceptDownloads = true
                            //        // NO userDataDir → cache limpia
                            //    });

                            //    page = await context.NewPageAsync();

                            //    var config = ConfigManager.CargarConfiguracion();
                            //    string sriUrl = config.SriUrl;

                            //    await page.GotoAsync(sriUrl, new PageGotoOptions
                            //    {
                            //        WaitUntil = WaitUntilState.Load,
                            //        Timeout = 60000
                            //    });

                            //    await page.WaitForTimeoutAsync(3000);

                            //    bool loginOk = await IniciarSesionSRI(
                            //        page,
                            //        sriUrl,
                            //        usuario,
                            //        nombre,
                            //        ci,
                            //        clave,
                            //        pathHilo
                            //    );

                            //    if (!loginOk)
                            //    {
                            //        LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ Falló login tras reinicio de contexto");
                            //        return false; // o continue según tu flujo
                            //    }

                            //    LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Sesión reiniciada correctamente");

                            //    await page.WaitForTimeoutAsync(3000);
                            //    // Reconfigurar formulario si es necesario
                            //    await page.SelectOptionAsync("#frmPrincipal\\:ano", anio.ToString());
                            //    await page.SelectOptionAsync("#frmPrincipal\\:mes", mes.ToString());
                            //    await page.SelectOptionAsync("#frmPrincipal\\:dia", "0");
                            //    await page.SelectOptionAsync("#frmPrincipal\\:cmbTipoComprobante", tipo);

                            //    await page.WaitForTimeoutAsync(5000);
                            //    await ClickConsultarAsync(page, botonConsultarSRI);
                            //}
                            #endregion

                            intentos++;
                        }
                        /// </summary>
                        //resolved = await capSolver.SolveOnButtonClickAsync(page, page.Url, botonConsultarSRI, null, true);

                        // resolved = await capSolver.SolveRecaptchaV3EnterpriseWithVerificationAsync(
                        //page,
                        //page.Url,
                        //botonConsultarSRI,
                        //"consulta_cel_recibidos");

                        // 5️⃣ Click normal (PrimeFaces hace TODO lo demás)
                        //await page.ClickAsync("button:has-text('Consultar')");


                    }
                    else if (_captchaSolver is RecaptchaV2Solver recaptchaSolver)
                    {
                        resolved = await recaptchaSolver.SolveOnButtonClickAsync(page, page.Url, botonConsultarSRI, null, true);
                    }

                    if (!resolved)
                    {
                        LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ Fallo en resolución de CAPTCHA Enterprise.");
                        return false;
                    }
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ CAPTCHA resuelto automáticamente.");
                    //await page.WaitForTimeoutAsync(2000);
                    //await ClickConsultarAsync(page, botonConsultarSRI);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ Fallo en resolución automática de CAPTCHA: {ex.Message}");
                    return false;
                }
            }

            // Esperar resultados
            //return await EsperarResultadosAsync(page, timeoutMs);
            return respuestageneral;
        }

        private async Task<bool> ConsultarDirectoSinBotoAsync(IPage page, string nombre, int timeoutMs, string pathHilo, int mes, int anio, string tipo)
        {
            try
            {
                LoggerHelper.Log(pathHilo, $"[{nombre}] 📤 Iniciando petición POST directa...");

                // 1) Obtener el ViewState dinámico de la página actual
                var viewState = await page.EvaluateAsync<string>(@"
                    () => {
                        return document.querySelector('input[name=""javax.faces.ViewState""]')?.value || '';
                    }
                ");

                if (string.IsNullOrEmpty(viewState))
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ No se pudo obtener ViewState");
                    return false;
                }

                LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ ViewState obtenido");

                // 2) Preparar el body de la petición - PRIMERO obtenemos todos los campos del formulario
                var allFormFields = await page.EvaluateAsync<Dictionary<string, string>>(@"
                    () => {
                        const form = document.querySelector('form') || document.querySelector('#frmPrincipal');
                        const fields = {};
                
                        if (form) {
                            const inputs = form.querySelectorAll('input, select, textarea');
                            inputs.forEach(input => {
                                if (input.name && input.value) {
                                    fields[input.name] = input.value;
                                }
                            });
                        }
                
                        return fields;
                    }
                ");

                // 3) Actualizar con los parámetros específicos
                var bodyParams = new Dictionary<string, string>(allFormFields);

                // Sobrescribir con los valores específicos
                bodyParams["javax.faces.partial.ajax"] = "true";
                bodyParams["javax.faces.source"] = "frmPrincipal:btnConsultar";
                bodyParams["javax.faces.partial.execute"] = "frmPrincipal";
                bodyParams["javax.faces.partial.render"] = "frmPrincipal:tablaCompRecibidos";
                bodyParams["frmPrincipal:btnConsultar"] = "frmPrincipal:btnConsultar";
                bodyParams["frmPrincipal:ano"] = anio.ToString();
                bodyParams["frmPrincipal:mes"] = mes.ToString();
                bodyParams["frmPrincipal:tipo"] = tipo;
                bodyParams["javax.faces.ViewState"] = viewState;

                LoggerHelper.Log(pathHilo, $"[{nombre}] 📋 Parámetros preparados: {bodyParams.Count} campos");

                // 4) Convertir a application/x-www-form-urlencoded
                var formContent = new FormUrlEncodedContent(bodyParams);
                var bodyString = await formContent.ReadAsStringAsync();

                LoggerHelper.Log(pathHilo, $"[{nombre}] 📤 Body: {bodyString.Substring(0, Math.Min(200, bodyString.Length))}...");

                // 5) Hacer la petición POST
                var response = await page.APIRequest.PostAsync(
                    "https://srienlinea.sri.gob.ec/comprobantes-electronicos-internet/pages/consultas/recibidos/comprobantesRecibidos.jsf",
                    new APIRequestContextOptions
                    {
                        Headers = new Dictionary<string, string>
                        {
                            { "Accept", "application/xml, text/xml, */*; q=0.01" },
                            { "Accept-Language", "es-ES,es;q=0.8" },
                            { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
                            { "Faces-Request", "partial/ajax" },
                            { "X-Requested-With", "XMLHttpRequest" },
                            { "Origin", "https://srienlinea.sri.gob.ec" },
                            { "Referer", page.Url }
                        },
                        Data = bodyString
                    }
                );

                if (!response.Ok)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ Petición fallida. Status: {response.Status}");
                    return false;
                }

                var responseText = await response.TextAsync();
                LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Petición exitosa");

                // DEBUG: Guardar la respuesta
                System.IO.File.WriteAllText(
                    Path.Combine(pathHilo, $"response_{DateTime.Now:yyyyMMdd_HHmmss}.xml"),
                    responseText);

                // 6) Actualizar el ViewState con el nuevo valor de la respuesta
                var newViewState = ExtractViewStateFromResponse(responseText);
                if (!string.IsNullOrEmpty(newViewState))
                {
                    await page.EvaluateAsync($@"
                        (newViewState) => {{
                            const input = document.querySelector('input[name=""javax.faces.ViewState""]');
                            if (input) {{
                                input.value = newViewState;
                            }}
                        }}
                    ", newViewState);
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ ViewState actualizado");
                }

                // 7) Hacer SEGUNDO request con el nuevo ViewState para obtener los datos reales
                LoggerHelper.Log(pathHilo, $"[{nombre}] 🔄 Realizando segundo request para obtener datos...");

                bodyParams["javax.faces.ViewState"] = newViewState ?? viewState;
                formContent = new FormUrlEncodedContent(bodyParams);
                bodyString = await formContent.ReadAsStringAsync();

                var response2 = await page.APIRequest.PostAsync(
                    "https://srienlinea.sri.gob.ec/comprobantes-electronicos-internet/pages/consultas/recibidos/comprobantesRecibidos.jsf",
                    new APIRequestContextOptions
                    {
                        Headers = new Dictionary<string, string>
                        {
                            { "Accept", "application/xml, text/xml, */*; q=0.01" },
                            { "Accept-Language", "es-ES,es;q=0.8" },
                            { "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8" },
                            { "Faces-Request", "partial/ajax" },
                            { "X-Requested-With", "XMLHttpRequest" },
                            { "Origin", "https://srienlinea.sri.gob.ec" },
                            { "Referer", page.Url }
                        },
                        Data = bodyString
                    }
                );

                if (response2.Ok)
                {
                    var responseText2 = await response2.TextAsync();
                    System.IO.File.WriteAllText(
                        Path.Combine(pathHilo, $"response2_{DateTime.Now:yyyyMMdd_HHmmss}.xml"),
                        responseText2);

                    // Inyectar la respuesta del segundo request
                    await page.EvaluateAsync($@"
                        (responseText) => {{
                            console.log('📥 Segunda respuesta:', responseText.substring(0, 300));
                    
                            const parser = new DOMParser();
                            const xmlDoc = parser.parseFromString(responseText, 'text/xml');
                    
                            const updateElements = xmlDoc.querySelectorAll('update');
                            console.log('🔍 Updates encontrados:', updateElements.length);
                    
                            updateElements.forEach(el => {{
                                const id = el.getAttribute('id');
                                const cdata = el.textContent;
                        
                                if (id && cdata) {{
                                    const targetElement = document.getElementById(id);
                                    if (targetElement) {{
                                        console.log('✅ Inyectando:', id);
                                        targetElement.innerHTML = cdata;
                                    }}
                                }}
                            }});
                        }}
                    ", responseText2);

                    LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Segunda respuesta inyectada");
                }

                // 8) Esperar a que la tabla sea visible
                await page.WaitForTimeoutAsync(1000);

                bool tablaVisible = await page.Locator("#frmPrincipal\\:tablaCompRecibidos")
                    .IsVisibleAsync();

                if (tablaVisible)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Tabla visible después de la petición");
                    return true;
                }
                else
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ Tabla NO visible");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Extrae el ViewState actualizado del XML de respuesta
        /// </summary>
        private string ExtractViewStateFromResponse(string responseXml)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    responseXml,
                    @"<update id=""javax\.faces\.ViewState""><!\[CDATA\[(.*?)\]\]></update>");

                return match.Success ? match.Groups[1].Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ForceUltimateClick(IPage page, string selector, string captchaErrorText, string pathHilo, string nombre)
        {
            // 1. DIAGNÓSTICO COMPLETO ANTES DE CUALQUIER ACCIÓN
            var diagnostic = await page.EvaluateAsync<string>(@"
                (selector) => {
                    const btn = document.querySelector(selector);
                    if (!btn) return 'ERROR: Elemento no encontrado';
            
                    // Verifica todo el árbol de elementos
                    let elementChain = [];
                    let current = btn;
                    while (current && current !== document.body) {
                        elementChain.push({
                            tag: current.tagName,
                            id: current.id,
                            class: current.className,
                            disabled: current.disabled,
                            style: window.getComputedStyle(current).display,
                            hidden: current.hidden,
                            parentOverflow: window.getComputedStyle(current.parentElement).overflow
                        });
                        current = current.parentElement;
                    }
            
                    return JSON.stringify({
                        elementExists: true,
                        element: {
                            id: btn.id,
                            tag: btn.tagName,
                            disabled: btn.disabled,
                            readonly: btn.readOnly,
                            styleDisplay: btn.style.display,
                            computedDisplay: window.getComputedStyle(btn).display,
                            visibility: window.getComputedStyle(btn).visibility,
                            opacity: window.getComputedStyle(btn).opacity,
                            pointerEvents: window.getComputedStyle(btn).pointerEvents,
                            tabIndex: btn.tabIndex,
                            hasOnclick: !!btn.onclick,
                            eventListeners: {
                                click: btn.hasEventListener?.('click'),
                                mousedown: btn.hasEventListener?.('mousedown')
                            }
                        },
                        hierarchy: elementChain,
                        documentState: {
                            readyState: document.readyState,
                            forms: document.forms.length,
                            activeElement: document.activeElement?.id
                        }
                    }, null, 2);
                }
            ", selector);

            LoggerHelper.Log(pathHilo, $"[{nombre}] 📊 Diagnóstico botón: {diagnostic}");

            // 2. EJECUTAR EN ORDEN DE EFECTIVIDAD (no todos a la vez)
            var methods = new List<Func<Task<bool>>>
            {
                // Método 1: Habilitación completa + clic nativo del DOM
                async () => {
                    return await page.EvaluateAsync<bool>(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn) return false;
                    
                            // Guardar estado original
                            const originalState = {
                                disabled: btn.disabled,
                                onclick: btn.onclick,
                                tabIndex: btn.tabIndex
                            };
                    
                            // Habilitación agresiva
                            btn.disabled = false;
                            btn.removeAttribute('disabled');
                            btn.setAttribute('aria-disabled', 'false');
                            btn.style.cssText += ';pointer-events:auto !important;opacity:1 !important;display:block !important;';
                    
                            // Eliminar listeners problemáticos
                            const newBtn = btn.cloneNode(true);
                            btn.parentNode.replaceChild(newBtn, btn);
                    
                            // Ejecutar clic nativo
                            newBtn.click();
                    
                            return true;
                        }
                    ", selector);
                },
        
                // Método 2: Disparar evento directamente en el elemento original
                async () => {
                    var element = await page.Locator(selector).ElementHandleAsync();
                    if (element == null) return false;
            
                    // Secuencia completa de eventos
                    await element.DispatchEventAsync("focus");
                    await element.DispatchEventAsync("mousedown");
                    await element.DispatchEventAsync("mouseup");
                    await element.DispatchEventAsync("click");

                    return true;
                },
        
                // Método 3: Forzar a través del formulario
                async () => {
                    return await page.EvaluateAsync<bool>(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn) return false;
                    
                            // Encontrar el formulario padre
                            let form = btn.closest('form');
                            if (!form && btn.form) form = btn.form;
                    
                            if (form) {
                                // Crear y disparar evento submit
                                const submitEvent = new Event('submit', { bubbles: true, cancelable: true });
                                form.dispatchEvent(submitEvent);
                        
                                // Si no se canceló, enviar el formulario
                                if (!submitEvent.defaultPrevented) {
                                    form.submit();
                                    return true;
                                }
                            }
                            return false;
                        }
                    ", selector);
                }
            };

            // 3. EJECUTAR MÉTODOS CON INTELIGENCIA
            for (int i = 0; i < methods.Count; i++)
            {
                try
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] 🧪 Intentando método {i + 1}");

                    bool success = await methods[i]();

                    // Esperar resultado
                    await page.WaitForTimeoutAsync(10000);

                    // Verificar si funcionó (sin error de CAPTCHA)
                    bool captchaError = await page.Locator($"text={captchaErrorText}").IsVisibleAsync();

                    if (!captchaError)
                    {
                        LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Método {i + 1} funcionó");
                        return true;
                    }

                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠️ Método {i + 1} no superó CAPTCHA");
                }
                catch (Exception ex)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ Error método {i + 1}: {ex.Message}");
                }
            }

            return false;
        }

        //public async Task<bool> ForceConsultarPrimeFacesAsync(IPage page, string selector, string pathHilo, string nombre, int timeoutMs = 15000)
        //{
        //    LoggerHelper.Log(pathHilo, $"[{nombre}] 🚀 Forzando ejecución PrimeFaces");

        //    var estrategias = new List<Func<Task>>
        //    {
        //        // 1️⃣ PrimeFaces.ab directo (LO MÁS EFECTIVO)
        //        async () =>
        //        {
        //            await page.EvaluateAsync(@"
        //                (selector) => {
        //                    const btn = document.querySelector(selector);
        //                    if (!btn) return;

        //                    const onclick = btn.getAttribute('onclick');
        //                    if (onclick && onclick.includes('PrimeFaces.ab')) {
        //                        eval(onclick);
        //                    }
        //                }
        //            ", selector);
        //        },

        //        // 2️⃣ Submit del formulario padre
        //        async () =>
        //        {
        //            await page.EvaluateAsync(@"
        //                (selector) => {
        //                    const btn = document.querySelector(selector);
        //                    if (!btn) return;
        //                    const form = btn.closest('form') || btn.form;
        //                    if (form) form.submit();
        //                }
        //            ", selector);
        //        },

        //        // 3️⃣ Eventos DOM completos
        //        async () =>
        //        {
        //            var el = await page.Locator(selector).ElementHandleAsync();
        //            if (el == null) return;

        //            await el.DispatchEventAsync("mouseover");
        //            await el.DispatchEventAsync("mousedown");
        //            await el.DispatchEventAsync("mouseup");
        //            await el.DispatchEventAsync("click");
        //        },

        //        // 4️⃣ Click Playwright forzado
        //        async () =>
        //        {
        //            await page.Locator(selector).ClickAsync(new LocatorClickOptions
        //            {
        //                Force = true,
        //                Timeout = 5000
        //            });
        //        }
        //    };

        //    foreach (var estrategia in estrategias)
        //    {
        //        try
        //        {
        //            await estrategia();

        //            bool resultado = await EsperarResultadoConsultaAsync(page, timeoutMs);
        //            if (resultado)
        //            {
        //                LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Consulta ejecutada correctamente");
        //                return true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ Estrategia falló: {ex.Message}");
        //        }
        //    }

        //    LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ No se logró ejecutar la consulta");
        //    return false;
        //}

        public async Task<bool> ForceConsultarPrimeFacesAsync(IPage page, string selector, string pathHilo, string nombre, int timeoutMs = 20000)
        {
            LoggerHelper.Log(pathHilo, $"[{nombre}] 🚀 Forzando ejecución PrimeFaces");

            var estrategias = new List<Func<Task>>
            {
                // 1️⃣ PrimeFaces.ab directo (MEJOR OPCIÓN)
                async () =>
                {
                    await page.EvaluateAsync(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn) return;

                            const onclick = btn.getAttribute('onclick');
                            if (onclick && onclick.includes('PrimeFaces.ab')) {
                                eval(onclick);
                            }
                        }
                    ", selector);
                },

                // 🔥 1️⃣ BIS — PrimeFaces.ab reconstruido (ULTRA)
                async () =>
                {
                    await page.EvaluateAsync(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn || !window.PrimeFaces) return;

                            const form = btn.closest('form') || btn.form;
                            if (!form) return;

                            PrimeFaces.ab({
                                s: btn.id,
                                f: form.id,
                                p: btn.id,
                                u: form.id
                            });
                        }
                    ", selector);
                },

                // 2️⃣ CLONAR BOTÓN + CLICK (quita listeners corruptos)
                async () =>
                {
                    await page.EvaluateAsync(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn) return;

                            btn.disabled = false;
                            btn.removeAttribute('disabled');
                            btn.style.pointerEvents = 'auto';

                            const clone = btn.cloneNode(true);
                            btn.parentNode.replaceChild(clone, btn);

                            clone.click();
                        }
                    ", selector);
                },

                // 3️⃣ Submit del formulario padre
                async () =>
                {
                    await page.EvaluateAsync(@"
                        (selector) => {
                            const btn = document.querySelector(selector);
                            if (!btn) return;

                            const form = btn.closest('form') || btn.form;
                            if (form) form.submit();
                        }
                    ", selector);
                },

                // 4️⃣ Eventos DOM completos
                async () =>
                {
                    var el = await page.Locator(selector).ElementHandleAsync();
                    if (el == null) return;

                    await el.DispatchEventAsync("mouseover");
                    await el.DispatchEventAsync("mousedown");
                    await el.DispatchEventAsync("mouseup");
                    await el.DispatchEventAsync("click");
                },

                // 5️⃣ Click Playwright forzado (ÚLTIMO RECURSO)
                //async () =>
                //{
                //    await page.Locator(selector).ClickAsync(new LocatorClickOptions
                //    {
                //        Force = true,
                //        Timeout = 5000
                //    });
                //}
            };

            foreach (var estrategia in estrategias)
            {
                try
                {
                    await estrategia();

                    bool resultado = await EsperarResultadoConsultaAsync(page, timeoutMs);
                    if (resultado)
                    {
                        LoggerHelper.Log(pathHilo, $"[{nombre}] ✅ Consulta ejecutada correctamente");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Log(pathHilo, $"[{nombre}] ⚠ Estrategia falló: {ex.Message}");
                }
            }

            LoggerHelper.Log(pathHilo, $"[{nombre}] ❌ No se logró ejecutar la consulta");
            return false;
        }


        private async Task<bool> EsperarResultadoConsultaAsync(IPage page, int timeoutMs)
        {
            var inicio = DateTime.Now;

            while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMs)
            {
                // ✅ Tabla de documentos
                if (await page.Locator("#frmPrincipal\\:tablaCompRecibidos").IsVisibleAsync())
                    return true;

                // ✅ Mensaje "No existen datos"
                var mensaje = await page.QuerySelectorAsync("#formMessages\\:messages");
                if (mensaje != null)
                {
                    var texto = await mensaje.InnerTextAsync();
                    if (!string.IsNullOrEmpty(texto) &&
                        texto.Contains("No existen datos"))
                        return true;
                }

                await page.WaitForTimeoutAsync(15000);
            }

            return false;
        }


        private async Task EsperarAsync(int milisegundos, string mensaje, string pathHilo)
        {
            try
            {
                if (milisegundos <= 0) return;
                LoggerHelper.Log(pathHilo, mensaje);
                await Task.Delay(milisegundos);
            }
            catch (TaskCanceledException)
            {
                // ignorar cancelaciones si usas CTS externo
            }
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
                        if (valor == "OK") bgColor = BaseColor.GREEN;
                        else if (valor == "ERROR") bgColor = BaseColor.RED;
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