namespace PeterPedia.Client.Book.Pages
{
    using PeterPedia.Shared;
    using Microsoft.AspNetCore.Components;

    public partial class BookView : ComponentBase
    {
        [Parameter]
        public Book Book { get; set; }

        [Parameter]
        public string ReturnUrl { get; set; }
    }
}