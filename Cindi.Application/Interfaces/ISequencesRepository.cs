﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;

namespace Cindi.Application.Interfaces
{
    public interface ISequencesRepository
    {
        long CountSequences();
        Task<int> GetNextChainId(Guid subjectId);
        Task<Sequence> GetSequenceAsync(Guid SequenceId);
        Task<List<Sequence>> GetSequencesAsync(int page = 0, int size = 10);
        Task<JournalEntry> InsertJournalEntryAsync(JournalEntry entry);
        Task<Sequence> InsertSequenceAsync(Sequence Sequence);
        Task<List<Step>> GetSequenceStepsAsync(Guid sequenceId);
    }
}