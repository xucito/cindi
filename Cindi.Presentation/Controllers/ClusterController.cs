﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Cluster.Commands;
using Cindi.Application.Cluster.Commands.UpdateClusterState;
using Cindi.Application.Cluster.Queries;
using Cindi.Application.Cluster.Queries.GetClusterState;
using Cindi.Application.Cluster.Queries.GetClusterStats;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class ClusterController : BaseController
    {
        public ClusterController(ILoggerFactory logger) : base(logger.CreateLogger<SequencesController>())
        {
        }

        [HttpPut("state")]
        public async Task<IActionResult> Create(UpdateClusterStateCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<ClusterState>("clusterstate", result, null));
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
                return Ok(await Mediator.Send(new GetClusterStateQuery()));
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
