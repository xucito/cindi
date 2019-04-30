using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.Utility;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Queries.UnencryptStepField
{
    public class UnencryptStepFieldQueryHandler : IRequestHandler<UnencryptStepFieldQuery, QueryResult<string>>
    {
        private readonly IStepsRepository _stepsRepository;
        private readonly IStepTemplatesRepository _stepTemplatesRepository;

        public UnencryptStepFieldQueryHandler(IStepsRepository stepsRepository, IStepTemplatesRepository stepTemplatesRepository)
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
        }

        public async Task<QueryResult<string>> Handle(UnencryptStepFieldQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var step = await _stepsRepository.GetStepAsync(request.StepId);

            if(step.CreatedBy != request.UserId)
            {
                throw new InvalidStepPermissionException("Only the creating user can decrypt the step secret.");
            }

            var stepTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(step.StepTemplateId);

            //Compare the to lower of inputs, TODO - this is inefficient
            if(!stepTemplate.InputDefinitions.ContainsKey(request.FieldName.ToLower()))
            {
                throw new InvalidUnencryptionRequestException("Field " + request.FieldName + " does not exist on step template " + stepTemplate.Id);
            }

            var input = stepTemplate.InputDefinitions[request.FieldName.ToLower()];

            if(input.Type != InputDataTypes.Secret)
            {
                throw new InvalidUnencryptionRequestException("Field " + request.FieldName + " is not a secret type.");
            }

            step.DecryptStepSecrets(EncryptionProtocol.AES256, stepTemplate, ClusterStateService.GetEncryptionKey());

            stopwatch.Stop();

            return new QueryResult<string>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Count = 1,
                Result = (string)step.Inputs[request.FieldName.ToLower()]
            };
        }
    }
}
