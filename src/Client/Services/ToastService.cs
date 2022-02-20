using Microsoft.JSInterop;

namespace PeterPedia.Client.Services;

public interface IToastService
{
    Task ShowError(string text);

    Task ShowSuccess(string text);
}

public class ToastService : IToastService
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public ToastService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task ShowError(string text)
    {
        if (_module == null)
        {
            _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/toast.js");
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ToastError", text);
        }
    }

    public async Task ShowSuccess(string text)
    {
        if (_module == null)
        {
            _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/toast.js");
        }

        if (_module is not null)
        {
            await _module.InvokeVoidAsync("ToastSuccess", text);
        }
    }
}
