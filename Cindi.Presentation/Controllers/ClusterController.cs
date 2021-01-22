using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Cluster.Commands;
using Cindi.Application.Cluster.Commands.InitializeCluster;
using Cindi.Application.Cluster.Commands.SetEncryptionKey;
using Cindi.Application.Cluster.Commands.UpdateClusterState;
using Cindi.Application.Cluster.Queries;
using Cindi.Application.Cluster.Queries.GetClusterStats;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Presentation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class ClusterController : BaseController
    {
        private readonly IStateMachine _stateMachine;
        public ClusterController(ILoggerFactory logger,
            IStateMachine stateMachine) : base(logger.CreateLogger<ClusterController>())
        {
            _stateMachine = stateMachine;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> InitializeCluster(InitializeClusterCommand command)
        {
            if (_stateMachine.GetState().Initialized == false)
            {
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<NewClusterResult>("", result, result.Result));
            }
            else
            {
                return BadRequest("Cluster is already initialized");
            }
        }

        [HttpPut("encryption-key")]
        public async Task<IActionResult> RegisterKey(SetEncryptionKeyCommand command)
        {
            try
            {
                await Mediator.Send(command);

                return Ok();

            }
            catch (InvalidPrivateKeyException e)
            {
                return BadRequest("Key is not matching...");
            }
        }

        [HttpPut("settings")]
        public async Task<IActionResult> Create(UpdateClusterStateCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<CindiClusterState>("clusterstate", result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet("state")]
        public async Task<IActionResult> Get()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                return Ok(_stateMachine.GetState());
                ///return Ok(await Mediator.Send(new GetClusterStateQuery()));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                return Ok(await Mediator.Send(new GetClusterStatsQuery()));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }
    }
}
