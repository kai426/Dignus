using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for video interview operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // TEMPORARILY DISABLED
    [Produces("application/json")]
    public class InterviewController : ControllerBase
    {
        private readonly ILogger<InterviewController> _logger;

        public InterviewController(ILogger<InterviewController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get interview questions for video recording
        /// </summary>
        /// <returns>List of interview questions</returns>
        [HttpGet("questions")]
        public ActionResult<IEnumerable<InterviewQuestionDto>> GetInterviewQuestions()
        {
            try
            {
                // Standard interview questions for all candidates
                var questions = new List<InterviewQuestionDto>
                {
                    new InterviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Conte sobre você, sua experiência profissional e porque está interessado nesta posição.",
                        Order = 1,
                        MaxDurationSeconds = 180
                    },
                    new InterviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Descreva uma situação desafiadora que enfrentou no trabalho e como a resolveu.",
                        Order = 2,
                        MaxDurationSeconds = 180
                    },
                    new InterviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Quais são seus principais pontos fortes e como eles contribuirão para nossa equipe?",
                        Order = 3,
                        MaxDurationSeconds = 120
                    },
                    new InterviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Onde você se vê profissionalmente em 5 anos? Quais são seus objetivos de carreira?",
                        Order = 4,
                        MaxDurationSeconds = 120
                    },
                    new InterviewQuestionDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Por que devemos escolher você para esta posição? O que diferencia você dos outros candidatos?",
                        Order = 5,
                        MaxDurationSeconds = 120
                    }
                };

                _logger.LogInformation("Retrieved {Count} interview questions", questions.Count);

                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interview questions");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get interview configuration settings
        /// </summary>
        /// <returns>Interview configuration</returns>
        [HttpGet("config")]
        public ActionResult<InterviewConfigDto> GetInterviewConfig()
        {
            try
            {
                var config = new InterviewConfigDto
                {
                    MaxTotalDurationMinutes = 20,
                    MaxQuestionDurationMinutes = 3,
                    MinVideoQuality = "720p",
                    RequiredQuestions = 5,
                    Instructions = new List<string>
                    {
                        "Clique em Iniciar gravação para iniciar sua câmera e microfone.",
                        "Aguarde que começar as perguntas antes de gravação e você terá 3-10 minutos para gravar seu vídeo.",
                        "Ao finalizar, clique em Encerrar gravação para salvar seu vídeo."
                    }
                };

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interview config");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}