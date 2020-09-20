using Cindi.Domain.Entities;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using System.Linq;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Models;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.WorkflowsTemplates;

namespace Cindi.Persistence
{
    public abstract class BaseRepository
    {
        public BaseRepository(string databaseLocation)
        {
        }

    }
}
