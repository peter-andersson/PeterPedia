using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Reader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PeterPedia.Shared;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class Subscriptions : ComponentBase
    {
        private enum NavigatePage
        {
            Next,
            Previous,
            First
        }

        [Inject]
        private RSSService RSSService { get; set; }

        private readonly int pageSize = 25;
        private int currentPage = 0;

        private string CurrentSearch = string.Empty;
        private EditContext SearchContext;
        private bool IsPreviousButtonDisabled = false;
        private bool IsNextButtonDisabled = false;
        private List<Subscription> DisplaySubscriptions;

        protected override async Task OnInitializedAsync()
        {
            SearchContext = new EditContext(CurrentSearch);

            await RSSService.FetchData();

            LoadDisplaySubscriptions();
        }

        private void NavigateToPage(NavigatePage navigateTo)
        {
            switch (navigateTo)
            {
                case NavigatePage.Next:
                    currentPage += 1;
                    break;
                case NavigatePage.Previous:
                    currentPage -= 1;
                    if (currentPage < 0)
                    {
                        currentPage = 0;
                    }
                    break;
                case NavigatePage.First:
                    currentPage = 0;
                    break;
            }

            LoadDisplaySubscriptions();
        }

        private void LoadDisplaySubscriptions()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSearch))
            {
                DisplaySubscriptions = RSSService.Subscriptions.Where(m => m.Title.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase)).OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
            }
            else
            {
                DisplaySubscriptions = RSSService.Subscriptions.OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
            }

            IsPreviousButtonDisabled = currentPage == 0;
            IsNextButtonDisabled = DisplaySubscriptions.Count < pageSize;

            StateHasChanged();
        }
    }
}
