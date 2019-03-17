﻿using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.UnassignStep
{
    public class UnassignStepCommandHandler : IRequestHandler<UnassignStepCommand, CommandResult>
    {
        public IStepsRepository _stepsRepository;
        public ILogger<UnassignStepCommandHandler> Logger;
        private CindiClusterOptions _option;

        public UnassignStepCommandHandler(IStepsRepository stepsRepository,
            ILogger<UnassignStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options)
        {
            _stepsRepository = stepsRepository;
            Logger = logger;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }


        public async Task<CommandResult> Handle(UnassignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var step = await _stepsRepository.GetStepAsync(request.StepId);

            if(step.Status != StepStatuses.Suspended)
            {
                throw new InvalidStepStatusInputException("Step " + request.StepId + " has status " + step.Status + ". Only suspended steps can be unassigned.");
            }

           await  _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
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
                                Value = StepStatuses.Unassigned,
                            }

                        }
            });

            await _stepsRepository.UpsertStepMetadataAsync(step.Id);

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = step.Id.ToString(),
                Type = CommandResultTypes.Update
            };
        }
    }
}