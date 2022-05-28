using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Error;

public partial class ErrorPage : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    public List<ErrorEF> Errors { get; set; } = new();

    protected override async Task OnInitializedAsync() =>
        Errors = await DbContext.Errors.OrderByDescending(c => c.Timestamp).Take(100).ToListAsync();
}
