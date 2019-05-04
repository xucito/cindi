using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
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
    public class AssignStepCommandHandler : IRequestHandler<AssignStepCommand, CommandResult<Step>>
    {
        private readonly IStepsRepository _stepsRepository;
        private readonly IClusterStateService _clusterStateService;
        private IStepTemplatesRepository _stepTemplateRepository;
        private IBotKeysRepository _botKeysRepository;
        public AssignStepCommandHandler(
            IStepsRepository stepsRepository,
            IClusterStateService stateService,
            IStepTemplatesRepository stepTemplateRepository,
            IBotKeysRepository botKeysRepository)
        {
            _stepsRepository = stepsRepository;
            _clusterStateService = stateService;
            _stepTemplateRepository = stepTemplateRepository;
            _botKeysRepository = botKeysRepository;
        }
        public async Task<CommandResult<Step>> Handle(AssignStepCommand request, CancellationToken cancellationToken)
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


                            unassignedStep.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                            {
                                CreatedBy = SystemUsers.QUEUE_MANAGER,
                                CreatedOn = DateTime.UtcNow,
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

                            await _stepsRepository.UpdateStep(unassignedStep);
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
                    }
                }
                while (!assignedStepSuccessfully);

                if (unassignedStep != null)
                {
                    var template = await _stepTemplateRepository.GetStepTemplateAsync(unassignedStep.StepTemplateId);
                    var botkey = await _botKeysRepository.GetBotKeyAsync(request.BotId);

                    //Decrypt the step
                    unassignedStep.DecryptStepSecrets(EncryptionProtocol.AES256, template, ClusterStateService.GetEncryptionKey());

                    //Encrypt the step
                    unassignedStep.EncryptStepSecrets(EncryptionProtocol.RSA, template, botkey.PublicEncryptionKey, true);
                }

                stopwatch.Stop();
                return new CommandResult<Step>()
                {
                    ObjectRefId = unassignedStep != null ? unassignedStep.Id.ToString() : "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update,
                    Result = unassignedStep != null ? unassignedStep : null
                };
            }
            else
            {
                return new CommandResult<Step>()
                {
                    ObjectRefId = "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

        }
    }
}
