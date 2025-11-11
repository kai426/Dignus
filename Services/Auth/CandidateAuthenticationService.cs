using Dignus.Data.Repositories.Interfaces;
using Dignus.Candidate.Back.Services.Email;
using System.Security.Cryptography;

namespace Dignus.Candidate.Back.Services.Auth;

public class CandidateAuthenticationService : ICandidateAuthenticationService
{
    private readonly ICandidateAuthTokenRepository _tokenRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtService;
    private readonly ILogger<CandidateAuthenticationService> _logger;
    private readonly TimeProvider _timeProvider;

    private const int MAX_FAILED_ATTEMPTS = 10;
    private const int LOCKOUT_MINUTES = 10;
    private const int TOKEN_LENGTH = 6;

    public CandidateAuthenticationService(
        ICandidateAuthTokenRepository tokenRepo,
        ICandidateRepository candidateRepo,
        IEmailService emailService,
        IJwtTokenService jwtService,
        ILogger<CandidateAuthenticationService> logger,
        TimeProvider timeProvider)
    {
        _tokenRepo = tokenRepo;
        _candidateRepo = candidateRepo;
        _emailService = emailService;
        _jwtService = jwtService;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<AuthTokenRequestResult> RequestAuthenticationTokenAsync(
        string cpf,
        string email,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            // Normalize CPF (remove formatting)
            cpf = NormalizeCpf(cpf);

            // Validate CPF format
            if (!IsValidCpf(cpf))
            {
                return new AuthTokenRequestResult
                {
                    Success = false,
                    ErrorCode = "INVALID_CPF",
                    Message = "CPF inválido"
                };
            }

            // Check lockout status
            var (isLocked, lockedUntil) = await _tokenRepo.CheckLockoutAsync(cpf);
            if (isLocked)
            {
                return new AuthTokenRequestResult
                {
                    Success = false,
                    ErrorCode = "ACCOUNT_LOCKED",
                    Message = $"Conta bloqueada devido a múltiplas tentativas falhas. Tente novamente após {lockedUntil:HH:mm}",
                    IsLockedOut = true,
                    LockedUntil = lockedUntil
                };
            }

            // Verify candidate exists in local database
            var candidate = await _candidateRepo.GetByCpfAsync(cpf);
            if (candidate == null)
            {
                _logger.LogWarning("Authentication attempt for CPF not found in database: {CPF}", cpf);
                return new AuthTokenRequestResult
                {
                    Success = false,
                    ErrorCode = "CANDIDATE_NOT_FOUND",
                    Message = "Candidato não encontrado no sistema. Verifique se você possui uma candidatura ativa."
                };
            }

            // Verify email matches
            if (!string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Email mismatch for CPF {CPF}. Expected: {Expected}, Got: {Got}",
                    cpf, candidate.Email, email);
                return new AuthTokenRequestResult
                {
                    Success = false,
                    ErrorCode = "EMAIL_MISMATCH",
                    Message = "O e-mail fornecido não corresponde ao cadastrado no sistema."
                };
            }

            // Invalidate any existing active tokens
            await _tokenRepo.InvalidateActiveTokensAsync(cpf);

            // Generate 6-digit token
            var tokenCode = GenerateNumericToken(TOKEN_LENGTH);

            // Create new token
            await _tokenRepo.CreateTokenAsync(
                cpf,
                email,
                tokenCode,
                ipAddress,
                userAgent);

            // Send email
            var emailSent = await _emailService.SendAuthenticationTokenEmailAsync(
                email,
                candidate.Name,
                tokenCode);

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send authentication email to {Email}", email);
            }

            _logger.LogInformation("Authentication token generated for CPF: {CPF}", cpf);

            return new AuthTokenRequestResult
            {
                Success = true,
                Message = $"Código de verificação enviado para {MaskEmail(email)}",
                TokenExpirationMinutes = 15
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting authentication token for CPF: {CPF}", cpf);
            return new AuthTokenRequestResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Erro ao processar solicitação. Tente novamente mais tarde."
            };
        }
    }

    public async Task<AuthTokenValidationResult> ValidateTokenAsync(
        string cpf,
        string tokenCode)
    {
        try
        {
            cpf = NormalizeCpf(cpf);

            // Check lockout
            var (isLocked, lockedUntil) = await _tokenRepo.CheckLockoutAsync(cpf);
            if (isLocked)
            {
                return new AuthTokenValidationResult
                {
                    Success = false,
                    ErrorCode = "ACCOUNT_LOCKED",
                    Message = $"Conta bloqueada. Tente novamente após {lockedUntil:HH:mm}"
                };
            }

            // Get active token
            var token = await _tokenRepo.GetActiveTokenAsync(cpf);

            if (token == null)
            {
                return new AuthTokenValidationResult
                {
                    Success = false,
                    ErrorCode = "TOKEN_NOT_FOUND",
                    Message = "Nenhum código válido encontrado. Solicite um novo código."
                };
            }

            // Check if expired
            if (token.ExpiresAt <= _timeProvider.GetUtcNow())
            {
                return new AuthTokenValidationResult
                {
                    Success = false,
                    ErrorCode = "TOKEN_EXPIRED",
                    Message = "Código expirado. Solicite um novo código."
                };
            }

            // Validate token code
            _logger.LogInformation("===== COMPARING TOKENS - Stored: '{StoredToken}' vs Provided: '{ProvidedToken}' =====",
                token.TokenCode, tokenCode);

            if (token.TokenCode != tokenCode)
            {
                await _tokenRepo.IncrementFailedAttemptsAsync(cpf);

                // Get updated token to check failed attempts
                var updatedToken = await _tokenRepo.GetLatestTokenAsync(cpf);

                if (updatedToken != null && updatedToken.FailedAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    var lockoutUntil = _timeProvider.GetUtcNow().AddMinutes(LOCKOUT_MINUTES);
                    await _tokenRepo.SetLockoutAsync(cpf, lockoutUntil);

                    _logger.LogWarning("Account locked for CPF {CPF} due to {Attempts} failed attempts",
                        cpf, MAX_FAILED_ATTEMPTS);

                    return new AuthTokenValidationResult
                    {
                        Success = false,
                        ErrorCode = "ACCOUNT_LOCKED",
                        Message = $"Conta bloqueada por {LOCKOUT_MINUTES} minutos devido a múltiplas tentativas falhas."
                    };
                }

                return new AuthTokenValidationResult
                {
                    Success = false,
                    ErrorCode = "INVALID_TOKEN",
                    Message = $"Código inválido. Tentativas restantes: {MAX_FAILED_ATTEMPTS - (updatedToken?.FailedAttempts ?? 0)}"
                };
            }

            // Token is valid - mark as used
            await _tokenRepo.MarkAsUsedAsync(token.Id);
            await _tokenRepo.ClearLockoutAsync(cpf);

            // Get candidate from local database
            var candidate = await _candidateRepo.GetByCpfAsync(cpf);

            if (candidate == null)
            {
                _logger.LogError("Candidate not found in database after token validation for CPF: {CPF}", cpf);
                return new AuthTokenValidationResult
                {
                    Success = false,
                    ErrorCode = "CANDIDATE_NOT_FOUND",
                    Message = "Candidato não encontrado no sistema."
                };
            }

            bool requiresLGPDConsent = !candidate.HasAcceptedLGPD;

            // Generate JWT tokens
            var accessToken = _jwtService.GenerateAccessToken(
                candidate.Id,
                candidate.Cpf,
                candidate.Email,
                candidate.HasAcceptedLGPD);

            var refreshToken = _jwtService.GenerateRefreshToken();

            _logger.LogInformation("Authentication successful for candidate {CandidateId}", candidate.Id);

            return new AuthTokenValidationResult
            {
                Success = true,
                Message = "Autenticação realizada com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                CandidateId = candidate.Id,
                RequiresLGPDConsent = requiresLGPDConsent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token for CPF: {CPF}", cpf);
            return new AuthTokenValidationResult
            {
                Success = false,
                ErrorCode = "INTERNAL_ERROR",
                Message = "Erro ao validar código. Tente novamente."
            };
        }
    }

    public async Task<LockoutStatus> CheckLockoutStatusAsync(string cpf)
    {
        cpf = NormalizeCpf(cpf);
        var (isLocked, lockedUntil) = await _tokenRepo.CheckLockoutAsync(cpf);

        var remainingMinutes = 0;
        if (isLocked && lockedUntil.HasValue)
        {
            var remaining = lockedUntil.Value - _timeProvider.GetUtcNow();
            remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
        }

        return new LockoutStatus
        {
            IsLockedOut = isLocked,
            LockedUntil = lockedUntil,
            RemainingMinutes = remainingMinutes
        };
    }

    #region Helper Methods

    private static string NormalizeCpf(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        // Check for known invalid CPFs
        if (cpf.Distinct().Count() == 1)
            return false;

        // Validate CPF check digits
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * (10 - i);

        var remainder = (sum * 10) % 11;
        if (remainder == 10) remainder = 0;
        if (remainder != int.Parse(cpf[9].ToString()))
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * (11 - i);

        remainder = (sum * 10) % 11;
        if (remainder == 10) remainder = 0;
        if (remainder != int.Parse(cpf[10].ToString()))
            return false;

        return true;
    }

    private static string GenerateNumericToken(int length)
    {
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        random.GetBytes(bytes);
        var randomNumber = BitConverter.ToUInt32(bytes, 0);

        var token = (randomNumber % (uint)Math.Pow(10, length)).ToString().PadLeft(length, '0');
        return token;
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2)
            return email;

        var localPart = parts[0];
        var visibleChars = Math.Min(3, localPart.Length);
        var masked = localPart[..visibleChars] + "***";

        return $"{masked}@{parts[1]}";
    }

    #endregion
}
