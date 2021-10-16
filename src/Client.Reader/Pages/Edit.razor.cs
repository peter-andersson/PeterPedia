using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Threading.Tasks;
using PeterPedia.Shared;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Edit : ComponentBase
    {
        [Inject]
        private RSSService RSSService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private bool IsTaskRunning;
        private Subscription Subscription;

        protected override async Task OnInitializedAsync()
        {
            IsTaskRunning = false;
            var subscription = await RSSService.GetSubscription(Id);

            if (subscription is null)
            {
                NavManager.NavigateTo("subscriptions");
            }
            else
            {
                Subscription = CreateCopy(subscription);
            }
        }

        private async Task Save()
        {
            IsTaskRunning = true;

            var result = await RSSService.UpdateSubscription(Subscription);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo("subscriptions");
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo("subscriptions");
        }

        private static Subscription CreateCopy(Subscription subscription)
        {
            var result = new Subscription()
            {
                Id = subscription.Id,
                Title = subscription.Title,
                UpdateIntervalMinute = subscription.UpdateIntervalMinute,
                LastUpdate = subscription.LastUpdate,
                Url = subscription.Url,
            };

            return result;
        }
    }
}
