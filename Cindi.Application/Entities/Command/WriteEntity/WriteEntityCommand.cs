using Cindi.Application.Results;
using Cindi.Domain.Entities;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Entities.Command.CreateTrackedEntity
{
    public class WriteEntityCommand<T> : IRequest<CommandResult> where T : ShardData
    {
        public T Data { get; set; }
        public ShardOperationOptions Operation { get; set; }
        public string User { get; set; }
        public bool RemoveLock { get; set; }
        public Guid LockId { get; set; }
        public bool WaitForSafeWrite = true;
    }
}
