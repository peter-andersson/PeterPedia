using Microsoft.AspNetCore.Components;

namespace PeterPedia.Client.Pages.Episodes;

public partial class EpisodesDialog : ComponentBase
{
    [Inject]
    private IEpisodeManager EpisodeManager { get; set; } = null!;

    [CascadingParameter]
    private BlazoredModalInstance ModalInstance { get; set; } = null!;

    public List<Episode> Episodes { get; set; } = new();

    protected override async Task OnInitializedAsync() => Episodes = await EpisodeManager.GetEpisodesAsync();

    private async Task CloseAsync() => await ModalInstance.CancelAsync();
}
