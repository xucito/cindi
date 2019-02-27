using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommand : IRequest<CommandResult>
    {
        public Guid Id { get; set; }
        public List<Dictionary<string, object>> Outputs { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string Logs { get; set; }
    }
}
