using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AutoMapper;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for media file management operations
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AzureStorageSettings _storageSettings;
        private readonly MediaUploadSettings _uploadSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<MediaService> _logger;

        public MediaService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<AzureStorageSettings> storageSettings,
            IOptions<MediaUploadSettings> uploadSettings,
            BlobServiceClient blobServiceClient,
            ILogger<MediaService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _storageSettings = storageSettings.Value;
            _uploadSettings = uploadSettings.Value;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<AudioSubmissionDto> UploadAudioAsync(UploadAudioDto uploadAudioDto)
        {
            try
            {
                _logger.LogInformation("Starting audio upload for test {TestId}", uploadAudioDto.TestId);

                // Validate file
                if (!await IsValidAudioFileAsync(uploadAudioDto.File))
                {
                    throw new ArgumentException("Invalid audio file format or size");
                }

                // Check if test exists - need to check different test types
                BaseTest? test = null;
                
                // Try Portuguese test first
                test = await _unitOfWork.PortugueseTests.GetByIdAsync(uploadAudioDto.TestId);
                
                // If not found, try other test types
                if (test == null)
                {
                    test = await _unitOfWork.MathTests.GetByIdAsync(uploadAudioDto.TestId);
                }
                
                if (test == null)
                {
                    test = await _unitOfWork.PsychologyTests.GetByIdAsync(uploadAudioDto.TestId);
                }
                
                if (test == null)
                {
                    throw new ArgumentException("Test not found");
                }

                // Upload to Azure Storage
                var blobUrl = await UploadToBlobStorageAsync(
                    uploadAudioDto.File, 
                    _storageSettings.ContainerNames.Audio,
                    $"audio/{test.Candidate?.Id}/{uploadAudioDto.TestId}"
                );

                // Create audio submission record
                var audioSubmission = new AudioSubmission
                {
                    Id = Guid.NewGuid(),
                    BlobUrl = blobUrl,
                    Type = uploadAudioDto.Type,
                    SubmittedAt = DateTimeOffset.UtcNow,
                    TestId = uploadAudioDto.TestId,
                    Test = test
                };

                await _unitOfWork.AudioSubmissions.AddAsync(audioSubmission);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audio upload completed successfully for test {TestId}", uploadAudioDto.TestId);

                return _mapper.Map<AudioSubmissionDto>(audioSubmission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading audio for test {TestId}", uploadAudioDto.TestId);
                throw;
            }
        }

        public async Task<VideoInterviewDto> UploadVideoAsync(UploadVideoDto uploadVideoDto)
        {
            try
            {
                _logger.LogInformation("Starting video upload for test {TestId}", uploadVideoDto.TestId);

                // Validate file
                if (!await IsValidVideoFileAsync(uploadVideoDto.File))
                {
                    throw new ArgumentException("Invalid video file format or size");
                }

                // Check if test exists - need to check different test types
                BaseTest? test = null;
                
                // Try Portuguese test first
                test = await _unitOfWork.PortugueseTests.GetByIdAsync(uploadVideoDto.TestId);
                
                // If not found, try other test types
                if (test == null)
                {
                    test = await _unitOfWork.MathTests.GetByIdAsync(uploadVideoDto.TestId);
                }
                
                if (test == null)
                {
                    test = await _unitOfWork.PsychologyTests.GetByIdAsync(uploadVideoDto.TestId);
                }
                
                if (test == null)
                {
                    throw new ArgumentException("Test not found");
                }

                // Upload to Azure Storage
                var blobUrl = await UploadToBlobStorageAsync(
                    uploadVideoDto.File,
                    _storageSettings.ContainerNames.Video,
                    $"video/{test.Candidate?.Id}/{uploadVideoDto.TestId}"
                );

                // Create video interview record
                var videoInterview = new VideoInterview
                {
                    Id = Guid.NewGuid(),
                    BlobUrl = blobUrl,
                    Status = TestStatus.NotStarted,
                    SubmittedAt = DateTimeOffset.UtcNow,
                    Test = test
                };

                await _unitOfWork.VideoInterviews.AddAsync(videoInterview);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Video upload completed successfully for test {TestId}", uploadVideoDto.TestId);

                return _mapper.Map<VideoInterviewDto>(videoInterview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video for test {TestId}", uploadVideoDto.TestId);
                throw;
            }
        }

        public async Task<IEnumerable<AudioSubmissionDto>> GetAudioSubmissionsByCandidateIdAsync(Guid candidateId)
        {
            try
            {
                var audioSubmissions = await _unitOfWork.AudioSubmissions.GetByCandidateIdAsync(candidateId);
                return _mapper.Map<IEnumerable<AudioSubmissionDto>>(audioSubmissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audio submissions for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<IEnumerable<VideoInterviewDto>> GetVideoInterviewsByCandidateIdAsync(Guid candidateId)
        {
            try
            {
                var videoInterviews = await _unitOfWork.VideoInterviews.GetByCandidateIdAsync(candidateId);
                return _mapper.Map<IEnumerable<VideoInterviewDto>>(videoInterviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video interviews for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<AudioSubmissionDto?> GetAudioSubmissionByIdAsync(Guid audioId, Guid candidateId)
        {
            try
            {
                var audioSubmission = await _unitOfWork.AudioSubmissions.GetByIdAsync(audioId);
                
                if (audioSubmission == null || audioSubmission.Test?.Candidate?.Id != candidateId)
                {
                    return null;
                }

                return _mapper.Map<AudioSubmissionDto>(audioSubmission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audio submission {AudioId} for candidate {CandidateId}", audioId, candidateId);
                throw;
            }
        }

        public async Task<VideoInterviewDto?> GetVideoInterviewByIdAsync(Guid videoId, Guid candidateId)
        {
            try
            {
                var videoInterview = await _unitOfWork.VideoInterviews.GetByIdAsync(videoId);
                
                if (videoInterview == null || videoInterview.Test?.Candidate?.Id != candidateId)
                {
                    return null;
                }

                return _mapper.Map<VideoInterviewDto>(videoInterview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video interview {VideoId} for candidate {CandidateId}", videoId, candidateId);
                throw;
            }
        }

        public async Task<bool> DeleteAudioSubmissionAsync(Guid audioId, Guid candidateId)
        {
            try
            {
                var audioSubmission = await _unitOfWork.AudioSubmissions.GetByIdAsync(audioId);
                
                if (audioSubmission == null || audioSubmission.Test?.Candidate?.Id != candidateId)
                {
                    return false;
                }

                // Delete from blob storage
                await DeleteFromBlobStorageAsync(audioSubmission.BlobUrl);

                // Delete from database
                _unitOfWork.AudioSubmissions.Remove(audioSubmission);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audio submission {AudioId} deleted successfully", audioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audio submission {AudioId} for candidate {CandidateId}", audioId, candidateId);
                return false;
            }
        }

        public async Task<bool> DeleteVideoInterviewAsync(Guid videoId, Guid candidateId)
        {
            try
            {
                var videoInterview = await _unitOfWork.VideoInterviews.GetByIdAsync(videoId);
                
                if (videoInterview == null || videoInterview.Test?.Candidate?.Id != candidateId)
                {
                    return false;
                }

                // Delete from blob storage
                await DeleteFromBlobStorageAsync(videoInterview.BlobUrl);

                // Delete from database
                _unitOfWork.VideoInterviews.Remove(videoInterview);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Video interview {VideoId} deleted successfully", videoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video interview {VideoId} for candidate {CandidateId}", videoId, candidateId);
                return false;
            }
        }

        public Task<bool> IsValidAudioFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            // Check file size (convert MB to bytes)
            var maxSizeBytes = _uploadSettings.MaxFileSizes.AudioMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
                return Task.FromResult(false);

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_uploadSettings.AllowedExtensions.Audio.Contains(extension))
                return Task.FromResult(false);

            // Check MIME type
            if (!_uploadSettings.AllowedMimeTypes.Audio.Contains(file.ContentType))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<bool> IsValidVideoFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            // Check file size (convert MB to bytes)
            var maxSizeBytes = _uploadSettings.MaxFileSizes.VideoMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
                return Task.FromResult(false);

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_uploadSettings.AllowedExtensions.Video.Contains(extension))
                return Task.FromResult(false);

            // Check MIME type
            if (!_uploadSettings.AllowedMimeTypes.Video.Contains(file.ContentType))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public long GetMaxAudioFileSize()
        {
            return _uploadSettings.MaxFileSizes.AudioMB * 1024 * 1024;
        }

        public long GetMaxVideoFileSize()
        {
            return _uploadSettings.MaxFileSizes.VideoMB * 1024 * 1024;
        }

        public async Task<bool> UpdateAudioTranscriptionAsync(Guid audioId, string transcription)
        {
            try
            {
                var audioSubmission = await _unitOfWork.AudioSubmissions.GetByIdAsync(audioId);
                if (audioSubmission == null)
                    return false;

                // Note: AudioSubmission model doesn't have Transcription or ProcessedAt properties
                // This functionality might need to be added to the model or handled differently
                
                _unitOfWork.AudioSubmissions.Update(audioSubmission);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Audio transcription updated for submission {AudioId}", audioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transcription for audio submission {AudioId}", audioId);
                return false;
            }
        }

        public async Task<bool> UpdateVideoAnalysisAsync(Guid videoId, string? transcription, string? behavioralAnalysis, decimal? communicationScore)
        {
            try
            {
                var videoInterview = await _unitOfWork.VideoInterviews.GetByIdAsync(videoId);
                if (videoInterview == null)
                    return false;

                // Note: VideoInterview model doesn't have Transcription, BehavioralAnalysis, or CommunicationScore properties
                // This functionality might need to be added to the model or handled differently

                videoInterview.AnalyzedAt = DateTimeOffset.UtcNow;

                _unitOfWork.VideoInterviews.Update(videoInterview);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Video analysis updated for interview {VideoId}", videoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating analysis for video interview {VideoId}", videoId);
                return false;
            }
        }

        public async Task<object> UploadMathQuestionVideoAsync(Guid testId, int questionNumber, Guid candidateId, IFormFile videoFile)
        {
            try
            {
                _logger.LogInformation("Starting Math test video upload for test {TestId}, question {QuestionNumber}, candidate {CandidateId}",
                    testId, questionNumber, candidateId);

                // Validate question number
                if (questionNumber < 1 || questionNumber > 2)
                {
                    throw new ArgumentException("Question number must be 1 or 2 for Math tests");
                }

                // Validate file
                if (!await IsValidVideoFileAsync(videoFile))
                {
                    throw new ArgumentException("Invalid video file format or size");
                }

                // Get the Math test and verify it belongs to the candidate
                var mathTest = await _unitOfWork.MathTests.GetByIdAsync(testId);
                if (mathTest == null)
                {
                    throw new ArgumentException("Math test not found");
                }

                if (mathTest.Candidate?.Id != candidateId)
                {
                    throw new UnauthorizedAccessException("Test does not belong to the specified candidate");
                }

                // Check if video already exists for this question
                var existingVideos = await _unitOfWork.VideoInterviews.GetByCandidateIdAsync(candidateId);
                var existingQuestionVideo = existingVideos?.FirstOrDefault(v =>
                    v.Test.Id == testId &&
                    v.QuestionNumber == questionNumber);

                if (existingQuestionVideo != null)
                {
                    throw new InvalidOperationException($"Video for question {questionNumber} already exists");
                }

                // Upload to Azure Storage
                var blobUrl = await UploadToBlobStorageAsync(
                    videoFile,
                    _storageSettings.ContainerNames.Video,
                    $"math-questions/{candidateId}/{testId}/question-{questionNumber}"
                );

                // Create video interview record with Math test specific properties
                var videoInterview = new VideoInterview
                {
                    Id = Guid.NewGuid(),
                    BlobUrl = blobUrl,
                    Status = TestStatus.Submitted,
                    SubmittedAt = DateTimeOffset.UtcNow,
                    Test = mathTest,
                    QuestionNumber = questionNumber,
                    TestType = "math",
                    Duration = null // Will be calculated later if needed
                };

                await _unitOfWork.VideoInterviews.AddAsync(videoInterview);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Math test video uploaded successfully for test {TestId}, question {QuestionNumber}",
                    testId, questionNumber);

                return new
                {
                    Id = videoInterview.Id,
                    TestId = testId,
                    QuestionNumber = questionNumber,
                    BlobUrl = blobUrl,
                    Status = videoInterview.Status.ToString(),
                    SubmittedAt = videoInterview.SubmittedAt,
                    TestType = videoInterview.TestType,
                    Message = $"Video uploaded successfully for Math test question {questionNumber}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading Math test video for test {TestId}, question {QuestionNumber}, candidate {CandidateId}",
                    testId, questionNumber, candidateId);
                throw;
            }
        }

        private async Task<string> UploadToBlobStorageAsync(IFormFile file, string containerName, string blobPrefix)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            
            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Generate unique blob name
            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{blobPrefix}/{Guid.NewGuid()}{extension}";
            
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set blob HTTP headers and metadata
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            var metadata = new Dictionary<string, string>
            {
                ["OriginalFileName"] = file.FileName,
                ["UploadedAt"] = DateTimeOffset.UtcNow.ToString("O")
            };

            // Upload file
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata
            });

            return blobClient.Uri.ToString();
        }

        private async Task DeleteFromBlobStorageAsync(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                var blobClient = new BlobClient(uri);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete blob from storage: {BlobUrl}", blobUrl);
            }
        }
    }
}