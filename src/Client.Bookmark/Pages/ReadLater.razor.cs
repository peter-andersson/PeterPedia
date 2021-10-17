namespace PeterPedia.Client.Bookmark.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Bookmark.Services;
    using System;
    using Microsoft.AspNetCore.Components.Web;

    public partial class ReadLater : ComponentBase
    {
        [Inject]
        public ReadListService ReadListService { get; set; }

        public List<ReadListItem> Items { get; set; } = new List<ReadListItem>();

        public string Url { get; set; }

        private ElementReference Input;

        protected override async Task OnInitializedAsync()
        {
            await ReadListService.FetchData();

            Items = ReadListService.Items;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Input.Id is not null)
            {
                await Input.FocusAsync();
            }
        }

        public async Task InputKeyDown(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter" || e.Code == "Return")
            {
                await Add();

                StateHasChanged();
            }
        }

        public async Task Add()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return;
            }

            ReadListItem item = new ReadListItem()
            {
                Id = 0,
                Url = Url,
                Added = DateTime.UtcNow
            };

            await ReadListService.Add(item);

            Url = string.Empty;
            await Input.FocusAsync();

            StateHasChanged();
        }

        public async Task Delete(int id)
        {
            if (id <= 0)
            {
                return;
            }

            await ReadListService.Delete(id);

            StateHasChanged();
        }
    }
}