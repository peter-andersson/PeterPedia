using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Shared;

public partial class Loader : ComponentBase
{
    [Parameter]
    public string? Text { get; set; }    
}