using Cindi.Application.Services.ClusterState;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Entities.States;
using System;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IStateMachine
    {
        bool AutoRegistrationEnabled { get; }
        ClusterSettings GetSettings { get; }
        CindiClusterState GetState();
        bool IsEncryptionKeyValid(string key);
        bool IsLogicBlockLocked(Guid workflowId, string logicBlockId);
        Task<bool> LockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId);
        void SetClusterName(string newName);
        void SetEncryptionKey(string key);
        void TransitionState();
        void UnlockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId);
        bool WasLockObtained(Guid lockKey, Guid workflowId, string logicBlockId);
        void UpdateClusterSettings(UpdateClusterDetails newSettings);
    }
}