﻿using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommand : IRequest<CommandResult>
    {
        public Guid Id { get; set; }
        public Dictionary<string, object> Outputs { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string Log { get; set; }
        public string SuspendFor { get; set; }
    }
}
