using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class Delete : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private string Title;
        private bool IsTaskRunning;

        protected override async Task OnInitializedAsync()
        {
            IsTaskRunning = false;
            var show = await TVService.Get(Id);

            if (show is null)
            {
                NavManager.NavigateTo("shows");
            }
            else
            {
                Title = show.Title;
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo("shows");
        }

        private async Task DeleteShow()
        {
            IsTaskRunning = true;

            var result = await TVService.Delete(Id);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo("shows");
            }
        }
    }
}
