using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PeterPedia.Client.Episodes.Services;
using System.Threading.Tasks;

namespace PeterPedia.Client.Episodes.Pages
{
    public partial class Add : ComponentBase
    {
        [Inject]
        private TVService TVService { get; set; }

        private ElementReference Input;
        private EditContext AddContext;
        private bool IsTaskRunning;
        private string TVUrl;

        protected override void OnInitialized()
        {
            TVUrl = string.Empty;
            IsTaskRunning = false;

            AddContext = new EditContext(TVUrl);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Input.Id is not null)
            {
                await Input.FocusAsync();
            }
        }

        private async Task AddShow()
        {
            IsTaskRunning = true;

            var result = await TVService.Add(TVUrl);
            IsTaskRunning = false;
            if (result)
            {
                TVUrl = string.Empty;
            }
        }
    }
}
