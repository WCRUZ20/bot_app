using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Playwright;
using botapp.Helpers;

namespace botapp.Helpers
{
    public static class CaptchaGate
    {
        // ¿Hay reto visible en esta página o en alguno de sus iframes?
        private static async Task<bool> IsChallengeVisibleAsync(IPage page)
        {
            try
            {
                // Revisar en todos los frames (algunos retos se montan en iframes)
                foreach (var f in page.Frames)
                {
                    // Elementos típicos de retos (genérico + casos similares al de SRI)
                    var checks = new ILocator[]
                    {
                        f.Locator("div[role='dialog']"),
                        f.Locator("div.rc-imageselect"),
                        f.Locator("#recaptcha-verify-button"),
                        f.Locator(".recaptcha-checkbox-border"),
                        f.Locator("text=/selecciona todos los cuadros/i"),
                        f.Locator("text=SALTAR")
                    };

                    foreach (var loc in checks)
                    {
                        try
                        {
                            // Si existe y está visible, el reto está activo
                            if (await loc.CountAsync() > 0 && await loc.First.IsVisibleAsync())
                                return true;
                        }
                        catch { /* ignora y sigue */ }
                    }
                }

                // También revisa en el documento principal (sin iframe)
                var pageChecks = new ILocator[]
                {
                    page.Locator("div[role='dialog']"),
                    page.Locator("div.rc-imageselect"),
                    page.Locator("#recaptcha-verify-button"),
                    page.Locator(".recaptcha-checkbox-border"),
                    page.Locator("text=/selecciona todos los cuadros/i"),
                    page.Locator("text=SALTAR")
                };
                foreach (var loc in pageChecks)
                {
                    try
                    {
                        if (await loc.CountAsync() > 0 && await loc.First.IsVisibleAsync())
                            return true;
                    }
                    catch { }
                }
            }
            catch { /* no visible */ }
            return false;
        }

        // Espera a que el reto deje de ser VISIBLE o a que aparezcan señales de éxito
        private static async Task WaitUntilClosedOrSuccessAsync(IPage page, int timeoutMs)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                // Si ya no hay reto visible -> salir
                var visible = await IsChallengeVisibleAsync(page);
                if (!visible) return;

                // Si ya aparecieron resultados o el link de descarga -> salir como éxito
                try
                {
                    var tabla = page.Locator("#frmPrincipal\\:tablaCompRecibidos");
                    if (await tabla.IsVisibleAsync()) return;
                }
                catch { }

                try
                {
                    var lnk = page.Locator("#frmPrincipal\\:lnkTxtlistado");
                    if (await lnk.IsVisibleAsync()) return;
                }
                catch { }

                await Task.Delay(300);
            }
            throw new TimeoutException("El reto no se resolvió dentro del tiempo de espera.");
        }

        public static async Task HandleIfOpenAsync(IPage page, Action<string> log, int timeoutMs)
        {
            // Solo levantamos la UI si REALMENTE está visible
            var open = await IsChallengeVisibleAsync(page);
            if (!open) return;

            await page.BringToFrontAsync();
            try { System.Media.SystemSounds.Beep.Play(); } catch { }

            log?.Invoke("⚠️ Se detectó un reto visual (reCAPTCHA). Resuélvelo manualmente; el proceso continuará automáticamente.");

            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    var owner = Application.OpenForms[0];
                    owner.BeginInvoke(new Action(() =>
                    {
                        //ToastManager.Show(owner, "warning",
                        //    "Se detectó un reto visual. Resuélvelo en el navegador; el bot seguirá automáticamente.",
                        //    6000);
                    }));
                }
            }
            catch { /* opcional */ }

            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    var owner = Application.OpenForms[0] as Form;
                    if (owner != null)
                    {
                        owner.BeginInvoke(new Action(() =>
                        {
                            //ToastManager.Show(owner, "exito", "Reto resuelto. Continuando…", 2000);
                        }));
                    }
                }
            }
            catch { }

            await WaitUntilClosedOrSuccessAsync(page, timeoutMs);
            log?.Invoke("✅ Reto resuelto. Continuando…");
        }
    }
}
