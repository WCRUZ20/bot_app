using botapp.Core;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Secundarias
{
    public partial class ConfServicioInterface : UserControl
    {
        private const string SupabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private const string ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";
        private static readonly ConfServicioScheduler Scheduler = new ConfServicioScheduler(SupabaseUrl, ApiKey);

        public ConfServicioInterface()
        {
            InitializeComponent();
            CargarConfiguracion();
            AplicarPlanificacionServicio();
        }

        private void CargarConfiguracion()
        {
            chkServicioActivo.Checked = EsTrue(ConfigurationManager.AppSettings["ServicioActivo"]);
            chkCargaAutomatica.Checked = EsTrue(ConfigurationManager.AppSettings["CargaAutomatica"]);

            if (int.TryParse(ConfigurationManager.AppSettings["FrecuenciaMinutos"], out var frecuencia) &&
                frecuencia >= nudFrecuencia.Minimum && frecuencia <= nudFrecuencia.Maximum)
            {
                nudFrecuencia.Value = frecuencia;
            }

            if (DateTime.TryParseExact(ConfigurationManager.AppSettings["HoraInicio"], "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var horaInicio))
            {
                dtpHoraInicio.Value = DateTime.Today.Add(horaInicio.TimeOfDay);
            }

            if (DateTime.TryParseExact(ConfigurationManager.AppSettings["HoraFin"], "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var horaFin))
            {
                dtpHoraFin.Value = DateTime.Today.Add(horaFin.TimeOfDay);
            }

            var diasActivosLegacy = (ConfigurationManager.AppSettings["DiasActivos"] ?? string.Empty)
                .Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            chkLunes.Checked = EsDiaActivo("DiaActivoLunes", "Lun", diasActivosLegacy);
            chkMartes.Checked = EsDiaActivo("DiaActivoMartes", "Mar", diasActivosLegacy);
            chkMiercoles.Checked = EsDiaActivo("DiaActivoMiercoles", "Mie", diasActivosLegacy);
            chkJueves.Checked = EsDiaActivo("DiaActivoJueves", "Jue", diasActivosLegacy);
            chkViernes.Checked = EsDiaActivo("DiaActivoViernes", "Vie", diasActivosLegacy);
            chkSabado.Checked = EsDiaActivo("DiaActivoSabado", "Sab", diasActivosLegacy);
            chkDomingo.Checked = EsDiaActivo("DiaActivoDomingo", "Dom", diasActivosLegacy);
        }

        private static bool EsDiaActivo(string keyDiaSeparado, string keyLegacy, HashSet<string> diasLegacy)
        {
            var valor = ConfigurationManager.AppSettings[keyDiaSeparado];
            return valor == null ? diasLegacy.Contains(keyLegacy) : EsTrue(valor);
        }

        private static bool EsTrue(string valor)
        {
            return string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase);
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
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            UpdateAppSetting(config, "ServicioActivo", chkServicioActivo.Checked ? "true" : "false");
            UpdateAppSetting(config, "CargaAutomatica", chkCargaAutomatica.Checked ? "true" : "false");
            UpdateAppSetting(config, "FrecuenciaMinutos", nudFrecuencia.Value.ToString(CultureInfo.InvariantCulture));
            UpdateAppSetting(config, "HoraInicio", dtpHoraInicio.Value.ToString("HH:mm", CultureInfo.InvariantCulture));
            UpdateAppSetting(config, "HoraFin", dtpHoraFin.Value.ToString("HH:mm", CultureInfo.InvariantCulture));

            UpdateAppSetting(config, "DiaActivoLunes", chkLunes.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoMartes", chkMartes.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoMiercoles", chkMiercoles.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoJueves", chkJueves.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoViernes", chkViernes.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoSabado", chkSabado.Checked ? "true" : "false");
            UpdateAppSetting(config, "DiaActivoDomingo", chkDomingo.Checked ? "true" : "false");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show("Configuración del servicio guardada correctamente.", "Configuración",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            AplicarPlanificacionServicio();
        }

        private void AplicarPlanificacionServicio()
        {
            var conf = new ServicioConfEjecucion
            {
                ServicioActivo = chkServicioActivo.Checked,
                CargaAutomatica = chkCargaAutomatica.Checked,
                FrecuenciaMinutos = (int)nudFrecuencia.Value,
                HoraInicio = dtpHoraInicio.Value.TimeOfDay,
                HoraFin = dtpHoraFin.Value.TimeOfDay,
                DiasActivos = ConstruirDiasActivos()
            };

            Scheduler.StartOrUpdate(conf);
        }

        private HashSet<DayOfWeek> ConstruirDiasActivos()
        {
            var dias = new HashSet<DayOfWeek>();
            if (chkLunes.Checked) dias.Add(DayOfWeek.Monday);
            if (chkMartes.Checked) dias.Add(DayOfWeek.Tuesday);
            if (chkMiercoles.Checked) dias.Add(DayOfWeek.Wednesday);
            if (chkJueves.Checked) dias.Add(DayOfWeek.Thursday);
            if (chkViernes.Checked) dias.Add(DayOfWeek.Friday);
            if (chkSabado.Checked) dias.Add(DayOfWeek.Saturday);
            if (chkDomingo.Checked) dias.Add(DayOfWeek.Sunday);
            return dias;
        }

    }
}
