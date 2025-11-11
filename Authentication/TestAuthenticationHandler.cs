using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Dignus.Candidate.Back.Authentication
{
    /// <summary>
    /// No-op authentication handler for development mode
    /// When DisableAuthentication is set to true, this handler does nothing,
    /// leaving all requests as unauthenticated
    /// </summary>
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Return NoResult to leave the request unauthenticated
            // This allows the application to function without any authentication checks
            Logger.LogDebug("Development mode: No authentication required");
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}