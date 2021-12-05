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
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;
        private readonly PeterPediaContext _dbContext;

        public BookController(ILogger<BookController> logger, PeterPediaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("{id:int?}")]
        public async Task<IActionResult> Get(int? id)
        {
            _logger.LogDebug($"Get book id: {id}");
            if (id.GetValueOrDefault(0) > 0)
            {
                var bookEF = await _dbContext.Books.Where(s => s.Id == id).Include(b => b.Authors).AsSplitQuery().SingleOrDefaultAsync().ConfigureAwait(false);

                if (bookEF is null)
                {
                    return NotFound();
                }

                return Ok(ConvertToBook(bookEF));
            }
            else
            {
                var books = await _dbContext.Books.Include(b => b.Authors).AsSplitQuery().ToListAsync().ConfigureAwait(false);

                var result = new List<Book>(books.Count);
                foreach (var book in books)
                {
                    result.Add(ConvertToBook(book));
                }

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Book book)
        {
            if (book is null)
            {
                return BadRequest();
            }

            var bookEF = new BookEF
            {
                Title = book.Title,
                State = (int)book.State,
                Authors = new List<AuthorEF>(),
            };

            foreach (var name in book.Authors)
            {
                var author = await _dbContext.Authors.Where(a => a.Name == name.Trim()).AsTracking().FirstOrDefaultAsync();

                if (author is null)
                {
                    author = new AuthorEF()
                    {
                        Name = name.Trim(),
                    };

                    _logger.LogDebug($"Adding new author {name}");
                    _dbContext.Authors.Add(author);
                }

                bookEF.Authors.Add(author);
            }

            _dbContext.Books.Add(bookEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug("Book addded");

            return Ok(ConvertToBook(bookEF));
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Book book)
        {
            if (book is null)
            {
                return BadRequest();
            }

            var bookEF = await _dbContext.Books.Where(b => b.Id == book.Id).Include(b => b.Authors).AsSplitQuery().AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);
            if (bookEF is null)
            {
                return NotFound();
            }

            bookEF.Authors.Clear();
            foreach (var name in book.Authors)
            {
                var author = await _dbContext.Authors.Where(a => a.Name == name.Trim()).AsTracking().FirstOrDefaultAsync();

                if (author is null)
                {
                    author = new AuthorEF()
                    {
                        Name = name.Trim(),
                    };

                    _logger.LogDebug($"Adding new author {name}");
                    _dbContext.Authors.Add(author);
                }

                bookEF.Authors.Add(author);
            }

            bookEF.State = (int)book.State;
            bookEF.Title = book.Title;

            _dbContext.Books.Update(bookEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogDebug("Book updated");

            return Ok(ConvertToBook(bookEF));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug($"Delete book with id {id}");
            if (id <= 0)
            {
                return BadRequest();
            }

            var bookEF = await _dbContext.Books.Where(b => b.Id == id).AsTracking().SingleOrDefaultAsync().ConfigureAwait(false);

            if (bookEF is null)
            {
                _logger.LogDebug("Book not found");
                return NotFound();
            }

            _dbContext.Books.Remove(bookEF);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogDebug("Book removed");

            return Ok();
        }

        private static Book ConvertToBook(BookEF bookEF)
        {
            if (bookEF is null)
            {
                throw new ArgumentNullException(nameof(bookEF));
            }

            var book = new Book()
            {
                Id = bookEF.Id,
                Title = bookEF.Title,
                State = (BookState)bookEF.State,
            };

            foreach (var author in bookEF.Authors)
            {
                book.Authors.Add(author.Name);
            }

            return book;
        }
    }
}
