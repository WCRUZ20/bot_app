using botapp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class ConfigBotInterface : UserControl
    {
        public ConfigBotInterface()
        {
            InitializeComponent();
            CargarConfiguracion();

            chkInteraccionHumana.CheckedChanged += CheckExclusive_CheckedChanged;
            chkUse2Captcha.CheckedChanged += CheckExclusive_CheckedChanged;
            chkUseCapSolver.CheckedChanged += CheckExclusive_CheckedChanged;
        }

        private void CargarConfiguracion()
        {
            txtSriUrl.Text = ConfigurationManager.AppSettings["SRI_LoginUrl"] ?? "https://srienlinea.sri.gob.ec/tuportal-internet/accederAplicacion.jspa?redireccion=57&idGrupo=55";
            txtEdocUrl.Text = ConfigurationManager.AppSettings["EDOC_Endpoint"] ?? "";
            txtRutaDirectorio.Text = ConfigurationManager.AppSettings["RutaDirectorio"] ?? "";
            txtRutaDirNP.Text = ConfigurationManager.AppSettings["RutaDirNP"] ?? "";
            txtToken2Captcha.Text = ConfigurationManager.AppSettings["TwoCaptchaApiKey"] ?? "";
            txtTokenCapSolver.Text = ConfigurationManager.AppSettings["CapSolverApiKey"] ?? "";
            chkHeadless.Checked = ConfigurationManager.AppSettings["Headless"] == "true";
            chkInteraccionHumana.Checked = ConfigurationManager.AppSettings["HumanInteraction"] == "true";
            chkUse2Captcha.Checked = ConfigurationManager.AppSettings["2captcha"] == "true";
            chkUseCapSolver.Checked = ConfigurationManager.AppSettings["Capsolver"] == "true";
            numericAdvertencia.Value = decimal.Parse(ConfigurationManager.AppSettings["numAdvertencia"]);
            numericBloqueo.Value = decimal.Parse(ConfigurationManager.AppSettings["numBloqueo"]);
            txtNamebtn.Text = ConfigurationManager.AppSettings["NombreBotonSRI"] ?? "";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.ValidateNames = false;
                dialog.CheckFileExists = false;
                dialog.CheckPathExists = true;
                dialog.FileName = "Seleccionar carpeta"; // Texto fantasma

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.GetDirectoryName(dialog.FileName);
                    txtRutaDirectorio.Text = path;
                }
            }
        }


        private void UpdateAppSetting(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if(!chkInteraccionHumana.Checked && !chkUse2Captcha.Checked && !chkUseCapSolver.Checked)
            {

                MensajeHelper.Mostrar(lblNoti, "Seleccionar al menos un medio de resolución del captcha", TipoMensaje.Precaucion);
            }
            else
            {
                // Update or add settings
                UpdateAppSetting(config, "SRI_LoginUrl", txtSriUrl.Text);
                UpdateAppSetting(config, "EDOC_Endpoint", txtEdocUrl.Text);
                UpdateAppSetting(config, "RutaDirectorio", txtRutaDirectorio.Text);
                UpdateAppSetting(config, "RutaDirNP", txtRutaDirNP.Text);
                UpdateAppSetting(config, "TwoCaptchaApiKey", txtToken2Captcha.Text);
                UpdateAppSetting(config, "CapSolverApiKey", txtTokenCapSolver.Text);
                UpdateAppSetting(config, "Headless", chkHeadless.Checked ? "true" : "false");
                UpdateAppSetting(config, "HumanInteraction", chkInteraccionHumana.Checked ? "true" : "false");
                UpdateAppSetting(config, "2captcha", chkUse2Captcha.Checked ? "true" : "false");
                UpdateAppSetting(config, "Capsolver", chkUseCapSolver.Checked ? "true" : "false");
                UpdateAppSetting(config, "numAdvertencia", numericAdvertencia.Value.ToString());
                UpdateAppSetting(config, "numBloqueo", numericBloqueo.Value.ToString());
                UpdateAppSetting(config, "NombreBotonSRI", txtNamebtn.Text);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                MensajeHelper.Mostrar(lblNoti, "Configuración guardada correctamente", TipoMensaje.Exito);
            }
        }

        private void CheckExclusive_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox changed)) return;

            if (changed.Checked)
            {
                // Si uno se marca, desmarcar los otros
                if (changed != chkInteraccionHumana) chkInteraccionHumana.Checked = false;
                if (changed != chkUse2Captcha) chkUse2Captcha.Checked = false;
                if (changed != chkUseCapSolver) chkUseCapSolver.Checked = false;
            }
        }

        private void btnBuscarNP_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Title = "Selecciona el ejecutable de Notepad++";
                openFile.Filter = "Ejecutables (*.exe)|*.exe";
                openFile.InitialDirectory = @"C:\"; // puedes poner Program Files como predeterminado
                openFile.RestoreDirectory = true;

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    txtRutaDirNP.Text = openFile.FileName; // ruta completa al .exe
                }
            }
        }
    }
}
