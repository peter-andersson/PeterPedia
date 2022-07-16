using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PeterPedia.Shared;

public static class HttpRequestExtensions
{
    public static IActionResult NotFound(this HttpRequest _) => new NotFoundResult();

    public static IActionResult InternalServerError(this HttpRequest _) => new StatusCodeResult(StatusCodes.Status500InternalServerError);

    public static IActionResult BadRequest(this HttpRequest _, string message) => string.IsNullOrWhiteSpace(message) ? new BadRequestResult() : new BadRequestObjectResult(message);

    public static IActionResult Ok(this HttpRequest _, object? value = null) => value is null ? new OkResult() : new OkObjectResult(value);

    public static IActionResult Conflict(this HttpRequest _) => new ConflictResult();

    public static IActionResult CachedFileStream(this HttpRequest req, Stream stream, string contentType)
    {
        req.HttpContext.Response.Headers.Add("Cache-Control", "public, max-age=604800");
        return new FileStreamResult(stream, contentType);
    }
}
