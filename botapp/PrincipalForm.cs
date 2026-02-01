using botapp.Helpers;
using botapp.Interfaces.Primarias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp
{
    public partial class PrincipalForm : Form
    {
        private Login login;
        public string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        public string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";
        private Size _lastSize;

        public PrincipalForm()
        {
            InitializeComponent();

            login = new Login();
            login.Dock = DockStyle.Fill;

            login.LoginExitoso += Login_LoginExitoso;

            pnlprincipal.Controls.Add(login);
        }

        private async void Login_LoginExitoso(object sender, string userCode)
        {
            pnlprincipal.Controls.Clear();

            var helper = new SupabaseDbHelper(supabaseUrl, apiKey);

            var (userCodeDb, profileImageUrl) = await helper.GetUserDataAsync(userCode);

            var modulesform = new ModulesInterface(userCodeDb, profileImageUrl);
            modulesform.Dock = DockStyle.Fill;
            modulesform.CerrarSesion += Modules_CerrarSesion;

            pnlprincipal.Controls.Add(modulesform);
        }

        //private void MostrarModulesInterface()
        //{
        //    var modules = new ModulesInterface();
        //    modules.Dock = DockStyle.Fill;
        //    modules.CerrarSesion += Modules_CerrarSesion;
        //    pnlprincipal.Controls.Clear();
        //    pnlprincipal.Controls.Add(modules);
        //}

        private void Modules_CerrarSesion(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Seguro que desea cerrar sesión?",
                                 "Cerrar sesión",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);

            if (result == DialogResult.No) return;

            login = new Login();
            login.Dock = DockStyle.Fill;
            login.LoginExitoso += Login_LoginExitoso;
            pnlprincipal.Controls.Clear();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["usuario"].Value = "";
            config.AppSettings.Settings["clave"].Value = "";
            config.AppSettings.Settings["savecredentials"].Value = "false";
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            login.LimpiarCampos();

            pnlprincipal.Controls.Add(login);
        }

        private void PrincipalForm_Load(object sender, EventArgs e)
        {
            _lastSize = this.Size;
            this.MinimumSize = this.Size;
        }

        private void PrincipalForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.Size.Width < _lastSize.Width || this.Size.Height < _lastSize.Height)
            {
                this.Size = _lastSize;
            }
            else
            {
                _lastSize = this.Size;
            }
        }
    }
}
