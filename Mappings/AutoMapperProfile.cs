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
            // ConfigureTestMappings(); // Old test types removed
            // ConfigureQuestionMappings(); // Old question types removed
            ConfigureEvaluationMappings();
            ConfigureMediaMappings();
            ConfigureJobMappings();
            // ConfigurePortugueseReadingSystemMappings(); // Old types removed
            // ConfigureMathTestSystemMappings(); // Old types removed
            // ConfigureInterviewMappings(); // Old types removed
        }

        private void ConfigureCandidateMappings()
        {
            // Candidate mappings
            CreateMap<Data.Models.Candidate, CandidateDto>();
            
            CreateMap<CreateCandidateDto, Data.Models.Candidate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.Evaluation, opt => opt.Ignore())
                .ForMember(dest => dest.Recruiter, opt => opt.Ignore())
                .ForMember(dest => dest.Job, opt => opt.Ignore());

            CreateMap<UpdateCandidateDto, Data.Models.Candidate>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
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
            // Legacy audio/video mappings removed - use unified TestVideoResponse instead
            // All media uploads now use TestVideoResponse and TestVideoResponseDto from UnifiedProfile
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