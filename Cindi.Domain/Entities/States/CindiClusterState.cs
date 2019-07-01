using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Domain.Entities.States
{
    public class CindiClusterState : BaseState
    {
        public const string DefaultId = "State";
        public string Id { get; set; } = DefaultId;
        public Dictionary<string, DateTime> LockedLogicBlocks = new Dictionary<string, DateTime>();
        public bool AssignmentEnabled { get; set; } = true;
        public string Version { get; set; } = "1.0";

        public string EncryptionKeyHash { get; set; }
        public byte[] EncryptionKeySalt { get; set; }

        public bool AllowAutoRegistration { get; set; } = true;
        public bool Initialized { get; set; } = false;

        public override void ApplyCommandToState(BaseCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
