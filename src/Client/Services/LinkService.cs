using System.Net.Http.Json;
using Blazored.Toast.Services;
using System.Text.Json;

namespace PeterPedia.Client.Services;

public class LinkService
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private static readonly PeterPediaJSONContext Context = new(Options);

    private readonly HttpClient _http;
    private readonly IToastService _toast;

    private readonly List<Link> _linkList = new();

    public LinkService(HttpClient httpClient, IToastService toastService)
    {
        _http = httpClient;
        _toast = toastService;
    }

    public async Task<List<Link>> GetLinks()
    {
        await FetchLinks();
       
        return _linkList.OrderBy(l => l.Title).ToList();
    }
   
    public async Task<bool> Upsert(Link link)
    {
        await FetchLinks();

        bool add = link.Id == 0;

        using var response = await _http.PostAsJsonAsync("/api/Link", link, Context.Link);

        if (response.IsSuccessStatusCode)
        {
            Link? serverLink = await response.Content.ReadFromJsonAsync(Context.Link);

            if (serverLink is not null)
            {
                if (add)
                {
                    _toast.ShowSuccess($"Added link for {link.Title} - {link.Url}");
                    _linkList.Add(serverLink);
                }
                else
                {
                    _toast.ShowSuccess($"Updated link for {link.Title} - {link.Url}");

                    Link? existingLink = _linkList.Where(l => l.Id == link.Id).FirstOrDefault();
                    if (existingLink is null)
                    {
                        _linkList.Add(serverLink);
                    }
                    else
                    {
                        existingLink.Title = serverLink.Title;
                        existingLink.Url = serverLink.Url;
                    }
                }
                              
                return true;
            }
            else
            {
                _toast.ShowError($"Failed to upsert link. No movie from server.");

                return false;
            }            
        }
        else
        {
            _toast.ShowError($"Failed to upsert link. StatusCode = {response.StatusCode}");

            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        await FetchLinks();

        Link? link = _linkList.Where(l => l.Id == id).FirstOrDefault();
        if (link is null)
        {
            _toast.ShowError($"{id} is not a valid link id. Can't remove link.");
            return false;
        }

        using var response = await _http.DeleteAsync($"/api/Link/{id}");

        if (response.IsSuccessStatusCode)
        {
            _toast.ShowSuccess($"Link {link.Title} deleted");

            _linkList.Remove(link);

            return true;
        }
        else
        {
            _toast.ShowError($"Failed to delete link. StatusCode = {response.StatusCode}");

            return false;
        }
    }   

    private async Task FetchLinks()
    {
        if (_linkList.Count > 0)
        {
            return;
        }

        Link[]? links = await _http.GetFromJsonAsync("/api/Link", Context.LinkArray);

        _linkList.Clear();

        if (links is not null)
        {
            _linkList.AddRange(links);
        }
    }
}
