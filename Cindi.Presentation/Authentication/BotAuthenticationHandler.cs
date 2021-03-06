﻿using Cindi.Application.BotKeys.Commands.Nonce;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ILogger<BotAuthenticationHandler> _logger;
        private Stopwatch stopwatch;
        private IMemoryCache _cache;

        public BotAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMediator mediator,
            IMemoryCache cache)
            : base(options, logger, encoder, clock)
        {
            _mediator = mediator;
            _logger = logger.CreateLogger<BotAuthenticationHandler>();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                stopwatch = new Stopwatch();
            }
            _cache = cache;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                stopwatch.Start();
            }
            var authHeader = Request.Headers["BotKey"];
            var nonceHeaders = Request.Headers["Nonce"];

            if (nonceHeaders.Count() == 0)
            {
                return AuthenticateResult.Fail("No nonce detected");
            }
            var id = authHeader;

            BotKey botkey;

            if (!_cache.TryGetValue(id, out botkey))
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));
                var result = await _mediator.Send(new GetEntityQuery<BotKey>()
                {
                    Expression = bk => bk.Id == new Guid(id)
                });

                if (result.Count == 0)
                {
                    return AuthenticateResult.Fail("Bot " + id.ToString() + " is not valid.");
                }

                botkey = result.Result;
                // Save data in cache.
                _cache.Set(id, botkey, cacheEntryOptions);
            }


            /*double nonce = Double.Parse(SecurityUtility.RsaDecryptWithPublic(nonceHeaders.First(), key.Result.PublicEncryptionKey));

            if (nonce <= key.Result.Nonce)
            {
                return AuthenticateResult.Fail("Nonce check failed.");
            }

            await _mediator.Send(new UpdateNonceCommand()
            {
                Id = key.Result.Id,
                Nonce = nonce
            });*/

            var claims = new[] {
                new Claim("id", botkey.Id.ToString()),
                new Claim("authenticationType", "bot")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                stopwatch.Stop();
                _logger.LogDebug("Bot authentication took " + stopwatch.ElapsedMilliseconds + "ms");
            }

            return AuthenticateResult.Success(ticket);
        }

    }
}
