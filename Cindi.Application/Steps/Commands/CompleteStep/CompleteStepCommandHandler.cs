using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, CommandResult>
    {
        public IStepsRepository _stepsRepository;

        public CompleteStepCommandHandler(IStepsRepository stepsRepository)
        {
            _stepsRepository = stepsRepository;
        }

        public async Task<CommandResult> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepToComplete = await _stepsRepository.GetStepAsync(request.Id);

            if(StepStatuses.IsCompleteStatus(request.Status))
            {
                throw new InvalidStepStatusInputException(request.Status + " is not a valid completion status.");
            }

            if(stepToComplete.IsComplete)
            {
                throw new InvalidStepStatusInputException("Step " + request.Id + " is already complete with status " + stepToComplete.Status + ".");
            }

            await _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = stepToComplete.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = stepToComplete.Journal.GetNextChainId(),
                Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "status",
                                Value = request.Status,
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "outputs",
                                Value = request.Outputs,
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "statuscode",
                                Value = request.StatusCode,
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "logs",
                                Value = request.Logs,
                            }
                        }
            });

            return new CommandResult()
            {
                ObjectRefId = request.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
