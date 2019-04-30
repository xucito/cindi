using Cindi.Application.Cluster.Commands.InitializeCluster;
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Users.Commands.CreateUserCommand;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterStateService : IClusterStateService
    {
        private ClusterState state;
        private IClusterRepository _clusterRepository;
        private Thread SaveThread;
        private ILogger<ClusterStateService> _logger;
        private bool changeDetected = false;
        static readonly object _locker = new object();
        private static string _encryptionKey { get; set; }
        //private IMediator _mediator { get; set; }
        private Thread _initThread { get; set; }
        public static bool Initialized { get; set; }
        public static bool HasValidEncryptionKey { get { return _encryptionKey != null; } }

        public ClusterStateService(IClusterRepository clusterRepository, ILogger<ClusterStateService> logger, IServiceScopeFactory serviceProvider)
        {
            state = new ClusterState();
            _clusterRepository = clusterRepository;

            state = _clusterRepository.GetClusterState().GetAwaiter().GetResult();

            Initialized = state == null ? false : state.Initialized;

            _logger = logger;

            if (state == null)
            {
                _logger.LogWarning("Existing cluster state not found. Creating new state.");
                state = new ClusterState();
            }
            else
            {
                Console.WriteLine("Existing cluster state found with name " + state.Id + ". Loading existing state.");
            }

            //var sp = serviceProvider.CreateScope().ServiceProvider;
           // _mediator = sp.GetService<IMediator>();

            changeDetected = true;
            ForceStateSave();
            InitializeSaveThread();
        }

        public static Func<string> GetEncryptionKey = () =>
        {
            return _encryptionKey;
        };

        public bool IsEncryptionKeyValid(string key)
        {
            return SecurityUtility.IsMatchingHash(key, state.EncryptionKeyHash, state.EncryptionKeySalt);
        }

        public void SetEncryptionKey(string key)
        {
            if (state.EncryptionKeyHash == null)
            {
                GenerateEncryptionKey(key);
            }
            else
            {
                if (SecurityUtility.IsMatchingHash(key, state.EncryptionKeyHash, state.EncryptionKeySalt))
                {
                    _encryptionKey = key;
                }
                else
                {
                    throw new InvalidPrivateKeyException("Key is not matching the cluster's decryption key.");
                }
            }
        }

        /*public string GetEncryptionKey()
        {
            return EncryptionKey;
        }*/

        public void SetClusterName(string newName)
        {
            state.Id = newName;
            ForceStateSave();
        }

        public string GenerateEncryptionKey(string key)
        {
            if(state.EncryptionKeyHash != null)
            {
                throw new InvalidClusterStateException("Encryption key already exists.");
            }

            var passPhrase = key == null ? SecurityUtility.RandomString(32, false): key;
            var salt = SecurityUtility.GenerateSalt(128);

            state.EncryptionKeyHash = SecurityUtility.OneWayHash(passPhrase, salt);
            state.EncryptionKeySalt = salt;

            _encryptionKey = passPhrase;

            //Initialize the cluster
            state.Initialized = true;
            ForceStateSave();

            return passPhrase;
        }

        public bool AutoRegistrationEnabled { get { return state.AllowAutoRegistration; } }

        public bool IsLogicBlockLocked(string logicBlockId)
        {
            if (state.LockedLogicBlocks.ContainsKey(logicBlockId))
            {
                return true;
            }
            return false;
        }

        public void LockLogicBlock(string logicBlockId)
        {
            lock (_locker)
            {
                state.LockedLogicBlocks.Add(logicBlockId, DateTime.UtcNow);
                changeDetected = true;
            }
        }

        public void UnlockLogicBlock(string logicBlockId)
        {
            lock (_locker)
            {
                state.LockedLogicBlocks.Remove(logicBlockId);
                changeDetected = true;
            }
        }

        public ClusterState GetState()
        {
            return state;
        }

        public void ForceStateSave()
        {
            lock (_locker)
            {
                _clusterRepository.SaveClusterState(state).GetAwaiter().GetResult();
            }
        }

        public void InitializeSaveThread()
        {
            if (_clusterRepository != null)
            {
                if (SaveThread == null || !SaveThread.IsAlive)
                {
                    SaveThread = new Thread(async () =>
                    {
                        while (true)
                        {
                            if (changeDetected)
                            {
                                lock (_locker)
                                {
                                    _logger.LogInformation("Saving cluster state...");
                                    _clusterRepository.SaveClusterState(state).GetAwaiter().GetResult();
                                    Thread.Sleep(100);
                                    changeDetected = false;
                                }
                            }
                        }
                    });
                    SaveThread.Start();
                }
            }
        }

        public void ChangeAssignmentEnabled(bool newState)
        {
            lock (_locker)
            {
                if (newState != state.AssignmentEnabled)
                {
                    state.AssignmentEnabled = newState;
                    changeDetected = true;
                }
            }
        }

        public void SetAllowAutoRegistration(bool allowAutoRegistration)
        {
            lock (_locker)
            {
                if (allowAutoRegistration != state.AllowAutoRegistration)
                {
                    state.AllowAutoRegistration = allowAutoRegistration;
                    changeDetected = true;
                }
            }
        }

        public bool IsAssignmentEnabled()
        {
            return state.AssignmentEnabled;
        }
    }
}
