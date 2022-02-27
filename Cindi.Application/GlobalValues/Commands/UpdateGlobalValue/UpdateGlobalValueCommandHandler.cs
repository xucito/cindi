using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.ValueObjects;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries.GetEntity;

namespace Cindi.Application.GlobalValues.Commands.UpdateGlobalValue
{
    public class UpdateGlobalValueCommandHandler : IRequestHandler<UpdateGlobalValueCommand, CommandResult<GlobalValue>>
    {
        ElasticClient _context;
        private IMediator _mediator;

        public UpdateGlobalValueCommandHandler(ElasticClient context,
            IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }
        public async Task<CommandResult<GlobalValue>> Handle(UpdateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            GlobalValue existingValue;
            if ((existingValue = (await _mediator.Send(new GetEntityQuery<GlobalValue>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.Name, request.Name)))
            })).Result) == null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " does not exist.");
            }

            existingValue.Value = request.Value;
            existingValue.Description = request.Description;
            await _context.IndexDocumentAsync(existingValue);
            
            stopwatch.Stop();

            return new CommandResult<GlobalValue>()
            {
                ObjectRefId = existingValue.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update,
                Result = existingValue
            };
        }
    }
}
