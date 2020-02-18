using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.AppendStepLog
{
    public class AppendStepLogCommandHandler : IRequestHandler<AppendStepLogCommand, CommandResult>
    {
        public IEntityRepository _entityRepository;
        public ILogger<AppendStepLogCommandHandler> Logger;
        private readonly IClusterRequestHandler _node;

        public AppendStepLogCommandHandler(IEntityRepository entityRepository,
            ILogger<AppendStepLogCommandHandler> logger,
            IClusterRequestHandler node
            )
        {
            _entityRepository = entityRepository;
            Logger = logger;
            _node = node;
        }

        public async Task<CommandResult> Handle(AppendStepLogCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _entityRepository.GetFirstOrDefaultAsync<Step>(e => e.Id == request.StepId);

            if (StepStatuses.IsCompleteStatus(step.Status))
            {
                throw new InvalidStepStatusException("Cannot append log to step, step status is complete with " + step.Status);
            }

            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.UtcNow,
                Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Append,
                                FieldName = "logs",
                                Value = new StepLog(){
                                    CreatedOn = DateTime.UtcNow,
                                    Message = request.Log
                                },
                            }
                        }
            });

            //await _entityRepository.UpdateStep(step);

            var createdWorkflowTemplateId = await _node.Handle(new AddShardWriteOperation()
            {
                Data = step,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
            });

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = step.Id.ToString(),
                Type = CommandResultTypes.Update
            };
        }
    }
}
