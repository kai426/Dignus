using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for calling external AI agent API
    /// </summary>
    public class ExternalAIAgentService : IExternalAIAgentService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalAIAgentSettings _settings;
        private readonly ILogger<ExternalAIAgentService> _logger;

        public ExternalAIAgentService(
            HttpClient httpClient,
            IOptions<ExternalAIAgentSettings> settings,
            ILogger<ExternalAIAgentService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Configure HttpClient base URL if provided
            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }

            // Configure authentication if API key is provided
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            }

            // Set timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<bool> SendVideoForAnalysisAsync(Guid videoResponseId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending video {VideoId} to external AI agent for analysis", videoResponseId);

                // Prepare request payload
                var payload = new
                {
                    videoResponseId = videoResponseId,
                    timestamp = DateTimeOffset.UtcNow
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send POST request to external AI agent
                var response = await _httpClient.PostAsync(_settings.AnalyzeEndpoint, httpContent, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation(
                        "Successfully sent video {VideoId} to AI agent. Response: {Response}",
                        videoResponseId,
                        responseContent);

                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Failed to send video {VideoId} to AI agent. Status: {StatusCode}, Error: {Error}",
                        videoResponseId,
                        response.StatusCode,
                        errorContent);

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error sending video {VideoId} to AI agent", videoResponseId);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout sending video {VideoId} to AI agent", videoResponseId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending video {VideoId} to AI agent", videoResponseId);
                return false;
            }
        }

        public async Task<int> SendVideosForAnalysisAsync(IEnumerable<Guid> videoResponseIds, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending {Count} videos to external AI agent for batch analysis", videoResponseIds.Count());

            var successCount = 0;

            foreach (var videoId in videoResponseIds)
            {
                var success = await SendVideoForAnalysisAsync(videoId, cancellationToken);
                if (success)
                {
                    successCount++;
                }

                // Optional: Add small delay between requests to avoid overwhelming the AI agent
                if (_settings.BatchDelayMilliseconds > 0)
                {
                    await Task.Delay(_settings.BatchDelayMilliseconds, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Batch analysis complete. Successfully sent {SuccessCount}/{TotalCount} videos to AI agent",
                successCount,
                videoResponseIds.Count());

            return successCount;
        }
    }
}
