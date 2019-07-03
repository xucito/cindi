using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.StepTemplates;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
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
        private readonly IConsensusCoreNode<CindiClusterState, IBaseRepository> _node;

        public CreateStepTemplateCommandHandler(IConfiguration configuration, IStepTemplatesRepository client, IConsensusCoreNode<CindiClusterState, IBaseRepository> node)
        {
            _stepTemplateRepository = client;
            _node = node;
        }

        public async Task<CommandResult> Handle(CreateStepTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var newId = Guid.NewGuid();
            var newStepTemplate = new StepTemplate(
                newId,
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

            var createdSequenceTemplateId = await _node.Send(new WriteData()
            {
                Data = newStepTemplate,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

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
