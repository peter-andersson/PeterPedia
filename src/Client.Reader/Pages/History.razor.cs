using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeterPedia.Shared;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class History : ComponentBase
    {
        [Inject]
        private RSSService RSSService { get; set; }

        private List<Article> Articles;

        protected override async Task OnInitializedAsync()
        {
            Articles = await RSSService.GetHistory();
        }
    }
}
