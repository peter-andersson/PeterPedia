namespace PeterPedia.Client.Movie.Pages
{
    using Microsoft.AspNetCore.Components;
    using PeterPedia.Client.Movie.Services;
    using PeterPedia.Shared;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public partial class MovieTable : ComponentBase
    {
        [Parameter]
        public List<Movie> Movies { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }
    }
}