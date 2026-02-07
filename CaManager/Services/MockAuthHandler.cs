using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

#if DEBUG
#pragma warning disable CS0618 // Type or member is obsolete
namespace CaManager.Services
{
    public class MockAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public MockAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, "Mock User"),
                new Claim(ClaimTypes.NameIdentifier, "mock-user-id")
            };
            var identity = new ClaimsIdentity(claims, "Mock");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Mock");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
#endif
