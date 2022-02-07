using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Shared;

public partial class Table<T> : ComponentBase
{
    [Parameter]
    public RenderFragment? TableHeader { get; set; }

    [Parameter]
    public RenderFragment<T>? RowTemplate { get; set; }

    [Parameter, AllowNull]
    public IReadOnlyList<T> Items { get; set; }
    
    [Parameter]
    public bool SearchEnabled { get; set; } = false;

    [Parameter, AllowNull]
    public EventCallback<string> OnSearch { get; set; }

    [Parameter]
    public bool PaginationEnabled { get; set; } = false;

    [Parameter]
    public int PageSize { get; set; } = 25;

    public string CurrentSearch { get; set; } = string.Empty;

    public EditContext SearchContext { get; set; } = null!;

    [AllowNull]
    public IReadOnlyList<T> PaginatedItems { get; set; }

    private int CurrentPage { get; set; } = 1;

    public int PageCount { get; set; }

    public bool IsPreviousButtonDisabled { get; set; } = false;

    public bool IsNextButtonDisabled { get; set; } = false;

    protected override void OnInitialized()
    {
        SearchContext = new EditContext(CurrentSearch);

        LoadPaginateditems();
    }

    protected override void OnParametersSet()
    {
        LoadPaginateditems();
    }

    public async Task Search()
    {
        await OnSearch.InvokeAsync(CurrentSearch);

        CurrentPage = 1;
    }

    public void NavigateToPage(int page)
    {
        if (page > 0)
        {
            CurrentPage = page;
        }
        else
        {
            CurrentPage = PageCount;
        }

        LoadPaginateditems();
    }

    private void LoadPaginateditems()
    {
        if (PaginationEnabled)
        {
            PaginatedItems = Items.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            double pages = (double)Items.Count / (double)PageSize;
            PageCount = (int)Math.Round(Math.Max(pages, 1.0), 0, MidpointRounding.AwayFromZero);

            IsPreviousButtonDisabled = CurrentPage == 1;
            IsNextButtonDisabled = CurrentPage * PageSize >= Items.Count;
        }
        else
        {
            PaginatedItems = Items;
        }
    }
}