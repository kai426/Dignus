using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for AI-powered video analysis using Google AI and LangChain
    /// </summary>
    public class AIVideoAnalysisService : IAIVideoAnalysisService
    {
        private readonly AISettings _aiSettings;
        private readonly ILogger<AIVideoAnalysisService> _logger;
        private readonly HttpClient _httpClient;
        
        // In-memory store for processing status (in production, use Redis or database)
        private static readonly ConcurrentDictionary<Guid, ProcessingStatusDto> _processingStatus = new();

        public AIVideoAnalysisService(
            IOptions<AISettings> aiSettings,
            ILogger<AIVideoAnalysisService> logger,
            HttpClient httpClient)
        {
            _aiSettings = aiSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<VideoAnalysisResultDto> AnalyzeVideoAsync(Guid videoInterviewId, string videoUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting video analysis for interview {VideoInterviewId}", videoInterviewId);

                // Update processing status
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 5, "Downloading video file");

                // Download video file from blob storage
                var videoData = await DownloadVideoFileAsync(videoUrl, cancellationToken);
                
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 20, "Extracting key frames");

                // Extract key frames for visual analysis
                var keyFrameUrls = await ExtractKeyFramesAsync(videoUrl, 5, cancellationToken);
                
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 40, "Processing visual analysis");

                // Analyze visual elements
                var visualAnalysis = await AnalyzeVisualElementsAsync(keyFrameUrls, cancellationToken);
                
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 60, "Processing audio analysis");

                // Analyze audio quality and transcription
                var audioAnalysis = await AnalyzeAudioQualityAsync(videoData, cancellationToken);
                
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 80, "Generating transcription");

                // Get spoken content transcription
                var transcription = await TranscribeVideoAudioAsync(videoData, cancellationToken);
                
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.InProgress, 95, "Finalizing analysis");

                var result = new VideoAnalysisResultDto
                {
                    VideoInterviewId = videoInterviewId,
                    SpokenTranscription = transcription,
                    VisualAnalysis = visualAnalysis,
                    AudioQuality = audioAnalysis,
                    DurationSeconds = await GetVideoDurationAsync(videoData),
                    ProcessedAt = DateTimeOffset.UtcNow
                };

                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.Completed, 100, "Video analysis completed");
                
                _logger.LogInformation("Video analysis completed successfully for interview {VideoInterviewId}", videoInterviewId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing video for interview {VideoInterviewId}", videoInterviewId);
                UpdateProcessingStatus(videoInterviewId, ProcessingStatus.Failed, 0, "Video analysis failed", ex.Message);
                throw;
            }
        }

        public async Task<BehavioralEvaluationDto> EvaluateBehavioralAnalysisAsync(Guid videoInterviewId, VideoAnalysisResultDto analysisResult, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting behavioral evaluation for interview {VideoInterviewId}", videoInterviewId);

                // Create comprehensive evaluation prompt
                var evaluationPrompt = CreateBehavioralEvaluationPrompt(analysisResult);

                // Use Google's Gemini model for behavioral evaluation
                var evaluationResult = await EvaluateWithGeminiAsync(evaluationPrompt, cancellationToken);

                var result = new BehavioralEvaluationDto
                {
                    VideoInterviewId = videoInterviewId,
                    CommunicationSkillsScore = evaluationResult.CommunicationSkillsScore,
                    ProfessionalPresenceScore = evaluationResult.ProfessionalPresenceScore,
                    EmotionalIntelligenceScore = evaluationResult.EmotionalIntelligenceScore,
                    ConfidenceScore = evaluationResult.ConfidenceScore,
                    AuthenticityScore = evaluationResult.AuthenticityScore,
                    StressManagementScore = evaluationResult.StressManagementScore,
                    DetailedFeedback = evaluationResult.DetailedFeedback,
                    BehavioralStrengths = evaluationResult.BehavioralStrengths,
                    BehavioralImprovementAreas = evaluationResult.BehavioralImprovementAreas,
                    OverallBehavioralScore = CalculateOverallBehavioralScore(evaluationResult),
                    PersonalityTraits = evaluationResult.PersonalityTraits,
                    EvaluatedAt = DateTimeOffset.UtcNow
                };

                _logger.LogInformation("Behavioral evaluation completed for interview {VideoInterviewId} with overall score {OverallScore}", 
                    videoInterviewId, result.OverallBehavioralScore);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating behavioral analysis for interview {VideoInterviewId}", videoInterviewId);
                throw;
            }
        }

        public Task<ProcessingStatusDto> GetAnalysisStatusAsync(Guid videoInterviewId)
        {
            _processingStatus.TryGetValue(videoInterviewId, out var status);
            return Task.FromResult(status ?? new ProcessingStatusDto
            {
                ItemId = videoInterviewId,
                ProcessingType = "VideoAnalysis",
                Status = ProcessingStatus.Queued,
                ProgressPercentage = 0,
                CurrentStage = "Not started",
                StartedAt = DateTimeOffset.UtcNow
            });
        }

        public Task<bool> CancelAnalysisAsync(Guid videoInterviewId)
        {
            try
            {
                if (_processingStatus.TryGetValue(videoInterviewId, out var status) && 
                    status.Status == ProcessingStatus.InProgress)
                {
                    UpdateProcessingStatus(videoInterviewId, ProcessingStatus.Cancelled, status.ProgressPercentage, "Cancelled by user");
                    _logger.LogInformation("Video analysis cancelled for interview {VideoInterviewId}", videoInterviewId);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling video analysis for interview {VideoInterviewId}", videoInterviewId);
                return Task.FromResult(false);
            }
        }

        public async Task<IEnumerable<string>> ExtractKeyFramesAsync(string videoUrl, int frameCount = 5, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Extracting {FrameCount} key frames from video {VideoUrl}", frameCount, videoUrl);

                // For now, return mock frame URLs - in production, use video processing library
                await Task.Delay(1000, cancellationToken);

                var frameUrls = new List<string>();
                for (int i = 0; i < frameCount; i++)
                {
                    frameUrls.Add($"{videoUrl}/frame_{i}.jpg");
                }

                return frameUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting key frames from video {VideoUrl}", videoUrl);
                throw;
            }
        }

        private async Task<byte[]> DownloadVideoFileAsync(string videoUrl, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(videoUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download video file from {VideoUrl}", videoUrl);
                throw;
            }
        }

        private async Task<VisualAnalysisDto> AnalyzeVisualElementsAsync(IEnumerable<string> keyFrameUrls, CancellationToken cancellationToken)
        {
            try
            {
                // Mock visual analysis - in production, use computer vision APIs
                await Task.Delay(2000, cancellationToken);

                return new VisualAnalysisDto
                {
                    FacialExpressions = new List<FacialExpressionDto>
                    {
                        new() { Timestamp = 10.0, Emotion = "Confident", Confidence = 0.85, Intensity = 0.7 },
                        new() { Timestamp = 30.0, Emotion = "Engaged", Confidence = 0.78, Intensity = 0.6 },
                        new() { Timestamp = 60.0, Emotion = "Thoughtful", Confidence = 0.82, Intensity = 0.5 }
                    },
                    BodyLanguage = new BodyLanguageDto
                    {
                        PostureScore = 85,
                        GestureScore = 78,
                        MovementPattern = "Composed with minimal distracting movements",
                        ConfidenceDisplay = 82
                    },
                    EyeContact = new EyeContactDto
                    {
                        EyeContactPercentage = 75,
                        ConsistencyScore = 80,
                        GazePattern = "Mostly direct with occasional thoughtful breaks"
                    },
                    EngagementLevel = 83,
                    ProfessionalAppearanceScore = 88
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing visual elements");
                throw;
            }
        }

        private async Task<AudioQualityDto> AnalyzeAudioQualityAsync(byte[] videoData, CancellationToken cancellationToken)
        {
            try
            {
                // Mock audio quality analysis - in production, use audio processing libraries
                await Task.Delay(1000, cancellationToken);

                return new AudioQualityDto
                {
                    ClarityScore = 85,
                    VolumeConsistency = 88,
                    BackgroundNoiseLevel = 15,
                    SpeakingPace = new SpeakingPaceDto
                    {
                        WordsPerMinute = 145,
                        ConsistencyScore = 82,
                        PausePattern = "Natural pauses with good timing",
                        AppropriatenessScore = 85
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing audio quality");
                throw;
            }
        }

        private async Task<string?> TranscribeVideoAudioAsync(byte[] videoData, CancellationToken cancellationToken)
        {
            try
            {
                // Mock transcription - in production, extract audio from video and transcribe
                await Task.Delay(2000, cancellationToken);
                
                return "Esta é uma transcrição simulada do áudio do vídeo. O candidato apresentou suas qualificações de forma clara e demonstrou conhecimento técnico apropriado para a posição.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing video audio");
                return null;
            }
        }

        private async Task<double> GetVideoDurationAsync(byte[] videoData)
        {
            // Mock duration - in production, use video processing library to get actual duration
            await Task.Delay(100);
            return 180.0; // 3 minutes
        }

        private string CreateBehavioralEvaluationPrompt(VideoAnalysisResultDto analysisResult)
        {
            return $@"
Analyze the following video interview data and provide a comprehensive behavioral evaluation.
Provide scores from 0 to 100 for each behavioral criterion.

Video Analysis Data:
- Duration: {analysisResult.DurationSeconds} seconds
- Transcription: {analysisResult.SpokenTranscription}
- Engagement Level: {analysisResult.VisualAnalysis.EngagementLevel}
- Professional Appearance: {analysisResult.VisualAnalysis.ProfessionalAppearanceScore}
- Eye Contact: {analysisResult.VisualAnalysis.EyeContact.EyeContactPercentage}%
- Posture Score: {analysisResult.VisualAnalysis.BodyLanguage.PostureScore}
- Audio Clarity: {analysisResult.AudioQuality.ClarityScore}

Evaluate these behavioral aspects:
1. Communication Skills (clarity, articulation, message structure)
2. Professional Presence (appearance, demeanor, confidence display)
3. Emotional Intelligence (self-awareness, empathy indicators)
4. Confidence Level (body language, voice tone, assertiveness)
5. Authenticity (genuineness, naturalness)
6. Stress Management (composure under pressure)

Also identify:
- Key personality traits demonstrated
- Behavioral strengths
- Areas for improvement
- Overall behavioral assessment

Format your response as JSON with appropriate structure for behavioral evaluation.";
        }

        private async Task<BehavioralEvaluationResult> EvaluateWithGeminiAsync(string prompt, CancellationToken cancellationToken)
        {
            try
            {
                // Mock evaluation - in production, use Google's Gemini API
                await Task.Delay(2000, cancellationToken);

                return new BehavioralEvaluationResult
                {
                    CommunicationSkillsScore = 85,
                    ProfessionalPresenceScore = 88,
                    EmotionalIntelligenceScore = 78,
                    ConfidenceScore = 82,
                    AuthenticityScore = 86,
                    StressManagementScore = 80,
                    DetailedFeedback = "O candidato demonstrou excelente presença profissional e habilidades de comunicação sólidas. Mostrou-se confiante e autêntico durante a entrevista, com boa gestão do estresse. Algumas áreas de desenvolvimento incluem maior demonstração de inteligência emocional.",
                    BehavioralStrengths = new List<string>
                    {
                        "Presença profissional excepcional",
                        "Comunicação clara e articulada",
                        "Demonstração de confiança apropriada",
                        "Autenticidade nas respostas"
                    },
                    BehavioralImprovementAreas = new List<string>
                    {
                        "Desenvolver indicadores de inteligência emocional",
                        "Aprimorar técnicas de gestão de estresse"
                    },
                    PersonalityTraits = new List<PersonalityTraitDto>
                    {
                        new() { TraitName = "Extroversão", Strength = 75, Confidence = 0.8, Description = "Demonstra facilidade em interações sociais" },
                        new() { TraitName = "Conscienciosidade", Strength = 82, Confidence = 0.85, Description = "Mostra organização e atenção aos detalhes" },
                        new() { TraitName = "Abertura", Strength = 78, Confidence = 0.75, Description = "Receptivo a novas ideias e experiências" }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating with Gemini");
                throw;
            }
        }

        private static int CalculateOverallBehavioralScore(BehavioralEvaluationResult evaluation)
        {
            // Weighted average calculation for behavioral score
            var weights = new Dictionary<string, double>
            {
                ["communication"] = 0.25,
                ["professional"] = 0.20,
                ["emotional"] = 0.15,
                ["confidence"] = 0.15,
                ["authenticity"] = 0.15,
                ["stress"] = 0.10
            };

            var weightedSum = 
                evaluation.CommunicationSkillsScore * weights["communication"] +
                evaluation.ProfessionalPresenceScore * weights["professional"] +
                evaluation.EmotionalIntelligenceScore * weights["emotional"] +
                evaluation.ConfidenceScore * weights["confidence"] +
                evaluation.AuthenticityScore * weights["authenticity"] +
                evaluation.StressManagementScore * weights["stress"];

            return (int)Math.Round(weightedSum);
        }

        private static void UpdateProcessingStatus(Guid itemId, ProcessingStatus status, int progressPercentage, string currentStage, string? errorMessage = null)
        {
            _processingStatus.AddOrUpdate(itemId, 
                new ProcessingStatusDto
                {
                    ItemId = itemId,
                    ProcessingType = "VideoAnalysis",
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

        private class BehavioralEvaluationResult
        {
            public int CommunicationSkillsScore { get; set; }
            public int ProfessionalPresenceScore { get; set; }
            public int EmotionalIntelligenceScore { get; set; }
            public int ConfidenceScore { get; set; }
            public int AuthenticityScore { get; set; }
            public int StressManagementScore { get; set; }
            public string DetailedFeedback { get; set; } = null!;
            public List<string> BehavioralStrengths { get; set; } = new();
            public List<string> BehavioralImprovementAreas { get; set; } = new();
            public List<PersonalityTraitDto> PersonalityTraits { get; set; } = new();
        }
    }
}