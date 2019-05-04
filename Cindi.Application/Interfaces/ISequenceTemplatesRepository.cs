using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.SequencesTemplates;

namespace Cindi.Application.Interfaces
{
    public interface ISequenceTemplatesRepository
    {
        Task<SequenceTemplate> GetSequenceTemplateAsync(string sequenceTemplateId);
        Task<List<SequenceTemplate>> GetSequenceTemplatesAsync(int page = 0, int size = 10);
        Task<string> InsertSequenceTemplateAsync(SequenceTemplate sequenceTemplate);
        long CountSequenceTemplates();
    }
}