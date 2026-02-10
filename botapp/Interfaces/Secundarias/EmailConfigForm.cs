using botapp.Helpers;
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

namespace botapp.Interfaces.Secundarias
{
    internal partial class EmailConfigForm : Form
    {
        
        public EmailConfigForm()
        {
            InitializeComponent();
            CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {
            txtHost.Text = ObtenerValor("BOT_SMTP_HOST", "SmtpHost");
            txtUsuario.Text = ObtenerValor("BOT_SMTP_USER", "SmtpUser");
            txtPassword.Text = ObtenerValor("BOT_SMTP_PASSWORD", "SmtpPassword");
            txtFrom.Text = ObtenerValor("BOT_SMTP_FROM", "SmtpFrom");
            txtTo.Text = ObtenerValor("BOT_REPORT_EMAIL_TO", "ReportEmailTo");

            if (int.TryParse(ObtenerValor("BOT_SMTP_PORT", "SmtpPort"), out int puerto) && puerto > 0)
            {
                numPort.Value = puerto;
            }

            chkSsl.Checked = string.Equals(ObtenerValor("BOT_SMTP_ENABLE_SSL", "SmtpEnableSsl"), "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerValor(string keyPrincipal, string keyAlterna)
        {
            return ConfigurationManager.AppSettings[keyPrincipal]
                ?? ConfigurationManager.AppSettings[keyAlterna]
                ?? string.Empty;
        }

        private void GuardarConfiguracion()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            UpdateAppSetting(config, "SmtpHost", txtHost.Text.Trim());
            UpdateAppSetting(config, "SmtpPort", numPort.Value.ToString());
            UpdateAppSetting(config, "SmtpUser", txtUsuario.Text.Trim());
            UpdateAppSetting(config, "SmtpPassword", txtPassword.Text);
            UpdateAppSetting(config, "SmtpFrom", txtFrom.Text.Trim());
            UpdateAppSetting(config, "ReportEmailTo", txtTo.Text.Trim());
            UpdateAppSetting(config, "SmtpEnableSsl", chkSsl.Checked ? "true" : "false");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void UpdateAppSetting(Configuration config, string key, string value)
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
            GuardarConfiguracion();
            lblEstado.ForeColor = Color.FromArgb(21, 128, 61);
            lblEstado.Text = "Configuración de correo guardada correctamente.";
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            if (EmailReportHelper.TrySendTestEmail(
                txtHost.Text.Trim(),
                (int)numPort.Value,
                txtUsuario.Text.Trim(),
                txtPassword.Text,
                txtFrom.Text.Trim(),
                txtTo.Text.Trim(),
                chkSsl.Checked,
                out string mensaje))
            {
                lblEstado.ForeColor = Color.FromArgb(21, 128, 61);
                lblEstado.Text = mensaje;
            }
            else
            {
                lblEstado.ForeColor = Color.FromArgb(185, 28, 28);
                lblEstado.Text = mensaje;
            }
        }
    }

}
