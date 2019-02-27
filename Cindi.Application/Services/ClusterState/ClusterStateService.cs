using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterStateService
    {
        private ClusterState state;

        public ClusterStateService()
        {
            state = new ClusterState();
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
        }
    }
}
