namespace PeterPedia.Client.VideoPlayer.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.VideoPlayer.Services;
    using System;
    using Microsoft.AspNetCore.Components.Web;

    public partial class Index : ComponentBase
    {
        [Inject]
        public VideoService VideoService { get; set; }

        public List<Video> Videos { get; set; } = new List<Video>();

        protected override async Task OnInitializedAsync()
        {
            await VideoService.FetchData();

            Videos = VideoService.Videos;
        }
    }
}