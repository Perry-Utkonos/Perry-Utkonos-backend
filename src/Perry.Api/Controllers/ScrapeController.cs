using Perry.Api.BackgroundServices;
using Microsoft.AspNetCore.Mvc;

namespace Perry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapeController : ControllerBase
{
    private readonly TaskQueue _taskQueue;
    private readonly ILogger<ScrapeController> _logger;

    public ScrapeController(TaskQueue taskQueue, ILogger<ScrapeController> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Enqueue([FromBody] ScrapeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { message = "URL is required" });

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return BadRequest(new { message = "URL is not valid" });

        var accepted = await _taskQueue.TryEnqueueAsync(request.Url);

        if (!accepted)
        {
            _logger.LogWarning("Queue overloaded, rejecting URL: {Url}", request.Url);
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Queue is overloaded, try again later" });
        }

        _logger.LogInformation("URL queued: {Url}", request.Url);
        return Accepted(new { message = "Scraping task queued successfully", url = request.Url });
    }
}

public record ScrapeRequest(string Url);
