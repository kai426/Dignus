using System.Security.Claims;

namespace Dignus.Candidate.Back.Services.Auth;

public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT access token for candidate
    /// </summary>
    string GenerateAccessToken(Guid candidateId, string cpf, string email, bool hasAcceptedLGPD);

    /// <summary>
    /// Generate JWT refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extract candidate ID from token
    /// </summary>
    Guid? GetCandidateIdFromToken(string token);
}
