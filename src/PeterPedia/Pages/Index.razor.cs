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
        //using CosmosClient client = new(accountEndpoint: Configuration["EndPointUrl"], authKeyOrResourceToken: Configuration["AccountKey"]);
        //var books = await DbContext.Books.Include(b => b.Authors).ToListAsync();
        //foreach (BookEF bookEF in books)
        //{
        //    CosmosBook cosmosBook = new CosmosBook()
        //    {
        //        id = Guid.NewGuid().ToString(),
        //        Title = bookEF.Title,                
        //    };

        //    foreach (var author in bookEF.Authors)
        //    {
        //        cosmosBook.Authors.Add(author.Name);
        //    }

        //    cosmosBook.Read = bookEF.State == 3;
        //    cosmosBook.WantToRead = bookEF.State == 1;
        //    cosmosBook.Reading = bookEF.State == 2;
           
        //    Database database = client.GetDatabase("peterpedia");
        //    Container container = database.GetContainer("books");

        //    await container.CreateItemAsync(cosmosBook, new PartitionKey(cosmosBook.id));
        //}
    }

    //public class CosmosBook
    //{
    //    public string id { get; set; } = string.Empty;

    //    public string Title { get; set; } = string.Empty;

    //    public bool Reading { get; set; }

    //    public bool Read { get; set; }

    //    public bool WantToRead { get; set; }

    //    public List<string> Authors { get; set; } = new();
    //}
}
