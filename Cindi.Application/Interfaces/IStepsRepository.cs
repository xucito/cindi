﻿using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IStepsRepository
    {
        Task<List<Step>> GetStepsAsync(int page = 0, int size = 1000);
        Task<Step> GetStepAsync(Guid stepId);
        Task<Step> InsertStepAsync(Step step);
        Task<JournalEntry> InsertJournalEntryAsync(JournalEntry entry);
    }
}