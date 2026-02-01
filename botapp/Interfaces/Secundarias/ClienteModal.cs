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
    public partial class ClienteModal : Form
    {
        private string _supabaseUrl;
        private string _apikey;
        public ClienteModal(string supabaseUrl, string apikey)
        {
            InitializeComponent();
            _supabaseUrl = supabaseUrl;
            _apikey = apikey;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnCreaActua_Click(object sender, EventArgs e)
        {
            var helper = new SupabaseDbHelper(_supabaseUrl,
                                      _apikey);

            var cliente = new ClienteBot
            {
                Identificacion = txtUsuario.Text,
                Nombre = txtNombUsuario.Text,
                Adicional = txtAdicional.Text,
                Clave = txtClave.Text,
                ClaveEdoc = txtClaveEdoc.Text,
                Dias = txtDias.Text,
                MesesAnte = txtMesesAnte.Text,
                Activo = chkActivo.Checked,
                Carga = chkCarga.Checked,
                Descarga = chkDescarga.Checked,
                Factura = chkFactura.Checked,
                NotaCredito = chkNotaCredito.Checked,
                Retencion = chkRetencion.Checked,
                LiquidacionCompra = chkLiqui.Checked,
                NotaDebito = chkNotaDebito.Checked,
                orden = txtOrden.Text,
                ConsultarMesActual = chkMesActual.Checked,
            };

            bool ok = false;

            if (btnCreaActua.Text == "CREAR")
            {
                ok = await helper.InsertarClienteAsync(cliente);
            }
            else if (btnCreaActua.Text == "ACTUALIZAR")
            {
                // Usamos PATCH existente
                ok = await helper.GuardarClientesBotBatchAsync(new List<ClienteBot> { cliente }, true);
            }

            if (ok)
            {
                MessageBox.Show("Operación realizada con éxito.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Error al guardar.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
