using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Node;
using System;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IClusterStateService
    {
        ClusterSettings GetSettings { get; }
        Task<string> GenerateEncryptionKeyAsync(string key = null);
        bool IsEncryptionKeyValid(string key);
        bool IsLogicBlockLocked(Guid workflowId, string logicBlocKId);
        void SetClusterName(string newName);
        void SetEncryptionKey(string key);
        Task<int> LockLogicBlock(Guid lockKey, Guid workflowid, string Value);
        Task<bool> UnlockLogicBlock(Guid lockKey, Guid workflowid, string Value);
        bool WasLockObtained(Guid lockKey, Guid workflowid, string Value);
        CindiClusterState GetState();

        //bool AutoRegistrationEnabled { get; }
        //void ChangeAssignmentEnabled(bool newState);
        //bool IsAssignmentEnabled();
        //void SetAllowAutoRegistration(bool allowAutoRegistration);
    }
}