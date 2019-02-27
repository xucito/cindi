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

            var checkpoints = _clusterStateService.GetLastStepAssignmentCheckpoints(request.CompatibleStepTemplateIds);

            var assignedStepSuccessfully = false;
            Step step = null;
            var dateChecked = DateTime.UtcNow;
            do
            {
                var unassignedStep = await _stepsRepository.GetStepsAsync(StepStatuses.Unassigned, checkpoints);

                if (unassignedStep != null)
                {
                    step = await _stepsRepository.GetStepAsync(unassignedStep.Id);

                    try
                    {
                        //This should not throw a error externally, the server should loop to the next one and log a error
                        if (step.Status != StepStatuses.Unassigned)
                        {
                            throw new InvalidStepQueueException("You cannot assign step " + step.Id + " as it is not unassigned.");
                        }

                        _clusterStateService.UpdateStepAssignmentCheckpoint(step.StepTemplateId, step.CreatedOn);


                        await _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
                        {
                            Entity = JournalEntityTypes.Step,
                            SubjectId = step.Id,
                            RecordedOn = DateTime.UtcNow,
                            ChainId = step.Journal.GetNextChainId(),
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
                }
                else
                {
                    assignedStepSuccessfully = true;
                    //Reflect that all the templates were checked with no template found.
                    foreach (var point in checkpoints)
                    {
                        _clusterStateService.UpdateStepAssignmentCheckpoint(point.Key, dateChecked);
                    }
                }
            }
            while (!assignedStepSuccessfully);

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = step != null ? step.Id.ToString(): "",
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
