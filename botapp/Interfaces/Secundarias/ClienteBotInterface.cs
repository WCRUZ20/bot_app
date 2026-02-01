using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class ClienteBotInterface : UserControl
    {
        private readonly string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";
        private Button btnToggleActivos;
        private ClienteModal clienteModal;

        public ClienteBotInterface()
        {
            InitializeComponent();

            //RedondearBoton(btnAgregar, 2);
            //RedondearBoton(btnElimLote, 2);
            //RedondearBoton(btnGuardar, 2);

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
            grdClientes.DefaultCellStyle.BackColor = Color.White;
            grdClientes.DefaultCellStyle.SelectionBackColor = Color.White;
            grdClientes.DefaultCellStyle.SelectionForeColor = Color.Black;
            grdClientes.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
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
            grdClientes.BackgroundColor = Color.FromArgb(245, 246, 250); // gris claro
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

            //Modificaciones visuales botones 
            // Guardar
            btnGuardar.BackColor = Color.FromArgb(34, 197, 94);   // verde éxito
            btnGuardar.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 163, 74);

            // Agregar
            btnAgregar.BackColor = Color.FromArgb(59, 130, 246);  // azul corporativo
            btnAgregar.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);

            // Eliminar
            btnElimLote.BackColor = Color.FromArgb(239, 68, 68);  // rojo serio
            btnElimLote.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);

            foreach (var btn in new[] { btnGuardar, btnAgregar, btnElimLote })
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                btn.Height = 35;
            }

            label1.ForeColor = Color.FromArgb(30, 30, 47); // texto oscuro
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold);

            grdClientes.DataBindingComplete += (s, e) =>
            {
                try
                {
                    if (grdClientes.Columns.Contains("orden"))
                    {
                        grdClientes.Columns["orden"].ReadOnly = false;
                        grdClientes.Columns["orden"].HeaderText = "Orden proceso";
                    }
                    
                    grdClientes.Columns["usuario"].ReadOnly = true;
                    grdClientes.Columns["usuario"].HeaderText = "Identificación";
                    grdClientes.Columns["NombUsuario"].ReadOnly = true;
                    grdClientes.Columns["NombUsuario"].HeaderText = "Empresa";
                    grdClientes.Columns["dias"].ReadOnly = false;
                    grdClientes.Columns["dias"].HeaderText = "Días permitidos";
                    grdClientes.Columns["meses_ante"].ReadOnly = false;
                    grdClientes.Columns["meses_ante"].HeaderText = "Meses consulta";

                    grdClientes.Columns["ci_adicional"].Visible = false;
                    grdClientes.Columns["clave"].Visible = false;
                    grdClientes.Columns["clave_ws"].Visible = false;

                    if (grdClientes.Columns.Contains("Activo"))
                    {
                        grdClientes.Columns["Activo"].ReadOnly = false;
                        grdClientes.Columns["Activo"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["Activo"].HeaderText = "Activo";
                        grdClientes.Columns["Activo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("carga"))
                    {
                        grdClientes.Columns["carga"].ReadOnly = false;
                        grdClientes.Columns["carga"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["carga"].HeaderText = "Carga";
                        grdClientes.Columns["carga"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("descarga"))
                    {
                        grdClientes.Columns["descarga"].ReadOnly = false;
                        grdClientes.Columns["descarga"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["descarga"].HeaderText = "Descarga";
                        grdClientes.Columns["descarga"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("factura"))
                    {
                        grdClientes.Columns["factura"].ReadOnly = false;
                        grdClientes.Columns["factura"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["factura"].HeaderText = "Factura";
                        grdClientes.Columns["factura"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("notacredito"))
                    {
                        grdClientes.Columns["notacredito"].ReadOnly = false;
                        grdClientes.Columns["notacredito"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["notacredito"].HeaderText = "Nota de crédito";
                        grdClientes.Columns["notacredito"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("retencion"))
                    {
                        grdClientes.Columns["retencion"].ReadOnly = false;
                        grdClientes.Columns["retencion"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["retencion"].HeaderText = "Retencion";
                        grdClientes.Columns["retencion"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("liquidacioncompra"))
                    {
                        grdClientes.Columns["liquidacioncompra"].ReadOnly = false;
                        grdClientes.Columns["liquidacioncompra"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["liquidacioncompra"].HeaderText = "Liquidación de compra";
                        grdClientes.Columns["liquidacioncompra"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("notadebito"))
                    {
                        grdClientes.Columns["notadebito"].ReadOnly = false;
                        grdClientes.Columns["notadebito"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["notadebito"].HeaderText = "Nota de débito";
                        grdClientes.Columns["notadebito"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }

                    if (grdClientes.Columns.Contains("consultar_mes_actual"))
                    {
                        grdClientes.Columns["consultar_mes_actual"].ReadOnly = false;
                        grdClientes.Columns["consultar_mes_actual"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["consultar_mes_actual"].HeaderText = "Mes Actual";
                        grdClientes.Columns["consultar_mes_actual"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        grdClientes.Columns["consultar_mes_actual"].Width = 80;
                    }

                    if (grdClientes.Columns.Contains("Seleccionar"))
                    {
                        grdClientes.Columns["Seleccionar"].ReadOnly = false;
                        grdClientes.Columns["Seleccionar"].CellTemplate = new DataGridViewCheckBoxCell();
                        grdClientes.Columns["Seleccionar"].HeaderText = "";
                        grdClientes.Columns["Seleccionar"].Width = 30;
                        grdClientes.Columns["Seleccionar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        grdClientes.Columns["Seleccionar"].DisplayIndex = 0;
                    }

                    if (!grdClientes.Columns.Contains("Editar"))
                    {
                        DataGridViewImageColumn editColumn = new DataGridViewImageColumn();
                        editColumn.Name = "Editar";
                        editColumn.HeaderText = "";
                        editColumn.Image = Properties.Resources.editar_icono; // aquí colocas tu ícono de lápiz en Resources
                        editColumn.Width = 30;
                        editColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        editColumn.ToolTipText = "Editar";
                        grdClientes.Columns.Add(editColumn);
                        editColumn.DisplayIndex = grdClientes.Columns.Count - 1;
                    }
                    else
                    {
                        grdClientes.Columns["Editar"].DisplayIndex = grdClientes.Columns.Count - 1;
                    }
                }
                catch { }

                //grdClientes.AutoResizeColumns();
                grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            };

            grdClientes.ColumnHeaderMouseDoubleClick += GrdClientes_ColumnHeaderMouseDoubleClick;

            grdClientes.CellContentClick += GrdClientes_CellContentClick;
            //grdClientes.AutoResizeColumns();
            grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            _ = CargarClientes();

        }

        private async Task CargarClientes()
        {
            try
            {
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                var dt = await helper.GetClientesBotAsync();

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

                    if (!dt.Columns.Contains("Seleccionar"))
                    {
                        DataColumn col = new DataColumn("Seleccionar", typeof(bool));
                        dt.Columns.Add(col);
                        col.SetOrdinal(0); // mueve la columna al índice 0
                    }

                    foreach (DataRow row in dt.Rows)
                        row["Seleccionar"] = false;

                    grdClientes.DataSource = dt;

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
                        grdClientes.Columns["orden"].DisplayIndex = 1;
                    }
                    grdClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    //grdClientes.AutoResizeColumns();
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

        private void GrdClientes_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                // Obtener la columna donde se hizo doble clic
                var column = grdClientes.Columns[e.ColumnIndex];

                // Solo aplicar si es una columna de checkbox
                if (column is DataGridViewCheckBoxColumn)
                {
                    // Ver si al menos un valor está en true (para saber si debemos desmarcar o marcar)
                    bool marcar = grdClientes.Rows.Cast<DataGridViewRow>()
                        .Any(r => r.Cells[column.Index].Value != null && !(bool)r.Cells[column.Index].Value);

                    foreach (DataGridViewRow row in grdClientes.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            row.Cells[column.Index].Value = marcar;
                        }
                    }

                    grdClientes.RefreshEdit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al marcar/desmarcar: " + ex.Message);
            }
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Crear lista de clientes según las filas del DataGridView
                List<ClienteBot> lista = new List<ClienteBot>();

                foreach (DataGridViewRow row in grdClientes.Rows)
                {
                    if (row.IsNewRow) continue;

                    lista.Add(new ClienteBot
                    {
                        Identificacion = row.Cells["usuario"].Value?.ToString(),
                        Activo = row.Cells["Activo"].Value != null && (bool)row.Cells["Activo"].Value,
                        Dias = row.Cells["Dias"].Value?.ToString(),
                        MesesAnte = row.Cells["meses_ante"].Value?.ToString(),
                        Carga = row.Cells["carga"].Value != null && (bool)row.Cells["carga"].Value,
                        Descarga = row.Cells["descarga"].Value != null && (bool)row.Cells["descarga"].Value,
                        Factura = row.Cells["factura"].Value != null && (bool)row.Cells["factura"].Value,
                        NotaCredito = row.Cells["notacredito"].Value != null && (bool)row.Cells["notacredito"].Value,
                        Retencion = row.Cells["retencion"].Value != null && (bool)row.Cells["retencion"].Value,
                        LiquidacionCompra = row.Cells["liquidacioncompra"].Value != null && (bool)row.Cells["liquidacioncompra"].Value,
                        NotaDebito = row.Cells["notadebito"].Value != null && (bool)row.Cells["notadebito"].Value,
                        orden = row.Cells["orden"].Value?.ToString(),
                        ConsultarMesActual = grdClientes.Columns.Contains("consultar_mes_actual") &&
                                     row.Cells["consultar_mes_actual"].Value != null &&
                                     (bool)row.Cells["consultar_mes_actual"].Value
                    });
                }

                if (lista.Count == 0)
                {
                    //MessageBox.Show("No hay datos para guardar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MensajeHelper.Mostrar(lblNoti, "No hay datos para guardar.", TipoMensaje.Precaucion);
                    return;
                }

                // Guardar en Supabase
                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                bool ok = await helper.GuardarClientesBotBatchAsync(lista);

                if (ok)
                {
                    //MessageBox.Show("Clientes guardados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Opcional: recargar DataGridView
                    MensajeHelper.Mostrar(lblNoti, "Clientes guardados correctamente.", TipoMensaje.Exito);
                    await CargarClientes();
                }
                else
                {
                    MessageBox.Show("Error al guardar los clientes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            clienteModal = new ClienteModal(supabaseUrl, apiKey);
            clienteModal.btnCreaActua.Text = "CREAR";
            if (clienteModal.ShowDialog() == DialogResult.OK)
                _ = CargarClientes();
        }

        private async void btnElimLote_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> usuariosAEliminar = new List<string>();

                foreach (DataGridViewRow row in grdClientes.Rows)
                {
                    if (row.IsNewRow) continue;

                    bool seleccionado = row.Cells["Seleccionar"].Value != null && (bool)row.Cells["Seleccionar"].Value;
                    if (seleccionado)
                    {
                        usuariosAEliminar.Add(row.Cells["usuario"].Value.ToString());
                    }
                }

                if (usuariosAEliminar.Count == 0)
                {
                    MensajeHelper.Mostrar(lblNoti, "No hay registros seleccionados.", TipoMensaje.Precaucion);
                    return;
                }

                var helper = new SupabaseDbHelper(supabaseUrl, apiKey);
                bool ok = await helper.EliminarClientesBatchAsync(usuariosAEliminar);

                if (ok)
                {
                    MensajeHelper.Mostrar(lblNoti, "Registros eliminados correctamente.", TipoMensaje.Exito);
                    await CargarClientes();
                }
                else
                {
                    MensajeHelper.Mostrar(lblNoti, "Error al eliminar registros.", TipoMensaje.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar registros: " + ex.Message);
            }
        }

        private void GrdClientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // header
            if (grdClientes.Columns[e.ColumnIndex].Name == "Editar")
            {
                DataGridViewRow row = grdClientes.Rows[e.RowIndex];
                clienteModal = new ClienteModal(supabaseUrl, apiKey);

                // Método auxiliar para asignar valores sin fallar
                string GetCellValue(string colName)
                {
                    return grdClientes.Columns.Contains(colName) && row.Cells[colName].Value != null
                        ? row.Cells[colName].Value.ToString()
                        : string.Empty;
                }

                clienteModal.txtUsuario.Text = GetCellValue("usuario");
                clienteModal.txtNombUsuario.Text = GetCellValue("NombUsuario");
                clienteModal.txtAdicional.Text = GetCellValue("ci_adicional");
                clienteModal.txtClave.Text = GetCellValue("clave");
                clienteModal.txtClaveEdoc.Text = GetCellValue("clave_ws");
                clienteModal.txtDias.Text = GetCellValue("dias");
                clienteModal.txtMesesAnte.Text = GetCellValue("meses_ante");
                clienteModal.txtOrden.Text = GetCellValue("orden");

                bool GetCellBool(string colName)
                {
                    return grdClientes.Columns.Contains(colName) && row.Cells[colName].Value != null
                        && bool.TryParse(row.Cells[colName].Value.ToString(), out bool result) && result;
                }

                clienteModal.chkActivo.Checked = GetCellBool("Activo");
                clienteModal.chkCarga.Checked = GetCellBool("carga");
                clienteModal.chkDescarga.Checked = GetCellBool("descarga");
                clienteModal.chkFactura.Checked = GetCellBool("factura");
                clienteModal.chkNotaCredito.Checked = GetCellBool("notacredito");
                clienteModal.chkRetencion.Checked = GetCellBool("retencion");
                clienteModal.chkLiqui.Checked = GetCellBool("liquidacioncompra");
                clienteModal.chkNotaDebito.Checked = GetCellBool("notadebito");
                clienteModal.chkMesActual.Checked = GetCellBool("consultar_mes_actual");

                clienteModal.btnCreaActua.Text = "ACTUALIZAR";

                clienteModal.ShowDialog();

                _ = CargarClientes();
            }
        }

        private void RedondearBoton(Button btn, int radio)
        {
            Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);
            GraphicsPath path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, radio, radio, 180, 90);
            path.AddArc(rect.Right - radio, rect.Y, radio, radio, 270, 90);
            path.AddArc(rect.Right - radio, rect.Bottom - radio, radio, radio, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radio, radio, radio, 90, 90);

            path.CloseAllFigures();
            btn.Region = new Region(path);
        }
    }
}
