using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Episodes.Api.Functions;

public class Image
{
    private readonly ILogger<Image> _log;
    private readonly IFileStorage _fileStorage;

    public Image(ILogger<Image> log, IFileStorage fileStorage)
    {
        _log = log;
        _fileStorage = fileStorage;
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
            if (await _fileStorage.DownloadBlobAsync(image, stream))
            {
                return new FileStreamResult(stream, "image/jpeg");
            }

            // 
            return new NotFoundResult();            
        }       
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return new StatusCodeResult(500);
        }
    }
}
