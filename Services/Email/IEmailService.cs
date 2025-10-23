namespace Dignus.Candidate.Back.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// Send authentication token email to candidate
    /// </summary>
    Task<bool> SendAuthenticationTokenEmailAsync(
        string toEmail,
        string candidateName,
        string tokenCode);

    /// <summary>
    /// Send welcome email after successful registration
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string candidateName);

    /// <summary>
    /// Send test invitation email
    /// </summary>
    Task<bool> SendTestInvitationEmailAsync(
        string toEmail,
        string candidateName,
        string testType);

    /// <summary>
    /// Send generic email with HTML content
    /// </summary>
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody);
}
