using Dignus.Candidate.Back.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Dignus.Candidate.Back.Services.Interfaces
{
    /// <summary>
    /// Service interface for report generation operations
    /// </summary>
    public interface IReportGenerationService
    {
        /// <summary>
        /// Generates comprehensive evaluation report for a candidate
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="reportType">Type of report to generate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated evaluation report</returns>
        Task<EvaluationReportDto> GenerateReportAsync(Guid candidateId, ReportType reportType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates batch reports for multiple candidates
        /// </summary>
        /// <param name="candidateIds">List of candidate identifiers</param>
        /// <param name="reportType">Type of report to generate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of generated reports</returns>
        Task<List<EvaluationReportDto>> GenerateBatchReportsAsync(List<Guid> candidateIds, ReportType reportType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates benchmark comparison report
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="position">Position to benchmark against</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Benchmark comparison data</returns>
        Task<BenchmarkComparisonDto> GenerateBenchmarkComparisonAsync(Guid candidateId, string position, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports report to specified format (PDF, Excel, etc.)
        /// </summary>
        /// <param name="reportId">Report identifier</param>
        /// <param name="format">Export format</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Export result with file information</returns>
        Task<ExportResultDto> ExportReportAsync(Guid reportId, ExportFormat format, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets list of available report templates
        /// </summary>
        /// <param name="reportType">Filter by report type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available templates</returns>
        Task<List<ReportTemplateDto>> GetAvailableTemplatesAsync(ReportType? reportType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates executive summary for recruiters
        /// </summary>
        /// <param name="candidateId">Candidate identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Executive summary text</returns>
        Task<string> GenerateExecutiveSummaryAsync(Guid candidateId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Schedules periodic report generation
        /// </summary>
        /// <param name="scheduleRequest">Schedule configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Schedule confirmation</returns>
        Task<ReportScheduleDto> ScheduleReportAsync(ReportScheduleRequestDto scheduleRequest, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO for export result information
    /// </summary>
    public class ExportResultDto
    {
        /// <summary>
        /// Export identifier
        /// </summary>
        public Guid ExportId { get; set; }

        /// <summary>
        /// File name of exported report
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Export format used
        /// </summary>
        public ExportFormat Format { get; set; }

        /// <summary>
        /// Download URL for the exported file
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Expiration time for the download link
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Export completion timestamp
        /// </summary>
        public DateTimeOffset ExportedAt { get; set; }
    }

    /// <summary>
    /// DTO for report template information
    /// </summary>
    public class ReportTemplateDto
    {
        /// <summary>
        /// Template identifier
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Report type this template is for
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Template categories/tags
        /// </summary>
        public List<string> Categories { get; set; } = new();

        /// <summary>
        /// Whether this template is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Template creation date
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for report scheduling
    /// </summary>
    public class ReportScheduleDto
    {
        /// <summary>
        /// Schedule identifier
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Schedule name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cron expression for scheduling
        /// </summary>
        public string CronExpression { get; set; } = string.Empty;

        /// <summary>
        /// Report type to generate
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Recipients for the scheduled reports
        /// </summary>
        public List<string> Recipients { get; set; } = new();

        /// <summary>
        /// Whether the schedule is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Next run time
        /// </summary>
        public DateTimeOffset? NextRunTime { get; set; }

        /// <summary>
        /// Last run time
        /// </summary>
        public DateTimeOffset? LastRunTime { get; set; }
    }

    /// <summary>
    /// DTO for report schedule request
    /// </summary>
    public class ReportScheduleRequestDto
    {
        /// <summary>
        /// Schedule name
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cron expression for scheduling
        /// </summary>
        [Required]
        public string CronExpression { get; set; } = string.Empty;

        /// <summary>
        /// Report type to generate
        /// </summary>
        public ReportType ReportType { get; set; }

        /// <summary>
        /// Filter criteria for candidates to include
        /// </summary>
        public EvaluationFilterDto? FilterCriteria { get; set; }

        /// <summary>
        /// Recipients for the scheduled reports
        /// </summary>
        public List<string> Recipients { get; set; } = new();

        /// <summary>
        /// Export format for scheduled reports
        /// </summary>
        public ExportFormat ExportFormat { get; set; } = ExportFormat.PDF;
    }

    /// <summary>
    /// Export format enumeration
    /// </summary>
    public enum ExportFormat
    {
        PDF,
        Excel,
        CSV,
        JSON,
        Word
    }
}