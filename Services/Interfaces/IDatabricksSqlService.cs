using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service for executing SQL queries on Databricks SQL Warehouse
    /// </summary>
    public interface IDatabricksSqlService
    {
        /// <summary>
        /// Executes a SQL query and returns the results
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Query results as list of dictionaries</returns>
        Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets in-progress applications from gupy_aplicacoes table
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of in-progress applications</returns>
        Task<List<GupyAplicacaoDto>> GetInProgressApplicationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets candidate details by IDs from gupy_candidatos table
        /// </summary>
        /// <param name="candidateIds">List of candidate IDs to fetch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of candidate details</returns>
        Task<List<GupyCandidatoDto>> GetCandidatesByIdsAsync(List<string> candidateIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Syncs in-progress Gupy applications and their candidates to local database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sync operation results</returns>
        Task<GupySyncResponseDto> SyncInProgressApplicationsAsync(CancellationToken cancellationToken = default);
    }
}using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service for executing SQL queries on Databricks SQL Warehouse
    /// </summary>
    public interface IDatabricksSqlService
    {
        /// <summary>
        /// Executes a SQL query and returns the results
        /// </summary>
        /// <param name="sqlQuery">SQL query to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Query results as list of dictionaries</returns>
        Task<List<Dictionary<string, object?>>> ExecuteQueryAsync(string sqlQuery, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets in-progress applications from gupy_aplicacoes table
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of in-progress applications</returns>
        Task<List<GupyAplicacaoDto>> GetInProgressApplicationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets candidate details by IDs from gupy_candidatos table
        /// </summary>
        /// <param name="candidateIds">List of candidate IDs to fetch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of candidate details</returns>
        Task<List<GupyCandidatoDto>> GetCandidatesByIdsAsync(List<string> candidateIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Syncs in-progress Gupy applications and their candidates to local database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sync operation results</returns>
        Task<GupySyncResponseDto> SyncInProgressApplicationsAsync(CancellationToken cancellationToken = default);
    }
}