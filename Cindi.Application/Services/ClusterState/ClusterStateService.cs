using Cindi.Application.Interfaces;
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

        public ClusterStateService(IClusterRepository clusterRepository)
        {
            state = new ClusterState();
            _clusterRepository = clusterRepository;

            state = _clusterRepository.GetClusterState().GetAwaiter().GetResult();

            if(state == null)
            {
                state = new ClusterState();
            }
        }

        public ClusterStateService()
        {
            state = new ClusterState();
        }

        public bool IsLogicBlockLocked(string logicBlockId)
        {
            if(state.LockedLogicBlocks.ContainsKey(logicBlockId))
            {
                return true;
            }
            return false;
        }

        public void LockLogicBlock(string logicBlockId)
        {
            state.LockedLogicBlocks.Add(logicBlockId, DateTime.UtcNow);
        }

        public void UnlockLogicBlock(string logicBlockId)
        {
            state.LockedLogicBlocks.Remove(logicBlockId);
        }

        public Dictionary<string, DateTime?> GetLastStepAssignmentCheckpoints(string[] stepTemplateIds)
        {
            var foundDateTimes = new Dictionary<string, DateTime?>();

            foreach (var templateId in stepTemplateIds)
            {
                if (state.StepAssignmentCheckpoints.ContainsKey(templateId))
                {
                    foundDateTimes.Add(templateId, state.StepAssignmentCheckpoints[templateId]);
                }
                else
                {
                    foundDateTimes.Add(templateId, null);
                }
            }
            return foundDateTimes;
        }

        public void UpdateStepAssignmentCheckpoints(Dictionary<string, DateTime> updates)
        {
            foreach (var update in updates)
            {
                if (state.StepAssignmentCheckpoints.ContainsKey(update.Key))
                {
                    if (state.StepAssignmentCheckpoints[update.Key] < update.Value)
                    {
                        state.StepAssignmentCheckpoints[update.Key] = update.Value;
                    }
                }
                else
                {
                    state.StepAssignmentCheckpoints.Add(update.Key, update.Value);
                }
            }

            SaveState();
        }

        public void SaveState()
        {
            if (_clusterRepository != null)
            {
                if (SaveThread == null || !SaveThread.IsAlive)
                {
                    SaveThread = new Thread(async () =>
                    {
                        await _clusterRepository.SaveClusterState(state);
                    });
                    SaveThread.Start();
                }
            }
        }

        public void UpdateStepAssignmentCheckpoint(string stepTemplateId, DateTime updatedTime)
        {
            if (state.StepAssignmentCheckpoints.ContainsKey(stepTemplateId))
            {
                if (state.StepAssignmentCheckpoints[stepTemplateId] < updatedTime)
                {
                    state.StepAssignmentCheckpoints[stepTemplateId] = updatedTime;
                }
            }
            else
            {
                state.StepAssignmentCheckpoints.Add(stepTemplateId, updatedTime);
            }

            SaveState();
        }
    }
}
