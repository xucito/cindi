﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IClusterStateService _clusterStateService;
        private readonly ApplicationDbContext _context;
        public CreateStepCommandHandler(
            IClusterStateService service, 
            ApplicationDbContext context)
        {
            _clusterStateService = service;
            _context = context;
        }

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await  _context.StepTemplates.FirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == request.StepTemplateId);

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

            _context.Add(newStep);
            await _context.SaveChangesAsync();


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
