using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Exceptions.State;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Middleware
{
    public class CindiClusterMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<CindiClusterMiddleware> _logger;
        private readonly IStateMachine _stateMachine;

        public CindiClusterMiddleware(RequestDelegate next, ILogger<CindiClusterMiddleware> logger,
            IStateMachine stateMachine)
        {
            _next = next;
            _logger = logger;
            _stateMachine = stateMachine;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var method = context.Request.Method;
            if(path.Contains("/api/node"))
            {
                await _next(context);
            }

            else if (_stateMachine.GetState().Initialized || path == "/api/cluster" && context.Request.Method == "POST")
            {
                if ((_stateMachine.EncryptionKey == null && _stateMachine.EncryptionKey == "") && path != "/api/cluster/encryption-key")
                {
                    _logger.LogWarning("Request cancelled as cluster is initialized however required encryption key, please post the key to /cluster/encryption-key ");
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Cluster node is not authenticated.");
                }
                else
                {
                    try
                    {
                        await _next(context);
                    }
                    catch(FailedClusterOperationException e)
                    {
                        context.Response.StatusCode = 409;
                        await context.Response.WriteAsync(e.Message);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Request cancelled as cluster has not been initialized.");
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Cluster has not been initialized");
            }
        }
    }
}
