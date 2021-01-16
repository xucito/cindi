using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.Services.ClusterState
{
    public class StateMachine : IStateMachine
    {
        private ILogger<StateMachine> _logger;

        static readonly object _locker = new object();
        private static string _encryptionKey { get; set; }
        public static bool HasValidEncryptionKey { get { return _encryptionKey != null; } }
        public bool AutoRegistrationEnabled { get { return _state.Settings.AllowAutoRegistration; } }
        public ClusterSettings GetSettings { get { return _state.Settings; } }

        CindiClusterState _state;

        private CindiClusterState _clonedState
        {
            get
            {
                var serialized = JsonConvert.SerializeObject(_clonedState);
                return JsonConvert.DeserializeObject<CindiClusterState>(serialized);
            }
        }

        private void UpdateState(Func<CindiClusterState, CindiClusterState> updateState)
        {
            lock (_locker)
            {
                var clonedState = JsonConvert.DeserializeObject<CindiClusterState>(JsonConvert.SerializeObject(_state));
                var updatedState = updateState.Invoke(clonedState);
                //Save state
                _state = updatedState;
            }
        }

        public static Func<string> GetEncryptionKey = () =>
        {
            return _encryptionKey;
        };

        public bool IsEncryptionKeyValid(string key)
        {
            return SecurityUtility.IsMatchingHash(key, _state.EncryptionKeyHash, _state.EncryptionKeySalt);
        }

        public void UpdateClusterSettings(UpdateClusterDetails newSettings)
        {
            UpdateState((newState) =>
            {
                if(newSettings.AllowAutoRegistration != null)
                {
                    newState.Settings.AllowAutoRegistration = newSettings.AllowAutoRegistration.Value;
                }
                if (newSettings.AssignmentEnabled != null)
                {
                    newState.Settings.AssignmentEnabled = newSettings.AssignmentEnabled.Value;
                }
                if (newSettings.MetricRetentionPeriod != null)
                {
                    newState.Settings.MetricRetentionPeriod = newSettings.MetricRetentionPeriod;
                }
                if (newSettings.StepRetentionPeriod != null)
                {
                    newState.Settings.StepRetentionPeriod = newSettings.StepRetentionPeriod;
                }
                if (newSettings.CleanupInterval != null)
                {
                    newState.Settings.CleanupInterval = newSettings.CleanupInterval.Value;
                }
                return newState;
            });
        }

        public void SetEncryptionKey(string key)
        {
            UpdateState((newState) =>
            {
                if (newState.EncryptionKeyHash == null)
                {
                    if (newState.EncryptionKeyHash != null)
                    {
                        throw new InvalidClusterStateException("Encryption key already exists.");
                    }

                    var passPhrase = key == null ? SecurityUtility.RandomString(32, false) : key;
                    var salt = SecurityUtility.GenerateSalt(128);
                    _encryptionKey = passPhrase;
                    newState.EncryptionKeyHash = SecurityUtility.OneWayHash(passPhrase, salt);
                    newState.EncryptionKeySalt = salt;
                    newState.Initialized = true;
                    return newState;
                }
                else
                {
                    if (SecurityUtility.IsMatchingHash(key, _state.EncryptionKeyHash, _state.EncryptionKeySalt))
                    {
                        _encryptionKey = key;
                    }
                    else
                    {
                        throw new InvalidPrivateKeyException("Key is not matching the cluster's decryption key.");
                    }
                    return _state;
                }
            });
        }

        public void SetClusterName(string newName)
        {
            UpdateState((newState) =>
            {
                newState.Id = newName;
                return newState;
            });
        }

        public async Task<bool> LockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            var lockAcquired = false;
            UpdateState((newState) =>
            {
                lockAcquired = newState.Locks.TryAdd("Workflow:" + workflowid + ":" + logicBlockId, new Lock()
                {
                    CreatedOn = DateTime.Now,
                    LockTimeoutMs = 30000,
                    LockId = lockKey,
                    Name = "Workflow:" + workflowid + ":" + logicBlockId
                });
                if (lockAcquired)
                    return newState;
                else
                    return _state;
            });
            return lockAcquired;
        }

        public void UnlockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            UpdateState((newState) =>
            {
                newState.Locks.TryRemove("Workflow:" + workflowid + ":" + logicBlockId, out _);
                return newState;
            });
        }

        public CindiClusterState GetState()
        {
            return _state;
        }

        public bool WasLockObtained(Guid lockKey, Guid workflowId, string logicBlockId)
        {
            if (_state.Locks.ContainsKey("Workflow:" + workflowId + ":" + logicBlockId) && _state.Locks[("Workflow:" + workflowId + ":" + logicBlockId)].LockId == lockKey)
            {
                return true;
            }
            return false;
        }


        public bool IsLogicBlockLocked(Guid workflowId, string logicBlockId)
        {
            if (_state.Locks.ContainsKey(workflowId + ":" + logicBlockId))
            {
                return true;
            }
            return false;
        }

        public void TransitionState()
        {

        }
    }
}
