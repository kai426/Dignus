using Dignus.Candidate.Back.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.Controllers;

/// <summary>
/// New authentication controller with email token verification
/// </summary>
[ApiController]
[Route("api/candidate-auth")]
public class CandidateAuthController : ControllerBase
{
    private readonly ICandidateAuthenticationService _authService;
    private readonly ILogger<CandidateAuthController> _logger;

    public CandidateAuthController(
        ICandidateAuthenticationService authService,
        ILogger<CandidateAuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Request authentication token via email
    /// </summary>
    /// <param name="request">CPF and Email</param>
    /// <response code="200">Token sent successfully</response>
    /// <response code="400">Invalid request or account locked</response>
    /// <response code="404">Candidate not found in Gupy</response>
    [HttpPost("request-token")]
    [ProducesResponseType(typeof(RequestTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestToken([FromBody] RequestTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _authService.RequestAuthenticationTokenAsync(
            request.CPF,
            request.Email,
            ipAddress,
            userAgent);

        if (!result.Success)
        {
            _logger.LogWarning("Token request failed for CPF {CPF}: {Error}",
                request.CPF, result.ErrorCode);

            return result.ErrorCode switch
            {
                "CANDIDATE_NOT_FOUND" => NotFound(new ErrorResponse
                {
                    Error = result.ErrorCode,
                    Message = result.Message!
                }),
                "ACCOUNT_LOCKED" => BadRequest(new ErrorResponse
                {
                    Error = result.ErrorCode,
                    Message = result.Message!,
                    LockedUntil = result.LockedUntil
                }),
                _ => BadRequest(new ErrorResponse
                {
                    Error = result.ErrorCode!,
                    Message = result.Message!
                })
            };
        }

        return Ok(new RequestTokenResponse
        {
            Message = result.Message!,
            ExpirationMinutes = result.TokenExpirationMinutes ?? 15
        });
    }

    /// <summary>
    /// Validate authentication token and get JWT access token
    /// </summary>
    /// <param name="request">CPF and token code</param>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid token or account locked</response>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(ValidateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        var result = await _authService.ValidateTokenAsync(
            request.CPF,
            request.TokenCode);

        if (!result.Success)
        {
            _logger.LogWarning("Token validation failed for CPF {CPF}: {Error}",
                request.CPF, result.ErrorCode);

            return BadRequest(new ErrorResponse
            {
                Error = result.ErrorCode!,
                Message = result.Message!
            });
        }

        return Ok(new ValidateTokenResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            CandidateId = result.CandidateId!.Value,
            RequiresLGPDConsent = result.RequiresLGPDConsent,
            Message = result.Message!
        });
    }

    /// <summary>
    /// Check lockout status for CPF
    /// </summary>
    /// <param name="cpf">Candidate CPF</param>
    /// <response code="200">Lockout status returned</response>
    [HttpGet("lockout-status/{cpf}")]
    [ProducesResponseType(typeof(LockoutStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLockoutStatus(string cpf)
    {
        var status = await _authService.CheckLockoutStatusAsync(cpf);

        return Ok(new LockoutStatusResponse
        {
            IsLockedOut = status.IsLockedOut,
            LockedUntil = status.LockedUntil,
            RemainingMinutes = status.RemainingMinutes
        });
    }
}

#region DTOs

public class RequestTokenRequest
{
    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string CPF { get; set; } = null!;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;
}

public class RequestTokenResponse
{
    public string Message { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}

public class ValidateTokenRequest
{
    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string CPF { get; set; } = null!;

    [Required(ErrorMessage = "Código é obrigatório")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Código deve ter 6 dígitos")]
    public string TokenCode { get; set; } = null!;
}

public class ValidateTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public Guid CandidateId { get; set; }
    public bool RequiresLGPDConsent { get; set; }
    public string Message { get; set; } = null!;
}

public class LockoutStatusResponse
{
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public int RemainingMinutes { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTimeOffset? LockedUntil { get; set; }
}

#endregion
