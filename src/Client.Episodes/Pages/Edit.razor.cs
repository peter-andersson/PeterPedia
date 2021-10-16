using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using PeterPedia.Shared;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class Edit : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        private bool IsTaskRunning;
        private Show Show;

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
                Show = CreateCopy(show);
            }

            TVService.RefreshRequested += Refresh;
        }

        private void Refresh()
        {
            StateHasChanged();
        }

        private async Task Save()
        {
            IsTaskRunning = true;

            var result = await TVService.Update(Show);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo("shows");
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo("shows");
        }

        private static Show CreateCopy(Show show)
        {
            var showCopy = new Show
            {
                Id = show.Id,
                Title = show.Title,
                Status = show.Status,
                TheMovieDbUrl = show.TheMovieDbUrl,
            };

            foreach (var season in show.Seasons)
            {
                var seasonCopy = new Season()
                {
                    Id = season.Id,
                    SeasonNumber = season.SeasonNumber,
                };
                showCopy.Seasons.Add(seasonCopy);

                foreach (var episode in season.Episodes)
                {
                    var episodeCopy = new Episode()
                    {
                        Id = episode.Id,
                        AirDate = episode.AirDate,
                        EpisodeNumber = episode.EpisodeNumber,
                        Title = episode.Title,
                        Watched = episode.Watched,
                    };

                    seasonCopy.Episodes.Add(episodeCopy);
                }
            }

            showCopy.Calculate();
            return showCopy;
        }
    }
}
