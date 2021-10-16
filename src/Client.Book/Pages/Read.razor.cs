namespace PeterPedia.Client.Book.Pages
{
    using Microsoft.AspNetCore.Components;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PeterPedia.Shared;
    using PeterPedia.Client.Book.Services;
    using Microsoft.AspNetCore.Components.Forms;

    public partial class Read : ComponentBase
    {
        [Inject]
        BookService BookService { get; set; }

        private List<Book> BookList = new();

        private string CurrentSearch = string.Empty;
        private EditContext SearchContext;

        protected override async Task OnInitializedAsync()
        {
            SearchContext = new EditContext(CurrentSearch);

            await BookService.FetchData();

            LoadDisplayBooks();
        }

        private void LoadDisplayBooks()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSearch))
            {
                BookList = BookService.Books
                    .Where(b => b.State == BookState.Read && (b.Title.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase) || b.SearchAuthor(CurrentSearch)))
                    .OrderBy(m => m.Title)
                    .ToList();
            }
            else
            {
                BookList = BookService.Books
                    .Where(b => b.State == BookState.Read)
                    .OrderBy(b => b.Title)
                    .ToList();
            }

            StateHasChanged();
        }
    }
}
