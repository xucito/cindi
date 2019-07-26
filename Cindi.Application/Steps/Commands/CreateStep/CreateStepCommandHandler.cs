using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
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
        private readonly IStepsRepository _stepsRepository;
        private readonly IStepTemplatesRepository _stepTemplatesRepository;
        private readonly IClusterStateService _clusterStateService;
        private readonly IConsensusCoreNode<CindiClusterState, IBaseRepository> _node;
        public CreateStepCommandHandler(IStepsRepository stepsRepository, 
            IStepTemplatesRepository steptemplatesRepository, 
            IClusterStateService service, 
            IConsensusCoreNode<CindiClusterState, 
                IBaseRepository> node)
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = steptemplatesRepository;
            _clusterStateService = service;
            _node = node;
        }

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(request.StepTemplateId);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.StepTemplateId + " not found.");
            }

            var newStep = resolvedTemplate.GenerateStep(request.StepTemplateId, request.CreatedBy, request.Name, request.Description, request.Inputs, request.Tests, request.StepRefId, request.WorkflowId, ClusterStateService.GetEncryptionKey() );

           /* var createdStepId = await _stepsRepository.InsertStepAsync(
                newStep
                );*/

            var createdWorkflowTemplateId = await _node.Send(new WriteData()
            {
                Data = newStep,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
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
