using botapp.Helpers;
using botapp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace botapp.Interfaces.Secundarias
{
    public partial class ConsutaBotInterface : UserControl
    {
        private readonly string supabaseUrl = "https://hgwbwaisngbyzaatwndb.supabase.co";
        private readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhnd2J3YWlzbmdieXphYXR3bmRiIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTcwNDIwMDYsImV4cCI6MjA3MjYxODAwNn0.WgHmnqOwKCvzezBM1n82oSpAMYCT5kNCb8cLGRMIsbk";
        private SupabaseDbHelper helper;
        private string xmlinternoglobal;
        public SAPbobsCOM.Company _oCompany;
        public ConnectSAP _connectSAP;
        private string _compania = ConfigurationManager.AppSettings["DevDatabase"];
        private string _usuario = ConfigurationManager.AppSettings["DevSBOUser"];
        private string _clave = ConfigurationManager.AppSettings["DevSBOPassword"];

        public ConsutaBotInterface()
        {
            InitializeComponent();

            helper = new SupabaseDbHelper(supabaseUrl, apiKey);

            ToolTip toolTip = new ToolTip();

            // Propiedades opcionales
            toolTip.AutoPopDelay = 5000;   // Tiempo visible
            toolTip.InitialDelay = 500;    // Retraso inicial
            toolTip.ReshowDelay = 500;     // Retraso entre reapariciones
            toolTip.ShowAlways = true;     // Se muestra aunque el formulario no tenga foco

            // Asignar texto a los botones
            toolTip.SetToolTip(btnExpandir, "Expandir todo");
            toolTip.SetToolTip(btnComprimir, "Comprimir todo");

            #region
            //grdClaves.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //grdClaves.AllowUserToAddRows = false;
            //grdClaves.ReadOnly = false;
            //grdClaves.SelectionMode = DataGridViewSelectionMode.CellSelect;
            //grdClaves.MultiSelect = false;
            //grdClaves.Enabled = true;
            //grdClaves.TabStop = false;
            //grdClaves.BackgroundColor = Color.WhiteSmoke;

            //grdClaves.DataError += (s, e) => { e.Cancel = true; };
            //grdClaves.EnableHeadersVisualStyles = false;
            //grdClaves.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
            //grdClaves.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            //grdClaves.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            //grdClaves.DefaultCellStyle.Font = new Font("Segoe UI", 7F);
            ////grdClaves.DefaultCellStyle.BackColor = Color.White;
            ////grdClaves.DefaultCellStyle.SelectionBackColor = Color.White;
            ////grdClaves.DefaultCellStyle.SelectionForeColor = Color.Black;
            ////grdClaves.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            //grdClaves.RowTemplate.Height = 20;
            //grdClaves.GridColor = Color.DarkGray;
            //grdClaves.BorderStyle = BorderStyle.None;
            //grdClaves.RowHeadersVisible = false;
            //grdClaves.ClearSelection();
            //grdClaves.CurrentCell = null;
            ////grdClaves.CellContentClick += grdClaves_CellContentClick;
            //grdClaves.DefaultCellStyle.SelectionBackColor = grdClaves.DefaultCellStyle.BackColor;
            //grdClaves.DefaultCellStyle.SelectionForeColor = grdClaves.DefaultCellStyle.ForeColor;

            ////Modificaciones visuales 
            //grdClaves.BackgroundColor = Color.White;//Color.FromArgb(245, 246, 250); // gris claro
            //grdClaves.BorderStyle = BorderStyle.None;
            //grdClaves.GridColor = Color.FromArgb(209, 213, 219); // líneas suaves

            //// Encabezados
            //grdClaves.EnableHeadersVisualStyles = false;
            //grdClaves.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 47); // azul grisáceo oscuro
            //grdClaves.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            //grdClaves.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7F, FontStyle.Bold);

            //// Celdas
            //grdClaves.DefaultCellStyle.BackColor = Color.White;
            //grdClaves.DefaultCellStyle.ForeColor = Color.FromArgb(46, 46, 46);
            //grdClaves.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 255); // azul muy suave
            //grdClaves.DefaultCellStyle.SelectionForeColor = Color.Black;
            //grdClaves.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            #endregion

            treeClaves.AfterSelect += treeClaves_AfterSelect;

            //cargaGrid();
            cargaTree();
        }

        private void cargaTree()
        {
            try
            {
                string clavesFileRootPath = Helpers.Utils.ObtenerRutaDescargaPersonalizada("CLAVES_ENVIADAS");

                if (!System.IO.Directory.Exists(clavesFileRootPath))
                {
                    MessageBox.Show("El directorio de claves no existe: " + clavesFileRootPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                treeClaves.Nodes.Clear(); // tu control TreeView en el diseñador
                treeClaves.Font = new Font("Segoe UI", 8F);
                treeClaves.BackColor = Color.White;
                treeClaves.ForeColor = Color.Black;
                treeClaves.ShowLines = false;

                string[] archivos = System.IO.Directory.GetFiles(clavesFileRootPath, "claves_enviadas_*.txt");

                foreach (string archivo in archivos)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(archivo);
                    string cliente = fileName.Replace("claves_enviadas_", "").Trim();

                    TreeNode clienteNode = new TreeNode(cliente);
                    clienteNode.NodeFont = new Font("Segoe UI", 8F, FontStyle.Bold);
                    clienteNode.Expand(); // opcional: mostrar abierto por defecto

                    string[] claves = System.IO.File.ReadAllLines(archivo, Encoding.UTF8);

                    foreach (string clave in claves)
                    {
                        string claveValor = Helpers.Utils.ExtraerClaveEnviada(clave);
                        if (!string.IsNullOrWhiteSpace(claveValor))
                        {
                            TreeNode claveNode = new TreeNode(claveValor);
                            clienteNode.Nodes.Add(claveNode);
                        }
                    }

                    treeClaves.Nodes.Add(clienteNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las claves: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExpandir_Click(object sender, EventArgs e)
        {
            treeClaves.BeginUpdate();
            treeClaves.ExpandAll();
            treeClaves.EndUpdate();
        }

        private void btnComprimir_Click(object sender, EventArgs e)
        {
            treeClaves.BeginUpdate();
            treeClaves.CollapseAll();
            treeClaves.EndUpdate();
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                string clienteFiltro = txtCliente.Text.Trim();
                string claveFiltro = txtClave.Text.Trim();

                string clavesFileRootPath = Helpers.Utils.ObtenerRutaDescargaPersonalizada("CLAVES_ENVIADAS");

                if (!System.IO.Directory.Exists(clavesFileRootPath))
                {
                    MessageBox.Show("El directorio de claves no existe: " + clavesFileRootPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(clienteFiltro) && string.IsNullOrWhiteSpace(claveFiltro))
                {
                    cargaTree();
                    return;
                }

                treeClaves.BeginUpdate(); // ✅ Mejora de rendimiento
                treeClaves.Nodes.Clear();
                treeClaves.Font = new Font("Segoe UI", 8F);
                treeClaves.BackColor = Color.White;
                treeClaves.ForeColor = Color.Black;
                treeClaves.ShowLines = false;
                treeClaves.CheckBoxes = true;

                string[] archivos = System.IO.Directory.GetFiles(clavesFileRootPath, "claves_enviadas_*.txt");
                bool encontrado = false;
                TreeNode primerNodoEncontrado = null; // ✅ Para guardar el primer nodo coincidente

                foreach (string archivo in archivos)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(archivo);
                    string cliente = fileName.Replace("claves_enviadas_", "").Trim();

                    // Si hay filtro por cliente y no coincide → continuar
                    if (!string.IsNullOrWhiteSpace(clienteFiltro) && !cliente.ToLower().Contains(clienteFiltro.ToLower()))
                        continue;

                    string[] claves = System.IO.File.ReadAllLines(archivo, Encoding.UTF8);

                    TreeNode clienteNode = new TreeNode(cliente);
                    clienteNode.NodeFont = new Font("Segoe UI", 8F, FontStyle.Bold);

                    foreach (string clave in claves)
                    {
                        string claveValor = Helpers.Utils.ExtraerClaveEnviada(clave);
                        if (string.IsNullOrWhiteSpace(claveValor))
                            continue;

                        TreeNode claveNode = new TreeNode(claveValor);

                        // ✅ Si hay filtro por clave → resaltar coincidencias
                        if (!string.IsNullOrWhiteSpace(claveFiltro) && claveValor.Contains(claveFiltro))
                        {
                            claveNode.NodeFont = new Font("Segoe UI", 8F, FontStyle.Bold);
                            claveNode.ForeColor = Color.RoyalBlue;
                            encontrado = true;

                            // ✅ Guardar el primer nodo encontrado para seleccionarlo después
                            if (primerNodoEncontrado == null)
                            {
                                primerNodoEncontrado = claveNode;
                            }
                        }

                        clienteNode.Nodes.Add(claveNode);
                    }

                    // Mostrar solo clientes que tengan al menos una coincidencia
                    if (clienteNode.Nodes.Cast<TreeNode>().Any(n => n.ForeColor == Color.RoyalBlue) || string.IsNullOrWhiteSpace(claveFiltro))
                    {
                        treeClaves.Nodes.Add(clienteNode);
                    }
                }

                treeClaves.ExpandAll(); // ✅ Expandir todo primero
                treeClaves.EndUpdate(); // ✅ Terminar actualización masiva

                // ✅ AHORA SÍ seleccionar y hacer visible el primer nodo encontrado
                if (primerNodoEncontrado != null)
                {
                    treeClaves.SelectedNode = primerNodoEncontrado;
                    primerNodoEncontrado.EnsureVisible();
                    treeClaves.Focus();

                    // ✅ Opcional: resaltar con color de selección del sistema
                    primerNodoEncontrado.BackColor = SystemColors.Highlight;
                    primerNodoEncontrado.ForeColor = SystemColors.HighlightText;
                }

                if (!encontrado && !string.IsNullOrWhiteSpace(claveFiltro))
                {
                    MessageBox.Show("No se encontraron claves que coincidan con el filtro.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void treeClaves_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string claveSeleccionada = e.Node.Text;
            string cliente = e.Node.Parent != null ? e.Node.Parent.Text : string.Empty;

            if (!string.IsNullOrEmpty(claveSeleccionada))
            {
                try
                {
                    webBrowserXML.DocumentText = "<html><body><p style='font-family:Segoe UI; font-size:12px;'>Consultando SRI...</p></body></html>";

                    string xml = await ConsultarSRIAsync(claveSeleccionada);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    XmlNode nodeEstado = doc.SelectSingleNode("//estado");
                    lblEstado.Text = nodeEstado.InnerText.Trim();
                    XmlNode nodeFechAut = doc.SelectSingleNode("//fechaAutorizacion");
                    lblFechAut.Text = nodeFechAut.InnerText.Trim();
                    XmlNode nodeAmbiente = doc.SelectSingleNode("//ambiente");
                    lblAmbiente.Text = nodeAmbiente.InnerText.Trim();

                    // Buscar el nodo <comprobante>
                    XmlNode comprobanteNode = doc.SelectSingleNode("//comprobante");
                    if (comprobanteNode == null)
                    {
                        MessageBox.Show("No se encontró la etiqueta <comprobante> en el XML.");
                        return;
                    }

                    // Extraer contenido del CDATA (que contiene otro XML)
                    string xmlInterno = comprobanteNode.InnerText.Trim();
                    xmlinternoglobal = xmlInterno;

                    XmlDocument xmlComprobante = new XmlDocument();
                    xmlComprobante.LoadXml(xmlInterno);

                    XmlNode infotributaria = xmlComprobante.SelectSingleNode("//infoTributaria");
                    XmlNode infofactura = xmlComprobante.SelectSingleNode("//infoFactura");
                    XmlNode detalles = xmlComprobante.SelectSingleNode("//detalles");
                    XmlNode infoadicional = xmlComprobante.SelectSingleNode("//infoAdicional");

                    // Guardar en archivo temporal
                    string tempPath = Path.Combine(Path.GetTempPath(), $"{claveSeleccionada}.xml");
                    File.WriteAllText(tempPath, xmlInterno);

                    // Cargar en el WebBrowser
                    webBrowserXML.Navigate(tempPath);

                    string token = await helper.GetClaveWSEdocClientesAsync(cliente);

                    //se comento porque no puedo consumir el metodo de validar clave
                    //if (!string.IsNullOrEmpty(token))
                    //    await MostrarPDFDesdeXMLAsync(claveSeleccionada, xml, token);
                }
                catch (Exception ex)
                {
                    webBrowserXML.DocumentText = $"<html><body><p style='font-family:Segoe UI; font-size:12px;'>Error al consultar el SRI: {ex.Message}</p></body></html>";
                }
            }
        }

        private async Task MostrarPDFDesdeXMLAsync(string clave, string xml, string _token)
        {
            try
            {
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BotApp_XMLSRI");
                if (!System.IO.Directory.Exists(tempDir))
                    System.IO.Directory.CreateDirectory(tempDir);

                string xmlPath = System.IO.Path.Combine(tempDir, clave + ".xml");
                System.IO.File.WriteAllText(xmlPath, xml, Encoding.UTF8);

                //convertir a bytes Base64
                byte[] xmlBytes = System.IO.File.ReadAllBytes(xmlPath);

                string token = _token; 

                var (pdfBytes, mensaje) = ValidarClavesWS(token, xmlBytes);

                var (estado, mensajexml) = ValidarClavesXMLWS(token, xmlBytes);

                if (estado) { MessageBox.Show($"XML Consultado correctamente {mensajexml}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information); } else { MessageBox.Show($"Error {mensajexml}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); };

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    MessageBox.Show("No se generó PDF.\n" + mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string pdfPath = System.IO.Path.Combine(tempDir, clave + ".pdf");
                System.IO.File.WriteAllBytes(pdfPath, pdfBytes);

                if (webBrowserPDF.InvokeRequired)
                {
                    webBrowserPDF.Invoke(new Action(() => webBrowserPDF.Navigate(pdfPath)));
                }
                else
                {
                    webBrowserPDF.Navigate(pdfPath);
                }

                if (tabControl.SelectedTab != tabPdf)
                {
                    tabControl.SelectedTab = tabPdf;
                }

                MessageBox.Show("PDF generado y mostrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Método para aplicar colores al XML
        private async Task<string> ConsultarSRIAsync(string claveAcceso)
        {
            string url = "https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";

            string soapBody = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
                                  xmlns:ws=""http://ec.gob.sri.ws.autorizacion"">
                  <soapenv:Header/>
                  <soapenv:Body>
                    <ws:autorizacionComprobante>
                      <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
                    </ws:autorizacionComprobante>
                  </soapenv:Body>
                </soapenv:Envelope>";

            using (var client = new HttpClient())
            {
                var content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        private async void btnXML_Click(object sender, EventArgs e)
        {
            try
            {
                if (webBrowserXML.Url == null || string.IsNullOrWhiteSpace(webBrowserXML.Url.LocalPath))
                {
                    MessageBox.Show("No hay XML para guardar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Obtener la clave seleccionada del TreeView
                string claveSeleccionada = treeClaves.SelectedNode?.Text;

                // Limpiar caracteres inválidos en nombres de archivo
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    claveSeleccionada = claveSeleccionada.Replace(c, '_');
                }

                string xmlContent = await ConsultarSRIAsync(claveSeleccionada);

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    MessageBox.Show("Consulta SRI no retorno nada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Archivo XML (*.xml)|*.xml|Todos los archivos (*.*)|*.*";
                    saveFileDialog.Title = "Guardar XML";
                    saveFileDialog.FileName = claveSeleccionada + ".xml";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, xmlContent, Encoding.UTF8);
                        MessageBox.Show("XML guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el XML: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (byte[] pdf, string mensaje) ValidarClavesWS(string token, byte[] xml_base_64, string wslink = "https://edocnube.com/4.3/WSEDOC_RECEPCION/WSRAD_KEY_VALIDAR.svc")
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10485760,
                ReaderQuotas = { MaxStringContentLength = 10485760 },
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(30),
                SendTimeout = TimeSpan.FromSeconds(30)
            };

            var endpoint = new EndpointAddress(wslink);
            var client = new EdocServiceKeyValidar.WSRAD_KEY_VALIDARClient(binding, endpoint);

            try
            {
                // 🔹 Verificar si el servicio SOAP está activo
                if (!ServicioSoapDisponible(wslink))
                    return (null, "El servicio SOAP no está disponible o no responde al WSDL.");

                string mensajeSalida = "";
                byte[] resultado = client.ValidarDocumentoXmlYGenerarPDF(token, xml_base_64, ref mensajeSalida);
                client.Close();

                return (resultado, mensajeSalida);
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                client.Abort();
                return (null, "No se pudo conectar al servicio SOAP. El endpoint no está disponible.");
            }
            catch (System.ServiceModel.FaultException ex)
            {
                client.Abort();
                return (null, $"El servicio SOAP devolvió una falla: {ex.Message}");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                client.Abort();
                return (null, $"Error de comunicación con el servicio SOAP: {ex.Message}");
            }
            catch (TimeoutException)
            {
                client.Abort();
                return (null, "El servicio SOAP no respondió dentro del tiempo esperado (timeout).");
            }
            catch (Exception ex)
            {
                client.Abort();
                return (null, $"Error inesperado: {ex.Message}");
            }
        }

        private (bool estado, string mensaje) ValidarClavesXMLWS(string token, byte[] xml_base_64, string wslink = "https://edocnube.com/4.3/WSEDOC_RECEPCION/WSRAD_KEY_VALIDAR.svc")
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10485760,
                ReaderQuotas = { MaxStringContentLength = 10485760 },
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(30),
                SendTimeout = TimeSpan.FromSeconds(30)
            };

            var endpoint = new EndpointAddress(wslink);
            var client = new EdocServiceKeyValidar.WSRAD_KEY_VALIDARClient(binding, endpoint);

            try
            {
                // se verifica si el servicio SOAP está activo
                if (!ServicioSoapDisponible(wslink))
                    return (false, "El servicio SOAP no está disponible o no responde al WSDL.");

                string mensajeSalida = "";
                bool resultado = client.ValidarDocumentoXml(token, xml_base_64, ref mensajeSalida);
                client.Close();

                return (resultado, mensajeSalida);
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                client.Abort();
                return (false, "No se pudo conectar al servicio SOAP. El endpoint no está disponible.");
            }
            catch (System.ServiceModel.FaultException ex)
            {
                client.Abort();
                return (false, $"El servicio SOAP devolvió una falla: {ex.Message}");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                client.Abort();
                return (false, $"Error de comunicación con el servicio SOAP: {ex.Message}");
            }
            catch (TimeoutException)
            {
                client.Abort();
                return (false, "El servicio SOAP no respondió dentro del tiempo esperado (timeout).");
            }
            catch (Exception ex)
            {
                client.Abort();
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        private bool ServicioSoapDisponible(string wslink)
        {
            try
            {
                string wsdlUrl = wslink.EndsWith("?wsdl", StringComparison.OrdinalIgnoreCase)
                    ? wslink
                    : wslink.TrimEnd('/') + "?wsdl";

                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(wsdlUrl);
                request.Method = "GET";
                request.Timeout = 4000;

                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
            }
        }

        public static FacturaModel CargarFacturaDesdeXml(string xmlInterno)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FacturaModel));
            using (StringReader reader = new StringReader(xmlInterno))
            {
                return (FacturaModel)serializer.Deserialize(reader);
            }
        }

        private void btnPrevi_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(xmlinternoglobal))
            {
                FacturaModel factura = CargarFacturaDesdeXml(xmlinternoglobal);

                Console.WriteLine("Cliente: " + factura.InfoFactura.RazonSocialComprador);
                Console.WriteLine("RUC Emisor: " + factura.InfoTributaria.Ruc);
                Console.WriteLine("Total: " + factura.InfoFactura.ImporteTotal);
            }
        }
    }
}
