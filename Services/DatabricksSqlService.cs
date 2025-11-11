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

        public DatabricksSqlService(
            HttpClient httpClient,
            IOptions<DatabricksSettings> settings,
            ILogger<DatabricksSqlService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

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
