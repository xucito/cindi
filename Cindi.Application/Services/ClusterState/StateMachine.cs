using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Events;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Services.ClusterState
{
    public class StateMachine : IStateMachine
    {
        private ILogger<StateMachine> _logger;

        static readonly object _locker = new object();
        private string _encryptionKey { get; set; }
        public bool HasValidEncryptionKey { get { return _encryptionKey != null; } }
        public bool AutoRegistrationEnabled { get { return _state.Settings.AllowAutoRegistration; } }
        public ClusterSettings GetSettings { get { if (_state == null) return null; else return _state.Settings; } }
        public string EncryptionKey { get { return _encryptionKey; } }

        CindiClusterState _state;

        public event EventHandler<StateChangedEventArgs> onStateChange;

        public Thread removeLocks;

        protected virtual void OnStateChange()
        {
            var handler = onStateChange;
            if (handler != null)
            {
                handler(this, new StateChangedEventArgs()
                {
                    NewState = _state
                }); ;
            }
        }

        public void RemoveExpiredLocks()
        {
            if (_state != null && _state.Initialized)
            {
                var currentLocks = _state.Locks.ToList();
                var expiredLocks = currentLocks.Where(cl => DateTime.Now > cl.Value.CreatedOn.AddMilliseconds(cl.Value.LockTimeoutMs));
                if (expiredLocks.Count() > 0)
                {
                    UpdateState((newState) =>
                    {
                        foreach (var expiredLock in expiredLocks)
                        {
                            newState.Locks.TryRemove(expiredLock.Key, out _);
                        }
                        return newState;
                    });
                }
            }
        }

        public StateMachine()
        {
            _state = new CindiClusterState();
            _state.Settings = new ClusterSettings();
        }

        public void LoadState(CindiClusterState loadedState)
        {
            _state = loadedState;
        }

        public void Start()
        {
            removeLocks = new Thread(async () =>
            {
                while (true)
                {
                    RemoveExpiredLocks();
                    await Task.Delay(1000);
                }
            });
            removeLocks.Start();
        }

        private CindiClusterState _clonedState
        {
            get
            {
                var serialized = JsonConvert.SerializeObject(_clonedState);
                return JsonConvert.DeserializeObject<CindiClusterState>(serialized);
            }
        }

        public void SetInitialized(bool isInitialized)
        {
            UpdateState((newState) =>
            {
                newState.Initialized = true;
                return newState;
            });
        }

        private void UpdateState(Func<CindiClusterState, CindiClusterState> updateState)
        {
            lock (_locker)
            {
                var clonedState = JsonConvert.DeserializeObject<CindiClusterState>(JsonConvert.SerializeObject(_state));
                var updatedState = updateState.Invoke(clonedState);
                //Save state
                OnStateChange();
                _state = updatedState;
            }
        }

        public bool IsEncryptionKeyValid(string key)
        {
            return SecurityUtility.IsMatchingHash(key, _state.EncryptionKeyHash, _state.EncryptionKeySalt);
        }

        public void UpdateClusterSettings(UpdateClusterDetails newSettings)
        {
            UpdateState((newState) =>
            {
                if (newSettings.AllowAutoRegistration != null)
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

        public bool LockEntity<T>(Guid id, int timeoutMs = 30000)
        {
            var addSuccessful = false;
            UpdateState((newState) =>
            {
                addSuccessful = newState.Locks.TryAdd(id.ToString(), new Lock()
                {
                    LockTimeoutMs = timeoutMs,
                    LockId = id,
                    CreatedOn = DateTime.UtcNow,
                    Name = "Lock for entity " + typeof(T).GetType().Name
                });

                return newState;
            });
            return addSuccessful;
        }

        public bool IsEntityLocked(Guid id)
        {
            return _state.Locks.ContainsKey(id.ToString());
        }

        public bool UnlockEntity<T>(Guid id)
        {
            var wasEntityUnlocked = false;
            UpdateState((newState) =>
            {
                wasEntityUnlocked = newState.Locks.TryRemove(id.ToString(), out _);
                return newState;
            });
            return wasEntityUnlocked;
        }
    }
}
