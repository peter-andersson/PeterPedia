using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PeterPediaContext _dbContext;

        public IndexModel(PeterPediaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ICollection<LinkEF> Links { get; set; } = null!;

        public async Task OnGetAsync()
        {
            Links = await _dbContext.Links.OrderBy(l => l.Title).ToListAsync();
        }
    }
}
