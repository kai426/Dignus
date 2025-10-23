using Dignus.Candidate.Back.DTOs;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for questionnaire operations
    /// </summary>
    public interface IQuestionnaireService
    {
        /// <summary>
        /// Get questionnaire structure (all sections and questions)
        /// </summary>
        Task<QuestionnaireDto> GetQuestionnaireAsync();

        /// <summary>
        /// Get questionnaire progress for a candidate
        /// </summary>
        Task<QuestionnaireProgressDto> GetProgressAsync(Guid candidateId);

        /// <summary>
        /// Save responses for a specific section
        /// </summary>
        Task<bool> SaveSectionResponseAsync(Guid candidateId, int sectionId, Dictionary<string, object> responses);

        /// <summary>
        /// Submit completed questionnaire
        /// </summary>
        Task<bool> SubmitQuestionnaireAsync(SubmitQuestionnaireDto submission);

        /// <summary>
        /// Get a specific section by ID
        /// </summary>
        Task<QuestionnaireSectionDto?> GetSectionAsync(int sectionId);

        /// <summary>
        /// Check if candidate can start questionnaire
        /// </summary>
        Task<bool> CanStartQuestionnaireAsync(Guid candidateId);

        /// <summary>
        /// Initialize questionnaire for candidate (create progress tracking)
        /// </summary>
        Task<QuestionnaireProgressDto> InitializeQuestionnaireAsync(Guid candidateId);

        /// <summary>
        /// Get complete questionnaire structure (all fixed psychology questions)
        /// </summary>
        Task<QuestionnaireDto> GetCompleteQuestionnaireAsync();

        /// <summary>
        /// Process fixed question responses for psychology test (A, B, C, D, E format)
        /// </summary>
        Task<bool> ProcessFixedResponsesAsync(PsychologyTestSubmissionDto submission);
    }
}