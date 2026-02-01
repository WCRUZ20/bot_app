using botapp.Automation.Models;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation
{
    public class PlaywrightManager : IAsyncDisposable
    {
        public IPlaywright Playwright { get; private set; }
        public IBrowser Browser { get; private set; }

        public async Task InitializeAsync(BrowserConfig config)
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = config.Headless,
                SlowMo = config.SlowMoMs
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (Browser != null)
                await Browser.CloseAsync();

            Playwright?.Dispose();
        }
    }
}
