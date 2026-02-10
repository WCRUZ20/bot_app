using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Helpers
{
    internal static class EmailReportHelper
    {
        public static bool TrySendTestEmail(string host, int port, string usuario, string clave, string remitente, string destinatario, bool enableSsl, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(destinatario))
            {
                mensaje = "Complete Host, From y Destinatario para realizar el envío de prueba.";
                return false;
            }

            if (port <= 0)
            {
                port = 587;
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(remitente);
                    message.To.Add(destinatario.Trim());
                    message.Subject = "Prueba de configuración de correo BOT";
                    message.Body = $"Correo de prueba generado por BOT.\nFecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = enableSsl;

                        if (!string.IsNullOrWhiteSpace(usuario))
                        {
                            smtp.Credentials = new NetworkCredential(usuario, clave ?? string.Empty);
                        }
                        else
                        {
                            smtp.UseDefaultCredentials = true;
                        }

                        smtp.Send(message);
                    }
                }

                mensaje = "Correo de prueba enviado correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = $"No se pudo enviar el correo de prueba: {ex.Message}";
                return false;
            }
        }

        public static bool TrySendPdfReport(string pdfPath, string proceso, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
            {
                mensaje = "No se encontró el PDF para enviar por correo.";
                return false;
            }

            string host = ObtenerValorConfiguracion("SmtpHost", "BOT_SMTP_HOST");
            string portRaw = ObtenerValorConfiguracion("SmtpPort", "BOT_SMTP_PORT");
            string usuario = ObtenerValorConfiguracion("SmtpUser", "BOT_SMTP_USER");
            string clave = ObtenerValorConfiguracion("SmtpPassword", "BOT_SMTP_PASSWORD");
            string remitente = ObtenerValorConfiguracion("SmtpFrom", "BOT_SMTP_FROM");
            string destinatariosRaw = ObtenerValorConfiguracion("ReportEmailTo", "BOT_REPORT_EMAIL_TO");
            string sslRaw = ObtenerValorConfiguracion("SmtpEnableSsl", "BOT_SMTP_ENABLE_SSL");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(destinatariosRaw))
            {
                mensaje = "Configuración de correo incompleta. Defina SmtpHost/SmtpFrom/ReportEmailTo (o variables BOT_SMTP_*).";
                return false;
            }

            int port = 587;
            if (!string.IsNullOrWhiteSpace(portRaw))
            {
                int.TryParse(portRaw, out port);
                if (port <= 0)
                {
                    port = 587;
                }
            }

            bool enableSsl = true;
            if (!string.IsNullOrWhiteSpace(sslRaw))
            {
                bool.TryParse(sslRaw, out enableSsl);
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(remitente);

                    foreach (string correo in destinatariosRaw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        message.To.Add(correo.Trim());
                    }

                    message.Subject = $"Reporte BOT - {proceso}";
                    message.Body = $"Se adjunta el reporte PDF generado para el proceso de {proceso}.\nFecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    message.Attachments.Add(new Attachment(pdfPath));

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = enableSsl;

                        if (!string.IsNullOrWhiteSpace(usuario))
                        {
                            smtp.Credentials = new NetworkCredential(usuario, clave ?? string.Empty);
                        }
                        else
                        {
                            smtp.UseDefaultCredentials = true;
                        }

                        smtp.Send(message);
                    }
                }

                mensaje = "Correo enviado correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = $"No se pudo enviar el correo: {ex.Message}";
                return false;
            }
        }

        private static string ObtenerValorConfiguracion(string keyConfig, string keyEnv)
        {
            string valorConfig = ConfigurationManager.AppSettings[keyConfig];
            if (!string.IsNullOrWhiteSpace(valorConfig))
            {
                return valorConfig;
            }

            string valorAlterno = ConfigurationManager.AppSettings[keyEnv];
            if (!string.IsNullOrWhiteSpace(valorAlterno))
            {
                return valorAlterno;
            }

            return Environment.GetEnvironmentVariable(keyEnv);
        }
    }
}
