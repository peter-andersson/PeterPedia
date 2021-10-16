﻿namespace PeterPedia.Client.Movie.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using PeterPedia.Client.Movie.Services;
    using PeterPedia.Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Movies : ComponentBase
    {
        [Inject]
        private MovieService MovieService { get; set; }

        private enum NavigatePage
        {
            Next,
            Previous,
            First
        }

        private readonly int pageSize = 25;
        private int currentPage = 0;

        private string CurrentSearch = string.Empty;
        private EditContext SearchContext;
        private List<Movie> MovieList;
        private bool IsPreviousButtonDisabled = false;
        private bool IsNextButtonDisabled = false;

        protected override async Task OnInitializedAsync()
        {
            SearchContext = new EditContext(CurrentSearch);

            await MovieService.FetchData();

            LoadAllMovies();
        }

        private void NavigateToPage(NavigatePage navigateTo)
        {
            switch (navigateTo)
            {
                case NavigatePage.Next:
                    currentPage += 1;
                    break;
                case NavigatePage.Previous:
                    currentPage -= 1;
                    if (currentPage < 0)
                    {
                        currentPage = 0;
                    }
                    break;
                case NavigatePage.First:
                    currentPage = 0;
                    break;
            }

            LoadAllMovies();
        }

        private void LoadAllMovies()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSearch))
            {
                MovieList = MovieService.Movies.Where(m => m.Title.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase) || m.OriginalTitle.Contains(CurrentSearch, StringComparison.InvariantCultureIgnoreCase)).OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
            }
            else
            {
                MovieList = MovieService.Movies.OrderBy(m => m.Title).Skip(currentPage * pageSize).Take(pageSize).ToList();
            }

            IsPreviousButtonDisabled = currentPage == 0;
            IsNextButtonDisabled = MovieList.Count < pageSize;

            StateHasChanged();
        }
    }
}
