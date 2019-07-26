using Cindi.Domain.ClusterCommands;
using Cindi.Domain.ClusterCommands.Enums;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.ValueObjects;
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
        public string Id { get; private set; } = DefaultId;
        public Dictionary<string, LogicBlockLock> LockedLogicBlocks = new Dictionary<string, LogicBlockLock>();
        public bool AssignmentEnabled { get; private set; } = true;
        public string Version { get; private set; } = "1.0";
        public string EncryptionKeyHash { get; private set; }
        public byte[] EncryptionKeySalt { get; private set; }
        public bool AllowAutoRegistration { get; private set; } = true;
        public bool Initialized { get; private set; } = false;

        public override void ApplyCommandToState(BaseCommand command)
        {
            switch (command)
            {
                case UpdateClusterDetails t1:
                    if (t1.Id != null)
                    {
                        Id = t1.Id;
                    }
                    if (t1.AssignmentEnabled != null)
                    {
                        AssignmentEnabled = t1.AssignmentEnabled.Value;
                    }
                    if (t1.Version != null)
                    {
                        Version = t1.Version;
                    }
                    if (t1.EncryptionKeyHash != null)
                    {
                        EncryptionKeyHash = t1.EncryptionKeyHash;
                    }
                    if (t1.EncryptionKeySalt != null)
                    {
                        EncryptionKeySalt = t1.EncryptionKeySalt;
                    }
                    if (t1.AllowAutoRegistration != null)
                    {
                        AllowAutoRegistration = t1.AllowAutoRegistration.Value;
                    }
                    if (t1.Initialized != null)
                    {
                        Initialized = t1.Initialized.Value;
                    }
                    break;
                case UpdateLogicBlockLock t1:
                    if (t1.Action == LockBlockActions.APPLY)
                    {
                        LockedLogicBlocks.Add(t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId, t1.Lock);
                    }
                    else if (t1.Action == LockBlockActions.REMOVE)
                    {
                        if (LockedLogicBlocks.ContainsKey(t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId))
                        {
                            if (LockedLogicBlocks[t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId].LockerCode == t1.Lock.LockerCode)
                            {

                                LockedLogicBlocks.Remove(t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId);
                            }
                            else
                            {
                                throw new InvalidLogicBlockUnlockException("Logic block " + t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId + " is held by a different locker code. Given " + t1.Lock.LockerCode + ", held by " + LockedLogicBlocks[t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId].LockerCode);
                            }
                        }
                        else
                        {
                            throw new InvalidLogicBlockUnlockException("Logic block " + t1.Lock.WorkflowId + ":" + t1.Lock.LogicBlockId + " does not exist for unlocking.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Received a logic block update for " + t1.Lock.WorkflowId + " that did not contain a action");
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
