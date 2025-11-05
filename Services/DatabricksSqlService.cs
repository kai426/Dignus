using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for executing SQL queries on Databricks SQL Warehouse
    /// </summary>
    public class DatabricksSqlService : IDatabricksSqlService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabricksSettings _settings;
        private readonly ILogger<DatabricksSqlService> _logger;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DatabricksSqlService(
            HttpClient httpClient,
            IOptions<DatabricksSettings> settings,
            ILogger<DatabricksSqlService> logger,
            ICandidateRepository candidateRepository,
            IJobRepository jobRepository,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _candidateRepository = candidateRepository;
            _jobRepository = jobRepository;
            _unitOfWork = unitOfWork;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_settings.WorkspaceUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.ConnectionTimeoutSeconds);
        }

        /// <summary>
        /// Executes a SQL query and returns the results
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing SQL query on Databricks: {Query}", sqlQuery);

            var request = new DatabricksSqlStatementRequest
            {
                WarehouseId = _settings.WarehouseId,
                Statement = sqlQuery,
                Catalog = _settings.Catalog ?? "spark_catalog",
                Schema = _settings.Schema ?? "default",
                WaitTimeout = "30s",
                Format = "JSON_ARRAY",
                Disposition = "INLINE"
            };

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/2.0/sql/statements/", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Databricks API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Databricks API error: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<DatabricksSqlStatementResponse>(jsonResponse);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize Databricks response");
            }

            // Check if we need to poll for results
            if (result.Status.State == "PENDING" || result.Status.State == "RUNNING")
            {
                result = await PollForResultsAsync(result.StatementId, cancellationToken);
            }

            // Check for errors
            if (result.Status.State == "FAILED" || result.Status.State == "CANCELED")
            {
                var errorMessage = result.Status.Error?.Message ?? "Query execution failed";
                _logger.LogError("Query execution failed: {Error}", errorMessage);
                throw new InvalidOperationException($"Query execution failed: {errorMessage}");
            }

            // Extract and convert results
            return ConvertResultsToDict(result);
        }

        /// <summary>
        /// Gets in-progress applications from gupy_aplicacoes table
        /// </summary>
        public async Task<List<GupyAplicacaoDto>> GetInProgressApplicationsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching in-progress applications from Databricks");

            // Query to get in-progress applications from the latest extraction date
            var sql = $@"
                WITH latest_extraction AS (
                    SELECT MAX(DATA_EXTRACAO) as max_date
                    FROM {_settings.DatabaseName}.{_settings.ApplicationsTableName}
                )
                SELECT
                    ID,
                    STATUS,
                    DATA_CRIACAO,
                    DATA_ATUALIZACAO,
                    ID_TRABALHO,
                    ID_PASSO_ATUAL,
                    ID_CANDIDATO,
                    AFINIDADE,
                    FONTE,
                    STATUS_PASSO,
                    PASSO_ATUAL_DATA,
                    PASSO_ATUAL_NOME,
                    NOME_TRABALHO,
                    STATUS_TRABALHO,
                    DESQUALIFICADO,
                    DESQUALIFICACACAO_RAZAO,
                    DATA_EXTRACAO
                FROM {_settings.DatabaseName}.{_settings.ApplicationsTableName}
                WHERE STATUS = 'inProgress'
                  AND DATA_EXTRACAO = (SELECT max_date FROM latest_extraction)";

            var results = await ExecuteQueryAsync(sql, cancellationToken);

            return results.Select(row => new GupyAplicacaoDto
            {
                Id = row.GetValueOrDefault("ID")?.ToString(),
                Status = row.GetValueOrDefault("STATUS")?.ToString(),
                DataCriacao = ParseDateTimeOffset(row.GetValueOrDefault("DATA_CRIACAO")),
                DataAtualizacao = ParseDateTimeOffset(row.GetValueOrDefault("DATA_ATUALIZACAO")),
                IdTrabalho = row.GetValueOrDefault("ID_TRABALHO")?.ToString(),
                IdPassoAtual = row.GetValueOrDefault("ID_PASSO_ATUAL")?.ToString(),
                IdCandidato = row.GetValueOrDefault("ID_CANDIDATO")?.ToString(),
                Afinidade = ParseInt(row.GetValueOrDefault("AFINIDADE")),
                Fonte = row.GetValueOrDefault("FONTE")?.ToString(),
                StatusPasso = row.GetValueOrDefault("STATUS_PASSO")?.ToString(),
                PassoAtualData = ParseDateTimeOffset(row.GetValueOrDefault("PASSO_ATUAL_DATA")),
                PassoAtualNome = row.GetValueOrDefault("PASSO_ATUAL_NOME")?.ToString(),
                NomeTrabalho = row.GetValueOrDefault("NOME_TRABALHO")?.ToString(),
                StatusTrabalho = row.GetValueOrDefault("STATUS_TRABALHO")?.ToString(),
                Desqualificado = ParseBool(row.GetValueOrDefault("DESQUALIFICADO")),
                DesqualificacaoRazao = row.GetValueOrDefault("DESQUALIFICACACAO_RAZAO")?.ToString(),
                DataExtracao = ParseDateTimeOffset(row.GetValueOrDefault("DATA_EXTRACAO"))
            }).ToList();
        }

        /// <summary>
        /// Gets candidate details by IDs from gupy_candidatos table
        /// </summary>
        public async Task<List<GupyCandidatoDto>> GetCandidatesByIdsAsync(List<string> candidateIds, CancellationToken cancellationToken = default)
        {
            if (!candidateIds.Any())
            {
                return new List<GupyCandidatoDto>();
            }

            _logger.LogInformation("Fetching {Count} candidates from Databricks", candidateIds.Count);

            // Build IN clause for SQL query
            var idsFormatted = string.Join(", ", candidateIds.Select(id => $"'{id}'"));

            var sql = $@"
                SELECT
                    ID,
                    PRIMEIRO_NOME,
                    SEGUNDO_NOME,
                    EMAIL,
                    NASCIMENTO,
                    TELEFONE,
                    GENERO,
                    RACA,
                    ESTADO_CIVIL,
                    CIDADE,
                    CODIGO_PAIS,
                    CEP,
                    CRIACAO,
                    ATUALIZACAO,
                    GRAU_EDUCACAO,
                    INSTITUICAO_EDUCACAO,
                    CURSO_EDUCACAO,
                    STATUS_EDUCACAO,
                    CARGO_EXPERIENCIA,
                    ORGANIZACAO_EXPERIENCIA,
                    ATIVIDADES_DESEMPENHADAS,
                    DATA_EXTRACAO
                FROM {_settings.DatabaseName}.{_settings.CandidateTableName}
                WHERE ID IN ({idsFormatted})
                  AND DATA_EXTRACAO = (SELECT MAX(DATA_EXTRACAO) FROM {_settings.DatabaseName}.{_settings.CandidateTableName})";

            var results = await ExecuteQueryAsync(sql, cancellationToken);

            return results.Select(row => new GupyCandidatoDto
            {
                Id = row.GetValueOrDefault("ID")?.ToString(),
                PrimeiroNome = row.GetValueOrDefault("PRIMEIRO_NOME")?.ToString(),
                SegundoNome = row.GetValueOrDefault("SEGUNDO_NOME")?.ToString(),
                Email = row.GetValueOrDefault("EMAIL")?.ToString(),
                Nascimento = ParseDateTimeOffset(row.GetValueOrDefault("NASCIMENTO")),
                Telefone = row.GetValueOrDefault("TELEFONE")?.ToString(),
                Genero = row.GetValueOrDefault("GENERO")?.ToString(),
                Raca = row.GetValueOrDefault("RACA")?.ToString(),
                EstadoCivil = row.GetValueOrDefault("ESTADO_CIVIL")?.ToString(),
                Cidade = row.GetValueOrDefault("CIDADE")?.ToString(),
                CodigoPais = row.GetValueOrDefault("CODIGO_PAIS")?.ToString(),
                Cep = row.GetValueOrDefault("CEP")?.ToString(),
                Criacao = ParseDateTimeOffset(row.GetValueOrDefault("CRIACAO")),
                Atualizacao = ParseDateTimeOffset(row.GetValueOrDefault("ATUALIZACAO")),
                GrauEducacao = row.GetValueOrDefault("GRAU_EDUCACAO")?.ToString(),
                InstituicaoEducacao = row.GetValueOrDefault("INSTITUICAO_EDUCACAO")?.ToString(),
                CursoEducacao = row.GetValueOrDefault("CURSO_EDUCACAO")?.ToString(),
                StatusEducacao = row.GetValueOrDefault("STATUS_EDUCACAO")?.ToString(),
                CargoExperiencia = row.GetValueOrDefault("CARGO_EXPERIENCIA")?.ToString(),
                OrganizacaoExperiencia = row.GetValueOrDefault("ORGANIZACAO_EXPERIENCIA")?.ToString(),
                AtividadesDesempenhadas = row.GetValueOrDefault("ATIVIDADES_DESEMPENHADAS")?.ToString(),
                DataExtracao = ParseDateTimeOffset(row.GetValueOrDefault("DATA_EXTRACAO"))
            }).ToList();
        }

        /// <summary>
        /// Syncs in-progress Gupy applications and their candidates to local database
        /// </summary>
        public async Task<GupySyncResponseDto> SyncInProgressApplicationsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Gupy sync operation");

            var response = new GupySyncResponseDto();

            try
            {
                // Step 1: Get in-progress applications
                var applications = await GetInProgressApplicationsAsync(cancellationToken);
                response.TotalApplications = applications.Count;

                _logger.LogInformation("Found {Count} in-progress applications", applications.Count);

                if (!applications.Any())
                {
                    _logger.LogInformation("No in-progress applications found");
                    return response;
                }

                // Step 2: Get unique candidate IDs and job IDs
                var candidateIds = applications
                    .Where(a => !string.IsNullOrEmpty(a.IdCandidato))
                    .Select(a => a.IdCandidato!)
                    .Distinct()
                    .ToList();

                var jobIds = applications
                    .Where(a => !string.IsNullOrEmpty(a.IdTrabalho))
                    .Select(a => a.IdTrabalho!)
                    .Distinct()
                    .ToList();

                // Step 3: Get candidate details from Databricks
                var gupyCandidates = await GetCandidatesByIdsAsync(candidateIds, cancellationToken);
                response.CandidatesFound = gupyCandidates.Count;

                _logger.LogInformation("Found {Count} candidates in Databricks", gupyCandidates.Count);

                // Step 4: Create or update candidates and jobs in local database
                foreach (var gupyCandidate in gupyCandidates)
                {
                    try
                    {
                        // Find corresponding application
                        var application = applications.FirstOrDefault(a => a.IdCandidato == gupyCandidate.Id);
                        if (application == null) continue;

                        // Check if candidate already exists by GupyId
                        var existingCandidate = await _candidateRepository.GetByGupyIdAsync(gupyCandidate.Id!);

                        if (existingCandidate == null)
                        {
                            // Create new candidate
                            var newCandidate = MapGupyCandidateToCandidate(gupyCandidate, application);
                            await _candidateRepository.AddAsync(newCandidate);
                            response.CandidatesCreated++;
                            _logger.LogInformation("Created new candidate: {GupyId}", gupyCandidate.Id);
                        }
                        else
                        {
                            // Update existing candidate
                            UpdateCandidateFromGupy(existingCandidate, gupyCandidate, application);
                            _candidateRepository.Update(existingCandidate);
                            response.CandidatesUpdated++;
                            _logger.LogInformation("Updated existing candidate: {GupyId}", gupyCandidate.Id);
                        }

                        // Create or update job if we have job data
                        if (!string.IsNullOrEmpty(application.IdTrabalho))
                        {
                            var existingJob = await _jobRepository.GetByGupyIdAsync(application.IdTrabalho);

                            if (existingJob == null && !string.IsNullOrEmpty(application.NomeTrabalho))
                            {
                                var newJob = new Job
                                {
                                    Id = Guid.NewGuid(),
                                    Name = application.NomeTrabalho,
                                    Status = application.StatusTrabalho ?? "Publicado",
                                    CreatedAt = application.DataCriacao?.UtcDateTime ?? DateTime.UtcNow,
                                    UpdatedAt = application.DataAtualizacao?.UtcDateTime ?? DateTime.UtcNow,
                                    Description = $"Importado do Gupy - ID: {application.IdTrabalho}"
                                };
                                await _jobRepository.AddAsync(newJob);
                                response.JobsCreated++;
                                _logger.LogInformation("Created new job: {GupyJobId}", application.IdTrabalho);
                            }
                            else if (existingJob != null)
                            {
                                // Update job status if needed
                                if (!string.IsNullOrEmpty(application.StatusTrabalho))
                                {
                                    existingJob.Status = application.StatusTrabalho;
                                    existingJob.UpdatedAt = DateTime.UtcNow;
                                    _jobRepository.Update(existingJob);
                                    response.JobsUpdated++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing candidate {GupyId}", gupyCandidate.Id);
                        response.Errors.Add($"Error syncing candidate {gupyCandidate.Id}: {ex.Message}");
                    }
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Sync completed: {Created} created, {Updated} updated, {Errors} errors",
                    response.CandidatesCreated,
                    response.CandidatesUpdated,
                    response.Errors.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Gupy sync operation");
                response.Errors.Add($"Sync operation failed: {ex.Message}");
                throw;
            }

            return response;
        }

        #region Private Helper Methods

        private async Task<DatabricksSqlStatementResponse> PollForResultsAsync(string statementId, CancellationToken cancellationToken)
        {
            var maxAttempts = 30; // 30 attempts with 2 second delay = 1 minute max
            var attempt = 0;

            while (attempt < maxAttempts)
            {
                await Task.Delay(2000, cancellationToken); // Wait 2 seconds between polls

                var response = await _httpClient.GetAsync($"/api/2.0/sql/statements/{statementId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to poll statement status: {response.StatusCode}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DatabricksSqlStatementResponse>(jsonResponse);

                if (result == null)
                {
                    throw new InvalidOperationException("Failed to deserialize polling response");
                }

                if (result.Status.State == "SUCCEEDED" || result.Status.State == "FAILED" || result.Status.State == "CANCELED")
                {
                    return result;
                }

                attempt++;
            }

            throw new TimeoutException("Query execution timed out after polling");
        }

        private List<Dictionary<string, object?>> ConvertResultsToDict(DatabricksSqlStatementResponse response)
        {
            var results = new List<Dictionary<string, object?>>();

            if (response.Result?.DataArray == null || response.Manifest?.Schema?.Columns == null)
            {
                return results;
            }

            var columns = response.Manifest.Schema.Columns;

            foreach (var row in response.Result.DataArray)
            {
                var dict = new Dictionary<string, object?>();

                for (int i = 0; i < columns.Count && i < row.Count; i++)
                {
                    dict[columns[i].Name] = row[i];
                }

                results.Add(dict);
            }

            return results;
        }

        private Data.Models.Candidate MapGupyCandidateToCandidate(GupyCandidatoDto gupyCandidate, GupyAplicacaoDto application)
        {
            var fullName = $"{gupyCandidate.PrimeiroNome} {gupyCandidate.SegundoNome}".Trim();

            return new Data.Models.Candidate
            {
                Id = Guid.NewGuid(),
                Name = fullName,
                Cpf = "00000000000", // CPF needs to be provided separately - Gupy might not expose it
                Email = gupyCandidate.Email ?? "",
                Phone = gupyCandidate.Telefone,
                BirthDate = gupyCandidate.Nascimento?.UtcDateTime ?? DateTime.UtcNow.AddYears(-30),
                Status = CandidateStatus.InProcess,
                CreatedAt = gupyCandidate.Criacao ?? DateTimeOffset.UtcNow,
                JobId = Guid.Empty, // Will need to be set based on job mapping
                RecruiterId = Guid.Empty, // Will need to be set based on recruiter assignment
                GupyId = gupyCandidate.Id,
                GupyLastSync = DateTimeOffset.UtcNow,
                HasAcceptedLGPD = false,
                IsActive = true
            };
        }

        private void UpdateCandidateFromGupy(Data.Models.Candidate candidate, GupyCandidatoDto gupyCandidate, GupyAplicacaoDto application)
        {
            var fullName = $"{gupyCandidate.PrimeiroNome} {gupyCandidate.SegundoNome}".Trim();

            candidate.Name = fullName;
            candidate.Email = gupyCandidate.Email ?? candidate.Email;
            candidate.Phone = gupyCandidate.Telefone ?? candidate.Phone;

            if (gupyCandidate.Nascimento.HasValue)
            {
                candidate.BirthDate = gupyCandidate.Nascimento.Value.UtcDateTime;
            }

            candidate.GupyLastSync = DateTimeOffset.UtcNow;
        }

        private DateTimeOffset? ParseDateTimeOffset(object? value)
        {
            if (value == null) return null;

            if (value is DateTimeOffset dto)
                return dto;

            if (value is DateTime dt)
                return new DateTimeOffset(dt);

            if (DateTimeOffset.TryParse(value.ToString(), out var result))
                return result;

            return null;
        }

        private int? ParseInt(object? value)
        {
            if (value == null) return null;

            if (value is int i)
                return i;

            if (int.TryParse(value.ToString(), out var result))
                return result;

            return null;
        }

        private bool? ParseBool(object? value)
        {
            if (value == null) return null;

            if (value is bool b)
                return b;

            if (bool.TryParse(value.ToString(), out var result))
                return result;

            return null;
        }

        #endregion
    }
}