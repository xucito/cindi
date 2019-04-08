using Cindi.Application.Services.ClusterState;

namespace Cindi.Application.Interfaces
{
    public interface IClusterStateService
    {
        bool AutoRegistrationEnabled { get; }

        void ChangeAssignmentEnabled(bool newState);
        void ForceStateSave();
        string GenerateEncryptionKey();
        ClusterState GetState();
        void InitializeSaveThread();
        bool IsAssignmentEnabled();
        bool IsEncryptionKeyValid(string key);
        bool IsLogicBlockLocked(string logicBlockId);
        void LockLogicBlock(string logicBlockId);
        void SetAllowAutoRegistration(bool allowAutoRegistration);
        void SetClusterName(string newName);
        void SetEncryptionKey(string key);
        void UnlockLogicBlock(string logicBlockId);
    }
}