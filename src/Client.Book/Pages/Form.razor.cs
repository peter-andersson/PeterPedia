namespace PeterPedia.Client.Book.Pages
{
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Book.Services;
    using PeterPedia.Shared;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    public partial class Form : ComponentBase
    {
        [Inject]
        BookService BookService { get; set; }

        [Inject]
        NavigationManager NavManager { get; set; }

        [Parameter]
        public int? Id { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }

        private IList<Author> SelectedAuthor;

        private Book Book;

        private List<Author> Authors = new List<Author>();

        private string Title
        {
            get
            {
                if (Book is null)
                {
                    return "Loading...";
                }

                if (Book.Id > 0)
                {
                    return "Edit book";
                }
                else
                {
                    return "Add book";
                }
            }
        }

        private bool IsTaskRunning = false;

        protected override async Task OnInitializedAsync()
        {
            await BookService.FetchData();

            ReturnUrl = ReturnUrl ?? "/";

            Book book = null;

            if (Id.HasValue)
            {
                book = await BookService.Get(Id.Value);

                if (book is null)
                {
                    Book = new Book();
                    Book.State = BookState.WantToRead;
                }
                else
                {
                    Book = await CreateCopy(book);
                }
            }

            if (book is null)
            {
                Book = new Book();
                Book.State = BookState.WantToRead;
            }

            Authors.Clear();
            Authors.AddRange(BookService.Authors);
        }

        private async Task<IEnumerable<Author>> GetAuthor(string searchText)
        {
            return await Task.FromResult(Authors.Where(x => x.Name.ToLower().Contains(searchText.ToLower())).ToList());
        }

        private Task<Author> ItemAddedMethod(string searchText)
        {
            var author = new Author();
            author.Name = searchText;
            Authors.Add(author);
            return Task.FromResult(author);
        }

        private async Task Save()
        {
            IsTaskRunning = true;

            Book.Authors.Clear();
            foreach (var author in SelectedAuthor)
            {
                Book.Authors.Add(author.Name);
            }

            bool result;
            if (Book.Id > 0)
            {
                result = await BookService.Update(Book);
            }
            else
            {
                result = await BookService.Add(Book);
            }

            IsTaskRunning = false;
            if (result)
            {
                if (Book.Id > 0)
                {
                    NavManager.NavigateTo(ReturnUrl);
                }
            }
        }

        private void Cancel()
        {
            if (Book.Id > 0)
            {
                NavManager.NavigateTo(ReturnUrl);
            }
        }

        private async Task<Book> CreateCopy(Book book)
        {
            var result = new Book()
            {
                Id = book.Id,
                State = book.State,
                Title = book.Title,
            };

            SelectedAuthor = new List<Author>();
            foreach (var author in book.Authors)
            {
                var authorData = await BookService.GetAuthor(author);
                if (authorData is not null)
                {
                    result.Authors.Add(author);
                    SelectedAuthor.Add(authorData);
                }
            }

            result.AuthorText = string.Join(", ", result.Authors);

            return result;
        }
    }
}
