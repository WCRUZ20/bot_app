using botapp.Automation.Models;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation
{
    public class BrowserSession : IAsyncDisposable
    {
        private readonly IBrowser _browser;
        private readonly BrowserConfig _config;

        public IBrowserContext Context { get; private set; }
        public IPage Page { get; private set; }

        public BrowserSession(IBrowser browser, BrowserConfig config)
        {
            _browser = browser;
            _config = config;
        }

        public async Task StartAsync()
        {
            Context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                AcceptDownloads = true
            });

            Page = await Context.NewPageAsync();
            Page.SetDefaultTimeout(_config.DefaultTimeoutMs);

            await Page.GotoAsync(_config.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });
        }

        public async Task RestartAsync()
        {
            await DisposeAsync();
            await StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (Context != null)
                await Context.CloseAsync();
        }
    }
}
