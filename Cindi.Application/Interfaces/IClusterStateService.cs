using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.States;
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
        bool IsLogicBlockLocked(string logicBlockId);
        void LockLogicBlock(string logicBlockId);
        void SetAllowAutoRegistration(bool allowAutoRegistration);
        void SetClusterName(string newName);
        void SetEncryptionKey(string key);
        void UnlockLogicBlock(string logicBlockId);
        CindiClusterState GetState();
    }
}