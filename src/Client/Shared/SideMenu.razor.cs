using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Models;

namespace PeterPedia.Client.Shared
{
    public partial class SideMenu : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public List<MenuItem> Items { get; private set; } = new List<MenuItem>();

        private string CurrentPage = string.Empty;

        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;

            LoadMenu();
        }

        private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            LoadMenu();
        }

        private void LoadMenu()
        {
            bool hasChanged;

            if (NavigationManager.Uri.Contains("/movies"))
            {
                hasChanged = LoadMoviesMenu();
            }
            else if (NavigationManager.Uri.Contains("/books"))
            {
                hasChanged = LoadBooksMenu();
            }
            else
            {
                hasChanged = LoadEmptyMenu();
            }

            if (hasChanged)
            {
                StateHasChanged();
            }            
        }

        private bool LoadEmptyMenu()
        {
            if (string.IsNullOrWhiteSpace(CurrentPage))
            {
                return false;
            }

            Items.Clear();
            CurrentPage = string.Empty;
            return true;
        }

        private bool LoadMoviesMenu()
        {
            if (CurrentPage == "movies")
            {
                return false;
            }

            Items.Clear();
            CurrentPage = "movies";

            Items.Add(new MenuItem("movies", "Watchlist"));
            Items.Add(new MenuItem("movies/all", "All movies"));
            Items.Add(new MenuItem("movies/add", "Add movie"));

            return true;
        }

        private bool LoadBooksMenu()
        {
            if (CurrentPage == "books")
            {
                return false;
            }

            Items.Clear();
            CurrentPage = "books";

            Items.Add(new MenuItem("books", "Reading"));
            Items.Add(new MenuItem("books/readlist", "Want to read"));
            Items.Add(new MenuItem("books/read", "Read"));            

            return true;
        }
    }
}
