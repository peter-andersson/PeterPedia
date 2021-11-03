using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using PeterPedia.Shared;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class ViewShow : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private Show Show;

        protected override async Task OnInitializedAsync()
        {
            await TVService.FetchData();

            Show = await TVService.Get(Id);

            TVService.RefreshRequested += Refresh;
        }
        private void Refresh()
        {
            StateHasChanged();
        }
    }
}
