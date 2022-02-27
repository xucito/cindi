using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Utilities;

namespace Cindi.Application.Steps.Commands.CreateStep
{
    public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, CommandResult<Step>>
    {
        private readonly IClusterStateService _clusterStateService;
        private readonly ElasticClient _context;
        public CreateStepCommandHandler(
            IClusterStateService service, 
            ElasticClient context)
        {
            _clusterStateService = service;
            _context = context;
        }

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await  _context.FirstOrDefaultAsync<StepTemplate>(st => st.Query(q => q.Term(f => f.ReferenceId, request.StepTemplateId)));

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

            await _context.IndexDocumentAsync(newStep);
            


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
