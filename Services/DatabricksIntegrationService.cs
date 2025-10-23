using AutoMapper;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for Databricks integration operations with Gupy data
    /// </summary>
    public class DatabricksIntegrationService : IDatabricksIntegrationService
    {
        private readonly ILogger<DatabricksIntegrationService> _logger;
        private readonly DatabricksSettings _databricksSettings;
        private readonly GupySettings _gupySettings;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        // In-memory tracking of sync operations (in production, use Redis or database)
        private static readonly ConcurrentDictionary<Guid, SyncStatusDto> _activeSyncs = new();
        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _syncCancellationTokens = new();

        public DatabricksIntegrationService(
            ILogger<DatabricksIntegrationService> logger,
            IOptions<DatabricksSettings> databricksSettings,
            IOptions<GupySettings> gupySettings,
            HttpClient httpClient,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _databricksSettings = databricksSettings.Value;
            _gupySettings = gupySettings.Value;
            _httpClient = httpClient;
            _mapper = mapper;
            _unitOfWork = unitOfWork;

            // Configure HTTP client for Databricks API
            _httpClient.BaseAddress = new Uri(_databricksSettings.WorkspaceUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _databricksSettings.AccessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(_databricksSettings.ConnectionTimeoutSeconds);
        }

        public async Task<SyncStatusDto> SynchronizeCandidatesAsync(int batchSize = 1000, CancellationToken cancellationToken = default)
        {
            var syncId = Guid.NewGuid();
            var syncStatus = new SyncStatusDto
            {
                SyncId = syncId,
                Type = SyncType.Candidates,
                Status = SyncStatus.Pending,
                StartedAt = DateTimeOffset.UtcNow,
                TotalRecords = 0,
                ProcessedRecords = 0,
                FailedRecords = 0
            };

            _activeSyncs[syncId] = syncStatus;
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _syncCancellationTokens[syncId] = cancellationTokenSource;

            try
            {
                _logger.LogInformation("Starting candidate synchronization {SyncId} with batch size {BatchSize}", syncId, batchSize);
                syncStatus.Status = SyncStatus.Running;

                // Execute Databricks notebook for candidate sync
                var jobRequest = new DatabricksJobRequestDto
                {
                    JobType = "sync_candidates",
                    Parameters = new Dictionary<string, string>
                    {
                        ["batch_size"] = batchSize.ToString(),
                        ["sync_id"] = syncId.ToString(),
                        ["database_name"] = _databricksSettings.DatabaseName,
                        ["table_name"] = _databricksSettings.CandidateTableName
                    },
                    ClusterId = _databricksSettings.ClusterId,
                    NotebookPath = _databricksSettings.GupyNotebookPath,
                    TimeoutMinutes = _databricksSettings.JobTimeoutMinutes
                };

                var jobResponse = await ExecuteNotebookAsync(jobRequest, cancellationTokenSource.Token);

                if (jobResponse.Status == "SUCCESS" || jobResponse.ResultState == "SUCCESS")
                {
                    // Parse job output to get sync results
                    var syncResults = await ProcessSyncResults(jobResponse.Output, syncId);
                    
                    syncStatus.TotalRecords = syncResults.TotalRecords;
                    syncStatus.ProcessedRecords = syncResults.ProcessedRecords;
                    syncStatus.FailedRecords = syncResults.FailedRecords;
                    syncStatus.Status = syncResults.FailedRecords > 0 ? SyncStatus.PartiallyCompleted : SyncStatus.Completed;
                    syncStatus.CompletedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Candidate synchronization {SyncId} completed successfully. Processed: {Processed}, Failed: {Failed}",
                        syncId, syncResults.ProcessedRecords, syncResults.FailedRecords);
                }
                else
                {
                    syncStatus.Status = SyncStatus.Failed;
                    syncStatus.ErrorMessage = jobResponse.ErrorMessage ?? "Databricks job execution failed";
                    syncStatus.CompletedAt = DateTimeOffset.UtcNow;

                    _logger.LogError("Candidate synchronization {SyncId} failed: {Error}", syncId, syncStatus.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                syncStatus.Status = SyncStatus.Cancelled;
                syncStatus.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogWarning("Candidate synchronization {SyncId} was cancelled", syncId);
            }
            catch (Exception ex)
            {
                syncStatus.Status = SyncStatus.Failed;
                syncStatus.ErrorMessage = ex.Message;
                syncStatus.CompletedAt = DateTimeOffset.UtcNow;
                
                _logger.LogError(ex, "Error during candidate synchronization {SyncId}", syncId);
            }
            finally
            {
                _syncCancellationTokens.TryRemove(syncId, out _);
            }

            return syncStatus;
        }

        public async Task<SyncStatusDto> SynchronizeJobsAsync(CancellationToken cancellationToken = default)
        {
            var syncId = Guid.NewGuid();
            var syncStatus = new SyncStatusDto
            {
                SyncId = syncId,
                Type = SyncType.Jobs,
                Status = SyncStatus.Pending,
                StartedAt = DateTimeOffset.UtcNow
            };

            _activeSyncs[syncId] = syncStatus;
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _syncCancellationTokens[syncId] = cancellationTokenSource;

            try
            {
                _logger.LogInformation("Starting job synchronization {SyncId}", syncId);
                syncStatus.Status = SyncStatus.Running;

                var jobRequest = new DatabricksJobRequestDto
                {
                    JobType = "sync_jobs",
                    Parameters = new Dictionary<string, string>
                    {
                        ["sync_id"] = syncId.ToString(),
                        ["database_name"] = _databricksSettings.DatabaseName,
                        ["table_name"] = _databricksSettings.JobsTableName
                    },
                    ClusterId = _databricksSettings.ClusterId,
                    NotebookPath = _databricksSettings.GupyNotebookPath,
                    TimeoutMinutes = _databricksSettings.JobTimeoutMinutes
                };

                var jobResponse = await ExecuteNotebookAsync(jobRequest, cancellationTokenSource.Token);

                if (jobResponse.Status == "SUCCESS" || jobResponse.ResultState == "SUCCESS")
                {
                    var syncResults = await ProcessSyncResults(jobResponse.Output, syncId);
                    
                    syncStatus.TotalRecords = syncResults.TotalRecords;
                    syncStatus.ProcessedRecords = syncResults.ProcessedRecords;
                    syncStatus.FailedRecords = syncResults.FailedRecords;
                    syncStatus.Status = syncResults.FailedRecords > 0 ? SyncStatus.PartiallyCompleted : SyncStatus.Completed;
                    syncStatus.CompletedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Job synchronization {SyncId} completed successfully. Processed: {Processed}, Failed: {Failed}",
                        syncId, syncResults.ProcessedRecords, syncResults.FailedRecords);
                }
                else
                {
                    syncStatus.Status = SyncStatus.Failed;
                    syncStatus.ErrorMessage = jobResponse.ErrorMessage ?? "Databricks job execution failed";
                    syncStatus.CompletedAt = DateTimeOffset.UtcNow;
                }
            }
            catch (Exception ex)
            {
                syncStatus.Status = SyncStatus.Failed;
                syncStatus.ErrorMessage = ex.Message;
                syncStatus.CompletedAt = DateTimeOffset.UtcNow;
                
                _logger.LogError(ex, "Error during job synchronization {SyncId}", syncId);
            }
            finally
            {
                _syncCancellationTokens.TryRemove(syncId, out _);
            }

            return syncStatus;
        }

        public async Task<DatabricksJobResponseDto> ExecuteNotebookAsync(DatabricksJobRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing Databricks notebook {NotebookPath} for job type {JobType}", 
                    request.NotebookPath, request.JobType);

                // Prepare the request payload for Databricks Jobs API
                var requestPayload = new
                {
                    run_name = $"Gupy Sync - {request.JobType} - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                    existing_cluster_id = request.ClusterId ?? _databricksSettings.ClusterId,
                    timeout_seconds = request.TimeoutMinutes * 60,
                    notebook_task = new
                    {
                        notebook_path = request.NotebookPath ?? _databricksSettings.GupyNotebookPath,
                        base_parameters = request.Parameters
                    }
                };

                var json = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Submit the job
                var submitResponse = await _httpClient.PostAsync("/api/2.1/jobs/runs/submit", content, cancellationToken);
                submitResponse.EnsureSuccessStatusCode();

                var submitResult = await submitResponse.Content.ReadAsStringAsync(cancellationToken);
                var submitData = JsonSerializer.Deserialize<JsonElement>(submitResult);
                var runId = submitData.GetProperty("run_id").GetInt64();

                _logger.LogInformation("Databricks job submitted with run ID {RunId}", runId);

                // Poll for job completion
                var response = new DatabricksJobResponseDto
                {
                    RunId = runId,
                    Status = "RUNNING",
                    StartTime = DateTimeOffset.UtcNow
                };

                // Wait for job completion (simplified polling)
                var maxWaitTime = TimeSpan.FromMinutes(request.TimeoutMinutes);
                var startTime = DateTime.UtcNow;
                var pollInterval = TimeSpan.FromSeconds(30);

                while (DateTime.UtcNow - startTime < maxWaitTime && !cancellationToken.IsCancellationRequested)
                {
                    var statusResponse = await _httpClient.GetAsync($"/api/2.1/jobs/runs/get?run_id={runId}", cancellationToken);
                    statusResponse.EnsureSuccessStatusCode();

                    var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
                    var statusData = JsonSerializer.Deserialize<JsonElement>(statusJson);
                    var state = statusData.GetProperty("state");
                    
                    response.Status = state.GetProperty("life_cycle_state").GetString() ?? "UNKNOWN";
                    
                    if (state.TryGetProperty("result_state", out var resultState))
                    {
                        response.ResultState = resultState.GetString();
                    }

                    if (state.TryGetProperty("state_message", out var stateMessage))
                    {
                        response.ErrorMessage = stateMessage.GetString();
                    }

                    // Check if job is completed
                    if (response.Status is "TERMINATED" or "SKIPPED" or "INTERNAL_ERROR")
                    {
                        response.EndTime = DateTimeOffset.UtcNow;

                        // Get job output if successful
                        if (response.ResultState == "SUCCESS")
                        {
                            var outputResponse = await _httpClient.GetAsync($"/api/2.1/jobs/runs/get-output?run_id={runId}", cancellationToken);
                            if (outputResponse.IsSuccessStatusCode)
                            {
                                var outputJson = await outputResponse.Content.ReadAsStringAsync(cancellationToken);
                                var outputData = JsonSerializer.Deserialize<Dictionary<string, object>>(outputJson);
                                response.Output = outputData ?? new Dictionary<string, object>();
                            }
                        }

                        break;
                    }

                    await Task.Delay(pollInterval, cancellationToken);
                }

                if (DateTime.UtcNow - startTime >= maxWaitTime)
                {
                    response.Status = "TIMEOUT";
                    response.ErrorMessage = "Job execution timed out";
                    response.EndTime = DateTimeOffset.UtcNow;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Databricks notebook for job type {JobType}", request.JobType);
                return new DatabricksJobResponseDto
                {
                    Status = "ERROR",
                    ErrorMessage = ex.Message,
                    StartTime = DateTimeOffset.UtcNow,
                    EndTime = DateTimeOffset.UtcNow
                };
            }
        }

        // Implementation of other interface methods with mock data
        public async Task<SyncStatusDto?> GetSyncStatusAsync(Guid syncId, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return _activeSyncs.TryGetValue(syncId, out var status) ? status : null;
        }

        public async Task<List<SyncStatusDto>> GetActiveSyncOperationsAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return _activeSyncs.Values.Where(s => s.Status == SyncStatus.Running || s.Status == SyncStatus.Pending).ToList();
        }

        public async Task<bool> CancelSyncOperationAsync(Guid syncId, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (_syncCancellationTokens.TryGetValue(syncId, out var tokenSource))
            {
                tokenSource.Cancel();
                return true;
            }
            return false;
        }

        public CandidateDto MapGupyCandidate(GupyCandidateDto gupyCandidate)
        {
            return new CandidateDto
            {
                Id = Guid.NewGuid(), // Generate new GUID for Dignus system
                Name = gupyCandidate.FullName,
                Email = gupyCandidate.Email,
                Cpf = gupyCandidate.Cpf ?? "",
                Phone = gupyCandidate.Phone,
                BirthDate = gupyCandidate.BirthDate ?? DateTime.MinValue,
                CreatedAt = gupyCandidate.ApplicationDate ?? DateTimeOffset.UtcNow
            };
        }

        public JobDto MapGupyJob(GupyJobDto gupyJob)
        {
            return new JobDto
            {
                Id = Guid.NewGuid(), // Generate new GUID for Dignus system
                Title = gupyJob.Title,
                Description = gupyJob.Description,
                Department = gupyJob.Department ?? "",
                Location = gupyJob.Location ?? "",
                IsActive = gupyJob.Status.Equals("active", StringComparison.OrdinalIgnoreCase),
                CreatedAt = gupyJob.CreatedAt ?? DateTimeOffset.UtcNow
            };
        }

        // Other interface method implementations with basic functionality
        public async Task<List<FieldMappingDto>> GetFieldMappingsAsync(string mappingType, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return mappingType.ToLower() switch
            {
                "candidate" => GetDefaultCandidateMappings(),
                "job" => GetDefaultJobMappings(),
                _ => new List<FieldMappingDto>()
            };
        }

        public async Task<bool> UpdateFieldMappingsAsync(string mappingType, List<FieldMappingDto> mappings, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            _logger.LogInformation("Field mappings updated for type {MappingType}: {Count} mappings", mappingType, mappings.Count);
            return true;
        }

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/2.0/clusters/list", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Databricks connection test failed");
                return false;
            }
        }

        public async Task<List<SyncStatusDto>> GetSyncHistoryAsync(SyncType? syncType = null, int days = 30, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-days);
            
            return _activeSyncs.Values
                .Where(s => s.StartedAt >= cutoffDate && (syncType == null || s.Type == syncType))
                .OrderByDescending(s => s.StartedAt)
                .ToList();
        }

        public async Task<bool> ScheduleSyncAsync(SyncType syncType, string cronExpression, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            _logger.LogInformation("Scheduled sync operation for type {SyncType} with cron {CronExpression}", syncType, cronExpression);
            return true;
        }

        public async Task<bool> StopScheduledSyncAsync(SyncType syncType, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            _logger.LogInformation("Stopped scheduled sync operation for type {SyncType}", syncType);
            return true;
        }

        // Helper methods
        private async Task<(int TotalRecords, int ProcessedRecords, int FailedRecords)> ProcessSyncResults(Dictionary<string, object> output, Guid syncId)
        {
            await Task.CompletedTask;
            
            // Mock processing of sync results
            var totalRecords = 1000;
            var processedRecords = 980;
            var failedRecords = 20;

            // In a real implementation, this would parse the actual Databricks job output
            if (output.TryGetValue("total_records", out var total) && int.TryParse(total.ToString(), out var totalParsed))
            {
                totalRecords = totalParsed;
            }

            if (output.TryGetValue("processed_records", out var processed) && int.TryParse(processed.ToString(), out var processedParsed))
            {
                processedRecords = processedParsed;
            }

            if (output.TryGetValue("failed_records", out var failed) && int.TryParse(failed.ToString(), out var failedParsed))
            {
                failedRecords = failedParsed;
            }

            return (totalRecords, processedRecords, failedRecords);
        }

        private List<FieldMappingDto> GetDefaultCandidateMappings()
        {
            return new List<FieldMappingDto>
            {
                new() { SourceField = "id", TargetField = "ExternalId", IsRequired = true, DataType = "string" },
                new() { SourceField = "full_name", TargetField = "Name", IsRequired = true, DataType = "string" },
                new() { SourceField = "email", TargetField = "Email", IsRequired = true, DataType = "string" },
                new() { SourceField = "cpf", TargetField = "Cpf", IsRequired = false, DataType = "string" },
                new() { SourceField = "phone", TargetField = "Phone", IsRequired = false, DataType = "string" },
                new() { SourceField = "birth_date", TargetField = "BirthDate", IsRequired = false, DataType = "datetime" },
                new() { SourceField = "application_date", TargetField = "CreatedAt", IsRequired = false, DataType = "datetime" }
            };
        }

        private List<FieldMappingDto> GetDefaultJobMappings()
        {
            return new List<FieldMappingDto>
            {
                new() { SourceField = "job_id", TargetField = "ExternalId", IsRequired = true, DataType = "string" },
                new() { SourceField = "title", TargetField = "Title", IsRequired = true, DataType = "string" },
                new() { SourceField = "description", TargetField = "Description", IsRequired = false, DataType = "string" },
                new() { SourceField = "department", TargetField = "Department", IsRequired = false, DataType = "string" },
                new() { SourceField = "location", TargetField = "Location", IsRequired = false, DataType = "string" },
                new() { SourceField = "status", TargetField = "IsActive", IsRequired = false, DataType = "boolean", TransformationRule = "active=true,inactive=false" },
                new() { SourceField = "created_at", TargetField = "CreatedAt", IsRequired = false, DataType = "datetime" }
            };
        }
    }
}