namespace PeterPedia.Client.Book.Pages
{
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Book.Services;
    using System.Threading.Tasks;

    public partial class Delete : ComponentBase
    {
        [Inject]
        private BookService BookService { get; set; }

        [Inject]
        private NavigationManager NavManager { get; set; }

        [Parameter]
        public int Id { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }

        private string Title;
        private bool IsTaskRunning;

        protected override async Task OnInitializedAsync()
        {
            IsTaskRunning = false;
            var book = await BookService.Get(Id);

            ReturnUrl = ReturnUrl ?? "/";

            if (book is null)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
            else
            {
                Title = book.Title;
            }
        }

        private void Cancel()
        {
            NavManager.NavigateTo(ReturnUrl);
        }

        private async Task DeleteBook()
        {
            IsTaskRunning = true;

            var result = await BookService.Delete(Id);

            IsTaskRunning = false;
            if (result)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
        }
    }
}
