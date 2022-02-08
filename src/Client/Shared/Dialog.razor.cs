using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace PeterPedia.Client.Shared;

public partial class Dialog : ComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public RenderFragment? Body { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }
    
    [Parameter]
    public string? CssClass { get; set; }
}