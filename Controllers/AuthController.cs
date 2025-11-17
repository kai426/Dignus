using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;

namespace Dignus.Candidate.Back.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            ICandidateService candidateService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _candidateService = candidateService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticate candidate using CPF
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and candidate information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Find candidate by CPF
                var candidate = await _candidateService.GetCandidateByCpfAsync(loginDto.Cpf);
                if (candidate == null)
                {
                    _logger.LogWarning("Login attempt with invalid CPF: {Cpf}", loginDto.Cpf);
                    return Unauthorized("Invalid CPF");
                }

                // Generate JWT token
                var token = GenerateJwtToken(candidate);
                var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

                var response = new LoginResponseDto
                {
                    Token = token,
                    Candidate = candidate,
                    ExpiresAt = expiresAt
                };

                _logger.LogInformation("Successful login for candidate {CandidateId}", candidate.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for CPF: {Cpf}", loginDto.Cpf);
                return StatusCode(500, "Internal server error");
            }
        }

        // NOTE: Progress endpoint has been moved to v2 API
        // Use: GET /api/v2/tests/candidate/{candidateId}/progress
        // The old endpoint at /api/auth/progress/{candidateId} has been removed
        // as it returned mock data instead of real test progress.

        private string GenerateJwtToken(CandidateDto candidate)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, candidate.Id.ToString()),
                new Claim("candidateId", candidate.Id.ToString()),
                new Claim("name", candidate.Name),
                new Claim("cpf", candidate.Cpf),
                new Claim("role", "candidate"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440");
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}