using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required by Azure function runtime.")]
public class Image
{
    private readonly ILogger<Image> _log;
    private readonly BlobStorage _blobStorage;

    public Image(ILogger<Image> log, BlobStorage fileStorage)
    {
        _log = log;
        _blobStorage = fileStorage;
    }

    [FunctionName("Image")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "image/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new NotFoundResult();
        }

        try
        {
            using var stream = new MemoryStream();
            await _blobStorage.GetPosterAsync(id, stream);
            return new FileStreamResult(stream, "image/jpeg");
        }
        catch (FileNotFoundException)
        {
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
