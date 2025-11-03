using Microsoft.Extensions.Options;
using Dignus.Candidate.Back.Configuration;

namespace Dignus.Candidate.Back.Services.External;

/// <summary>
/// Mock implementation of Gupy integration service for development
/// In production, this would call Gupy API via Databricks
/// </summary>
public class GupyIntegrationService : IGupyIntegrationService
{
    private readonly GupySettings _settings;
    private readonly ILogger<GupyIntegrationService> _logger;
    private readonly bool _useMockData;

    public GupyIntegrationService(
        IOptions<GupySettings> settings,
        ILogger<GupyIntegrationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _useMockData = _settings.UseMockData;
    }

    public Task<GupyCandidate?> GetCandidateByCpfAsync(string cpf)
    {
        if (_useMockData)
        {
            _logger.LogInformation("Using mock Gupy data for CPF: {CPF}", cpf);
            return Task.FromResult(GetMockCandidate(cpf));
        }

        try
        {
            // TODO: In production, call Databricks/Gupy API to fetch candidate
            _logger.LogInformation("Fetching candidate from Gupy for CPF: {CPF}", cpf);

            // For now, return null to indicate candidate not found in Gupy
            // Real implementation would make HTTP call to Databricks API
            return Task.FromResult<GupyCandidate?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching candidate from Gupy for CPF: {CPF}", cpf);
            return Task.FromResult<GupyCandidate?>(null);
        }
    }

    public Task<bool> SyncCandidateAsync(string cpf)
    {
        if (_useMockData)
        {
            _logger.LogInformation("Mock sync - skipping actual sync for CPF: {CPF}", cpf);
            return Task.FromResult(true);
        }

        try
        {
            // TODO: In production, trigger Databricks job to sync candidate data
            _logger.LogInformation("Syncing candidate data from Gupy for CPF: {CPF}", cpf);

            // Real implementation would:
            // 1. Call Databricks API to trigger sync job
            // 2. Wait for job completion or poll status
            // 3. Return success/failure

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing candidate from Gupy for CPF: {CPF}", cpf);
            return Task.FromResult(false);
        }
    }

    private GupyCandidate? GetMockCandidate(string cpf)
    {
        // Mock data for testing - recognizes specific CPFs
        var mockCandidates = new Dictionary<string, GupyCandidate>
        {
            ["12345678901"] = new GupyCandidate
            {
                CPF = "12345678901",
                Name = "João Silva",
                Email = "joao.silva@example.com",
                JobTitle = "Desenvolvedor Backend",
                RecruiterName = "Maria Santos",
                ApplicationDate = DateTimeOffset.UtcNow.AddDays(-10),
                GupyId = "gupy-123456"
            },
            ["98765432109"] = new GupyCandidate
            {
                CPF = "98765432109",
                Name = "Ana Costa",
                Email = "ana.costa@example.com",
                JobTitle = "Analista de Dados",
                RecruiterName = "Pedro Oliveira",
                ApplicationDate = DateTimeOffset.UtcNow.AddDays(-5),
                GupyId = "gupy-654321"
            }
        };

        return mockCandidates.TryGetValue(cpf, out var candidate) ? candidate : null;
    }
}