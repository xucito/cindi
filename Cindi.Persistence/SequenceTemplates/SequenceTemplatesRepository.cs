using Cindi.Domain.Entities.StepTemplates;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.SequenceTemplates
{
    public class SequenceTemplatesRepository
    {
        public readonly IMongoCollection<StepTemplate> _stepTemplates;

        public SequenceTemplatesRepository(string mongoDbConnectionString, string databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            var database = client.GetDatabase(databaseName);
            _stepTemplates = database.GetCollection<StepTemplate>("SequenceTemplates");
        }

        public StepTemplate Create(StepTemplate stepTemplate)
        {
            _stepTemplates.InsertOne(stepTemplate);
            return stepTemplate;
        }
    }
}
