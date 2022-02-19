using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class PhotoController : Controller
{
    private readonly PeterPediaContext _dbContext;
    private readonly IConfiguration _configuration;

    public PhotoController(PeterPediaContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _dbContext.Photos.Include(p => p.Album).ToListAsync().ConfigureAwait(false);

        var result = new List<Photo>(items.Count);
        foreach (var item in items)
        {
            result.Add(ConvertToItem(item));
        }

        return Ok(result);
    }
    
    private Photo ConvertToItem(PhotoEF itemEF)
    {
        if (itemEF is null)
        {
            throw new ArgumentNullException(nameof(itemEF));
        }

        var photo = new Photo()
        {
            Album = itemEF.Album.Name,
        };

        var basePath = _configuration["PhotoPath"] ?? "/photos";
        var relativePath = Path.GetRelativePath(basePath, itemEF.AbsolutePath);

        photo.Url = "/photo/" + relativePath.Replace('\\', '/');

        return photo;
    }
}
