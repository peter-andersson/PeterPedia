using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using PeterPedia.Server.Data;
using PeterPedia.Shared;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadListController : Controller
    {
        private readonly ILogger<ReadListController> _logger;
        private readonly PeterPediaContext _dbContext;

        public ReadListController(ILogger<ReadListController> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogDebug("Get readlist items");
            var items = await _dbContext.ReadListItems.AsNoTracking().ToListAsync().ConfigureAwait(false);

            var result = new List<ReadListItem>(items.Count);
            foreach (var item in items)
            {
                result.Add(ConvertToItem(item));
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ReadListItem item)
        {
            _logger.LogDebug($"Add url: {item?.Url}");
            if (string.IsNullOrWhiteSpace(item?.Url))
            {
                return BadRequest();
            }

            if (!Uri.TryCreate(item.Url, UriKind.Absolute, out Uri? uriResult))
            {
                return BadRequest();
            }

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                return BadRequest();
            }

            var existingItem = await _dbContext.ReadListItems.Where(r => r.Url == item.Url).SingleOrDefaultAsync();

            if (existingItem is not null)
            {
                return Conflict();
            }

            var dbItem = new ReadListEF()
            {
                Added = DateTime.UtcNow,
                Url = item.Url,
            };

            _dbContext.ReadListItems.Add(dbItem);

            await _dbContext.SaveChangesAsync();

            return Ok(ConvertToItem(dbItem));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete id: {id}");
            var item = await _dbContext.ReadListItems.FindAsync(id).ConfigureAwait(false);

            if (item is null)
            {
                return NotFound();
            }

            _dbContext.ReadListItems.Remove(item);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok();
        }

        private static ReadListItem ConvertToItem(ReadListEF itemEF)
        {
            if (itemEF is null)
            {
                throw new ArgumentNullException(nameof(itemEF));
            }

            var readListItem = new ReadListItem()
            {
                Id = itemEF.Id,
                Added = itemEF.Added,
                Url = itemEF.Url
            };

            return readListItem;
        }
    }
}
