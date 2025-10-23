using Dignus.Data.Models.Consent;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services.Consent;

public class ConsentService : IConsentService
{
    private readonly ICandidateConsentRepository _consentRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly ConsentOptions _options;
    private readonly ILogger<ConsentService> _logger;
    private readonly TimeProvider _timeProvider;

    public ConsentService(
        ICandidateConsentRepository consentRepo,
        ICandidateRepository candidateRepo,
        IOptions<ConsentOptions> options,
        ILogger<ConsentService> logger,
        TimeProvider timeProvider)
    {
        _consentRepo = consentRepo;
        _candidateRepo = candidateRepo;
        _options = options.Value;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<ConsentStatusResult> GetConsentStatusAsync(string cpf)
    {
        var cleanCpf = CleanCpf(cpf);
        var consent = await _consentRepo.GetByCpfAsync(cleanCpf);

        if (consent == null)
        {
            return new ConsentStatusResult
            {
                HasAccepted = false
            };
        }

        var hasAcceptedAll = consent.AcceptedPrivacyPolicy &&
                            consent.AcceptedDataSharing &&
                            consent.AcceptedCreditAnalysis;

        return new ConsentStatusResult
        {
            HasAccepted = hasAcceptedAll,
            AcceptedAt = consent.CreatedAt,
            PrivacyPolicyVersion = consent.PrivacyPolicyVersion
        };
    }

    public async Task<SubmitConsentResult> SubmitConsentAsync(
        SubmitConsentRequest request,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var cleanCpf = CleanCpf(request.CPF);

            // Validation: All consents MUST be true
            if (!request.AcceptPrivacyPolicy ||
                !request.AcceptDataSharing ||
                !request.AcceptCreditAnalysis)
            {
                _logger.LogWarning("CPF {CPF} attempted to submit incomplete consent", cleanCpf);
                return new SubmitConsentResult
                {
                    Success = false,
                    Message = "Todos os consentimentos são obrigatórios para prosseguir",
                    ErrorCode = "INCOMPLETE_CONSENT"
                };
            }

            // Get candidate
            var candidate = await _candidateRepo.GetByCpfAsync(cleanCpf);
            if (candidate == null)
            {
                _logger.LogError("Candidate not found for CPF {CPF} during consent submission", cleanCpf);
                return new SubmitConsentResult
                {
                    Success = false,
                    Message = "Candidato não encontrado",
                    ErrorCode = "CANDIDATE_NOT_FOUND"
                };
            }

            // Check if already consented
            var existingConsent = await _consentRepo.GetByCpfAsync(cleanCpf);
            if (existingConsent != null)
            {
                _logger.LogInformation("CPF {CPF} already has consent record", cleanCpf);
                return new SubmitConsentResult
                {
                    Success = true,
                    Message = "Consentimento já registrado"
                };
            }

            // Create consent record
            var now = _timeProvider.GetUtcNow();
            var consent = new CandidateConsent
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                CPF = cleanCpf,
                AcceptedPrivacyPolicy = true,
                PrivacyPolicyAcceptedAt = now,
                PrivacyPolicyVersion = _options.CurrentPrivacyPolicyVersion,
                AcceptedDataSharing = true,
                DataSharingAcceptedAt = now,
                AcceptedCreditAnalysis = true,
                CreditAnalysisAcceptedAt = now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = now
            };

            await _consentRepo.CreateConsentAsync(consent);

            // Update Candidate LGPD status (EF Core tracks changes automatically)
            candidate.HasAcceptedLGPD = true;
            candidate.LGPDAcceptedAt = now;
            await _candidateRepo.SaveChangesAsync();

            _logger.LogInformation("LGPD consent recorded for CPF {CPF}, Candidate {CandidateId}",
                cleanCpf, candidate.Id);

            return new SubmitConsentResult
            {
                Success = true,
                Message = "Consentimento registrado com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting consent for CPF {CPF}", request.CPF);
            return new SubmitConsentResult
            {
                Success = false,
                Message = "Erro ao registrar consentimento. Tente novamente.",
                ErrorCode = "INTERNAL_ERROR"
            };
        }
    }

    public async Task<bool> HasAcceptedLGPDAsync(string cpf)
    {
        var cleanCpf = CleanCpf(cpf);
        return await _consentRepo.HasAcceptedAllConsentsAsync(cleanCpf);
    }

    private static string CleanCpf(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }
}

public class ConsentOptions
{
    public const string SectionName = "Consent";

    public string CurrentPrivacyPolicyVersion { get; set; } = "v1.0";
    public string PrivacyPolicyUrl { get; set; } = "/docs/privacy-policy.pdf";
}
