using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.StepTemplates;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.SequenceTemplates
{
    public class SequenceTemplatesRepository : BaseRepository, ISequenceTemplatesRepository
    {
        public IMongoCollection<SequenceTemplate> _sequenceTemplate;

        public SequenceTemplatesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }
        
        public SequenceTemplatesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _sequenceTemplate = database.GetCollection<SequenceTemplate>("SequenceTemplates");
        }

        public long CountSequenceTemplates() { return _sequenceTemplate.EstimatedDocumentCount(); }

        public async Task<SequenceTemplate> InsertSequenceTemplateAsync(SequenceTemplate sequenceTemplate)
        {
            await _sequenceTemplate.InsertOneAsync(sequenceTemplate);
            return sequenceTemplate;
        }

        public async Task<SequenceTemplate> GetSequenceTemplateAsync(string sequenceTemplateId)
        {
            var foundSequenceTemplate = (await _sequenceTemplate.FindAsync(s => s.Id == sequenceTemplateId)).FirstOrDefault();
            return foundSequenceTemplate;
        }

        public async Task<List<SequenceTemplate>> GetSequenceTemplatesAsync(int page = 0, int size = 10)
        {
            FilterDefinition<SequenceTemplate> filter = FilterDefinition<SequenceTemplate>.Empty;
            FindOptions<SequenceTemplate> options = new FindOptions<SequenceTemplate>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var sequences = (await _sequenceTemplate.FindAsync(filter, options)).ToList();

            return sequences;
        }
    }
}
