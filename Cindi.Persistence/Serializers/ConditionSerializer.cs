using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Serializers
{
    public class ConditionSerializer : SerializerBase<Condition>
    {
        public override Condition Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();

            var result = BsonExtensionMethods.ToJson(bsonDocument);
            return JsonConvert.DeserializeObject<Condition>(result);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Condition value)
        {
            var jsonDocument = JsonConvert.SerializeObject(value);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }
}
