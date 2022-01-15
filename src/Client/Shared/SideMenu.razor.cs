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

            var url = NavigationManager.Uri.ToLowerInvariant();

            if (url.Contains("/movies"))
            {
                hasChanged = LoadMoviesMenu();
            }
            else if (url.Contains("/books"))
            {
                hasChanged = LoadBooksMenu();
            }
            else if (url.Contains("/episodes"))
            {
                hasChanged = LoadEpisodesMenu();
            }
            else if (url.Contains("/reader"))
            {
                hasChanged = LoadReaderMenu();
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

        private bool LoadEpisodesMenu()
        {
            if (CurrentPage == "episodes")
            {
                return false;
            }

            Items.Clear();
            CurrentPage = "episodes";

            Items.Add(new MenuItem("episodes", "Unwatched"));
            Items.Add(new MenuItem("episodes/shows", "Shows"));
            Items.Add(new MenuItem("episodes/add", "Add show"));

            return true;
        }

        public bool LoadReaderMenu()
        {
            if (CurrentPage == "reader")
            {
                return false;
            }

            Items.Clear();
            CurrentPage = "reader";

            Items.Add(new MenuItem("reader", "Unread"));
            Items.Add(new MenuItem("reader/subscriptions", "Subscriptions"));
            Items.Add(new MenuItem("reader/history", "History"));
            Items.Add(new MenuItem("reader/add", "Add subscriptions"));

            return true;
        }
    }
}
