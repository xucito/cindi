using Cindi.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Serializers
{
    public class TrackedEntitySerializer : SerializerBase<TrackedEntity>
    {
        public override TrackedEntity Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();
            var result = BsonExtensionMethods.ToJson(bsonDocument);
            var jsonSerializerSettings = new JsonSerializerSettings();
            return JsonConvert.DeserializeObject<TrackedEntity>(bsonDocument.ToString(), jsonSerializerSettings);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TrackedEntity value)
        {
            TrackedEntity tempCache = value;
            var jsonDocument = JsonConvert.SerializeObject(tempCache);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }
}
