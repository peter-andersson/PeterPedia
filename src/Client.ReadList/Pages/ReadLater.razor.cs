namespace PeterPedia.Client.ReadList.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.ReadList.Services;
    using System;
    using Microsoft.AspNetCore.Components.Forms;

    public partial class ReadLater : ComponentBase
    {
        [Inject]
        public ReadListService ReadListService { get; set; }

        public List<ReadListItem> Items { get; set; } = new List<ReadListItem>();

        public string Url { get; set; } = string.Empty;

        private EditContext AddContext;

        private ElementReference Input;

        protected override async Task OnInitializedAsync()
        {
            AddContext = new EditContext(Url);

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