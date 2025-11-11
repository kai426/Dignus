using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Dignus.Candidate.Back.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly ISendGridClient _sendGridClient;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Initialize SendGrid client with API key
        if (!string.IsNullOrWhiteSpace(_settings.SendGridApiKey))
        {
            _sendGridClient = new SendGridClient(_settings.SendGridApiKey);
        }
        else
        {
            _logger.LogWarning("SendGrid API key not configured. Email sending will be mocked.");
            _sendGridClient = null!;
        }
    }

    public async Task<bool> SendAuthenticationTokenEmailAsync(
        string toEmail,
        string candidateName,
        string tokenCode)
    {
        _logger.LogInformation("===== AUTH TOKEN CODE: {TokenCode} for {Email} =====", tokenCode, toEmail);
        var subject = "Seu código de verificação - Dignus";

        var htmlBody = $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Código de Verificação</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>Dignus</h1>
        <p style='color: #f0f0f0; margin: 10px 0 0 0;'>Plataforma de Recrutamento</p>
    </div>

    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #667eea; margin-top: 0;'>Olá, {candidateName}!</h2>

        <p>Recebemos uma solicitação de acesso à sua conta. Use o código abaixo para continuar:</p>

        <div style='background: white; border: 2px solid #667eea; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0;'>
            <p style='color: #666; margin: 0 0 10px 0; font-size: 14px;'>Seu código de verificação:</p>
            <h1 style='color: #667eea; margin: 0; font-size: 48px; letter-spacing: 10px; font-family: monospace;'>{tokenCode}</h1>
        </div>

        <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404;'><strong>⚠️ Importante:</strong> Este código expira em <strong>15 minutos</strong>.</p>
        </div>

        <p style='color: #666; font-size: 14px;'>Se você não solicitou este código, ignore este e-mail. Sua conta permanecerá segura.</p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>

        <p style='color: #999; font-size: 12px; text-align: center;'>
            Este é um e-mail automático, por favor não responda.<br>
            © 2025 Dignus. Todos os direitos reservados.
        </p>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string candidateName)
    {
        var subject = "Bem-vindo(a) ao Dignus!";

        var htmlBody = $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bem-vindo</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>Bem-vindo(a)!</h1>
    </div>

    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #667eea;'>Olá, {candidateName}!</h2>

        <p>Estamos muito felizes em tê-lo(a) conosco na plataforma Dignus!</p>

        <p>Aqui você poderá:</p>
        <ul style='color: #555;'>
            <li>Realizar testes de avaliação</li>
            <li>Acompanhar seu processo seletivo</li>
            <li>Receber feedbacks sobre seu desempenho</li>
        </ul>

        <div style='background: #e3f2fd; border-left: 4px solid #2196f3; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; color: #1565c0;'><strong>💡 Dica:</strong> Complete todos os testes com atenção para aumentar suas chances!</p>
        </div>

        <p>Boa sorte!</p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>

        <p style='color: #999; font-size: 12px; text-align: center;'>
            © 2025 Dignus. Todos os direitos reservados.
        </p>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendTestInvitationEmailAsync(
        string toEmail,
        string candidateName,
        string testType)
    {
        var subject = $"Convite para teste: {testType} - Dignus";

        var htmlBody = $@"
<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Convite para Teste</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>Novo Teste Disponível!</h1>
    </div>

    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #667eea;'>Olá, {candidateName}!</h2>

        <p>Um novo teste está disponível para você:</p>

        <div style='background: white; border: 2px solid #667eea; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0;'>
            <h3 style='color: #667eea; margin: 0;'>{testType}</h3>
        </div>

        <p>Acesse a plataforma para iniciar seu teste.</p>

        <div style='text-align: center; margin: 30px 0;'>
            <a href='#' style='background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Acessar Plataforma</a>
        </div>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>

        <p style='color: #999; font-size: 12px; text-align: center;'>
            © 2025 Dignus. Todos os direitos reservados.
        </p>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody)
    {
        // In development or when email is disabled, just log
        if (_settings.UseMockEmail)
        {
            _logger.LogInformation(
                "MOCK EMAIL - To: {Email}, Subject: {Subject}",
                toEmail, subject);
            _logger.LogDebug("Email body: {Body}", htmlBody);
            return true;
        }

        // Validate SendGrid client is available
        if (_sendGridClient == null)
        {
            _logger.LogError("SendGrid client not initialized. Cannot send email to {Email}", toEmail);
            return false;
        }

        try
        {
            var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
            var to = new EmailAddress(toEmail);
            var plainTextContent = StripHtmlTags(htmlBody); // Generate plain text version

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent,
                htmlBody);

            var response = await _sendGridClient.SendEmailAsync(msg);

            // SendGrid returns 202 Accepted for successfully queued emails
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email sent successfully to {Email} via SendGrid. Status: {StatusCode}",
                    toEmail,
                    response.StatusCode);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send email to {Email}. Status: {StatusCode}, Response: {Response}",
                    toEmail,
                    response.StatusCode,
                    responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Strips HTML tags to create a plain text version of the email
    /// </summary>
    private string StripHtmlTags(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Simple HTML tag removal - for production consider using HtmlAgilityPack
        var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        return plainText.Trim();
    }
}

public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// SendGrid API Key for authentication
    /// Get from: https://app.sendgrid.com/settings/api_keys
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// From email address (must be verified in SendGrid)
    /// </summary>
    public string FromEmail { get; set; } = "noreply@dignus.com";

    /// <summary>
    /// From name displayed in emails
    /// </summary>
    public string FromName { get; set; } = "Dignus Platform";

    /// <summary>
    /// Use mock email (log only, don't actually send)
    /// </summary>
    public bool UseMockEmail { get; set; } = true;
}
