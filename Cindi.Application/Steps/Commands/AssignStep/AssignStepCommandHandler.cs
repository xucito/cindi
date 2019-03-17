using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommandHandler : IRequestHandler<AssignStepCommand, CommandResult>
    {
        private readonly IStepsRepository _stepsRepository;
        private readonly ClusterStateService _clusterStateService;

        public AssignStepCommandHandler(IStepsRepository stepsRepository, ClusterStateService stateService)
        {
            _stepsRepository = stepsRepository;
            _clusterStateService = stateService;

        }
        public async Task<CommandResult> Handle(AssignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_clusterStateService.IsAssignmentEnabled())
            {
                var assignedStepSuccessfully = false;
                Step unassignedStep = null;
                var dateChecked = DateTime.UtcNow;
                do
                {
                    unassignedStep = (await _stepsRepository.GetStepsAsync(1, 0, StepStatuses.Unassigned, request.StepTemplateIds)).FirstOrDefault();

                    if (unassignedStep != null)
                    {
                        try
                        {
                            //This should not throw a error externally, the server should loop to the next one and log a error
                            if (unassignedStep.Status != StepStatuses.Unassigned)
                            {
                                throw new InvalidStepQueueException("You cannot assign step " + unassignedStep.Id + " as it is not unassigned.");
                            }


                            await _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
                            {
                                Entity = JournalEntityTypes.Step,
                                SubjectId = unassignedStep.Id,
                                RecordedOn = DateTime.UtcNow,
                                ChainId = unassignedStep.Journal.GetNextChainId(),
                                Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "status",
                                Value = StepStatuses.Assigned,
                            }

                        }
                            });

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            //throw e;
                        }
                        assignedStepSuccessfully = true;
                        await _stepsRepository.UpsertStepMetadataAsync(unassignedStep.Id);
                    }
                    else
                    {
                        assignedStepSuccessfully = true;
                    }
                }
                while (!assignedStepSuccessfully);

                stopwatch.Stop();
                return new CommandResult()
                {
                    ObjectRefId = unassignedStep != null ? unassignedStep.Id.ToString() : "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update
                };
            }
            else
            {
                return new CommandResult()
                {
                    ObjectRefId = "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

        }
    }
}
