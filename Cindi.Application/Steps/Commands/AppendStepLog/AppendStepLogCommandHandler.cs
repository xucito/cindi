using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
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
   public class AppendStepLogCommandHandler: IRequestHandler<AppendStepLogCommand, CommandResult>
    {
        public IStepsRepository _stepsRepository;
        public ILogger<AppendStepLogCommandHandler> Logger;

        public AppendStepLogCommandHandler(IStepsRepository stepsRepository,
            ILogger<AppendStepLogCommandHandler> logger
            )
        {
            _stepsRepository = stepsRepository;
            Logger = logger;
        }

        public async Task<CommandResult> Handle(AppendStepLogCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _stepsRepository.GetStepAsync(request.StepId);

            if(StepStatuses.IsCompleteStatus(step.Status))
            {
                throw new InvalidStepStatusException("Cannot append log to step, step status is complete with " + step.Status);
            }

            await _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.UtcNow,
                ChainId = step.Journal.GetNextChainId(),
                Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Append,
                                FieldName = "logs",
                                Value = request.Log,
                            }
                        }
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
