namespace PeterPedia.Client.Book.Pages
{
    using Microsoft.AspNetCore.Components;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using PeterPedia.Client.Book.Services;

    public partial class ReadList : ComponentBase
    {
        [Inject]
        BookService BookService { get; set; }

        private List<Book> BookList = new();

        protected override async Task OnInitializedAsync()
        {
            await BookService.FetchData();

            BookList = BookService.Books.Where(b => b.State == BookState.WantToRead).OrderBy(b => b.Title).ToList();
        }
    }
}
