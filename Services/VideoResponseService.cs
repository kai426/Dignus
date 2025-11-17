using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Core;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services;

/// <summary>
/// Service for managing video responses with Azure Blob Storage
/// </summary>
public class VideoResponseService : IVideoResponseService
{
    private readonly ITestVideoResponseRepository _videoResponseRepo;
    private readonly ITestInstanceRepository _testInstanceRepo;
    private readonly ITestQuestionSnapshotRepository _questionSnapshotRepo;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _storageSettings;
    private readonly IMapper _mapper;
    private readonly ILogger<VideoResponseService> _logger;
    private readonly IExternalAIAgentService _externalAIAgentService;

    // Video validation constants
    private const long MAX_FILE_SIZE_BYTES = 200 * 1024 * 1024; // 200 MB
    private const int MAX_DURATION_SECONDS = 600; // 10 minutes
    private static readonly HashSet<string> ALLOWED_CONTENT_TYPES = new()
    {
        "video/mp4",
        "video/webm",
        "video/quicktime", // .mov
        "video/x-msvideo" // .avi
    };

    public VideoResponseService(
        ITestVideoResponseRepository videoResponseRepo,
        ITestInstanceRepository testInstanceRepo,
        ITestQuestionSnapshotRepository questionSnapshotRepo,
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageSettings> storageSettings,
        IMapper mapper,
        ILogger<VideoResponseService> logger,
        IExternalAIAgentService externalAIAgentService)
    {
        _videoResponseRepo = videoResponseRepo;
        _testInstanceRepo = testInstanceRepo;
        _questionSnapshotRepo = questionSnapshotRepo;
        _blobServiceClient = blobServiceClient;
        _storageSettings = storageSettings.Value;
        _mapper = mapper;
        _logger = logger;
        _externalAIAgentService = externalAIAgentService;
    }

    public async Task<VideoResponseDto> UploadVideoResponseAsync(UploadVideoRequest request)
    {
        _logger.LogInformation("Uploading video response for test {TestId}, questionSnapshotId {QuestionSnapshotId}",
            request.TestId, request.QuestionSnapshotId);

        // 1. Validate test and ownership
        var test = await _testInstanceRepo.GetByIdAsync(request.TestId);
        if (test == null)
        {
            throw new NotFoundException("TestInstance", request.TestId);
        }

        if (test.CandidateId != request.CandidateId)
        {
            throw new UnauthorizedAccessException($"Test {request.TestId} does not belong to candidate {request.CandidateId}");
        }

        if (test.Status == Data.Models.TestStatus.Submitted)
        {
            throw new InvalidOperationException("Cannot upload videos to a submitted test");
        }

        // 2. Derive QuestionNumber from QuestionSnapshotId if not provided
        int questionNumber = request.QuestionNumber ?? await DeriveQuestionNumberAsync(request.TestId, request.QuestionSnapshotId);

        // 3. Validate video file
        var (isValid, errorMessage) = await ValidateVideoFileAsync(
            request.VideoFile.FileName,
            request.VideoFile.ContentType,
            request.VideoFile.Length);

        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
        }

        // 4. Upload to Azure Blob Storage
        var blobUrl = await UploadToBlobStorageAsync(request.VideoFile, request.TestId, request.CandidateId, questionNumber);

        // 5. Create video response entity
        var videoResponse = new TestVideoResponse
        {
            Id = Guid.NewGuid(),
            TestInstanceId = request.TestId,
            CandidateId = request.CandidateId,
            QuestionSnapshotId = request.QuestionSnapshotId,
            QuestionNumber = questionNumber,
            ResponseType = request.ResponseType, // For Portuguese: Reading or QuestionAnswer
            BlobUrl = blobUrl,
            BlobContainerPath = GetContainerName(request.TestId),
            FileSizeBytes = request.VideoFile.Length,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _videoResponseRepo.AddAsync(videoResponse);
        await _videoResponseRepo.SaveAsync();

        _logger.LogInformation("Video uploaded successfully: {VideoId}", videoResponse.Id);

        // Trigger AI analysis asynchronously (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _externalAIAgentService.SendVideoForAnalysisAsync(videoResponse.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send video {VideoId} to AI agent", videoResponse.Id);
            }
        });

        return _mapper.Map<VideoResponseDto>(videoResponse);
    }

    public async Task<List<VideoResponseDto>> GetTestVideoResponsesAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdAsync(testId);
        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        var videoResponses = await _videoResponseRepo.GetByTestIdAsync(testId);
        return _mapper.Map<List<VideoResponseDto>>(videoResponses);
    }

    public async Task<VideoResponseDto> GetVideoResponseAsync(Guid videoResponseId, Guid candidateId)
    {
        var videoResponse = await _videoResponseRepo.GetByIdAsync(videoResponseId);
        if (videoResponse == null)
        {
            throw new NotFoundException("VideoResponse", videoResponseId);
        }

        if (videoResponse.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Video response {videoResponseId} does not belong to candidate {candidateId}");
        }

        return _mapper.Map<VideoResponseDto>(videoResponse);
    }

    public async Task DeleteVideoResponseAsync(Guid videoResponseId, Guid candidateId)
    {
        var videoResponse = await _videoResponseRepo.GetByIdAsync(videoResponseId);
        if (videoResponse == null)
        {
            throw new NotFoundException("VideoResponse", videoResponseId);
        }

        if (videoResponse.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Video response {videoResponseId} does not belong to candidate {candidateId}");
        }

        // Check if test is still in progress (cannot delete videos after submission)
        var test = await _testInstanceRepo.GetByIdAsync(videoResponse.TestInstanceId);
        if (test?.Status == Data.Models.TestStatus.Submitted)
        {
            throw new InvalidOperationException("Cannot delete videos from a submitted test");
        }

        // Delete from Azure Blob Storage
        try
        {
            await DeleteFromBlobStorageAsync(videoResponse.BlobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob {BlobUrl}, continuing with database deletion", videoResponse.BlobUrl);
        }

        // Delete from database
        await _videoResponseRepo.DeleteAsync(videoResponseId);
        await _videoResponseRepo.SaveAsync();

        _logger.LogInformation("Video response {VideoId} deleted", videoResponseId);
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateVideoFileAsync(string fileName, string contentType, long fileSizeBytes)
    {
        // Check file size
        if (fileSizeBytes > MAX_FILE_SIZE_BYTES)
        {
            return Task.FromResult<(bool, string)>((false, $"File size ({fileSizeBytes / 1024 / 1024} MB) exceeds maximum allowed size ({MAX_FILE_SIZE_BYTES / 1024 / 1024} MB)"));
        }

        if (fileSizeBytes == 0)
        {
            return Task.FromResult<(bool, string)>((false, "File is empty"));
        }

        // Check content type
        if (!ALLOWED_CONTENT_TYPES.Contains(contentType.ToLowerInvariant()))
        {
            return Task.FromResult<(bool, string)>((false, $"File type '{contentType}' is not allowed. Allowed types: {string.Join(", ", ALLOWED_CONTENT_TYPES)}"));
        }

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
        if (!allowedExtensions.Contains(extension))
        {
            return Task.FromResult<(bool, string)>((false, $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}"));
        }

        return Task.FromResult<(bool, string)>((true, string.Empty));
    }

    public async Task<string> GetSecureVideoUrlAsync(Guid videoResponseId, Guid candidateId, int expirationMinutes = 60)
    {
        var videoResponse = await _videoResponseRepo.GetByIdAsync(videoResponseId);
        if (videoResponse == null)
        {
            throw new NotFoundException("VideoResponse", videoResponseId);
        }

        if (videoResponse.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Video response {videoResponseId} does not belong to candidate {candidateId}");
        }

        // Generate SAS token for temporary access
        var blobName = GetBlobNameFromUrl(videoResponse.BlobUrl);
        var containerClient = _blobServiceClient.GetBlobContainerClient(videoResponse.BlobContainerPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            // Fallback to the stored URL if SAS generation is not available
            _logger.LogWarning("Cannot generate SAS URI, returning stored blob URL");
            return videoResponse.BlobUrl;
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = videoResponse.BlobContainerPath,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    #region Private Helper Methods

    private async Task<int> DeriveQuestionNumberAsync(Guid testId, Guid? questionSnapshotId)
    {
        if (questionSnapshotId == null)
        {
            // If no question snapshot ID provided, generate a sequential number based on existing videos
            var existingVideos = await _videoResponseRepo.GetByTestIdAsync(testId);
            return existingVideos.Count + 1;
        }

        // Look up the question snapshot to get its order
        var questionSnapshot = await _questionSnapshotRepo.GetByIdAsync(questionSnapshotId.Value);
        if (questionSnapshot == null)
        {
            throw new NotFoundException("QuestionSnapshot", questionSnapshotId.Value);
        }

        if (questionSnapshot.TestInstanceId != testId)
        {
            throw new InvalidOperationException($"Question snapshot {questionSnapshotId} does not belong to test {testId}");
        }

        return questionSnapshot.QuestionOrder;
    }

    private async Task<string> UploadToBlobStorageAsync(IFormFile file, Guid testId, Guid candidateId, int questionNumber)
    {
        var containerName = GetContainerName(testId);
        var blobName = GenerateBlobName(candidateId, testId, questionNumber, Path.GetExtension(file.FileName));

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return blobClient.Uri.ToString();
    }

    private async Task DeleteFromBlobStorageAsync(string blobUrl)
    {
        var uri = new Uri(blobUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        var containerName = Path.GetFileName(Path.GetDirectoryName(uri.LocalPath));

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    private string GetContainerName(Guid testId)
    {
        return $"test-videos-{testId}".ToLowerInvariant();
    }

    private string GenerateBlobName(Guid candidateId, Guid testId, int questionNumber, string extension)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"candidate-{candidateId}/test-{testId}/q{questionNumber}_{timestamp}{extension}";
    }

    private string GetBlobNameFromUrl(string blobUrl)
    {
        var uri = new Uri(blobUrl);
        return Path.GetFileName(uri.LocalPath);
    }

    #endregion
}
