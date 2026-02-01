using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapp.Automation
{
    public class PageActions
    {
        private readonly IPage _page;

        public PageActions(IPage page)
        {
            _page = page;
        }

        public async Task SetTextAsync(string selector, string value)
        {
            await _page.WaitForSelectorAsync(selector);
            await _page.FillAsync(selector, value);
        }

        public async Task ClickAsync(string selector)
        {
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible
            });

            await _page.ClickAsync(selector);
        }

        public async Task SelectAsync(string selector, string value)
        {
            await _page.WaitForSelectorAsync(selector);
            await _page.SelectOptionAsync(selector, value);
        }
    }
}
