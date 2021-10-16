using Microsoft.AspNetCore.Components;
using PeterPedia.Client.Reader.Services;
using System.Threading.Tasks;
using PeterPedia.Shared;

namespace PeterPedia.Client.Reader.Pages
{
    public partial class ArticleView
    {
        [Inject]
        RSSService RSSService { get; set; }

        [Parameter]
        public Article Article { get; set; }

        [Parameter]
        public EventCallback<Article> OnArticleRemove { get; set; }

        private bool IsTaskRunning;

        private async Task Delete()
        {
            if (Article is null)
            {
                return;
            }

            IsTaskRunning = true;
            var result = await RSSService.DeleteArticle(Article.Id);
            IsTaskRunning = false;

            if (result)
            {
                await OnArticleRemove.InvokeAsync(Article);
            }
        }
    }
}
