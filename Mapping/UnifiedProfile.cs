using AutoMapper;
using Dignus.Data.Models.Core;
using Dignus.Data.Models.Enums;
using Dignus.Data.Models.Specialized;
using Dignus.Candidate.Back.DTOs.Unified;
using System.Text.Json;
using TestStatus = Dignus.Data.Models.TestStatus;

namespace Dignus.Candidate.Back.Mapping;

public class UnifiedProfile : Profile
{
    public UnifiedProfile()
    {
        // TestInstance → TestInstanceDto
        CreateMap<TestInstance, TestInstanceDto>()
            .ForMember(dest => dest.Questions,
                opt => opt.MapFrom(src => src.QuestionSnapshots.OrderBy(q => q.QuestionOrder)))
            .ForMember(dest => dest.VideoResponses,
                opt => opt.MapFrom(src => src.VideoResponses))
            .ForMember(dest => dest.QuestionResponses,
                opt => opt.MapFrom(src => src.QuestionResponses));

        // TestQuestionSnapshot → QuestionSnapshotDto
        CreateMap<TestQuestionSnapshot, QuestionSnapshotDto>();
        // ⚠️ SECURITY: CorrectAnswerSnapshot and ExpectedAnswerGuideSnapshot
        // are NOT properties in QuestionSnapshotDto, so they won't be mapped

        // TestVideoResponse → VideoResponseDto
        CreateMap<TestVideoResponse, VideoResponseDto>();

        // TestQuestionResponse → QuestionResponseDto
        CreateMap<TestQuestionResponse, QuestionResponseDto>()
            .ForMember(dest => dest.SelectedAnswers,
                opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.SelectedAnswersJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(src.SelectedAnswersJson, (JsonSerializerOptions?)null) ?? new List<string>()));

        // PortugueseReadingText → PortugueseReadingTextDto
        CreateMap<PortugueseReadingText, PortugueseReadingTextDto>();

        // QuestionTemplate → QuestionTemplateDto (without answer)
        CreateMap<QuestionTemplate, QuestionTemplateDto>()
            .ForMember(dest => dest.HasAnswer, opt => opt.MapFrom(src => src.Answer != null))
            .ForMember(dest => dest.AnswerLastUpdated, opt => opt.MapFrom(src => src.Answer != null ? (DateTimeOffset?)src.Answer.UpdatedAt : null));

        // QuestionTemplate → QuestionTemplateDetailDto (with answer - admin only)
        CreateMap<QuestionTemplate, QuestionTemplateDetailDto>()
            .ForMember(dest => dest.Answer, opt => opt.MapFrom(src => src.Answer));

        // QuestionAnswer → QuestionAnswerDto
        CreateMap<QuestionAnswer, QuestionAnswerDto>();

        // CreateTestInstanceRequest → TestInstance
        CreateMap<CreateTestInstanceRequest, TestInstance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TestStatus.NotStarted))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Score, opt => opt.Ignore())
            .ForMember(dest => dest.RawScore, opt => opt.Ignore())
            .ForMember(dest => dest.MaxPossibleScore, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DurationSeconds, opt => opt.Ignore())
            .ForMember(dest => dest.TimeLimitSeconds, opt => opt.Ignore())
            .ForMember(dest => dest.TestConfigVersion, opt => opt.Ignore())
            .ForMember(dest => dest.MetadataJson, opt => opt.Ignore())
            .ForMember(dest => dest.PortugueseReadingTextId, opt => opt.Ignore())
            .ForMember(dest => dest.PortugueseReadingTextVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Candidate, opt => opt.Ignore())
            .ForMember(dest => dest.PortugueseReadingText, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionSnapshots, opt => opt.Ignore())
            .ForMember(dest => dest.VideoResponses, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionResponses, opt => opt.Ignore())
            .ForMember(dest => dest.VisualRetentionSelections, opt => opt.Ignore());
    }
}
