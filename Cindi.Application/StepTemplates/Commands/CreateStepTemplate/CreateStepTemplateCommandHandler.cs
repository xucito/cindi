﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.StepTemplates;
using ConsensusCore.Node;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
        private readonly IStepTemplatesRepository _stepTemplateRepository;
        

        public CreateStepTemplateCommandHandler(IConfiguration configuration, IStepTemplatesRepository client, ConsensusCoreNode<CindiClusterState,IStateRepository> node)
        {
            _stepTemplateRepository = client;
        }

        public async Task<CommandResult> Handle(CreateStepTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var newStepTemplate = new StepTemplate(
             request.Name + ":" + request.Version,
             request.Description,
             request.AllowDynamicInputs,
             request.InputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
             entry => entry.Value),
             request.OutputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
             entry => entry.Value),
             request.CreatedBy,
             DateTime.UtcNow
             );
            var stepTemplateId = await _stepTemplateRepository.InsertAsync();

            stopwatch.Stop();
            return new CommandResult() {
                ObjectRefId = stepTemplateId,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
