using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cindi.Presentation.Authentication
{
    public class OptionsAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public OptionsAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return AuthenticateResult.NoResult();
        }
    }
}
