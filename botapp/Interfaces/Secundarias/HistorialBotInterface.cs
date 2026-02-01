using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class HistorialBotInterface : UserControl
    {
        private readonly SupabaseDbHelper _dbHelper;
        private List<ClienteBot> _clientes;

        private readonly string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";

        public HistorialBotInterface()
        {
            InitializeComponent();
            llenarCombos();

            dtpDesde.Value = DateTime.Now;
            dtpHasta.Value = DateTime.Now;

            grdHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdHistorial.AllowUserToAddRows = false;
            grdHistorial.ReadOnly = false;
            grdHistorial.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdHistorial.MultiSelect = false;
            grdHistorial.Enabled = true;
            grdHistorial.TabStop = false;
            grdHistorial.BackgroundColor = Color.WhiteSmoke;

            grdHistorial.DataError += (s, e) => { e.Cancel = true; };
            grdHistorial.EnableHeadersVisualStyles = false;
            grdHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
            grdHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            grdHistorial.DefaultCellStyle.Font = new Font("Segoe UI", 7F);
            grdHistorial.DefaultCellStyle.BackColor = Color.White;
            grdHistorial.DefaultCellStyle.SelectionBackColor = Color.White;
            grdHistorial.DefaultCellStyle.SelectionForeColor = Color.Black;
            grdHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            grdHistorial.RowTemplate.Height = 20;
            grdHistorial.GridColor = Color.DarkGray;
            grdHistorial.BorderStyle = BorderStyle.None;
            grdHistorial.RowHeadersVisible = false;
            grdHistorial.ClearSelection();
            grdHistorial.CurrentCell = null;
            //grdHistorial.CellContentClick += grdHistorial_CellContentClick;
            grdHistorial.DefaultCellStyle.SelectionBackColor = grdHistorial.DefaultCellStyle.BackColor;
            grdHistorial.DefaultCellStyle.SelectionForeColor = grdHistorial.DefaultCellStyle.ForeColor;

            //Modificaciones visuales 
            grdHistorial.BackgroundColor = Color.White;//Color.FromArgb(245, 246, 250); // gris claro
            grdHistorial.BorderStyle = BorderStyle.None;
            grdHistorial.GridColor = Color.FromArgb(209, 213, 219); // líneas suaves

            // Encabezados
            grdHistorial.EnableHeadersVisualStyles = false;
            grdHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 47); // azul grisáceo oscuro
            grdHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grdHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);

            // Celdas
            grdHistorial.DefaultCellStyle.BackColor = Color.White;
            grdHistorial.DefaultCellStyle.ForeColor = Color.FromArgb(46, 46, 46);
            grdHistorial.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 255); // azul muy suave
            grdHistorial.DefaultCellStyle.SelectionForeColor = Color.Black;
            grdHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            grdHistorial.ClearSelection();
            grdHistorial.CurrentCell = null;

            label6.ForeColor = Color.FromArgb(30, 30, 47); // texto oscuro
            label6.Font = new Font("Segoe UI", 17F, FontStyle.Bold);

            _dbHelper = new SupabaseDbHelper(supabaseUrl, apiKey);

            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            lstSugerencias.Visible = false;
            lstSugerencias.Click += LstSugerencias_Click;

            this.Load += async (s, e) =>
            {
                await CargarClientesAsync();

                await BuscarTransaccionesAsync(false);
            };

            //_ = CargarTransacciones();

            grdHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdHistorial.Enabled = false;
        }

        private async Task CargarClientesAsync()
        {
            _clientes = await _dbHelper.GetClientesListAsync();
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (_clientes == null || _clientes.Count == 0) return;

            string texto = txtBuscar.Text.ToLower();
            if (string.IsNullOrWhiteSpace(texto))
            {
                lstSugerencias.Visible = false;
                return;
            }

            var resultados = _clientes
                .Where(c => c.Identificacion.Contains(texto) || c.Nombre.ToLower().Contains(texto))
                .Select(c => c.Identificacion + " - " + c.Nombre)
                .ToList();

            if (resultados.Any())
            {
                lstSugerencias.DataSource = resultados;
                lstSugerencias.Visible = true;
            }
            else
            {
                lstSugerencias.Visible = false;
            }
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstSugerencias.Visible)
            {
                lstSugerencias.Focus();
                if (lstSugerencias.Items.Count > 0)
                {
                    lstSugerencias.SelectedIndex = 0;
                }
            }
        }

        private void LstSugerencias_Click(object sender, EventArgs e)
        {
            if (lstSugerencias.SelectedItem != null)
            {
                txtBuscar.Text = lstSugerencias.SelectedItem.ToString();
                lstSugerencias.Visible = false;
            }
        }

        private async Task CargarTransacciones()
        {
            try
            {
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                var dt = await helper.GetTransaccionesBotAsync();

                grdHistorial.DataSource = dt;
                grdHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            {

            }
        }

        private void llenarCombos()
        {
            var estados = new List<TipoEstado>
            {
                new TipoEstado { IdEstado = "0", DesEstado = "Exitoso" },
                new TipoEstado { IdEstado = "1", DesEstado = "Fallido" },
                new TipoEstado { IdEstado = "*", DesEstado = "Todo" },
            };

            var tipodocs = new List<TipoDoc>
            {
                new TipoDoc { IdDoc = "1", DesDoc = "Factura" },
                new TipoDoc { IdDoc = "3", DesDoc = "Nota de crédito" },
                new TipoDoc { IdDoc = "6", DesDoc = "Retención" },
                new TipoDoc { IdDoc = "*", DesDoc = "Todo" },
            };

            var tipotrans = new List<TipoTrans>
            {
                new TipoTrans { IdTrans = "C", DesTrans = "Carga" },
                new TipoTrans { IdTrans = "D", DesTrans = "Descarga" },
                new TipoTrans { IdTrans = "*", DesTrans = "Todo" }
            };

            cmbEstado.DataSource = estados;
            cmbEstado.DisplayMember = "Display";
            cmbEstado.ValueMember = "IdEstado";
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.SelectedValue = "*";

            cmbTipoDoc.DataSource = tipodocs;
            cmbTipoDoc.DisplayMember = "Display";
            cmbTipoDoc.ValueMember = "IdDoc";
            cmbTipoDoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoDoc.SelectedValue = "*";

            cmbTipotrans.DataSource = tipotrans;
            cmbTipotrans.DisplayMember = "Display";
            cmbTipotrans.ValueMember = "IdTrans";
            cmbTipotrans.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipotrans.SelectedValue = "*";
        }

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            await BuscarTransaccionesAsync(true);
        }

        private async Task BuscarTransaccionesAsync(bool boton = false)
        {
            try
            {
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);

                // Valores de filtros
                DateTime desde = dtpDesde.Value.Date;
                DateTime hasta = dtpHasta.Value.Date;
                string idCliente = txtBuscar.Text.Trim();

                // Extraer solo la parte antes del guion, si existe
                if (idCliente.Contains("-"))
                {
                    idCliente = idCliente.Split('-')[0].Trim();
                }

                string tipoDoc = cmbTipoDoc.SelectedValue?.ToString();
                string estado = cmbEstado.SelectedValue?.ToString();
                string tipoTrans = cmbTipotrans.SelectedValue?.ToString();

                // Construir filtros dinámicos para Supabase
                var filtros = new List<string>
                {
                    $"date_transaction=gte.{desde:yyyy-MM-dd}",
                    $"date_transaction=lte.{hasta:yyyy-MM-dd}"
                };

                if (!string.IsNullOrEmpty(idCliente))
                    filtros.Add($"id_cliente=eq.{idCliente}");

                if (!string.IsNullOrEmpty(tipoDoc) && tipoDoc != "*")
                    filtros.Add($"tipodocumento=eq.{tipoDoc}");

                if (!string.IsNullOrEmpty(estado) && estado != "*")
                    filtros.Add($"estado=eq.{estado}");

                if (!string.IsNullOrEmpty(tipoTrans) && tipoTrans != "*")
                    filtros.Add($"trans_type=eq.{tipoTrans}");

                string queryString = string.Join("&", filtros);

                // Obtener transacciones filtradas
                var dt = await helper.GetTransaccionesBotAsync(queryString, boton);

                grdHistorial.DataSource = dt;
                grdHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grdHistorial.ClearSelection();
                grdHistorial.CurrentCell = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar transacciones: " + ex.Message);
            }
        }

    }

    public class TipoEstado
    {
        public string IdEstado { get; set; }
        public string DesEstado { get; set; }

        // Esta propiedad se usará para mostrar en pantalla
        public string Display => $"{IdEstado} - {DesEstado}";
    }

    public class TipoDoc
    {
        public string IdDoc { get; set; }
        public string DesDoc { get; set; }

        // Esta propiedad se usará para mostrar en pantalla
        public string Display => $"{IdDoc} - {DesDoc}";
    }

    public class TipoTrans
    {
        public string IdTrans { get; set; }
        public string DesTrans { get; set; }

        // Esta propiedad se usará para mostrar en pantalla
        public string Display => $"{IdTrans} - {DesTrans}";
    }
}
