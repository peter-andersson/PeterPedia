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

    // [Inject]
    // private IConfiguration Configuration { get; set; } = null!;

    [Inject]
    private IMemoryCache Cache { get; set; } = null!;

    public List<Link> Links { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Links.Clear();

        if (Cache.TryGetValue(CacheKey.Links, out List<Link> list))
        {
            Links.AddRange(list);
            return;
        }

        List<LinkEF> links = await DbContext.Links.ToListAsync();

        Links.Clear();

        foreach (LinkEF link in links)
        {
            Links.Add(new Link()
            {
                Id = link.Id,
                Title = link.Title,
                Url = link.Url,
            });
        }
       
        Links.Add(new Link()
        {
            Title = "Library",
            Url = "library/books"
        });

        Links.Add(new Link()
        {
            Title = "TV Shows",
            Url = "tv"
        });

        Links.Add(new Link()
        {
            Title = "Reader",
            Url = "reader"
        });

        Links.Sort((link1, link2) => link1.Title.CompareTo(link2.Title));

        Cache.Set(CacheKey.Links, Links, TimeSpan.FromMinutes(5));

        // Move to cosmosDB
        // using CosmosClient client = new(accountEndpoint: Configuration["EndPointUrl"], authKeyOrResourceToken: Configuration["AccountKey"]);
        // var movies = await DbContext.Movies.ToListAsync();
        //foreach (MovieEF movieEF in movies)
        //{
        //    CosmosMovie cosmosMovie = new CosmosMovie()
        //    {
        //        Id = movieEF.Id.ToString(),
        //        ImdbId = movieEF.ImdbId,
        //        OriginalTitle = movieEF.OriginalTitle,
        //        OriginalLanguage = movieEF.OriginalLanguage,
        //        ReleaseDate = movieEF.ReleaseDate,
        //        RunTime = movieEF.RunTime,
        //        Title = movieEF.Title,
        //        WatchedDate = movieEF.WatchedDate,
        //        ETag = movieEF.ETag
        //    };

        //    Database database = client.GetDatabase("peterpedia");
        //    Container container = database.GetContainer("movies");

        //    await container.CreateItemAsync(cosmosMovie, new PartitionKey(cosmosMovie.Id));
        //} 
    }

    //public class CosmosMovie
    //{
    //    [JsonProperty(PropertyName = "id")]
    //    public string Id { get; set; } = string.Empty;

    //    public string ImdbId { get; set; } = string.Empty;

    //    public string OriginalTitle { get; set; } = string.Empty;

    //    public string OriginalLanguage { get; set; } = string.Empty;

    //    public string Title { get; set; } = string.Empty;

    //    public DateTime? ReleaseDate { get; set; }

    //    public DateTime? WatchedDate { get; set; }

    //    public int? RunTime { get; set; }

    //    public string ETag { get; set; } = string.Empty;
    //}
}
