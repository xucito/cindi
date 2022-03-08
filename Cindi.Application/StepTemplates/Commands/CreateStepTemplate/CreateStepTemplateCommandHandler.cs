using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions;
using Nest;
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
using Cindi.Application.Utilities;

namespace Cindi.Application.StepTemplates.Commands.CreateStepTemplate
{
    public class CreateStepTemplateCommandHandler : IRequestHandler<CreateStepTemplateCommand, CommandResult>
    {
        private readonly ElasticClient _context;
        private ILogger<CreateStepTemplateCommandHandler> Logger;

        public CreateStepTemplateCommandHandler( ElasticClient context, ILogger<CreateStepTemplateCommandHandler> logger)
        {
            
            _context = context;
            Logger = logger;
        }

        public async Task<CommandResult> Handle(CreateStepTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if(request.ReferenceId != null)
            {
                request.Name = request.ReferenceId.Split(':')[0];
                request.Version = request.ReferenceId.Split(':')[1];
            }


            var newId = Guid.NewGuid();
            var newStepTemplate = new StepTemplate()
            {
                Id = Guid.NewGuid(),
                ReferenceId = request.ReferenceId == null ? request.Name + ":" + request.Version : request.ReferenceId,
                Description = request.Description,
                AllowDynamicInputs = request.AllowDynamicInputs,
                InputDefinitions = request.InputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
                entry => entry.Value),
                OutputDefinitions = request.OutputDefinitions.ToDictionary(entry => entry.Key.ToLower(),
                entry => entry.Value),
                CreatedBy = request.CreatedBy
            };

            var existingStepTemplate = await _context.FirstOrDefaultAsync<StepTemplate>(st => st.Query(q => q.Term(f => f.Field(field => field.ReferenceId.Suffix("keyword")).Value(newStepTemplate.ReferenceId))));

            if (existingStepTemplate == null)
            {
                if (request.Name.First() == '_' && request.CreatedBy != SystemUsers.SYSTEM_TEMPLATES_MANAGER)
                {
                    throw new InvalidStepTemplateException("Only system workflows can start with _");
                }

                var indexResult = await _context.IndexDocumentAsync(newStepTemplate);
                
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
