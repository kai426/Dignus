using Dignus.Candidate.Back.Services.Consent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.Controllers;

[ApiController]
[Route("api/consent")]
public class ConsentController : ControllerBase
{
    private readonly IConsentService _consentService;
    private readonly ILogger<ConsentController> _logger;

    public ConsentController(
        IConsentService consentService,
        ILogger<ConsentController> logger)
    {
        _consentService = consentService;
        _logger = logger;
    }

    /// <summary>
    /// Check if candidate has accepted LGPD consent
    /// </summary>
    [HttpGet("status/{cpf}")]
    [ProducesResponseType(typeof(ConsentStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsentStatusResponse>> GetConsentStatus(
        [FromRoute] [Required] string cpf)
    {
        var result = await _consentService.GetConsentStatusAsync(cpf);

        return Ok(new ConsentStatusResponse
        {
            HasAccepted = result.HasAccepted,
            AcceptedAt = result.AcceptedAt,
            PrivacyPolicyVersion = result.PrivacyPolicyVersion
        });
    }

    /// <summary>
    /// Submit LGPD consent (all three consents required)
    /// </summary>
    [HttpPost]
    // [Authorize] // Requires JWT from authentication
    [ProducesResponseType(typeof(SubmitConsentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubmitConsentResponse>> SubmitConsent(
        [FromBody] SubmitConsentRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "INVALID_REQUEST",
                Message = "Dados inválidos"
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var serviceRequest = new SubmitConsentRequest
        {
            CPF = request.CPF,
            AcceptPrivacyPolicy = request.AcceptPrivacyPolicy,
            AcceptDataSharing = request.AcceptDataSharing,
            AcceptCreditAnalysis = request.AcceptCreditAnalysis
        };

        var result = await _consentService.SubmitConsentAsync(
            serviceRequest,
            ipAddress,
            userAgent);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode ?? "ERROR",
                Message = result.Message
            });
        }

        return Ok(new SubmitConsentResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    /// <summary>
    /// Get privacy policy document URL
    /// </summary>
    [HttpGet("privacy-policy")]
    [ProducesResponseType(typeof(PrivacyPolicyResponse), StatusCodes.Status200OK)]
    public IActionResult GetPrivacyPolicy()
    {
        return Ok(new PrivacyPolicyResponse
        {
            Version = "v1.0",
            Url = "/docs/privacy-policy.pdf",
            LastUpdated = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
    }
}

#region DTOs

public class SubmitConsentRequestDto
{
    [Required]
    public string CPF { get; set; } = null!;

    [Required]
    public bool AcceptPrivacyPolicy { get; set; }

    [Required]
    public bool AcceptDataSharing { get; set; }

    [Required]
    public bool AcceptCreditAnalysis { get; set; }
}

public class ConsentStatusResponse
{
    public bool HasAccepted { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public string? PrivacyPolicyVersion { get; set; }
}

public class SubmitConsentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}

public class PrivacyPolicyResponse
{
    public string Version { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTimeOffset LastUpdated { get; set; }
}

#endregion
