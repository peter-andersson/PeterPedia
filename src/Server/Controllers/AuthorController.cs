using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using PeterPedia.Shared;
using PeterPedia.Server.Data;
using PeterPedia.Server.Data.Models;

namespace PeterPedia.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : Controller
    {
        private readonly ILogger<AuthorController> _logger;
        private readonly PeterPediaContext _dbContext;

        public AuthorController(ILogger<AuthorController> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Get(int? id)
        {
            _logger.LogDebug($"Get author id: {id}");
            if (id.GetValueOrDefault(0) > 0)
            {
                var authorEF = await _dbContext.Authors.Where(s => s.Id == id).SingleOrDefaultAsync().ConfigureAwait(false);

                if (authorEF is null)
                {
                    return NotFound();
                }

                return Ok(ConvertToAuthor(authorEF));
            }
            else
            {
                var authors = await _dbContext.Authors.ToListAsync().ConfigureAwait(false);

                var result = new List<Author>(authors.Count);
                foreach (var author in authors)
                {
                    result.Add(ConvertToAuthor(author));
                }

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Author author)
        {
            if (author is null)
            {
                return BadRequest();
            }

            _logger.LogDebug($"Adding new author {author.Name}");

            var existingAuthor = await _dbContext.Authors.Where(s => s.Name == author.Name.Trim()).FirstOrDefaultAsync().ConfigureAwait(false);
            if (existingAuthor != null)
            {
                _logger.LogDebug("Author with name already exists.");
                return Conflict();
            }

            var authorEF = new AuthorEF
            {
                Name = author.Name.Trim(),
            };

            _dbContext.Authors.Add(authorEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug("Author addded");

            return Ok(ConvertToAuthor(authorEF));
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Author author)
        {
            if (author is null)
            {
                return BadRequest();
            }

            var existingAuthor = await _dbContext.Authors.Where(s => s.Id == author.Id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
            if (existingAuthor is null)
            {
                return NotFound();
            }

            if (author.Name.Trim() != existingAuthor.Name)
            {
                existingAuthor.Name = author.Name.Trim();
                _dbContext.Authors.Update(existingAuthor);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogDebug("Author updated");
            }

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete author with id {id}");
            if (id <= 0)
            {
                return BadRequest();
            }

            var author = await _dbContext.Authors.Where(a => a.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

            if (author is null)
            {
                _logger.LogDebug("Author not found");
                return NotFound();
            }

            _dbContext.Authors.Remove(author);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogDebug("Author removed");

            return Ok();
        }

        private static Author ConvertToAuthor(AuthorEF authorEF)
        {
            if (authorEF is null)
            {
                throw new ArgumentNullException(nameof(authorEF));
            }

            var author = new Author()
            {
                Id = authorEF.Id,
                Name = authorEF.Name,
            };

            return author;
        }
    }
}
