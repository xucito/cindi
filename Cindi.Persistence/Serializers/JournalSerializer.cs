using Cindi.Domain.Entities.JournalEntries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Serializers
{
    public class JournalSerializer : SerializerBase<Journal>
    {
        public override Journal Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();

            var result = BsonExtensionMethods.ToJson(bsonDocument);
            return JsonConvert.DeserializeObject<Journal>(result);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Journal value)
        {
            //var jsonDocument = JsonConvert.SerializeObject(value);
            //var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(value);

            var serializer = BsonSerializer.LookupSerializer(typeof(Journal));
            serializer.Serialize(context, value);
        }
    }
}
