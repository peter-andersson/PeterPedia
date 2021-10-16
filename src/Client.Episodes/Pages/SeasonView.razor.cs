using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using PeterPedia.Shared;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class SeasonView : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        [Parameter]
        public Show Show { get; set; }

        [Parameter]
        public Season Season { get; set; }

        [Parameter]
        public bool DisplayAllEpisodes { get; set; }

        private bool IsTaskRunning = false;

        public async Task WatchSeason()
        {
            IsTaskRunning = true;
            var result = await TVService.WatchSeason(Show.Id, Season.Id);
            IsTaskRunning = false;

            if (result)
            {
                foreach (var season in Show.Seasons)
                {
                    if (season.Id == Season.Id)
                    {
                        foreach (var episode in season.Episodes)
                        {
                            episode.Watched = true;
                        }

                        Show.Calculate();
                        TVService.CallRequestRefresh();

                        return;
                    }
                }
            }
        }

        public async Task UnwatchSeason()
        {
            IsTaskRunning = true;
            var result = await TVService.UnwatchSeason(Show.Id, Season.Id);
            IsTaskRunning = false;

            if (result)
            {
                foreach (var season in Show.Seasons)
                {
                    if (season.Id == Season.Id)
                    {
                        foreach (var episode in season.Episodes)
                        {
                            episode.Watched = false;
                        }

                        Show.Calculate();
                        TVService.CallRequestRefresh();

                        return;
                    }
                }
            }
        }
    }
}
