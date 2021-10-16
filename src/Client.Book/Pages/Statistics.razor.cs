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

    public partial class Statistics : ComponentBase
    {
        [Inject]
        BookService BookService { get; set; }

        private int Reading { get; set; }
        private int Read { get; set; }
        private int WantToRead { get; set; }
        private int Total
        {
            get
            {
                return Reading + Read + WantToRead;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Read = 0;
            Reading = 0;
            WantToRead = 0;

            await BookService.FetchData();

            CalculateStatisticts();
        }

        private void CalculateStatisticts()
        {
            foreach (var book in BookService.Books)
            {
                switch (book.State)
                {
                    case BookState.WantToRead:
                        WantToRead += 1;
                        break;
                    case BookState.Reading:
                        Reading += 1;
                        break;
                    case BookState.Read:
                        Read += 1;
                        break;
                }
            }
        }
    }
}
