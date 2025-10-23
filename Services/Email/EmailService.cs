using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendAuthenticationTokenEmailAsync(
        string toEmail,
        string candidateName,
        string tokenCode)
    {
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

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }
}

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@dignus.com";
    public string FromName { get; set; } = "Dignus Platform";
    public bool UseMockEmail { get; set; } = true;
}
