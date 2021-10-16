using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using PeterPedia.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class Episodes : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        public List<Show> Shows { get; set; }

        private Show SelectedShow;

        protected override async Task OnInitializedAsync()
        {
            await TVService.FetchData();

            Shows = TVService.Shows.Where(s => s.UnwatchedEpisodeCount > 0).OrderBy(m => m.Title).ToList();

            TVService.RefreshRequested += Refresh;
        }

        private void Refresh()
        {
            StateHasChanged();
        }

        private void Display(Show show)
        {
            SelectedShow = show;
        }
    }
}
