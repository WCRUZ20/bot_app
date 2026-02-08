using botapp.Automation;
using botapp.Automation.Models;
using botapp.Helpers;
using botapp.Models;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace botapp.Core
{
    public class ServicioDescargaDocumentos
    {
        private const int Reintentos = 3;
        private const int Hilos = 8;
        private const string DownloadFolderName = "Carga_Botcito";
        private string downloadPath = "";

        private enum ResultadoConsulta
        {
            Descargado,
            SinDatos
        }

        private sealed class ClienteProcesable
        {
            public string Usuario { get; set; }
            public string Nombre { get; set; }
            public string CiAdicional { get; set; }
            public string Password { get; set; }
            public List<DateTime> Periodos { get; set; }
            public List<TipoComprobante> Tipos { get; set; }
        }

        public async Task<List<ServicioClienteResultado>> EjecutarAsync(DataTable clientes)
        {
            if (clientes == null)
                return new List<ServicioClienteResultado>();

            var clientesProcesables = ConstruirClientesProcesables(clientes);
            var resultados = new List<ServicioClienteResultado>();
            var sync = new object();
            using (var gate = new SemaphoreSlim(Hilos))
            {
                var tareas = clientesProcesables.Select(async cliente =>
                {
                    await gate.WaitAsync();
                    try
                    {
                        var resultado = await ProcesarClienteAsync(cliente);
                        lock (sync)
                        {
                            resultados.Add(resultado);
                        }
                    }
                    finally
                    {
                        gate.Release();
                    }
                }).ToList();

                await Task.WhenAll(tareas);
            }

            return resultados.OrderBy(x => x.NombreCliente).ToList();
        }

        private async Task<ServicioClienteResultado> ProcesarClienteAsync(ClienteProcesable cliente)
        {
            var resultadoCliente = new ServicioClienteResultado
            {
                Usuario = cliente.Usuario,
                NombreCliente = cliente.Nombre
            };

            string pageUrl = ConfigurationManager.AppSettings["SRI_LoginUrl"] ?? string.Empty;
            bool headless = false;

            foreach (var periodo in cliente.Periodos)
            {
                foreach (var tipo in cliente.Tipos)
                {
                    var parametros = new ParametrosConsulta
                    {
                        Anio = periodo.Year.ToString(),
                        Mes = periodo.Month.ToString(),
                        Dia = "0",
                        Tipo = tipo
                    };

                    string estado = await EjecutarProcesoConReintentosAsync(
                        pageUrl,
                        headless,
                        cliente.Usuario,
                        cliente.CiAdicional,
                        cliente.Password,
                        cliente.Nombre,
                        parametros);

                    resultadoCliente.Descargas.Add(new ServicioDescargaResultado
                    {
                        MesAnio = periodo.ToString("MM-yyyy"),
                        TipoDocumento = tipo.Nombre ?? tipo.Value,
                        Estado = estado
                    });
                }
            }

            return resultadoCliente;
        }

        private async Task<string> EjecutarProcesoConReintentosAsync(string pageUrl, bool headless, string usuario, string ciAdicional, string password, string nombre, ParametrosConsulta parametros)
        {
            for (int intento = 1; intento <= Reintentos; intento++)
            {
                try
                {
                    var resultado = await EjecutarProcesoAsyncPorCliente(pageUrl, headless, usuario, ciAdicional, password, nombre, parametros);
                    if (resultado == ResultadoConsulta.SinDatos)
                        return "Sin datos";

                    return "Exitoso";
                }
                catch
                {
                    if (intento == Reintentos)
                        return "Fallido";

                    await Task.Delay(250);
                }
            }

            return "Fallido";
        }

        private async Task<ResultadoConsulta> EjecutarProcesoAsyncPorCliente(string pageUrl, bool headless, string usuario, string ciAdicional, string password, string nombre, ParametrosConsulta parametros)
        {
            PlaywrightManager manager = null;
            BrowserSession session = null;

            try
            {
                var config = new BrowserConfig
                {
                    Url = pageUrl,
                    Headless = headless
                };

                manager = new PlaywrightManager();
                await manager.InitializeAsync(config);

                session = new BrowserSession(manager.Browser, config);
                await session.StartAsync();

                var actions = new PageActions(session.Page);
                await IniciarSesionAsync(session, actions, usuario, ciAdicional, password);

                return await EjecutarConsultaYDescargaAsync(session, actions, parametros, usuario, nombre);
            }
            finally
            {
                if (session != null)
                {
                    try
                    {
                        await CerrarSesionAsync(session);
                    }
                    catch
                    {
                    }

                    await session.DisposeAsync();
                }

                if (manager != null)
                    await manager.DisposeAsync();
            }
        }

        private static async Task IniciarSesionAsync(BrowserSession session, PageActions actions, string usuario, string ciAdicional, string password)
        {
            await session.Page.WaitForSelectorAsync("#usuario", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible
            });

            await actions.SetTextAsync("#usuario", usuario);
            await actions.SetTextAsync("#ciAdicional", ciAdicional);
            await actions.SetTextAsync("#password", password);
            await actions.ClickAsync("#kc-login");

            await session.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            bool loginExitoso = await WaitHelper.ExistsAsync(session.Page, "a[tooltip='Cerrar sesión']", 25000);
            if (!loginExitoso)
                throw new Exception("No se detectó el botón Cerrar sesión (login fallido)");
        }

        private static async Task CerrarSesionAsync(BrowserSession session)
        {
            bool existeCerrarSesion = await WaitHelper.ExistsAsync(session.Page, "a[tooltip='Cerrar sesión']", 5000);
            if (!existeCerrarSesion)
                return;

            await session.Page.ClickAsync("a[tooltip='Cerrar sesión']");
            await session.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task<ResultadoConsulta> EjecutarConsultaYDescargaAsync(BrowserSession session, PageActions actions, ParametrosConsulta parametros, string usuario, string nombreUsuario)
        {
            await actions.SelectAsync("#frmPrincipal\\:ano", parametros.Anio);
            await actions.SelectAsync("#frmPrincipal\\:mes", parametros.Mes);
            await actions.SelectAsync("#frmPrincipal\\:dia", parametros.Dia);
            await actions.SelectAsync("#frmPrincipal\\:cmbTipoComprobante", parametros.Tipo.Value);

            await actions.ClickAsync("#frmPrincipal\\:btnBuscar");

            bool captchaincorrecta = await WaitHelper.ExistsAsync(session.Page, "text=Captcha incorrecta", 3000);
            if (captchaincorrecta)
                await ConsultarConRecuperacionCaptchaAsync(session.Page, actions, "#frmPrincipal\\:btnBuscar");

            bool sinDatos = await WaitHelper.ExistsAsync(session.Page, "text=No existen datos", 3000);
            if (sinDatos)
                return ResultadoConsulta.SinDatos;

            bool tablaCargada = await WaitHelper.ExistsAsync(session.Page, "#frmPrincipal\\:tablaCompRecibidos", 10000);
            if (!tablaCargada)
                throw new Exception("No se cargó la tabla de comprobantes electrónicos");

            //string rutaBase = Utils.ObtenerRutaDescargaPersonalizada(DownloadFolderName);
            var basePath = ConfigurationManager.AppSettings["RutaDirectorio"];
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                downloadPath = Path.Combine(basePath, DownloadFolderName);
            }
            string rutaUsuario = PrepararRutaDescarga(downloadPath, usuario, nombreUsuario);

            var download = await session.Page.RunAndWaitForDownloadAsync(async () =>
            {
                await actions.ClickAsync("#frmPrincipal\\:lnkTxtlistado");
            });

            string extension = Path.GetExtension(download.SuggestedFilename);
            string nombreArchivo = $"{parametros.Tipo.PrefijoArchivo}_{parametros.Mes.PadLeft(2, '0')}_{parametros.Anio}{extension}";
            string rutaFinal = Path.Combine(rutaUsuario, nombreArchivo);

            await download.SaveAsAsync(rutaFinal);
            return ResultadoConsulta.Descargado;
        }

        private static string PrepararRutaDescarga(string basePath, string usuario, string nombre)
        {
            string userFolder = Path.Combine(basePath, $"{usuario} - {nombre}");
            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            return userFolder;
        }

        private static async Task ConsultarConRecuperacionCaptchaAsync(IPage page, PageActions actions, string btnBuscarSelector)
        {
            await actions.ClickAsync(btnBuscarSelector);
            await page.WaitForTimeoutAsync(6000);

            bool captchaIncorrecta = await ExisteCaptchaIncorrectaAsync(page, 6000);
            if (!captchaIncorrecta)
                return;

            var estrategias = ConstruirEstrategiasForzadas(btnBuscarSelector);
            for (int i = 0; i < estrategias.Count; i++)
            {
                await page.WaitForTimeoutAsync(6000);
                await estrategias[i](page);
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForTimeoutAsync(9000);

                captchaIncorrecta = await ExisteCaptchaIncorrectaAsync(page, 1200);
                if (!captchaIncorrecta)
                    return;
            }
        }

        private static async Task<bool> ExisteCaptchaIncorrectaAsync(IPage page, int timeoutMs)
        {
            try
            {
                var locator = page.Locator("text=/captcha\\s+incorrecta/i");
                await locator.First.WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = timeoutMs,
                    State = WaitForSelectorState.Visible
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static List<Func<IPage, Task>> ConstruirEstrategiasForzadas(string btnBuscarSelector)
        {
            return new List<Func<IPage, Task>>
            {
                async page =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });
                        el.focus();
                    }", btnBuscarSelector);

                    await page.ClickAsync(btnBuscarSelector, new PageClickOptions { Force = true, Timeout = 5000 });
                },
                async page =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });
                        el.click();
                    }", btnBuscarSelector);
                },
                async page =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;
                        el.scrollIntoView({ block: 'center', inline: 'center' });

                        const fire = (type) => el.dispatchEvent(new MouseEvent(type, { bubbles: true, cancelable: true, view: window }));
                        fire('mousedown');
                        fire('mouseup');
                        fire('click');
                    }", btnBuscarSelector);
                },
                async page =>
                {
                    await page.FocusAsync(btnBuscarSelector);
                    await page.Keyboard.PressAsync("Enter");
                },
                async page =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;

                        const form = el.closest('form');
                        if (!form) return;

                        if (typeof form.requestSubmit === 'function') {
                            form.requestSubmit();
                        } else {
                            form.submit();
                        }
                    }", btnBuscarSelector);
                },
                async page =>
                {
                    await page.EvaluateAsync(@"(sel) => {
                        const el = document.querySelector(sel);
                        if (!el) return;

                        const onclick = el.getAttribute('onclick') || '';
                        if (onclick.includes('PrimeFaces.ab')) {
                            try { eval(onclick); } catch (e) {}
                        } else {
                            el.click();
                        }
                    }", btnBuscarSelector);
                },
            };
        }

        private List<ClienteProcesable> ConstruirClientesProcesables(DataTable clientes)
        {
            var salida = new List<ClienteProcesable>();

            foreach (DataRow row in clientes.Rows)
            {
                if (!EsValorYN(row, "Activo") || !EsValorYN(row, "descarga"))
                    continue;

                salida.Add(new ClienteProcesable
                {
                    Usuario = ObtenerTexto(row, "usuario"),
                    Nombre = ObtenerTexto(row, "NombUsuario"),
                    CiAdicional = ObtenerTexto(row, "ci_adicional"),
                    Password = ObtenerTexto(row, "clave"),
                    Periodos = ConstruirPeriodosConsulta(row).ToList(),
                    Tipos = ObtenerTiposHabilitados(row).ToList()
                });
            }

            return salida;
        }

        private IEnumerable<DateTime> ConstruirPeriodosConsulta(DataRow row)
        {
            DateTime hoy = DateTime.Today;
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var periodos = new List<DateTime>();

            if (EsValorYN(row, "consultar_mes_actual"))
                periodos.Add(inicioMes);

            int mesesAnteriores = ObtenerEntero(row, "meses_ante");
            int diasPermitidos = ObtenerEntero(row, "dias");

            if (mesesAnteriores > 0 && (hoy.Day <= diasPermitidos || diasPermitidos == 0))
            {
                for (int i = 1; i <= mesesAnteriores; i++)
                    periodos.Add(inicioMes.AddMonths(-i));
            }

            return periodos;
        }

        private IEnumerable<TipoComprobante> ObtenerTiposHabilitados(DataRow row)
        {
            var tipos = new Dictionary<string, string>
            {
                { "factura", "1" },
                { "liquidacioncompra", "2" },
                { "notacredito", "3" },
                { "notadebito", "4" },
                { "retencion", "6" }
            };

            foreach (var tipo in tipos)
            {
                if (!EsValorYN(row, tipo.Key))
                    continue;

                var encontrado = CatalogoComprobantes.ObtenerPorValue(tipo.Value);
                if (encontrado != null)
                    yield return encontrado;
            }
        }

        private static string ObtenerTexto(DataRow row, string nombreColumna)
        {
            if (!row.Table.Columns.Contains(nombreColumna))
                return string.Empty;

            return row[nombreColumna]?.ToString() ?? string.Empty;
        }

        private static int ObtenerEntero(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return 0;

            if (int.TryParse(row[columnName]?.ToString(), out int valor))
                return valor;

            return 0;
        }

        private static bool EsValorYN(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return false;

            return EsTrue(row[columnName]);
        }

        private static bool EsTrue(object valor)
        {
            return string.Equals(valor == null ? string.Empty : valor.ToString(), "Y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor == null ? string.Empty : valor.ToString(), "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor == null ? string.Empty : valor.ToString(), "1", StringComparison.OrdinalIgnoreCase);
        }

    }
}

