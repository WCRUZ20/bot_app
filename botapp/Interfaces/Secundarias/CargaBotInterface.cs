using botapp.Core;
using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class CargaBotInterface : UserControl
    {
        private readonly string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";

        private DataTable dt;
        private string _usuarioActual = "";
        private bool writepermission = false;

        public CargaBotInterface(string usuarioActual)
        {
            InitializeComponent();

            _usuarioActual = usuarioActual;

            //lblNoti.Text = "Sin conexión";
            //lblNoti.ForeColor = System.Drawing.SystemColors.ControlDark;

            ModificaLblableWrite();

            grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdClientes.AllowUserToAddRows = false;
            grdClientes.ReadOnly = false;
            grdClientes.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdClientes.MultiSelect = false;
            grdClientes.Enabled = true;
            grdClientes.TabStop = false;
            grdClientes.BackgroundColor = Color.WhiteSmoke;

            grdClientes.DataError += (s, e) => { e.Cancel = true; };
            grdClientes.EnableHeadersVisualStyles = false;
            grdClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
            grdClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            grdClientes.DefaultCellStyle.Font = new Font("Segoe UI", 7F);
            //grdClientes.DefaultCellStyle.BackColor = Color.White;
            //grdClientes.DefaultCellStyle.SelectionBackColor = Color.White;
            //grdClientes.DefaultCellStyle.SelectionForeColor = Color.Black;
            //grdClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            grdClientes.RowTemplate.Height = 20;
            grdClientes.GridColor = Color.DarkGray;
            grdClientes.BorderStyle = BorderStyle.None;
            grdClientes.RowHeadersVisible = false;
            grdClientes.ClearSelection();
            grdClientes.CurrentCell = null;
            //grdClientes.CellContentClick += grdClientes_CellContentClick;
            grdClientes.DefaultCellStyle.SelectionBackColor = grdClientes.DefaultCellStyle.BackColor;
            grdClientes.DefaultCellStyle.SelectionForeColor = grdClientes.DefaultCellStyle.ForeColor;

            //Modificaciones visuales 
            grdClientes.BackgroundColor = Color.White;//Color.FromArgb(245, 246, 250); // gris claro
            grdClientes.BorderStyle = BorderStyle.None;
            grdClientes.GridColor = Color.FromArgb(209, 213, 219); // líneas suaves

            // Encabezados
            grdClientes.EnableHeadersVisualStyles = false;
            grdClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 47); // azul grisáceo oscuro
            grdClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);

            // Celdas
            grdClientes.DefaultCellStyle.BackColor = Color.White;
            grdClientes.DefaultCellStyle.ForeColor = Color.FromArgb(46, 46, 46);
            grdClientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 255); // azul muy suave
            grdClientes.DefaultCellStyle.SelectionForeColor = Color.Black;
            grdClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            grdClientes.DataBindingComplete += (s, e) =>
            {
                try
                {
                    grdClientes.Columns["orden"].ReadOnly = true;
                    grdClientes.Columns["orden"].HeaderText = "Orden proceso";

                    grdClientes.Columns["usuario"].ReadOnly = true;
                    grdClientes.Columns["usuario"].HeaderText = "Identificación";

                    grdClientes.Columns["NombUsuario"].ReadOnly = true;
                    grdClientes.Columns["NombUsuario"].HeaderText = "Empresa";

                    grdClientes.Columns["dias"].Visible = false;
                    grdClientes.Columns["meses_ante"].Visible = false;
                    grdClientes.Columns["ci_adicional"].Visible = false;
                    grdClientes.Columns["clave"].Visible = false;
                    grdClientes.Columns["clave_ws"].Visible = false;
                    grdClientes.Columns["descarga"].Visible = false;
                    grdClientes.Columns["factura"].Visible = false;
                    grdClientes.Columns["notacredito"].Visible = false;
                    grdClientes.Columns["retencion"].Visible = false;
                    grdClientes.Columns["liquidacioncompra"].Visible = false;
                    grdClientes.Columns["notadebito"].Visible = false;


                    if (grdClientes.Columns.Contains("Activo"))
                    {
                        grdClientes.Columns["Activo"].ReadOnly = true;
                        grdClientes.Columns["Activo"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["Activo"].HeaderText = "Activo";
                        grdClientes.Columns["Activo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("carga"))
                    {
                        grdClientes.Columns["carga"].ReadOnly = true;
                        grdClientes.Columns["carga"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["carga"].HeaderText = "Carga";
                        grdClientes.Columns["carga"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("Resultado"))
                    {
                        grdClientes.Columns["Resultado"].ReadOnly = true;
                        grdClientes.Columns["Resultado"].HeaderText = "Resultado";
                        grdClientes.Columns["Resultado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }


                    if (grdClientes.Columns.Contains("orden"))
                    {
                        grdClientes.Sort(grdClientes.Columns["orden"], ListSortDirection.Ascending);
                    }

                    if (grdClientes.Columns.Contains("Claves cargadas"))
                    {
                        DataColumn col = new DataColumn("Claves cargadas", typeof(int));
                        dt.Columns.Add(col);
                        //col.SetOrdinal(0); // mueve la columna al índice 0
                    }

                    grdClientes.Columns["Claves cargadas"].ReadOnly = true;

                }
                catch { }

                //grdClientes.AutoResizeColumns();
                grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grdClientes.ClearSelection();
                grdClientes.CurrentCell = null;
            };

            chkToggleAutoCarga.Checked = false;

            _ = CargarClientes();
        }

        public void ActualizarUCCarga()
        {
            _ = CargarClientes();
            ModificaLblableWrite();

            try
            {
                if (grdClientes.Columns.Contains("orden") && grdClientes.Rows.Count > 0)
                {
                    grdClientes.Sort(grdClientes.Columns["orden"], ListSortDirection.Ascending);
                    grdClientes.ClearSelection();
                    grdClientes.CurrentCell = null;
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

            if (validuser)
            {
                lblableWrite.Text = "Permiso escritura: Sí";
            }
            else if (!validuser)
            {
                lblableWrite.Text = "Permiso escritura: No";
            }
        }

        private async Task CargarClientes()
        {
            try
            {
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                dt = await helper.GetClientesBotActivosAsync();

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

                    if (!dt.Columns.Contains("Resultado"))
                    {
                        DataColumn col = new DataColumn("Resultado", typeof(string));
                        dt.Columns.Add(col);
                        //col.SetOrdinal(0); // mueve la columna al índice 0
                    }

                    if (!dt.Columns.Contains("Claves caragadas"))
                    {
                        DataColumn col = new DataColumn("Claves cargadas", typeof(int));
                        dt.Columns.Add(col);
                        //col.SetOrdinal(0); // mueve la columna al índice 0
                    }

                    foreach (DataRow row in dt.Rows)
                        row["Resultado"] = string.Empty;

                    foreach (DataRow row in dt.Rows)
                        row["Claves cargadas"] = false;

                    grdClientes.DataSource = dt;
                    grdClientes.ClearSelection();
                    grdClientes.CurrentCell = null;

                    grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
        }

        private async void btnCargar_Click(object sender, EventArgs e)
        {
            int? last_id_trans;
            int orden = 0;
            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
            string logPath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG"), "procesoCarga.log");
            string edocUrl = ConfigurationManager.AppSettings["EDOC_Endpoint"].ToString();
            string sol_provider = "";
            string _estado = "";
            string _obervacion = "";

            writepermission = await helper.TienePermisoEscrituraAsync(_usuarioActual);

            if (ConfigurationManager.AppSettings["2captcha"] == "true")
            {
                sol_provider = "2Captcha";
            }

            if (ConfigurationManager.AppSettings["Capsolver"] == "true")
            {
                sol_provider = "Capsolver";
            }

            if (ConfigurationManager.AppSettings["HumanInteraction"] == "true")
            {
                sol_provider = "HumanInter";
            }

            //LoggerHelper.Init(logPath);
            LoggerHelper.Log(logPath, "🟢 Inicio de ejecución carga - BOT");

            last_id_trans = await helper.GetMaxIdTransactionAsync();
            last_id_trans = last_id_trans + 1;

            if (dt != null)
            {
                DataView view = new DataView(dt);
                view.Sort = "orden ASC";

                foreach (DataRowView rowView in view)
                {
                    string usuario = rowView["usuario"].ToString();
                    string nombre = rowView["NombUsuario"].ToString();
                    string claveWS = rowView["clave_ws"].ToString(); // ojo: antes estaba vacío
                    
                    string carpeta = $"{usuario} - {nombre}";
                    string path = Helpers.Utils.ObtenerRutaDescargaPersonalizada(carpeta);
                    string[] claves = Helpers.Utils.LeerClavesDesdeTXT(path);
                    rowView["Claves cargadas"] = claves.Length;

                    LoggerHelper.Log(logPath, $"[{nombre}] 📤 Enviando a EDOC ({claves.Length} claves)");
                    string clavesFilePath = Path.Combine(Helpers.Utils.ObtenerRutaDescargaPersonalizada("CLAVES_ENVIADAS"), $"claves_enviadas_{nombre}.txt");
                    var fechasExistentes = Helpers.Utils.ObtenerFechasClavesEnviadas(clavesFilePath);

                    var resultado = EnviarClavesWS(claveWS, claves, edocUrl);
                   
                    rowView["Resultado"] = resultado.exito;

                    foreach (DataGridViewRow gridRow in grdClientes.Rows)
                    {
                        if (gridRow.Cells["usuario"].Value.ToString() == usuario)
                        {
                            var resultadoCell = gridRow.Cells["Resultado"];

                            if (resultado.exito)
                            {
                                resultadoCell.Value = "OK";
                                resultadoCell.Style.BackColor = Color.Green;
                                resultadoCell.Style.ForeColor = Color.White;
                                _estado = "0";
                                _obervacion = "Carga exitosa";
                                string fechaCarga = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                var lineasSalida = new List<string>(claves.Length);

                                foreach (var clave in claves)
                                {
                                    if (!fechasExistentes.TryGetValue(clave, out string fechaExistente) || string.IsNullOrWhiteSpace(fechaExistente))
                                        fechaExistente = fechaCarga;

                                    lineasSalida.Add($"{clave}\t{fechaExistente}");
                                }

                                File.WriteAllLines(clavesFilePath, lineasSalida, Encoding.UTF8);
                            }
                            else
                            {
                                resultadoCell.Value = "ERROR";
                                resultadoCell.Style.BackColor = Color.Red;
                                resultadoCell.Style.ForeColor = Color.White;
                                _estado = "1";
                                _obervacion = $"Fallo carga, {resultado.mensaje}";
                            }

                            grdClientes.Refresh();
                            Application.DoEvents();   // 🔹 repinta de inmediato
                            await Task.Delay(200);

                            break;
                        }
                    }

                    var trans = new TransactionBot
                    {
                        id_transaction = last_id_trans.ToString(),
                        transaction_order = (orden++).ToString(),
                        resolution_provider = sol_provider,
                        id_cliente = usuario,
                        tipodocumento = "0",
                        mes_consulta = "0",
                        estado = _estado,
                        observacion = _obervacion,
                        saldo = "0",
                        trans_type = "C"
                    };

                    if (writepermission) { var ok = helper.InsertarTransactionAsync(trans); }
                    
                    LoggerHelper.Log(logPath, $"[{nombre}] Resultado EDOC: {(resultado.exito ? "✅" : "❌")} - {resultado.mensaje}");
                }
            }
            else
            {
                LoggerHelper.Log(logPath, "Deteniendo proceso de carga. Sin clientes que cargar.");
            }
            LoggerHelper.Log(logPath, "Fin Ejecución proceso de carga.");
        }

        private (bool exito, string mensaje) EnviarClavesWS(string token, string[] claves, string wslink)
        {
            try
            {
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 10485760,
                    ReaderQuotas = { MaxStringContentLength = 10485760 }
                };
                var endpoint = new EndpointAddress(wslink);
                var client = new EdocServiceReference.WSRAD_KEY_CARGARClient(binding, endpoint);
                var arrayOfClaves = new EdocServiceReference.ArrayOfString();
                arrayOfClaves.AddRange(claves);

                string mensajeSalida = "";
                bool resultado = client.CargarClavesAcceso(token, arrayOfClaves, ref mensajeSalida);

                return (resultado, mensajeSalida);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private void btnAbrirLog_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = Helpers.Utils.ObtenerRutaDescargaPersonalizada("BOT_LOG");
                string logPath = Path.Combine(logDir, "procesoCarga.log");

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

        private void chkToggleAutoCarga_CheckedChanged(object sender, EventArgs e)
        {
            if (chkToggleAutoCarga.Checked)
            {
                BotBackgroundService.Start((int)numIntervalo.Value);
                lblEstado.Text = $"AutoCarga activa (cada {numIntervalo.Value} min)";
                lblEstado.ForeColor = Color.Green;
            }
            else
            {
                BotBackgroundService.Stop();
                lblEstado.Text = "AutoCarga inactiva";
                lblEstado.ForeColor = Color.Red;
            }
        }
    }
}
