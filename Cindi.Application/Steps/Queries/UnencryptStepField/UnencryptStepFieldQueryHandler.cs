using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.Utility;
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

namespace Cindi.Application.Steps.Queries.UnencryptStepField
{
    public class UnencryptStepFieldQueryHandler : IRequestHandler<UnencryptStepFieldQuery, QueryResult<string>>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public UnencryptStepFieldQueryHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine)
        {
            _entitiesRepository = entitiesRepository;
        }

        public async Task<QueryResult<string>> Handle(UnencryptStepFieldQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (request.Type != StepEncryptionTypes.Inputs && request.Type != StepEncryptionTypes.Outputs)
            {
                throw new InvalidUnencryptionRequestException("No such encryption type " + request.Type);
            }
            var step = await _entitiesRepository.GetFirstOrDefaultAsync<Step>(s => s.Id == request.StepId);


            if (step.CreatedBy != request.UserId)
            {
                throw new InvalidStepPermissionException("Only the creating user can decrypt the step secret.");
            }

            var stepTemplate = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == step.StepTemplateId);

            //Compare the to lower of inputs, TODO - this is inefficient
            if (!stepTemplate.InputDefinitions.ContainsKey(request.FieldName.ToLower()))
            {
                throw new InvalidUnencryptionRequestException("Field " + request.FieldName + " does not exist on step template " + stepTemplate.Id);
            }

            DynamicDataDescription kv = request.Type == StepEncryptionTypes.Inputs ? stepTemplate.InputDefinitions[request.FieldName.ToLower()] : stepTemplate.OutputDefinitions[request.FieldName.ToLower()];

            if (kv.Type != InputDataTypes.Secret)
            {
                throw new InvalidUnencryptionRequestException("Field " + request.FieldName + " is not a secret type.");
            }

            var decryptedInput = DynamicDataUtility.DecryptDynamicData(stepTemplate.InputDefinitions, request.Type == StepEncryptionTypes.Inputs ? step.Inputs : step.Outputs, EncryptionProtocol.AES256, _stateMachine.EncryptionKey, false);

            stopwatch.Stop();

            return new QueryResult<string>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Count = 1,
                Result = (string)decryptedInput[request.FieldName.ToLower()]
            };
        }
    }
}
