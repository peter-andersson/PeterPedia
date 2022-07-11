using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Movies.Api.Functions;

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
#pragma warning disable IDE0060 // Remove unused parameter
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "image/{image}")] HttpRequest req,
        string image,
        CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return new NotFoundResult();
        }

        try
        {
            var stream = new MemoryStream();
            await _blobStorage.GetPosterAsync(image, stream);
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
