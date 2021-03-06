﻿using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.ExecutionTemplates;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using Cronos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.ExecutionSchedules.Commands.CreateExecutionSchedule
{
    public class CreateExecutionScheduleCommandHandler : IRequestHandler<CreateExecutionScheduleCommand, CommandResult>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IClusterStateService _clusterStateService;
        private readonly IClusterRequestHandler _node;
        private IMediator _mediator;

        public CreateExecutionScheduleCommandHandler(
            IEntitiesRepository entitiesRepository,
            IClusterStateService service,
            IClusterRequestHandler node,
            IMediator mediator)
        {
            _entitiesRepository = entitiesRepository;
            _clusterStateService = service;
            _node = node;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CreateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

            if (schedule != null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            ExecutionTemplate template = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionTemplate>(st => st.Name == request.ExecutionTemplateName);

            if (template == null)
            {
                throw new InvalidExecutionScheduleException("Execution Template with name " + request.ExecutionTemplateName + " is invalid.");
            }

            foreach (var scheduleString in request.Schedule)
            {
                var isValid = SchedulerUtility.IsValidScheduleString(scheduleString);
                if(!isValid)
                {
                    throw new InvalidExecutionScheduleException("Schedule " + scheduleString + " is invalid.");
                }
            }

            var executionSchedule = new ExecutionSchedule(
                    Guid.NewGuid(),
                    request.Name,
                    request.ExecutionTemplateName,
                    request.Description,
                    request.CreatedBy,
                    request.Schedule,
                    SchedulerUtility.NextOccurence(request.Schedule)
                    );

            var executionScheduleResponse = await _node.Handle(new AddShardWriteOperation()
            {
                Data = executionSchedule,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

            if(request.RunImmediately)
            {
                await _mediator.Send(new ExecuteExecutionTemplateCommand()
                {
                    CreatedBy = request.CreatedBy,
                    ExecutionScheduleId = executionSchedule.Id,
                    Name = executionSchedule.ExecutionTemplateName
                });
            }


            stopwatch.Stop();
            return new CommandResult<ExecutionSchedule>()
            {
                ObjectRefId = executionSchedule.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = executionSchedule
            };
        }
    }
}
