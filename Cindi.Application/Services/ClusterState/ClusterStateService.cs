using Cindi.Application.Cluster.Commands.InitializeCluster;
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Users.Commands.CreateUserCommand;
using Cindi.Domain.ClusterCommands;
using Cindi.Domain.ClusterCommands.Enums;
using Cindi.Domain.ClusterRPC;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Domain.RPCs.Raft;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterStateService : IClusterStateService
    {
        private ILogger<ClusterStateService> _logger;
        static readonly object _locker = new object();
        private static string _encryptionKey { get; set; }
        private Thread _initThread { get; set; }
        public static bool Initialized { get; set; }
        public static bool HasValidEncryptionKey { get { return _encryptionKey != null; } }
        public IClusterRequestHandler _node;
        CindiClusterState state { get { return _stateMachine.CurrentState; } }
        private IStateMachine<CindiClusterState> _stateMachine;

        public ClusterStateService(
            ILogger<ClusterStateService> logger,
            IServiceScopeFactory serviceProvider,
            IClusterRequestHandler node,
            IStateMachine<CindiClusterState> stateMachine)
        {
            _node = node;
            _stateMachine = stateMachine;
            Initialized = state == null ? false : state.Initialized;

            _logger = logger;
            if (state == null)
            {
                _logger.LogWarning("Existing cluster state not found. Creating new state.");
            }
            else
            {
                Console.WriteLine("Existing cluster state found with name " + state.Id + ". Loading existing state.");
            }
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
            lock (_locker)
            {
                if (state.EncryptionKeyHash == null)
                {
                    GenerateEncryptionKeyAsync(key).GetAwaiter().GetResult();
                }
                else
                {
                    if (SecurityUtility.IsMatchingHash(key, state.EncryptionKeyHash, state.EncryptionKeySalt))
                    {
                        _encryptionKey = key;
                        Initialized = true;
                    }
                    else
                    {
                        throw new InvalidPrivateKeyException("Key is not matching the cluster's decryption key.");
                    }
                }
            }
        }

        /*public string GetEncryptionKey()
        {
            return EncryptionKey;
        }*/

        public async void SetClusterName(string newName)
        {
            // state.Id = newName;
            // ForceStateSave();
            lock (_locker)
            {
                _node.Handle(new ExecuteCommands()
                {
                    WaitForCommits = true,
                    Commands = new List<BaseCommand>()
                {
                    new UpdateClusterDetails()
                    {
                        Id = newName
                    }
                }
                }).GetAwaiter().GetResult();
            }
        }

        public async Task<string> GenerateEncryptionKeyAsync(string key)
        {
            if (state.EncryptionKeyHash != null)
            {
                throw new InvalidClusterStateException("Encryption key already exists.");
            }

            var passPhrase = key == null ? SecurityUtility.RandomString(32, false) : key;
            var salt = SecurityUtility.GenerateSalt(128);

            lock (_locker)
            {
                _node.Handle(new ExecuteCommands()
                {
                    WaitForCommits = true,
                    Commands = new List<BaseCommand>()
                {
                    new UpdateClusterDetails()
                    {
                        EncryptionKeyHash =  SecurityUtility.OneWayHash(passPhrase, salt),
                        EncryptionKeySalt = salt,
                        Initialized = true
                    }
                }
                }).GetAwaiter().GetResult();
            }

            Initialized = true;

            _encryptionKey = passPhrase;

            //Initialize the cluster
            // state.Initialized = true;
            // ForceStateSave();

            return passPhrase;
        }

        public bool AutoRegistrationEnabled { get { return state.Settings.AllowAutoRegistration; } }

        public ClusterSettings GetSettings { get { return state.Settings; } }

        public async Task<int> LockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            return (await _node.Handle(new ExecuteCommands()
            {
                Commands = new List<BaseCommand>()
                {
                    new UpdateLogicBlockLock(){
                        Action = LockBlockActions.APPLY,
                        Lock = new LogicBlockLock(){
                            WorkflowId = workflowid,
                            LockerCode = lockKey,
                            LogicBlockId = logicBlockId,
                            CreatedOn = DateTime.Now
                        }
                    }
                }
            })).EntryNo;
            //state.LockedLogicBlocks.Add(block.LogicBlockId, DateTime.UtcNow);
        }

        public async Task<bool> UnlockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            return (await _node.Handle(new ExecuteCommands()
            {
                Commands = new List<BaseCommand>()
                {
                    new UpdateLogicBlockLock(){
                        Action = LockBlockActions.REMOVE,
                        Lock = new LogicBlockLock(){
                            WorkflowId = workflowid,
                            LockerCode = lockKey,
                            LogicBlockId = logicBlockId
                        }
                    }
                }
            })).IsSuccessful;
            //state.LockedLogicBlocks.Remove(logicBlockId);
        }

        public CindiClusterState GetState()
        {
            return state;
        }

        /*public void ForceStateSave()
        {
            lock (_locker)
            {
                _clusterRepository.SaveClusterState(state).GetAwaiter().GetResult();
            }
        }*/

        /*public void InitializeSaveThread()
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
                                    //_clusterRepository.SaveClusterState(state).GetAwaiter().GetResult();
                                    Thread.Sleep(100);
                                    changeDetected = false;
                                }
                            }
                        }
                    });
                    SaveThread.Start();
                }
            }
        }*/

        /*public void ChangeAssignmentEnabled(bool newState)
        {
            lock (_locker)
            {
                _node.Handle(new ExecuteCommands()
                {
                    WaitForCommits = true,
                    Commands = new List<BaseCommand>()
                {
                    new UpdateClusterDetails()
                    {
                        AssignmentEnabled = newState
                    }
                }
                }).GetAwaiter().GetResult();
            }
        }*/

       /* public void SetAllowAutoRegistration(bool allowAutoRegistration)
        {
            lock (_locker)
            {
                if (allowAutoRegistration != state.Options.AllowAutoRegistration)
                {
                    _node.Handle(new ExecuteCommands()
                    {
                        WaitForCommits = true,
                        Commands = new List<BaseCommand>()
                {
                    new UpdateClusterDetails()
                    {
                        AllowAutoRegistration = allowAutoRegistration
                    }
                }
                    });
                }
            }
        }*/

        /*public bool IsAssignmentEnabled()
        {
            return state.Options.AssignmentEnabled;
        }*/

        public bool WasLockObtained(Guid lockKey, Guid workflowId, string logicBlockId)
        {
            if (state.LockedLogicBlocks.ContainsKey(workflowId + ":" + logicBlockId) && state.LockedLogicBlocks[(workflowId + ":" + logicBlockId)].LockerCode == lockKey)
            {
                return true;
            }
            return false;
        }


        public bool IsLogicBlockLocked(Guid workflowId, string logicBlockId)
        {
            if (state.LockedLogicBlocks.ContainsKey(workflowId + ":" + logicBlockId))
            {
                return true;
            }
            return false;
        }
    }
}
