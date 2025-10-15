using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for AI-powered audio transcription using Google AI and LangChain
    /// </summary>
    public class AITranscriptionService : IAITranscriptionService
    {
        private readonly AISettings _aiSettings;
        private readonly ILogger<AITranscriptionService> _logger;
        private readonly HttpClient _httpClient;
        
        // In-memory store for processing status (in production, use Redis or database)
        private static readonly ConcurrentDictionary<Guid, ProcessingStatusDto> _processingStatus = new();

        public AITranscriptionService(
            IOptions<AISettings> aiSettings,
            ILogger<AITranscriptionService> logger,
            HttpClient httpClient)
        {
            _aiSettings = aiSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<TranscriptionResultDto> TranscribeAudioAsync(Guid audioSubmissionId, string audioUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting audio transcription for submission {AudioSubmissionId}", audioSubmissionId);

                // Update processing status
                UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.InProgress, 10, "Downloading audio file");

                // Download audio file from blob storage
                var audioData = await DownloadAudioFileAsync(audioUrl, cancellationToken);
                
                UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.InProgress, 30, "Processing with AI model");

                // Use Google's Gemini model for transcription
                var transcriptionResult = await ProcessAudioWithGeminiAsync(audioData, cancellationToken);
                
                UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.InProgress, 80, "Finalizing transcription");

                var result = new TranscriptionResultDto
                {
                    AudioSubmissionId = audioSubmissionId,
                    TranscribedText = transcriptionResult.TranscribedText,
                    Confidence = transcriptionResult.Confidence,
                    DurationSeconds = transcriptionResult.DurationSeconds,
                    DetectedLanguage = transcriptionResult.DetectedLanguage ?? "pt-BR",
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Errors = transcriptionResult.Errors
                };

                UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.Completed, 100, "Transcription completed");
                
                _logger.LogInformation("Audio transcription completed successfully for submission {AudioSubmissionId}", audioSubmissionId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing audio for submission {AudioSubmissionId}", audioSubmissionId);
                UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.Failed, 0, "Transcription failed", ex.Message);
                throw;
            }
        }

        public async Task<AudioEvaluationDto> EvaluateTranscriptionAsync(Guid audioSubmissionId, string transcription, string testType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting evaluation of transcription for submission {AudioSubmissionId}, test type {TestType}", audioSubmissionId, testType);

                // Create evaluation prompt based on test type
                var evaluationPrompt = CreateEvaluationPrompt(transcription, testType);

                // Use Google's Gemini model for evaluation
                var evaluationResult = await EvaluateWithGeminiAsync(evaluationPrompt, cancellationToken);

                var result = new AudioEvaluationDto
                {
                    AudioSubmissionId = audioSubmissionId,
                    CommunicationScore = evaluationResult.CommunicationScore,
                    GrammarScore = evaluationResult.GrammarScore,
                    VocabularyScore = evaluationResult.VocabularyScore,
                    FluencyScore = evaluationResult.FluencyScore,
                    ContentRelevanceScore = evaluationResult.ContentRelevanceScore,
                    DetailedFeedback = evaluationResult.DetailedFeedback,
                    Strengths = evaluationResult.Strengths,
                    ImprovementAreas = evaluationResult.ImprovementAreas,
                    OverallScore = CalculateOverallScore(evaluationResult),
                    EvaluatedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Transcription evaluation completed for submission {AudioSubmissionId} with overall score {OverallScore}", audioSubmissionId, result.OverallScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating transcription for submission {AudioSubmissionId}", audioSubmissionId);
                throw;
            }
        }

        public Task<ProcessingStatusDto> GetTranscriptionStatusAsync(Guid audioSubmissionId)
        {
            _processingStatus.TryGetValue(audioSubmissionId, out var status);
            return Task.FromResult(status ?? new ProcessingStatusDto
            {
                ItemId = audioSubmissionId,
                ProcessingType = "Transcription",
                Status = ProcessingStatus.Queued,
                ProgressPercentage = 0,
                CurrentStage = "Not started",
                StartedAt = DateTimeOffset.UtcNow
            });
        }

        public Task<bool> CancelTranscriptionAsync(Guid audioSubmissionId)
        {
            try
            {
                if (_processingStatus.TryGetValue(audioSubmissionId, out var status) && 
                    status.Status == ProcessingStatus.InProgress)
                {
                    UpdateProcessingStatus(audioSubmissionId, ProcessingStatus.Cancelled, status.ProgressPercentage, "Cancelled by user");
                    _logger.LogInformation("Transcription cancelled for submission {AudioSubmissionId}", audioSubmissionId);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transcription for submission {AudioSubmissionId}", audioSubmissionId);
                return Task.FromResult(false);
            }
        }

        private async Task<byte[]> DownloadAudioFileAsync(string audioUrl, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(audioUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download audio file from {AudioUrl}", audioUrl);
                throw;
            }
        }

        private async Task<(string TranscribedText, double Confidence, double DurationSeconds, string? DetectedLanguage, string? Errors)> ProcessAudioWithGeminiAsync(byte[] audioData, CancellationToken cancellationToken)
        {
            try
            {
                // For now, return a mock transcription - in production, integrate with Google Speech-to-Text API
                // or use Gemini's audio processing capabilities when available
                
                await Task.Delay(2000, cancellationToken); // Simulate processing time
                
                return (
                    TranscribedText: "Esta é uma transcrição simulada do áudio fornecido. O candidato respondeu com clareza e demonstrou conhecimento sobre o tema solicitado.",
                    Confidence: 0.85,
                    DurationSeconds: 120.0,
                    DetectedLanguage: "pt-BR",
                    Errors: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio with Gemini");
                throw;
            }
        }

        private string CreateEvaluationPrompt(string transcription, string testType)
        {
            var prompt = testType.ToLowerInvariant() switch
            {
                "portuguese" => $@"
Avalie a seguinte transcrição de áudio em português brasileiro considerando os critérios abaixo.
Forneça pontuações de 0 a 100 para cada critério e um feedback detalhado.

Transcrição: ""{transcription}""

Critérios de avaliação:
1. Comunicação (clareza e expressão)
2. Gramática (correção gramatical)
3. Vocabulário (riqueza e adequação)
4. Fluência (naturalidade e ritmo)
5. Relevância do conteúdo

Formate sua resposta como JSON com as seguintes chaves:
- communicationScore
- grammarScore  
- vocabularyScore
- fluencyScore
- contentRelevanceScore
- detailedFeedback
- strengths (array)
- improvementAreas (array)",

                _ => $@"
Evaluate the following audio transcription considering communication skills.
Provide scores from 0 to 100 for each criterion and detailed feedback.

Transcription: ""{transcription}""

Evaluation criteria:
1. Communication (clarity and expression)
2. Grammar (grammatical correctness)
3. Vocabulary (richness and appropriateness)
4. Fluency (naturalness and rhythm)
5. Content relevance

Format your response as JSON with the following keys:
- communicationScore
- grammarScore
- vocabularyScore
- fluencyScore
- contentRelevanceScore
- detailedFeedback
- strengths (array)
- improvementAreas (array)"
            };

            return prompt;
        }

        private async Task<AudioEvaluationResult> EvaluateWithGeminiAsync(string prompt, CancellationToken cancellationToken)
        {
            try
            {
                // For now, return a mock evaluation - in production, use Google's Gemini API
                await Task.Delay(1000, cancellationToken);

                return new AudioEvaluationResult
                {
                    CommunicationScore = 85,
                    GrammarScore = 78,
                    VocabularyScore = 82,
                    FluencyScore = 80,
                    ContentRelevanceScore = 88,
                    DetailedFeedback = "O candidato demonstrou boa capacidade de comunicação com resposta clara e organizada. Houve alguns pequenos erros gramaticais, mas o conteúdo foi relevante e bem estruturado.",
                    Strengths = new List<string> 
                    {
                        "Comunicação clara e objetiva",
                        "Boa organização das ideias",
                        "Conteúdo relevante ao tema"
                    },
                    ImprovementAreas = new List<string>
                    {
                        "Revisar algumas estruturas gramaticais",
                        "Enriquecer o vocabulário técnico"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating with Gemini");
                throw;
            }
        }

        private static int CalculateOverallScore(AudioEvaluationResult evaluation)
        {
            // Weighted average calculation
            var weights = new Dictionary<string, double>
            {
                ["communication"] = 0.25,
                ["grammar"] = 0.20,
                ["vocabulary"] = 0.20,
                ["fluency"] = 0.20,
                ["content"] = 0.15
            };

            var weightedSum = 
                evaluation.CommunicationScore * weights["communication"] +
                evaluation.GrammarScore * weights["grammar"] +
                evaluation.VocabularyScore * weights["vocabulary"] +
                evaluation.FluencyScore * weights["fluency"] +
                evaluation.ContentRelevanceScore * weights["content"];

            return (int)Math.Round(weightedSum);
        }

        private static void UpdateProcessingStatus(Guid itemId, ProcessingStatus status, int progressPercentage, string currentStage, string? errorMessage = null)
        {
            _processingStatus.AddOrUpdate(itemId, 
                new ProcessingStatusDto
                {
                    ItemId = itemId,
                    ProcessingType = "Transcription",
                    Status = status,
                    ProgressPercentage = progressPercentage,
                    CurrentStage = currentStage,
                    StartedAt = DateTimeOffset.UtcNow,
                    CompletedAt = status is ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Cancelled ? DateTimeOffset.UtcNow : null,
                    ErrorMessage = errorMessage
                },
                (key, existingStatus) =>
                {
                    existingStatus.Status = status;
                    existingStatus.ProgressPercentage = progressPercentage;
                    existingStatus.CurrentStage = currentStage;
                    existingStatus.ErrorMessage = errorMessage;
                    if (status is ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Cancelled)
                        existingStatus.CompletedAt = DateTimeOffset.UtcNow;
                    return existingStatus;
                });
        }

        private class AudioEvaluationResult
        {
            public int CommunicationScore { get; set; }
            public int GrammarScore { get; set; }
            public int VocabularyScore { get; set; }
            public int FluencyScore { get; set; }
            public int ContentRelevanceScore { get; set; }
            public string DetailedFeedback { get; set; } = null!;
            public List<string> Strengths { get; set; } = new();
            public List<string> ImprovementAreas { get; set; } = new();
        }
    }
}