namespace PeterPedia.Client.Movie.Pages
{
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Movie.Services;
    using System.Threading.Tasks;

    public partial class Delete : ComponentBase
    {
        [Inject]
        private MovieService MovieService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }

        private string Title;

        private bool IsTaskRunning;

        protected override async Task OnInitializedAsync()
        {
            ReturnUrl = ReturnUrl ?? "";

            IsTaskRunning = false;
            var movie = await MovieService.Get(Id);

            if (movie is null)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
            else
            {
                Title = movie.Title;
            }
        }

        private async Task DeleteMovie()
        {
            IsTaskRunning = true;

            var result = await MovieService.Delete(Id);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo(ReturnUrl);
        }
    }
}
