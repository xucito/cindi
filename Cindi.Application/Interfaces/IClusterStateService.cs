using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
using Cindi.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IClusterStateService
    {
        bool AutoRegistrationEnabled { get; }

        void ChangeAssignmentEnabled(bool newState);
        Task<string> GenerateEncryptionKeyAsync(string key = null);
        bool IsAssignmentEnabled();
        bool IsEncryptionKeyValid(string key);
        bool IsLogicBlockLocked(Guid workflowId, string logicBlocKId);
        void SetAllowAutoRegistration(bool allowAutoRegistration);
        void SetClusterName(string newName);
        void SetEncryptionKey(string key);
        Task<int> LockLogicBlock(Guid lockKey, Guid workflowid, string Value);
        Task<bool> UnlockLogicBlock(Guid lockKey, Guid workflowid, string Value);
        bool WasLockObtained(Guid lockKey, Guid workflowid, string Value);
        CindiClusterState GetState();
    }
}