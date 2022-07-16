using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace Library.Api.Functions;

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
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "image/{image}")] HttpRequest req,
        string image,
        CancellationToken _)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return req.NotFound();
        }

        try
        {
            var stream = new MemoryStream();
            if (await _fileStorage.DownloadAsync(image, stream))
            {
                return req.CachedFileStream(stream, "image/jpeg");
            }

            // 
            return req.NotFound();
        }       
        catch (Exception ex)
        {
            _log.LogError(ex, "Something went wrong.");
            return req.InternalServerError();
        }
    }
}
