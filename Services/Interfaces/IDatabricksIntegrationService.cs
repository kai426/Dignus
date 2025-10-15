using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Interface for Databricks integration operations
    /// </summary>
    public interface IDatabricksIntegrationService
    {
        /// <summary>
        /// Synchronize candidate data from Gupy via Databricks
        /// </summary>
        /// <param name="batchSize">Number of candidates to process in each batch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization status</returns>
        Task<SyncStatusDto> SynchronizeCandidatesAsync(int batchSize = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronize job listings from Gupy via Databricks
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization status</returns>
        Task<SyncStatusDto> SynchronizeJobsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a custom Databricks notebook
        /// </summary>
        /// <param name="request">Databricks job execution request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Job execution response</returns>
        Task<DatabricksJobResponseDto> ExecuteNotebookAsync(DatabricksJobRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get synchronization status by sync ID
        /// </summary>
        /// <param name="syncId">Synchronization operation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Current sync status</returns>
        Task<SyncStatusDto?> GetSyncStatusAsync(Guid syncId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all active synchronization operations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active sync operations</returns>
        Task<List<SyncStatusDto>> GetActiveSyncOperationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a running synchronization operation
        /// </summary>
        /// <param name="syncId">Synchronization operation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if cancelled successfully</returns>
        Task<bool> CancelSyncOperationAsync(Guid syncId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Map Gupy candidate data to Dignus candidate format
        /// </summary>
        /// <param name="gupyCandidate">Gupy candidate data</param>
        /// <returns>Mapped Dignus candidate DTO</returns>
        CandidateDto MapGupyCandidate(GupyCandidateDto gupyCandidate);

        /// <summary>
        /// Map Gupy job data to Dignus job format
        /// </summary>
        /// <param name="gupyJob">Gupy job data</param>
        /// <returns>Mapped Dignus job DTO</returns>
        JobDto MapGupyJob(GupyJobDto gupyJob);

        /// <summary>
        /// Get field mapping configuration
        /// </summary>
        /// <param name="mappingType">Type of mapping (candidate, job)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of field mappings</returns>
        Task<List<FieldMappingDto>> GetFieldMappingsAsync(string mappingType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update field mapping configuration
        /// </summary>
        /// <param name="mappingType">Type of mapping (candidate, job)</param>
        /// <param name="mappings">Updated field mappings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateFieldMappingsAsync(string mappingType, List<FieldMappingDto> mappings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Test Databricks connection
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get synchronization history
        /// </summary>
        /// <param name="syncType">Type of sync operations to retrieve</param>
        /// <param name="days">Number of days to look back</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of historical sync operations</returns>
        Task<List<SyncStatusDto>> GetSyncHistoryAsync(SyncType? syncType = null, int days = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Start scheduled synchronization
        /// </summary>
        /// <param name="syncType">Type of sync to schedule</param>
        /// <param name="cronExpression">Cron expression for scheduling</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if scheduled successfully</returns>
        Task<bool> ScheduleSyncAsync(SyncType syncType, string cronExpression, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop scheduled synchronization
        /// </summary>
        /// <param name="syncType">Type of sync to stop</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if stopped successfully</returns>
        Task<bool> StopScheduledSyncAsync(SyncType syncType, CancellationToken cancellationToken = default);
    }
}