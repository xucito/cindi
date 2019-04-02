using Cindi.Application.BotKeys.Queries.GetBotKey;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cindi.Presentation.Authentication
{
    public class BotAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IMediator _mediator;

        public BotAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMediator mediator)
            : base(options, logger, encoder, clock)
        {
            _mediator = mediator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var id = Convert.FromBase64String(authHeader.Parameter);
            var key = _mediator.Send(new GetBotKeyQuery()
            {
                Id = new Guid(id)
            }).GetAwaiter().GetResult();

            if (key.Count == 0)
            {
                return AuthenticateResult.Fail("Bot Id is not valid.");
            }
            else
            {
                var claims = new[] {
                new Claim("botId", key.Result.Id.ToString())
            };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }

        }
    }
}
