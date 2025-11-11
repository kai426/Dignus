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
    }
}
