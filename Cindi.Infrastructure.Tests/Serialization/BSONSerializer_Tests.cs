/*using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence;
using Cindi.Test.Global.TestData;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Cindi.Infrastructure.Tests.Serialization
{
    public class BSONSerializer_Tests : IDisposable
    {
        string testDBName;
        IMongoClient client;

        public BSONSerializer_Tests()
        {
            BaseRepository.RegisterClassMaps();
            //https://mongodb.github.io/mongo-csharp-driver/1.11/serialization/#supplementing-the-default-serializer-provider
            //BsonSerializer.RegisterSerializationProvider(new ObjectSerializerProvider());
            // BsonClassMap.RegisterClassMap<Update>(gv => UpdateClassMap.Register(gv));
        }

        public void Dispose()
        {
            client.DropDatabase(testDBName);

        }

        [Fact]
        public void SerializeObjects()
        {

            testDBName = "test_" + MethodBase.GetCurrentMethod().Name;
            client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase(testDBName);
            var collection = db.GetCollection<Step>("Test");
            Guid test = new Guid();
            var step = FibonacciSampleData.Step;
            step.Inputs.Add("test", new
            {
                blue = "this is the one"
            });

            collection.InsertOne(step);


            var result = collection.Find(t => true).FirstOrDefault();

            Assert.Equal(typeof(System.Dynamic.ExpandoObject), result.Inputs["test"].GetType());


            var newStep = new Step(result.Journal);
            // BsonSerializer.Serialize(, )
        }
    }
}
*/