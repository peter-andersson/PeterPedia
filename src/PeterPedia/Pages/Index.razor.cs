using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace PeterPedia.Pages;

public partial class Index : ComponentBase
{    
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    //[Inject]
    //private IConfiguration Configuration { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {       
        // Move to cosmosDB
        //using CosmosClient client = new(accountEndpoint: Configuration["EndPointUrl"], authKeyOrResourceToken: Configuration["AccountKey"]);
        //var books = await DbContext.Shows.Include(b => b.Seasons).ThenInclude(s => s.Episodes).ToListAsync();
        //foreach (ShowEF showEF in books)
        //{
        //    var show = new TVShowEntity()
        //    {
        //        Id = showEF.Id.ToString(),
        //        Title = showEF.Title,
        //        ETag = showEF.ETag,
        //        Status = showEF.Status,
        //        NextUpdate = DateTime.UtcNow.AddDays(-1),
        //    };

        //    foreach (SeasonEF seasonEF in showEF.Seasons)
        //    {
        //        var season = new SeasonEntity()
        //        {
        //            SeasonNumber = seasonEF.SeasonNumber,
        //        };

        //        foreach (EpisodeEF episodeEF in seasonEF.Episodes)
        //        {
        //            var episode = new EpisodeEntity()
        //            {
        //                EpisodeNumber = episodeEF.EpisodeNumber,
        //                AirDate = episodeEF.AirDate,
        //                Title = episodeEF.Title,
        //                Watched = episodeEF.Watched
        //            };

        //            season.Episodes.Add(episode);
        //        }

        //        show.Seasons.Add(season);
        //    }
        
        //    Database database = client.GetDatabase("peterpedia");
        //    Container container = database.GetContainer("episodes");

        //    await container.UpsertItemAsync(show, new PartitionKey(show.Id));
        //}
    }
}
