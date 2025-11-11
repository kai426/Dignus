using Dignus.Candidate.Back.DTOs.Admin;

namespace Dignus.Candidate.Back.Services.Admin;

/// <summary>
/// Service for managing Portuguese reading texts with questions
/// ⚠️ SECURITY: All methods should be restricted to admin/recruiter roles
/// </summary>
public interface IPortugueseContentService
{
    /// <summary>
    /// Create a Portuguese reading text with its associated questions
    /// </summary>
    /// <param name="request">Portuguese content creation request</param>
    /// <returns>Created Portuguese content with questions</returns>
    Task<PortugueseContentDto> CreatePortugueseContentAsync(CreatePortugueseContentRequest request);

    /// <summary>
    /// Get a Portuguese reading text with its questions
    /// </summary>
    /// <param name="readingTextId">Reading text ID</param>
    /// <returns>Portuguese content with questions</returns>
    Task<PortugueseContentDto> GetPortugueseContentAsync(Guid readingTextId);

    /// <summary>
    /// Get all Portuguese reading texts (paginated)
    /// </summary>
    /// <param name="includeInactive">Include deactivated texts</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>List of Portuguese content</returns>
    Task<List<PortugueseContentDto>> GetAllPortugueseContentAsync(
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Deactivate a Portuguese reading text and its questions
    /// </summary>
    /// <param name="readingTextId">Reading text ID</param>
    Task DeactivatePortugueseContentAsync(Guid readingTextId);
}
