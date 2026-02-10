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
        private TextBox txtHost;
        private NumericUpDown numPort;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private TextBox txtFrom;
        private TextBox txtTo;
        private CheckBox chkSsl;
        private Label lblEstado;
        private Button btnGuardar;
        private Button btnProbar;

        public EmailConfigForm()
        {
            InitializeComponent();
            InitializeComponent2();
            CargarConfiguracion();
        }

        private void InitializeComponent2()
        {
            Text = "Configuración de Correo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 390);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(16),
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            txtHost = CrearTextBox();
            numPort = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 65535, Value = 587 };
            txtUsuario = CrearTextBox();
            txtPassword = CrearTextBox();
            txtPassword.PasswordChar = '●';
            txtFrom = CrearTextBox();
            txtTo = CrearTextBox();
            chkSsl = new CheckBox { Text = "Usar SSL", Dock = DockStyle.Left, AutoSize = true };

            lblEstado = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnGuardar = new Button
            {
                Text = "Guardar",
                Dock = DockStyle.Right,
                Width = 120,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += btnGuardar_Click;

            btnProbar = new Button
            {
                Text = "Enviar prueba",
                Dock = DockStyle.Right,
                Width = 140,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnProbar.FlatAppearance.BorderSize = 0;
            btnProbar.Click += btnProbar_Click;

            AgregarFila(layout, "Host SMTP", txtHost);
            AgregarFila(layout, "Puerto", numPort);
            AgregarFila(layout, "Usuario", txtUsuario);
            AgregarFila(layout, "Contraseña", txtPassword);
            AgregarFila(layout, "Correo remitente", txtFrom);
            AgregarFila(layout, "Correo destino", txtTo);
            AgregarFila(layout, "Seguridad", chkSsl);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.Controls.Add(lblEstado, 0, 7);
            layout.SetColumnSpan(lblEstado, 2);

            var panelBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            panelBotones.Controls.Add(btnGuardar);
            panelBotones.Controls.Add(btnProbar);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.Controls.Add(panelBotones, 0, 8);
            layout.SetColumnSpan(panelBotones, 2);

            Controls.Add(layout);
        }

        private static TextBox CrearTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)))
            };
        }

        private static void AgregarFila(TableLayoutPanel layout, string etiqueta, Control control)
        {
            int fila = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var label = new Label
            {
                Text = etiqueta,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)))
            };

            layout.Controls.Add(label, 0, fila);
            layout.Controls.Add(control, 1, fila);
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

            UpdateAppSetting(config, "BOT_SMTP_HOST", txtHost.Text.Trim());
            UpdateAppSetting(config, "BOT_SMTP_PORT", numPort.Value.ToString());
            UpdateAppSetting(config, "BOT_SMTP_USER", txtUsuario.Text.Trim());
            UpdateAppSetting(config, "BOT_SMTP_PASSWORD", txtPassword.Text);
            UpdateAppSetting(config, "BOT_SMTP_FROM", txtFrom.Text.Trim());
            UpdateAppSetting(config, "BOT_REPORT_EMAIL_TO", txtTo.Text.Trim());
            UpdateAppSetting(config, "BOT_SMTP_ENABLE_SSL", chkSsl.Checked ? "true" : "false");

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
