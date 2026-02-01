using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation
{
    public static class WaitHelper
    {
        public static async Task<bool> ExistsAsync(
            IPage page,
            string selector,
            int timeoutMs = 3000)
        {
            try
            {
                await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    Timeout = timeoutMs
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task WaitForNetworkAsync(IPage page)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}
