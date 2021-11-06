using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Pages
{
    public class EditModel : PageModel
    {
        private readonly PeterPediaContext _dbContext;

        public EditModel(PeterPediaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ICollection<LinkEF> Links { get; set; } = null!;

        [BindProperty]
        public LinkEF Link { get; set; } = null!;

        public async Task OnGetAsync(int? id)
        {
            Links = await _dbContext.Links.OrderBy(l => l.Title).ToListAsync();

            if (id.HasValue)
            {
                var link = Links.Where(l => l.Id == id.Value).SingleOrDefault();

                if (link is not null)
                {
                    Link = link;
                }
            }

            if (Link is null)
            {
                Link = new LinkEF();
            }
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            if (id > 0)
            {
                var link = _dbContext.Links.Where(l => l.Id == id).SingleOrDefault();

                if (link is not null)
                {
                    _dbContext.Links.Remove(link);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToPage("./Edit", new { });
                }
                else
                {
                    return NotFound();
                }
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Link is null)
            {
                return Page();
            }

            if (Link.Id == 0)
            {
                _dbContext.Links.Add(Link);
            }
            else
            {
                _dbContext.Links.Update(Link);
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = (int?)null });
        }
    }
}
