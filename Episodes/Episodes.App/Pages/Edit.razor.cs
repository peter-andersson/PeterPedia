using Microsoft.AspNetCore.Components;

namespace Episodes.App.Pages;

public partial class Edit : ComponentBase, IDisposable
{
    [Inject]
    private ITVService Service { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private Navigation Navigation { get; set; } = null!;

    [Parameter]
    public string Id { get; set; } = null!;

    private EditModel EditModel { get; set; } = new();

    private bool IsSaveTaskRunning { get; set; }

    private bool IsDeleteTaskRunning { get; set; }

    private bool Loading { get; set; } = true;

    private bool ShowAll { get; set; } = false;

    private bool _disposed = false;

    protected override async Task OnInitializedAsync()
    {
        IsSaveTaskRunning = false;
        IsDeleteTaskRunning = false;

        Service.OnChange += StateHasChanged;

        TVShow? show = await Service.GetAsync(Id);
        if (show is not null)
        {
            EditModel = new EditModel(show);
        }

        Loading = false;
    }

    private void ToggleShowAll() => ShowAll = !ShowAll;

    private async Task SaveAsync()
    {
        if (EditModel.Show is null)
        {
            return;
        }

        EditModel.SaveProperties();

        IsSaveTaskRunning = true;

        Result result = await Service.UpdateAsync(EditModel.Show);

        if (result.Success)
        {
            ToastService.ShowSuccess("TV show updated.");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsSaveTaskRunning = false;
    }

    private async Task DeleteAsync()
    {
        if (EditModel.Show is null)
        {
            return;
        }

        IsDeleteTaskRunning = true;

        Result result = await Service.DeleteAsync(EditModel.Show);

        if (result.Success)
        {
            ToastService.ShowSuccess("TV show updated.");
        }
        else
        {
            ToastService.ShowError(result.ErrorMessage);
        }

        IsDeleteTaskRunning = false;
    }

    private void Close()
    {
        if (Navigation.CanNavigateBack)
        {
            Navigation.NavigateBack();
        }
        else
        {
            Navigation.NavigateTo("/");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Service.OnChange -= StateHasChanged;
            EditModel = new();
        }

        _disposed = true;
    }
}
