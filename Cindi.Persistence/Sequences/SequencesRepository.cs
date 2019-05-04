using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.Sequences
{
    public class SequencesRepository : BaseRepository, ISequencesRepository
    {
        public IMongoCollection<Sequence> _sequence;
        private IMongoCollection<JournalEntry> _sequenceJournalEntries;
        private IMongoCollection<JournalEntry> _stepJournalEntries;
        private IMongoCollection<Step> _steps;
        private IMongoCollection<SequenceMetadata> _sequenceMetadata;

        public SequencesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public SequencesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        public async Task<List<Step>> GetSequenceStepsAsync(Guid sequenceId)
        {
            var steps = (await _steps.FindAsync(s => s.SequenceId != null && s.SequenceId.Value == sequenceId)).ToList();

            return steps;
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _sequence = database.GetCollection<Sequence>("Sequences");
            _sequenceJournalEntries = database.GetCollection<JournalEntry>("JournalEntries");
            _steps = database.GetCollection<Step>("Steps");
            _stepJournalEntries = database.GetCollection<JournalEntry>("JournalEntries");
            _sequenceMetadata = database.GetCollection<SequenceMetadata>("_Sequences");
        }

        public long CountSequences() { return _sequence.EstimatedDocumentCount(); }

        public async Task<Guid> InsertSequenceAsync(Sequence Sequence)
        {
            await _sequence.InsertOneAsync(Sequence);
            return Sequence.Id;
        }

        public async Task<Sequence> GetSequenceAsync(Guid SequenceId)
        {
            var foundSequence = (await _sequence.FindAsync(s => s.Id == SequenceId)).FirstOrDefault();
            return foundSequence;
        }

        public async Task<List<Sequence>> GetSequencesAsync(Guid[] sequenceIds)
        {
            var builder = Builders<Sequence>.Filter;
            var sequenceFilter = builder.In("Id", sequenceIds);
            var sequences = (await _sequence.FindAsync(sequenceFilter)).ToList();

            return sequences;
        }

        public async Task<bool> UpdateSequence(Sequence sequence)
        {
            var result = await _sequence.ReplaceOneAsync(
                  doc => doc.Id == sequence.Id,
                  sequence,
                  new UpdateOptions
                  {
                      IsUpsert = false
                  });

            if (result.IsAcknowledged)
            {
                return true;
            }
            else
            {
                throw new SequenceUpdateFailureException("Update of sequence " + sequence.Id + " failed.");
            }
        }

        public async Task<List<Sequence>> GetSequencesAsync(int size = 10, int page = 0, string status = null, string[] sequenceTemplateIds = null)
        {
            if (status != null && !SequenceStatuses.IsValid(status))
            {
                throw new InvalidSequenceStatusException(status + " is not a valid step status entry.");
            }

            var builder = Builders<SequenceMetadata>.Filter;
            var filters = new List<FilterDefinition<SequenceMetadata>>();
            var ignoreFilters = true;

            if (sequenceTemplateIds != null)
            {
                filters.Add(builder.In("SequenceId", sequenceTemplateIds));
                ignoreFilters = false;
            }
            if (status != null)
            {
                filters.Add(builder.Eq("Status", status));
                ignoreFilters = false;
            }

            var stepMetadataFilter = builder.And(filters);

            if (ignoreFilters)
            {
                stepMetadataFilter = FilterDefinition<SequenceMetadata>.Empty; ;
            }

            var sort = Builders<SequenceMetadata>.Sort.Ascending("CreatedOn");
            FindOptions<SequenceMetadata> options = new FindOptions<SequenceMetadata>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size,
                Sort = sort
            };

            var validSequences = (await _sequenceMetadata.FindAsync(stepMetadataFilter, options)).ToList();

            return await GetSequencesAsync(validSequences.Select(vs => vs.SequenceId).ToArray());
        }

        public async Task<bool> UpsertSequenceMetadataAsync(Guid sequenceId)
        {
            var sequenceToUpdate = await GetSequenceAsync(sequenceId);

            var md = sequenceToUpdate.Metadata;

            var replaceResult = await _sequenceMetadata.ReplaceOneAsync(
                    doc => doc.SequenceId == sequenceId,
                    sequenceToUpdate.Metadata,
                    new UpdateOptions { IsUpsert = true }
                    );

            return replaceResult.IsAcknowledged;
        }
    }
}
