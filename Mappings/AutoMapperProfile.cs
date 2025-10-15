using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Data.Models;

namespace Dignus.Candidate.Back.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between entities and DTOs
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            ConfigureCandidateMappings();
            ConfigureTestMappings();
            ConfigureQuestionMappings();
            ConfigureEvaluationMappings();
            ConfigureMediaMappings();
            ConfigureJobMappings();
        }

        private void ConfigureCandidateMappings()
        {
            // Candidate mappings
            CreateMap<Data.Models.Candidate, CandidateDto>();
            
            CreateMap<CreateCandidateDto, Data.Models.Candidate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PortugueseTests, opt => opt.Ignore())
                .ForMember(dest => dest.MathTests, opt => opt.Ignore())
                .ForMember(dest => dest.PsychologyTests, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.Evaluation, opt => opt.Ignore())
                .ForMember(dest => dest.Recruiter, opt => opt.Ignore())
                .ForMember(dest => dest.Job, opt => opt.Ignore());

            CreateMap<UpdateCandidateDto, Data.Models.Candidate>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }

        private void ConfigureTestMappings()
        {
            // Base test mappings
            CreateMap<BaseTest, BaseTestDto>()
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Candidate.Id))
                .Include<MathTest, MathTestDto>()
                .Include<PortugueseTest, PortugueseTestDto>()
                .Include<PsychologyTest, PsychologyTestDto>()
                .Include<VisualRetentionTest, VisualRetentionTestDto>();

            CreateMap<BaseTestDto, BaseTest>()
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.AudioSubmission, opt => opt.Ignore())
                .ForMember(dest => dest.VideoSubmission, opt => opt.Ignore())
                .Include<MathTestDto, MathTest>()
                .Include<PortugueseTestDto, PortugueseTest>()
                .Include<PsychologyTestDto, PsychologyTest>()
                .Include<VisualRetentionTestDto, VisualRetentionTest>();

            // Specific test type mappings
            CreateMap<MathTest, MathTestDto>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore()); // Simplified for now

            CreateMap<MathTestDto, MathTest>();

            CreateMap<PortugueseTest, PortugueseTestDto>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore()) // Simplified for now
                .ForMember(dest => dest.AudioSubmission, opt => opt.MapFrom(src => src.AudioSubmission));

            CreateMap<PortugueseTestDto, PortugueseTest>();

            CreateMap<PsychologyTest, PsychologyTestDto>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore()) // Simplified for now
                .ForMember(dest => dest.VideoSubmission, opt => opt.MapFrom(src => src.VideoSubmission));

            CreateMap<PsychologyTestDto, PsychologyTest>();

            CreateMap<VisualRetentionTest, VisualRetentionTestDto>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore()); // Simplified for now

            CreateMap<VisualRetentionTestDto, VisualRetentionTest>();
        }

        private void ConfigureQuestionMappings()
        {
            // Question mappings - Updated for new structured format
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => ParseOptionsFromJson(src.OptionsJson)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AllowMultipleAnswers ? "multi" : "single"))
                .ForMember(dest => dest.MaxSelections, opt => opt.MapFrom(src => src.MaxAnswers));

            CreateMap<QuestionDto, Question>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Prompt))
                .ForMember(dest => dest.OptionsJson, opt => opt.MapFrom(src => SerializeOptionsToJson(src.Options)))
                .ForMember(dest => dest.AllowMultipleAnswers, opt => opt.MapFrom(src => src.Type == "multi"))
                .ForMember(dest => dest.MaxAnswers, opt => opt.MapFrom(src => src.MaxSelections));

            // Question response mappings
            CreateMap<QuestionResponse, QuestionResponseDto>()
                .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.Question.Id))
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Candidate.Id))
                .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Question));

            CreateMap<QuestionResponseDto, QuestionResponse>()
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());

            CreateMap<SubmitAnswerDto, QuestionResponse>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.AnsweredAt, opt => opt.Ignore());
        }

        // Helper methods for options serialization
        private List<QuestionOptionDto> ParseOptionsFromJson(string? optionsJson)
        {
            if (string.IsNullOrWhiteSpace(optionsJson))
                return new List<QuestionOptionDto>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<QuestionOptionDto>>(optionsJson) ?? new List<QuestionOptionDto>();
            }
            catch
            {
                return new List<QuestionOptionDto>();
            }
        }

        private string? SerializeOptionsToJson(List<QuestionOptionDto> options)
        {
            if (options == null || !options.Any())
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(options);
            }
            catch
            {
                return null;
            }
        }

        private void ConfigureEvaluationMappings()
        {
            // Evaluation mappings
            CreateMap<Evaluation, EvaluationDto>()
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Candidate.Id));

            CreateMap<CreateEvaluationDto, Evaluation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.EvaluatedAt, opt => opt.Ignore());

            CreateMap<UpdateEvaluationDto, Evaluation>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }

        private void ConfigureMediaMappings()
        {
            // Audio submission mappings
            CreateMap<AudioSubmission, AudioSubmissionDto>();

            CreateMap<UploadAudioDto, AudioSubmission>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlobUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Test, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore());

            // Video interview mappings
            CreateMap<VideoInterview, VideoInterviewDto>();

            CreateMap<UploadVideoDto, VideoInterview>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlobUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Test, opt => opt.Ignore())
                .ForMember(dest => dest.Score, opt => opt.Ignore())
                .ForMember(dest => dest.Feedback, opt => opt.Ignore())
                .ForMember(dest => dest.Duration, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AnalyzedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Verdict, opt => opt.Ignore())
                .ForMember(dest => dest.InterviewQuestions, opt => opt.Ignore());
        }

        private void ConfigureJobMappings()
        {
            // Job mappings
            CreateMap<Job, JobListingDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.JobTags))
                .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src =>
                    src.PublishedAt.HasValue ? src.PublishedAt.Value.ToString("dd/MM/yyyy") : ""))
                .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src =>
                    src.ExpiresAt.HasValue ? src.ExpiresAt.Value.ToString("dd/MM/yyyy") : ""))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner ?? ""))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src =>
                    DeserializeRequirements(src.Requirements)))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // JobTag mappings
            CreateMap<JobTag, JobTagDto>()
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.Tone, opt => opt.MapFrom(src => src.Tone));

            // JobApplication mappings
            CreateMap<ApplyToJobDto, JobApplication>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.JobId, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalDocuments, opt => opt.MapFrom(src =>
                    SerializeAdditionalDocuments(src.AdditionalDocuments)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.AppliedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Job, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());
        }

        // Helper methods for Job mappings
        private List<string> DeserializeRequirements(string? requirements)
        {
            if (string.IsNullOrEmpty(requirements))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(requirements) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private string? SerializeAdditionalDocuments(List<string> documents)
        {
            if (documents == null || documents.Count == 0)
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(documents);
            }
            catch
            {
                return null;
            }
        }
    }
}