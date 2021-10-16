using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using System.Threading.Tasks;
using System;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class Statistics : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        private int WatchedEpisodes { get; set; }
        private int UnwatchedEpisodes { get; set; }
        private int TVShows { get; set; }

        protected override async Task OnInitializedAsync()
        {
            WatchedEpisodes = 0;
            UnwatchedEpisodes = 0;
            TVShows = 0;

            await TVService.FetchData();

            CalculateStatistics();
        }

        private void CalculateStatistics()
        {
            TVShows = TVService.Shows.Count;

            foreach (var show in TVService.Shows)
            {
                foreach (var season in show.Seasons)
                {
                    foreach (var episode in season.Episodes)
                    {
                        if (episode.Watched)
                        {
                            WatchedEpisodes += 1;
                        }
                        else if ((episode.AirDate != null) && (episode.AirDate <= DateTime.UtcNow))
                        {
                            UnwatchedEpisodes += 1;
                        }
                    }
                }
            }
        }
    }
}
