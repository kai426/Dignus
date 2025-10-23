namespace Dignus.Candidate.Back.Services.Consent;

public interface IConsentService
{
    Task<ConsentStatusResult> GetConsentStatusAsync(string cpf);
    Task<SubmitConsentResult> SubmitConsentAsync(
        SubmitConsentRequest request,
        string? ipAddress = null,
        string? userAgent = null);
    Task<bool> HasAcceptedLGPDAsync(string cpf);
}

public class ConsentStatusResult
{
    public bool HasAccepted { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public string? PrivacyPolicyVersion { get; set; }
}

public class SubmitConsentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? ErrorCode { get; set; }
}

public class SubmitConsentRequest
{
    public string CPF { get; set; } = null!;
    public bool AcceptPrivacyPolicy { get; set; }
    public bool AcceptDataSharing { get; set; }
    public bool AcceptCreditAnalysis { get; set; }
}
