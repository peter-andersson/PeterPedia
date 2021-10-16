using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PeterPedia.Shared;
using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Bookmark.Pages
{
    public partial class ReadLater : ComponentBase
    {
        public List<ReadListItem> Items { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // TODO: Load data...
            Items = new List<ReadListItem>();

            Items.Add(new ReadListItem()
            {
                Id = 1,
                Url = "https://norran.se",
                Added = DateTime.Now
            });

            Items.Add(new ReadListItem()
            {
                Id = 2,
                Url = "https://aftonbladet.se",
                Added = DateTime.Now
            });
        }
    }
}