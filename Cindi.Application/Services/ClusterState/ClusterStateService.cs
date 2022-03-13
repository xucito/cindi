using Cindi.Application.Cluster.Commands.InitializeCluster;
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Users.Commands.CreateUserCommand;

using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using System.Linq;

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
        public ElasticClient _context;
        CindiClusterState state { get; set; }
        DateTime lastSettingReload { get; set; }

        public ClusterStateService(
            ILogger<ClusterStateService> logger,
            IServiceScopeFactory serviceProvider,
            ElasticClient context)
        {
            _context = context;
            try
            {
                state = _context.Search<CindiClusterState>(s => s.Query(q => q.MatchAll())).Hits.FirstOrDefault()?.Source;
                if(state != null && state.Settings == null)
                {
                    state.Settings = new ClusterSettings();
                }
            }
            catch(Exception e)
            {
                state = null; 
            }
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
            lastSettingReload = DateTime.Now;
        }

        public void Initialize()
        {
            var newState = new CindiClusterState();
            newState.Initialized = true;
            newState.Settings = new ClusterSettings();
            _context.IndexDocument(newState);
            state = newState;
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
                state.EncryptionKeyHash = SecurityUtility.OneWayHash(passPhrase, salt);
                state.EncryptionKeySalt = salt;
                state.Initialized = true;
            }

            Initialized = true;

            _encryptionKey = passPhrase;

            //Initialize the cluster
            // state.Initialized = true;
            // ForceStateSave();

            return passPhrase;
        }

        public bool AutoRegistrationEnabled { get { return state.Settings.AllowAutoRegistration; } }

        public ClusterSettings GetSettings { get { 
                if(lastSettingReload < DateTime.UtcNow.AddMinutes(-1))
                {
                    state = _context.Search<CindiClusterState>(s => s.Query(q => q.MatchAll())).Hits.FirstOrDefault()?.Source;
                    lastSettingReload = DateTime.Now;
                }
                return state?.Settings; 
            } }

        public async Task<int> LockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            return 1;
            /*return (await _node.Handle(new ExecuteCommands()
            {
                Commands = new List<BaseCommand>()
                {
                    new SetLock(){
                        Name = "Workflow:" + workflowid + ":" + logicBlockId,
                        LockId = lockKey,
                        CreatedOn = DateTimeOffset.UtcNow,
                        TimeoutMs = 30000
                    }
                },
                WaitForCommits = true
            })).EntryNo;*/
            //state.LockedLogicBlocks.Add(block.LogicBlockId, DateTime.UtcNow);
        }

        public async Task<bool> UnlockLogicBlock(Guid lockKey, Guid workflowid, string logicBlockId)
        {
            return true;
            /*
            return (await _node.Handle(new ExecuteCommands()
            {
                Commands = new List<BaseCommand>()
                {
                    new RemoveLock()
                    {
                            Name = "Workflow:" + workflowid + ":" + logicBlockId,
                            LockId =lockKey
                    }
                },
                WaitForCommits = true
            })).IsSuccessful;*/
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
            /*if (state.Locks.ContainsKey("Workflow:" + workflowId + ":" + logicBlockId) && state.Locks[("Workflow:" + workflowId + ":" + logicBlockId)].LockId == lockKey)
            {*/
                return true;
           /* }
            return false;*/
        }


        public bool IsLogicBlockLocked(Guid workflowId, string logicBlockId)
        {
            /*if (state.Locks.ContainsKey(workflowId + ":" + logicBlockId))
            {*/
                return true;
           /* }
            return false;*/
        }
    }
}
