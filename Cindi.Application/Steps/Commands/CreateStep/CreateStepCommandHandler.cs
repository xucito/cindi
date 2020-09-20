using Cindi.Application.Entities.Command.CreateTrackedEntity;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.CreateStep
{
    public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, CommandResult<Step>>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IClusterStateService _clusterStateService;
        private readonly IClusterRequestHandler _node;
        private readonly IMediator _mediator;
        public CreateStepCommandHandler(
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

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await  _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == request.StepTemplateId);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.StepTemplateId + " not found.");
            }

            var newStep = resolvedTemplate.GenerateStep(request.StepTemplateId, 
                request.CreatedBy, 
                request.Name, request.Description, 
                request.Inputs, 
                request.Tests, request.WorkflowId, 
                ClusterStateService.GetEncryptionKey(),
                request.ExecutionTemplateId,
                request.ExecutionScheduleId);

            /* var createdStepId = await _entitiesRepository.InsertStepAsync(
                 newStep
                 );*/


            await _mediator.Send(new WriteEntityCommand<Step>()
            {
                Data = newStep,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                User = request.CreatedBy
            });


            stopwatch.Stop();
            return new CommandResult<Step>()
            {
                ObjectRefId = newStep.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = newStep
            };
        }
    }
}
