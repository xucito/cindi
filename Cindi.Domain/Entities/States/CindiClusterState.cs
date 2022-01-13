using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Domain.Entities.States
{
    public class CindiClusterState
    {
        public const string DefaultId = "State";
        public string Id { get; set; } = DefaultId;
        //public Dictionary<string, LogicBlockLock> LockedLogicBlocks = new Dictionary<string, LogicBlockLock>();
        public string Version { get; private set; } = "1.0";
        public string EncryptionKeyHash { get; set; }
        public byte[] EncryptionKeySalt { get; set; }
        public ClusterSettings Settings { get; set; }
        public bool Initialized { get; set; } = false;
    }
}
