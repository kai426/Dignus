using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.DTOs
{
    /// <summary>
    /// DTO for video analysis result from AI services
    /// </summary>
    public class VideoAnalysisResultDto
    {
        /// <summary>
        /// The ID of the video interview
        /// </summary>
        public Guid VideoInterviewId { get; set; }
        
        /// <summary>
        /// Transcription of spoken content in the video
        /// </summary>
        public string? SpokenTranscription { get; set; }
        
        /// <summary>
        /// Visual analysis results
        /// </summary>
        public VisualAnalysisDto VisualAnalysis { get; set; } = null!;
        
        /// <summary>
        /// Audio quality analysis
        /// </summary>
        public AudioQualityDto AudioQuality { get; set; } = null!;
        
        /// <summary>
        /// Duration of the video in seconds
        /// </summary>
        public double DurationSeconds { get; set; }
        
        /// <summary>
        /// Processing timestamp
        /// </summary>
        public DateTimeOffset ProcessedAt { get; set; }
        
        /// <summary>
        /// Any errors encountered during processing
        /// </summary>
        public string? Errors { get; set; }
    }

    /// <summary>
    /// DTO for visual analysis of video content
    /// </summary>
    public class VisualAnalysisDto
    {
        /// <summary>
        /// Facial expression analysis throughout the video
        /// </summary>
        public List<FacialExpressionDto> FacialExpressions { get; set; } = new();
        
        /// <summary>
        /// Body language and posture analysis
        /// </summary>
        public BodyLanguageDto BodyLanguage { get; set; } = null!;
        
        /// <summary>
        /// Eye contact and gaze analysis
        /// </summary>
        public EyeContactDto EyeContact { get; set; } = null!;
        
        /// <summary>
        /// Overall engagement level detected
        /// </summary>
        [Range(0, 100)]
        public int EngagementLevel { get; set; }
        
        /// <summary>
        /// Professional appearance score
        /// </summary>
        [Range(0, 100)]
        public int ProfessionalAppearanceScore { get; set; }
    }

    /// <summary>
    /// DTO for facial expression analysis
    /// </summary>
    public class FacialExpressionDto
    {
        /// <summary>
        /// Timestamp in the video (seconds)
        /// </summary>
        public double Timestamp { get; set; }
        
        /// <summary>
        /// Detected emotion
        /// </summary>
        public string Emotion { get; set; } = null!;
        
        /// <summary>
        /// Confidence of emotion detection (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Confidence { get; set; }
        
        /// <summary>
        /// Emotional intensity (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Intensity { get; set; }
    }

    /// <summary>
    /// DTO for body language analysis
    /// </summary>
    public class BodyLanguageDto
    {
        /// <summary>
        /// Overall posture score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int PostureScore { get; set; }
        
        /// <summary>
        /// Hand gesture appropriateness (0-100)
        /// </summary>
        [Range(0, 100)]
        public int GestureScore { get; set; }
        
        /// <summary>
        /// Movement and fidgeting analysis
        /// </summary>
        public string MovementPattern { get; set; } = null!;
        
        /// <summary>
        /// Overall confidence display (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ConfidenceDisplay { get; set; }
    }

    /// <summary>
    /// DTO for eye contact analysis
    /// </summary>
    public class EyeContactDto
    {
        /// <summary>
        /// Percentage of time maintaining eye contact
        /// </summary>
        [Range(0, 100)]
        public int EyeContactPercentage { get; set; }
        
        /// <summary>
        /// Consistency of eye contact throughout interview
        /// </summary>
        [Range(0, 100)]
        public int ConsistencyScore { get; set; }
        
        /// <summary>
        /// Direction of gaze analysis
        /// </summary>
        public string GazePattern { get; set; } = null!;
    }

    /// <summary>
    /// DTO for audio quality analysis
    /// </summary>
    public class AudioQualityDto
    {
        /// <summary>
        /// Audio clarity score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ClarityScore { get; set; }
        
        /// <summary>
        /// Volume consistency (0-100)
        /// </summary>
        [Range(0, 100)]
        public int VolumeConsistency { get; set; }
        
        /// <summary>
        /// Background noise level (0-100, lower is better)
        /// </summary>
        [Range(0, 100)]
        public int BackgroundNoiseLevel { get; set; }
        
        /// <summary>
        /// Speaking pace analysis
        /// </summary>
        public SpeakingPaceDto SpeakingPace { get; set; } = null!;
    }

    /// <summary>
    /// DTO for speaking pace analysis
    /// </summary>
    public class SpeakingPaceDto
    {
        /// <summary>
        /// Average words per minute
        /// </summary>
        public int WordsPerMinute { get; set; }
        
        /// <summary>
        /// Pace consistency score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ConsistencyScore { get; set; }
        
        /// <summary>
        /// Pause analysis
        /// </summary>
        public string PausePattern { get; set; } = null!;
        
        /// <summary>
        /// Overall pace appropriateness (0-100)
        /// </summary>
        [Range(0, 100)]
        public int AppropriatenessScore { get; set; }
    }

    /// <summary>
    /// DTO for behavioral evaluation results
    /// </summary>
    public class BehavioralEvaluationDto
    {
        /// <summary>
        /// The ID of the video interview
        /// </summary>
        public Guid VideoInterviewId { get; set; }
        
        /// <summary>
        /// Communication skills score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int CommunicationSkillsScore { get; set; }
        
        /// <summary>
        /// Professional presence score (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ProfessionalPresenceScore { get; set; }
        
        /// <summary>
        /// Emotional intelligence indicators (0-100)
        /// </summary>
        [Range(0, 100)]
        public int EmotionalIntelligenceScore { get; set; }
        
        /// <summary>
        /// Confidence level assessment (0-100)
        /// </summary>
        [Range(0, 100)]
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Authenticity and genuineness (0-100)
        /// </summary>
        [Range(0, 100)]
        public int AuthenticityScore { get; set; }
        
        /// <summary>
        /// Stress management indicators (0-100)
        /// </summary>
        [Range(0, 100)]
        public int StressManagementScore { get; set; }
        
        /// <summary>
        /// Detailed behavioral feedback
        /// </summary>
        public string DetailedFeedback { get; set; } = null!;
        
        /// <summary>
        /// Key behavioral strengths identified
        /// </summary>
        public List<string> BehavioralStrengths { get; set; } = new();
        
        /// <summary>
        /// Areas for behavioral improvement
        /// </summary>
        public List<string> BehavioralImprovementAreas { get; set; } = new();
        
        /// <summary>
        /// Overall behavioral score (calculated from component scores)
        /// </summary>
        [Range(0, 100)]
        public int OverallBehavioralScore { get; set; }
        
        /// <summary>
        /// Personality traits detected
        /// </summary>
        public List<PersonalityTraitDto> PersonalityTraits { get; set; } = new();
        
        /// <summary>
        /// Evaluation timestamp
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; set; }
    }

    /// <summary>
    /// DTO for personality trait detection
    /// </summary>
    public class PersonalityTraitDto
    {
        /// <summary>
        /// Name of the personality trait
        /// </summary>
        public string TraitName { get; set; } = null!;
        
        /// <summary>
        /// Strength of the trait (0-100)
        /// </summary>
        [Range(0, 100)]
        public int Strength { get; set; }
        
        /// <summary>
        /// Confidence in trait detection (0.0-1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double Confidence { get; set; }
        
        /// <summary>
        /// Description of how the trait manifested
        /// </summary>
        public string Description { get; set; } = null!;
    }
}