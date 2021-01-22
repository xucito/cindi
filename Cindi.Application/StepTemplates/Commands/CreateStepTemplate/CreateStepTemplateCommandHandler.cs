﻿using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;

using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions;





using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.StepTemplates.Commands.CreateStepTemplate
{
    public class CreateStepTemplateCommandHandler : IRequestHandler<CreateStepTemplateCommand, CommandResult>
    {
        private ILogger<CreateStepTemplateCommandHandler> Logger;
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public CreateStepTemplateCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine,
            ILogger<CreateStepTemplateCommandHandler> logger)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
            Logger = logger;
        }

        public async Task<CommandResult> Handle(CreateStepTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (request.ReferenceId != null)
            {
                request.Name = request.ReferenceId.Split(':')[0];
                request.Version = request.ReferenceId.Split(':')[1];
            }


            var newId = Guid.NewGuid();
            var newStepTemplate = new StepTemplate()
            {
                Id = newId,
                ReferenceId = request.ReferenceId == null ? request.Name + ":" + request.Version : request.ReferenceId,
                Description = request.Description,
                AllowDynamicInputs = request.AllowDynamicInputs,
                InputDefinitions = request.InputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
                entry => entry.Value),
                OutputDefinitions = request.OutputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
                entry => entry.Value),
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.UtcNow
            };

            var existingStepTemplate = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == newStepTemplate.ReferenceId);

            if (existingStepTemplate == null)
            {
                if (request.Name.First() == '_' && request.CreatedBy != SystemUsers.SYSTEM_TEMPLATES_MANAGER)
                {
                    throw new InvalidStepTemplateException("Only system workflows can start with _");
                }
                await _entitiesRepository.Insert(newStepTemplate);
            }
            else
            {
                Logger.LogDebug("Found existing step template");

                BaseException exception;
                if (!existingStepTemplate.IsEqual(newStepTemplate, out exception))
                {
                    throw exception;
                }
            }

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = newStepTemplate.ReferenceId,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
