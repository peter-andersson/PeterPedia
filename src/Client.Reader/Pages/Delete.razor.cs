using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Threading.Tasks;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Delete : ComponentBase
    {
        private readonly string returnUrl = "subscriptions";

        [Inject]
        private RSSService RSSService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private string Title;
        private bool IsTaskRunning;

        protected override async Task OnInitializedAsync()
        {
            IsTaskRunning = false;
            var subscription = await RSSService.GetSubscription(Id);

            if (subscription is null)
            {
                NavManager.NavigateTo(returnUrl);
            }
            else
            {
                Title = subscription.Title;
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo(returnUrl);
        }

        private async Task DeleteSubscription()
        {
            IsTaskRunning = true;

            var result = await RSSService.DeleteSubscription(Id);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo(returnUrl);
            }
        }
    }
}
