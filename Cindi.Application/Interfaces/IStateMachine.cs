using Cindi.Application.Services.ClusterState;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Events;
using System;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IStateMachine
    {
        string EncryptionKey { get; }
        bool AutoRegistrationEnabled { get; }
        ClusterSettings GetSettings { get; }
        CindiClusterState GetState();
        bool IsEncryptionKeyValid(string key);
        bool IsLogicBlockLocked(Guid workflowId, string logicBlockId);
        Task<bool> LockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId);
        void SetEncryptionKey(string key);
        void UnlockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId);
        bool LockEntity<T>(Guid id, int timeoutMs = 30000);
        bool UnlockEntity<T>(Guid id);
        bool IsEntityLocked(Guid id);
        void UpdateClusterSettings(UpdateClusterDetails newSettings);
        void SetInitialized(bool isInitialized);

        event EventHandler<StateChangedEventArgs> onStateChange;

        void LoadState(CindiClusterState loadedState);
        void Start();
    }
}