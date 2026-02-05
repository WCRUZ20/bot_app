using botapp.Core;
using botapp.Interfaces.Secundarias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace botapp.Interfaces.Primarias
{
    public partial class ModulesInterface : UserControl
    {
        public string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        public string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";

        public event EventHandler CerrarSesion;

        public ConfigBotInterface configBotInterface;
        public ClienteBotInterface clienteBotInterface;
        public HistorialBotInterface historialBotInterface;
        public CargaBotInterface cargaBotInterface;
        public DescargaBotInterface descargaBotInterface;
        public ConfServicioInterface confServicioInterface;
        //public ConsutaBotInterface consutaBotInterface;

        //logo sri
        private Image _imgSriExpanded;
        private Image _imgSriCollapsed;

        //logo edoc
        private Image _imgEdocExpanded;
        private Image _imgEdocCollapsed;

        //imagen usuario    
        private Image _imgUserExpanded;
        private Image _imgUserCollapsed;

        //imagen historial
        private Image _imgHistExpanded;
        private Image _imgHistCollapsed;

        //imagen buscar
        private Image _imgBusExpanded;
        private Image _imgBusCollapsed;

        //imagen configuracion
        private Image _imgConfExpanded;
        private Image _imgConfCollapsed;

        //imagen servicio
        private Image _imgServExpanded;
        private Image _imgServCollapsed;

        private Color botonNormal = Color.FromArgb(44, 44, 62);
        private Color botonHover = Color.FromArgb(59, 130, 246);
        private string _username = "";

        private bool sidebarExpand = true;
        private ToolTip toolTipSidebar;


        public ModulesInterface(string userName, string profileImageFileName)
        {
            InitializeComponent();
            //AplicarEstilos();

            _imgSriExpanded = ScaleToFit(Properties.Resources.sri_logo, 26, 26);
            _imgSriCollapsed = ScaleToFit(Properties.Resources.sri_logo, 65, 55);

            _imgEdocExpanded = ScaleToFit(Properties.Resources.edoc_logo_recorte, 80, 26);  
            _imgEdocCollapsed = ScaleToFit(Properties.Resources.edoc_logo_recorte, 45, 35);

            _imgUserExpanded = ScaleToFit(Properties.Resources.usuario_imagen, 26, 26);
            _imgUserCollapsed = ScaleToFit(Properties.Resources.usuario_imagen, 30, 30);

            _imgHistExpanded = ScaleToFit(Properties.Resources.historial_imagen, 26, 26);
            _imgHistCollapsed = ScaleToFit(Properties.Resources.historial_imagen, 30, 30);

            _imgBusExpanded = ScaleToFit(Properties.Resources.buscar_imagen, 26, 26);
            _imgBusCollapsed = ScaleToFit(Properties.Resources.buscar_imagen, 30, 30);

            _imgConfExpanded = ScaleToFit(Properties.Resources.config_bot, 26, 26);
            _imgConfCollapsed = ScaleToFit(Properties.Resources.config_bot, 30, 30);

            _imgServExpanded = ScaleToFit(Properties.Resources.reloj_logo, 26, 26);
            _imgServCollapsed = ScaleToFit(Properties.Resources.reloj_logo, 30, 30);

            _username = userName;
            lbluser.Text = $"USUARIO: {userName}";

            BotBackgroundService.Configure(supabaseUrl, apiKey, _username);

            toolTipSidebar = new ToolTip();
            toolTipSidebar.AutoPopDelay = 3000;
            toolTipSidebar.InitialDelay = 500;
            toolTipSidebar.ReshowDelay = 500;
            toolTipSidebar.ShowAlways = true;

            if (!string.IsNullOrEmpty(profileImageFileName))
            {
                _ = CargarImagenAsync(profileImageFileName); // llamamos async sin bloquear
            }

            cargaBotInterface = new CargaBotInterface(_username) { Dock = DockStyle.Fill };
            descargaBotInterface = new DescargaBotInterface(_username) { Dock = DockStyle.Fill };
            clienteBotInterface = new ClienteBotInterface() { Dock = DockStyle.Fill };
            historialBotInterface = new HistorialBotInterface() { Dock = DockStyle.Fill };
            configBotInterface = new ConfigBotInterface() { Dock = DockStyle.Fill };
            confServicioInterface = new ConfServicioInterface() { Dock = DockStyle.Fill };
            //consutaBotInterface = new ConsutaBotInterface() { Dock = DockStyle.Fill };

            pnlsecondary.Controls.Add(cargaBotInterface);
            pnlsecondary.Controls.Add(descargaBotInterface);
            pnlsecondary.Controls.Add(clienteBotInterface);
            pnlsecondary.Controls.Add(historialBotInterface);
            pnlsecondary.Controls.Add(configBotInterface);
            pnlsecondary.Controls.Add(confServicioInterface);
            //pnlsecondary.Controls.Add(consutaBotInterface);

            //clienteBotInterface = new ClienteBotInterface();
            //clienteBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Add(clienteBotInterface);

            foreach (Control c in this.Controls)
            {
                if (c is Button btn)
                {
                    if (btn == btnCarga) ApplyButtonLayout(btnCarga, "CARGA", Properties.Resources.edoc_logo_recorte, expanded: true);
                    else if (btn == btnProceso) ApplyButtonLayout(btnProceso, "DESCARGA", Properties.Resources.sri_logo, expanded: true);
                    else if (btn == btnClientes) ApplyButtonLayout(btnClientes, "CLIENTES", Properties.Resources.usuario_imagen, expanded: true);
                    else if (btn == btnHistorial) ApplyButtonLayout(btnHistorial, "HISTORIAL", Properties.Resources.historial_imagen, expanded: true);
                    else if (btn == btnConfBot) ApplyButtonLayout(btnConfBot, "CONFIG. BOT", Properties.Resources.config_bot, expanded: true);
                    else if (btn == btnclavescarga) ApplyButtonLayout(btnConfBot, "CLAVES CARGADAS", Properties.Resources.buscar_imagen, expanded: true);
                    else if (btn == btnConfServicio) ApplyButtonLayout(btnConfServicio, "CONF. SERVICIO", Properties.Resources.reloj_logo, expanded: true);

                    btn.TabStop = false;
                }
            }

            clienteBotInterface.BringToFront();
            ActivarBoton(btnClientes);

            if (sidebarExpand)
            {
                ToggleSidebar();
            }
        }

        private static Image ScaleToFit(Image source, int maxWidth, int maxHeight)
        {
            if (source == null) return null;

            // Mantener proporción
            double ratioW = (double)maxWidth / source.Width;
            double ratioH = (double)maxHeight / source.Height;
            double ratio = Math.Min(ratioW, ratioH);

            int w = Math.Max(1, (int)Math.Round(source.Width * ratio));
            int h = Math.Max(1, (int)Math.Round(source.Height * ratio));

            var bmp = new Bitmap(w, h);
            bmp.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(source, new Rectangle(0, 0, w, h));
            }

            return bmp;
        }

        private async Task CargarImagenAsync(string fileName)
        {
            try
            {
                string url = $"{supabaseUrl}/storage/v1/object/public/imgs/{fileName}";

                using (var client = new HttpClient())
                {
                    // Para bucket privado
                    client.DefaultRequestHeaders.Add("apikey", apiKey);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                    var bytes = await client.GetByteArrayAsync(url);

                    using (var ms = new MemoryStream(bytes))
                    {
                        pctBoxAvatar.Image = Image.FromStream(ms);
                    }
                }
            }
            catch(Exception ex)
            {
                // Opcional: asignar avatar por defecto
                // pctBoxAvatar.Image = Properties.Resources.default_avatar;
            }
        }

        private void btncerrarsesion_Click(object sender, EventArgs e)
        {
            CerrarSesion?.Invoke(this, EventArgs.Empty);
        }

        private void btnConfBot_Click(object sender, EventArgs e)
        {            
            //configBotInterface = new ConfigBotInterface();
            //configBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Clear();
            //pnlsecondary.Controls.Add(configBotInterface);
            //ActivarBoton(btnConfBot);

            configBotInterface.BringToFront();
            ActivarBoton(btnConfBot);
        }

        private void btnConfServicio_Click(object sender, EventArgs e)
        {
            confServicioInterface.BringToFront();
            ActivarBoton(btnConfServicio);
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            //clienteBotInterface = new ClienteBotInterface();
            //clienteBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Clear();
            //pnlsecondary.Controls.Add(clienteBotInterface);
            //ActivarBoton(btnClientes);

            clienteBotInterface.BringToFront();
            ActivarBoton(btnClientes);
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            //historialBotInterface = new HistorialBotInterface();
            //historialBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Clear();
            //pnlsecondary.Controls.Add(historialBotInterface);
            //ActivarBoton(btnHistorial);

            historialBotInterface.BringToFront();
            ActivarBoton(btnHistorial);
        }

        private void btnCarga_Click(object sender, EventArgs e)
        {
            //cargaBotInterface = new CargaBotInterface(_username);
            //cargaBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Clear();
            //pnlsecondary.Controls.Add(cargaBotInterface);
            //ActivarBoton(btnCarga);

            cargaBotInterface.BringToFront();
            cargaBotInterface.ActualizarUCCarga();
            ActivarBoton(btnCarga);
        }

        private void ActivarBoton(Button botonActivo)
        {
            // Restaurar todos los botones al color normal
            foreach (var ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is Button btn && btn != btncerrarsesion)
                {
                    btn.BackColor = botonNormal;
                }
            }

            // Botón activo se queda con el hover
            botonActivo.BackColor = botonHover;
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

        private void btnProceso_Click(object sender, EventArgs e)
        {
            //descargaBotInterface = new DescargaBotInterface(_username);
            //descargaBotInterface.Dock = DockStyle.Fill;
            //pnlsecondary.Controls.Clear();
            //pnlsecondary.Controls.Add(descargaBotInterface);
            //ActivarBoton(btnProceso);

            descargaBotInterface.BringToFront();
            descargaBotInterface.ActualizarUCDescarga();
            ActivarBoton(btnProceso);
        }

        private void btnclavescarga_Click(object sender, EventArgs e)
        {
            //consutaBotInterface.BringToFront();
            ActivarBoton(btnclavescarga);
        }

        private void btnHm_Click(object sender, EventArgs e)
        {
            ToggleSidebar();
        }

        private void ToggleSidebar()
        {
            if (sidebarExpand)
            {
                pnlPrincipal.ColumnStyles[0].Width = 5F;
                pnlPrincipal.ColumnStyles[1].Width = 95F;

                tableLayoutPanel2.ColumnStyles[0].Width = 0F;
                tableLayoutPanel2.ColumnStyles[1].Width = 100F;

                //ApplyButtonLayout(btnCarga, "CARGA", Properties.Resources.edoc_logo_recorte, expanded: false);
               //ApplyButtonLayout(btnProceso, "DESCARGA", Properties.Resources.sri_logo, expanded: false);

                foreach (Control ctrl in tableLayoutPanel1.Controls)
                {
                    if (ctrl is Button btn && (btn != btncerrarsesion && btn != btnHm))
                    {
                        btn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

                        // 🔹 Tooltips aquí
                        if (btn == btnCarga) toolTipSidebar.SetToolTip(btn, "Carga");
                        else if (btn == btnProceso) toolTipSidebar.SetToolTip(btn, "Descarga");
                        else if (btn == btnClientes) toolTipSidebar.SetToolTip(btn, "Clientes");
                        else if (btn == btnHistorial) toolTipSidebar.SetToolTip(btn, "Historial");
                        else if (btn == btnConfBot) toolTipSidebar.SetToolTip(btn, "Configuración Bot");
                        else if (btn == btnclavescarga) toolTipSidebar.SetToolTip(btn, "Claves cargadas");
                        else if (btn == btnConfServicio) toolTipSidebar.SetToolTip(btn, "Conf. Servicio");

                        switch (btn.Text)
                        {
                            case "CARGA":
                                ApplyButtonLayout(btnCarga, "CARGA", _imgEdocCollapsed, expanded: false);
                                continue;
                            case "DESCARGA":
                                ApplyButtonLayout(btnProceso, "DESCARGA", _imgSriCollapsed, expanded: false);
                                continue;
                            case "CLIENTES":
                                ApplyButtonLayout(btnClientes, "CLIENTES", _imgUserCollapsed, expanded: false);
                                continue;
                            case "HISTORIAL":
                                ApplyButtonLayout(btnHistorial, "HISTORIAL", _imgHistCollapsed, expanded: false);
                                continue;
                            case "CONFIG. BOT":
                                ApplyButtonLayout(btnConfBot, "CONFIG. BOT", _imgConfCollapsed, expanded: false);
                                continue;
                            case "CONF. SERVICIO":
                                ApplyButtonLayout(btnConfServicio, "CONF. SERVICIO", _imgServCollapsed, expanded: false);
                                continue;
                            case "CLAVES CARGADAS":
                                ApplyButtonLayout(btnclavescarga, "CLAVES CARGADAS", _imgBusCollapsed, expanded: false);
                                continue;
                        }
                    }
                }
            }
            else
            {
                pnlPrincipal.ColumnStyles[0].Width = 15F;
                pnlPrincipal.ColumnStyles[1].Width = 85F;

                tableLayoutPanel2.ColumnStyles[0].Width = 70F;
                tableLayoutPanel2.ColumnStyles[1].Width = 30F;

                btnCarga.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnProceso.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnClientes.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnHistorial.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnConfBot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnConfServicio.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                btnclavescarga.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

                ApplyButtonLayout(btnCarga, "CARGA", _imgEdocExpanded, expanded: true);
                ApplyButtonLayout(btnProceso, "DESCARGA", _imgSriExpanded, expanded: true);
                ApplyButtonLayout(btnClientes, "CLIENTES", _imgUserExpanded, expanded: true);
                ApplyButtonLayout(btnHistorial, "HISTORIAL", _imgHistExpanded, expanded: true);
                ApplyButtonLayout(btnConfBot, "CONFIG. BOT", _imgConfExpanded, expanded: true);
                ApplyButtonLayout(btnConfServicio, "CONF. SERVICIO", _imgServExpanded, expanded: true);
                ApplyButtonLayout(btnclavescarga, "CLAVES CARGADAS", _imgBusExpanded, expanded: true);

                // 🔹 Limpiar tooltips
                toolTipSidebar.RemoveAll();
            }
            sidebarExpand = !sidebarExpand;
        }

        private void ApplyButtonLayout(Button btn, string text, Image img, bool expanded)
        {
            btn.Image = img;

            if (expanded)
            {
                // Expandido: imagen a la izquierda + texto
                btn.Text = text;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;

                // Ajusta el “aire” entre icono y texto
                btn.Padding = new Padding(12, 0, 0, 0);
            }
            else
            {
                // Colapsado: solo imagen centrada
                btn.Text = "";
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.ImageAlign = ContentAlignment.MiddleCenter;
                btn.TextImageRelation = TextImageRelation.Overlay;
                btn.Padding = new Padding(0);
            }
        }

    }
}
