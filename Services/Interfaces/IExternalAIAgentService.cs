namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for calling external AI agent API
    /// </summary>
    public interface IExternalAIAgentService
    {
        /// <summary>
        /// Sends video response ID to external AI agent for analysis
        /// </summary>
        /// <param name="videoResponseId">The ID of the video response to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successfully sent to AI agent, false otherwise</returns>
        Task<bool> SendVideoForAnalysisAsync(Guid videoResponseId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends multiple video response IDs for batch analysis
        /// </summary>
        /// <param name="videoResponseIds">List of video response IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of videos successfully sent to AI agent</returns>
        Task<int> SendVideosForAnalysisAsync(IEnumerable<Guid> videoResponseIds, CancellationToken cancellationToken = default);
    }
}
