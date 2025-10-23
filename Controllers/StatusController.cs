using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dignus.Candidate.Back.Controllers;

/// <summary>
/// API Status and Information Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get API status and version information
    /// </summary>
    /// <returns>API status information</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Status endpoint accessed");
        
        var status = new
        {
            Status = "Healthy",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow,
            Service = "Dignus Candidate API"
        };

        return Ok(status);
    }

    /// <summary>
    /// Protected endpoint to test authentication
    /// </summary>
    /// <returns>Authentication status</returns>
    [HttpGet("protected")]
    //TODO: Remove after testing - Authorization temporarily disabled for manual testing
    // [Authorize]
    public IActionResult GetProtectedStatus()
    {
        _logger.LogInformation("Protected status endpoint accessed by user: {User}", User.Identity?.Name);
        
        var status = new
        {
            Message = "You are authenticated",
            User = User.Identity?.Name,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
            Timestamp = DateTime.UtcNow
        };

        return Ok(status);
    }
}