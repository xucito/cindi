using System;
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
        Task<Sequence> GetSequenceAsync(Guid SequenceId);
        Task<List<Sequence>> GetSequencesAsync(int size = 10, int page = 0, string status = null, string[] sequenceTemplateIds = null);
        Task<Guid> InsertSequenceAsync(Sequence Sequence);
        Task<List<Step>> GetSequenceStepsAsync(Guid sequenceId);
        Task<bool> UpsertSequenceMetadataAsync(Guid sequenceId);
        Task<bool> UpdateSequence(Sequence sequence);
    }
}