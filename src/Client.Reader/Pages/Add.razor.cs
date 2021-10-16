using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Reader.Services;
using System.Threading.Tasks;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Add : ComponentBase
    {
        [Inject]
        private RSSService RSSService { get; set; }

        private EditContext AddContext;
        private string NewSubscriptionUrl;
        private bool IsTaskRunning;

        protected override void OnInitialized()
        {
            IsTaskRunning = false;
            NewSubscriptionUrl = string.Empty;
            AddContext = new EditContext(NewSubscriptionUrl);
        }

        private async Task AddSubscription()
        {
            IsTaskRunning = true;

            await RSSService.AddSubscription(NewSubscriptionUrl);
            IsTaskRunning = false;
        }
    }
}
