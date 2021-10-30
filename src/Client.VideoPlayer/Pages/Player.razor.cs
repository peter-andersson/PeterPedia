namespace PeterPedia.Client.VideoPlayer.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.VideoPlayer.Services;
    using System;
    using Microsoft.AspNetCore.Components.Web;

    public partial class Player : ComponentBase
    {
        [Inject]
        public VideoService VideoService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        public Video Video { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Video = await VideoService.GetVideoAsync(Id);
        }

        public async Task Delete()
        {
            if (await VideoService.Delete(Id))
            {
                NavManager.NavigateTo("");
            }
        }
    }
}