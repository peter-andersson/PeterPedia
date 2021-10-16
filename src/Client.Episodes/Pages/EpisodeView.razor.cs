using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Episodes.Services;
using PeterPedia.Shared;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class EpisodeView : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        [Parameter]
        public Show Show { get; set; }

        [Parameter]
        public Episode Episode { get; set; }

        private bool IsTaskRunning = false;

        public async Task WatchEpisode()
        {
            IsTaskRunning = true;

            var result = await TVService.WatchEpisode(Show.Id, Episode.Id);
            IsTaskRunning = false;
            if (result)
            {
                foreach (var season in Show.Seasons)
                {
                    foreach (var episode in season.Episodes)
                    {
                        if (episode.Id == Episode.Id)
                        {
                            episode.Watched = true;

                            Show.Calculate();
                            TVService.CallRequestRefresh();

                            return;
                        }
                    }
                }
            }
        }

        public async Task UnwatchEpisode()
        {
            IsTaskRunning = true;

            var result = await TVService.UnwatchEpisode(Show.Id, Episode.Id);
            IsTaskRunning = false;
            if (result)
            {
                foreach (var season in Show.Seasons)
                {
                    foreach (var episode in season.Episodes)
                    {
                        if (episode.Id == Episode.Id)
                        {
                            episode.Watched = false;

                            Show.Calculate();
                            TVService.CallRequestRefresh();

                            return;
                        }
                    }
                }
            }
        }
    }
}
