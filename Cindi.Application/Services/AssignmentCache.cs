using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.Services
{
    public class AssignmentCache : IAssignmentCache
    {
        public ConcurrentDictionary<string, Queue<Step>> assignmentCache = new ConcurrentDictionary<string, Queue<Step>>();
        public IEntitiesRepository _entitiesRepository;
        public DateTime lastRecordTime = new DateTime();
        public Task updateCache;
        public ConcurrentDictionary<string, StepTemplate> stepTemplatesCache = new ConcurrentDictionary<string, StepTemplate>();
        public ILogger _logger;
        public Stopwatch stopwatch = new Stopwatch();
        Random rnd = new Random();

        public int UnassignedCount
        {
            get
            {
                var totalUnassigned = 0;
                foreach (var key in assignmentCache)
                {
                    totalUnassigned += key.Value.Count();
                }
                return totalUnassigned;
            }
        }

        public long LastRefreshTime = 0;

        public AssignmentCache(IEntitiesRepository entitiesRepository, ILogger<AssignmentCache> logger)
        {
            _entitiesRepository = entitiesRepository;
            _logger = logger;
        }

        public async Task<bool> RefreshCache()
        {
            foreach (var st in await _entitiesRepository.GetAsync<StepTemplate>(null, null, null, 10000))
            {
                if (!stepTemplatesCache.ContainsKey(st.ReferenceId))
                {
                    stepTemplatesCache.TryAdd(st.ReferenceId, st);
                    assignmentCache.TryAdd(st.ReferenceId, new Queue<Step>());
                }
            }

            var steps = (await _entitiesRepository.GetAsync<Step>(s => s.CreatedOn > lastRecordTime && s.Status == StepStatuses.Unassigned, null, "CreatedOn:1", 10000, 0)).ToList();
            if (steps.Count() > 0)
            {
                foreach (var step in steps)
                {
                    assignmentCache[step.StepTemplateId].Enqueue(step);
                }
                lastRecordTime = steps.Last().CreatedOn;
            }
            else
            {
                await Task.Delay(100);
            }
            return true;
        }

        public Step GetNext(string[] stepTemplateIds)
        {
            foreach (var stepTemplate in stepTemplateIds.OrderBy(x => rnd.Next()).ToArray())
            {
                try
                {
                    var step = assignmentCache[stepTemplate].Dequeue();
                    return step;
                }
                catch (InvalidOperationException e)
                {
                }
            }
            return null;
        }

        public StepTemplate GetStepTemplate(string referenceId)
        {
            return stepTemplatesCache[referenceId];
        }

        public void Start()
        {
            updateCache = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        stopwatch.Restart();
                        await RefreshCache();
                        _logger.LogInformation("Finished refreshing cache, took " + stopwatch.ElapsedMilliseconds);
                        LastRefreshTime = stopwatch.ElapsedMilliseconds;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to refresh assignment queue with error " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                    await Task.Delay(1000);
                }
            });
        }
    }
}
