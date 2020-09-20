using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterOperation
{
    public class EntityWriteOperation<T> where T : ShardData
    {
        public T Data { get; set; }
        public ShardOperationOptions Operation { get; set; }
        public string User { get; set; }
        public bool RemoveLock { get; set; }
        public Guid LockId { get; set; }
        public bool WaitForSafeWrite = true;
    }
}
