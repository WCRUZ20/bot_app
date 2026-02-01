using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Primarias
{
    public partial class Login : UserControl
    {
        public event EventHandler<string> LoginExitoso;
        public string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        public string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";

        public Login()
        {
            InitializeComponent();

            //btnVerClave.BackColor = Color.FromArgb(0, 123, 255);

            

            btnVerClave.BackColor = Color.Transparent;
            btnVerClave.ForeColor = Color.LightGray;
            btnVerClave.FlatAppearance.BorderSize = 1;
            btnVerClave.FlatAppearance.BorderColor = Color.LightGray;

            btningresar.Enabled = InternetHelper.HayConexionInternet();

            string usuario_ = ConfigurationManager.AppSettings["usuario"] ?? "";
            string clave_ = ConfigurationManager.AppSettings["clave"] ?? "";
            string saveCreds = ConfigurationManager.AppSettings["savecredentials"] ?? "false";

            if (usuario_.Length > 0 && clave_.Length > 0 && saveCreds == "true")
            {
                txtusuario.Text = usuario_;
                txtclave.Text = clave_;
                chkgurdcontr.Checked = true;
                AutoLogin();
            }

            foreach (Control c in this.Controls)
            {
                if (c is Button b)
                {
                    b.TabStop = false;
                }
            }

            //this.ActiveControl = txtusuario;
            //txtusuario.Focus();

            //txtusuario.TabIndex = 0;
            //txtclave.TabIndex = 1;
            //btnVerClave.TabIndex = 2;
            //chkgurdcontr.TabIndex = 3;
            //btningresar.TabIndex = 4;

            //btncerrar.TabIndex = 5;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BeginInvoke((Action)(() => txtusuario.Focus()));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btningresar.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void btningresar_Click(object sender, EventArgs e)
        {
            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);

            bool validaconnsupa = await helper.TestConnectionAsync();

            if (!validaconnsupa)
            {
                //MessageBox.Show("Sin conexión Supabase");
                MensajeHelper.Mostrar(lblmsg, "Sin conexión Supabase", TipoMensaje.Error);
                //lblmsg.Text = "Sin conexión Supabase";
                //lblmsg.ForeColor = Color.White;
                //lblmsg.BackColor = Color.Red;
            }
            else
            {
                bool autenticado = await helper.AuthenticateAsync(txtusuario.Text, txtclave.Text);

                if (autenticado)
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (chkgurdcontr.Checked)
                    {
                        config.AppSettings.Settings["usuario"].Value = txtusuario.Text;
                        config.AppSettings.Settings["clave"].Value = txtclave.Text;
                        config.AppSettings.Settings["savecredentials"].Value = "true";
                    }
                    else
                    {
                        config.AppSettings.Settings["usuario"].Value = "";
                        config.AppSettings.Settings["clave"].Value = "";
                        config.AppSettings.Settings["savecredentials"].Value = "false";
                    }

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");

                    LoginExitoso?.Invoke(this, txtusuario.Text);
                }
                else
                {
                    //MessageBox.Show("❌ Usuario o contraseña incorrectos");

                    MensajeHelper.Mostrar(lblmsg, "❌ Usuario o contraseña incorrectos", TipoMensaje.Error);
                    //lblmsg.Text = "❌ Usuario o contraseña incorrectos";
                    //lblmsg.ForeColor = Color.White;
                    //lblmsg.BackColor = Color.Red;
                }
            }
        }

        private void btnVerClave_Click(object sender, EventArgs e)
        {
            txtclave.PasswordChar = (txtclave.PasswordChar == '*') ? '\0' : '*';
            if(txtclave.PasswordChar == '*')
            {
                btnVerClave.BackColor = Color.FromArgb(0, 105, 217);
                btnVerClave.BackColor = Color.Transparent;
                btnVerClave.ForeColor = Color.LightGray;
                btnVerClave.FlatAppearance.BorderSize = 1;
                btnVerClave.FlatAppearance.BorderColor = Color.LightGray;
            }
            else
            {
                btnVerClave.BackColor = Color.FromArgb(0, 123, 255);
                btnVerClave.ForeColor = Color.White;
                btnVerClave.FlatAppearance.BorderSize = 0;
            }
        }

        private async void AutoLogin()
        {
            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);

            bool validaconnsupa = await helper.TestConnectionAsync();
            if (!validaconnsupa)
            {
                //MessageBox.Show("Sin conexión Supabase");
                MensajeHelper.Mostrar(lblmsg, "Sin conexión Supabase", TipoMensaje.Error);
                //lblmsg.Text = "Sin conexión Supabase";
                //lblmsg.ForeColor = Color.White;
                //lblmsg.BackColor = Color.Red;
                return;
            }

            bool autenticado = await helper.AuthenticateAsync(txtusuario.Text, txtclave.Text);
            if (autenticado)
            {
                LoginExitoso?.Invoke(this, txtusuario.Text);
            }
        }

        public void LimpiarCampos()
        {
            txtusuario.Text = "";
            txtclave.Text = "";
            chkgurdcontr.Checked = false;
        }

        private void btncerrar_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
