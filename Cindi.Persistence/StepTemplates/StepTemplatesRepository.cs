using Cindi.Domain.Entities.StepTemplates;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;
using Cindi.Domain.Exceptions;
using Cindi.Application.Interfaces;
using MongoDB.Bson.Serialization;

namespace Cindi.Persistence.StepTemplates
{
    public class StepTemplatesRepository : BaseRepository, IStepTemplatesRepository
    {
        private IMongoCollection<StepTemplate> _stepTemplates;

        public StepTemplatesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public StepTemplatesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _stepTemplates = database.GetCollection<StepTemplate>("StepTemplates");
        }

        public async Task<StepTemplate> InsertAsync(StepTemplate stepTemplate)
        {
            try
            {
                stepTemplate.CreatedOn = DateTime.UtcNow;
                await _stepTemplates.InsertOneAsync(stepTemplate);
                return stepTemplate;
            }
            catch (MongoDB.Driver.MongoWriteException e)
            {
                if (e.Message.Contains("E11000 duplicate key error collection"))
                {
                    var existingTemplate = await GetStepTemplateAsync(stepTemplate.Name, stepTemplate.Version);
                    BaseException comparisonException;
                    var isConflicting = existingTemplate.IsEqual(stepTemplate, out comparisonException);
                    if(!isConflicting)
                    {
                        throw comparisonException;
                    }

                    return existingTemplate;
                }
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public async Task<StepTemplate> GetStepTemplateAsync(string name, string version)
        {
            return (await _stepTemplates.FindAsync(st => st.Name == name.ToLower() && st.Version == version)).FirstOrDefault();
        }

        public async Task<List<StepTemplate>> GetStepTemplatesAsync(int page, int size)
        {
            FilterDefinition<StepTemplate> filter = FilterDefinition<StepTemplate>.Empty;
            FindOptions<StepTemplate> options = new FindOptions<StepTemplate>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page
            };

            return (await _stepTemplates.FindAsync(filter, options)).ToList();
        }
    }
}
