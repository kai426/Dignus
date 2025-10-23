namespace Dignus.Candidate.Back.Services.External;

public interface IGupyIntegrationService
{
    /// <summary>
    /// Validate if candidate exists in Gupy by CPF
    /// </summary>
    Task<GupyCandidate?> GetCandidateByCpfAsync(string cpf);

    /// <summary>
    /// Sync candidate data from Gupy to local database
    /// </summary>
    Task<bool> SyncCandidateAsync(string cpf);
}

public class GupyCandidate
{
    public string CPF { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? RecruiterName { get; set; }
    public DateTimeOffset ApplicationDate { get; set; }
    public string? GupyId { get; set; }
}
