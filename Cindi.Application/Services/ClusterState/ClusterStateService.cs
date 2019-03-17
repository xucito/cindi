using Cindi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterStateService
    {
        private ClusterState state;
        private IClusterRepository _clusterRepository;
        private Thread SaveThread;
        private ILogger<ClusterStateService> _logger;
        private bool changeDetected = false;
        static readonly object _locker = new object();

        public ClusterStateService(IClusterRepository clusterRepository, ILogger<ClusterStateService> logger)
        {
            state = new ClusterState();
            _clusterRepository = clusterRepository;

            state = _clusterRepository.GetClusterState().GetAwaiter().GetResult();

            _logger = logger;

            if (state == null)
            {
                _logger.LogWarning("Existing cluster state not found. Creating new state.");
                state = new ClusterState();
            }
            else
            {
                _logger.LogInformation("Existing cluster state found. Loading existing state.");
            }

            SaveState();
        }

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


        public void SaveState()
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

        public bool IsAssignmentEnabled()
        {
            return state.AssignmentEnabled;
        }
    }
}
