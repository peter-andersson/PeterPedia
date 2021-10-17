namespace PeterPedia.Client.Bookmark.Services
{
    using PeterPedia.Shared;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Blazored.Toast.Services;

    public class ReadListService
    {
        private readonly HttpClient _http;

        private readonly IToastService _toast;

        public ReadListService(HttpClient httpClient, IToastService toastService)
        {
            _http = httpClient;
            _toast = toastService;
        }

        public List<ReadListItem> Items { get; private set; }

        public async Task FetchData()
        {
            if ((Items is null) || (Items.Count == 0))
            {
                var items = await _http.GetFromJsonAsync<ReadListItem[]>("/api/ReadList");

                Items = new List<ReadListItem>(items.Length);
                Items.AddRange(items);

                Items = Items.OrderBy(a => a.Added).ToList();
            }
        }

        public async Task<bool> Add(ReadListItem item)
        {
            if (item is null)
            {
                _toast.ShowError("Invalid item, can't add");
                return false;
            }

            using var response = await _http.PostAsJsonAsync("/api/ReadList", item);

            if (response.IsSuccessStatusCode)
            {
                _toast.ShowSuccess("Added new item to reading list.");

                item = await response.Content.ReadFromJsonAsync<ReadListItem>();

                Items.Add(item);

                return true;
            }
            else
            {
                _toast.ShowError($"Failed to add item. StatusCode = {response.StatusCode}");
                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var item = Items.Where(item => item.Id == id).SingleOrDefault();
            if (item is null)
            {
                _toast.ShowError($"{id} is not a valid id. Can't remove item.");
                return false;
            }

            using var response = await _http.DeleteAsync($"/api/ReadList/{id}");

            if (response.IsSuccessStatusCode)
            {
                _toast.ShowSuccess("Item deleted");

                Items.Remove(item);

                return true;
            }
            else
            {
                _toast.ShowSuccess($"Failed to delete item. StatusCode = {response.StatusCode}");

                return false;
            }
        }
    }
}
