namespace Dignus.Candidate.Back.Services.Auth;

public interface ICandidateAuthenticationService
{
    /// <summary>
    /// Request authentication token for CPF + Email combination
    /// </summary>
    Task<AuthTokenRequestResult> RequestAuthenticationTokenAsync(
        string cpf,
        string email,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Validate authentication token and generate JWT access token
    /// </summary>
    Task<AuthTokenValidationResult> ValidateTokenAsync(
        string cpf,
        string tokenCode);

    /// <summary>
    /// Check if CPF is currently locked out
    /// </summary>
    Task<LockoutStatus> CheckLockoutStatusAsync(string cpf);
}

public class AuthTokenRequestResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public int? TokenExpirationMinutes { get; set; }
}

public class AuthTokenValidationResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Guid? CandidateId { get; set; }
    public bool RequiresLGPDConsent { get; set; }
}

public class LockoutStatus
{
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public int RemainingMinutes { get; set; }
}
