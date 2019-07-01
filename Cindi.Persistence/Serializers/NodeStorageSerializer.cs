using Cindi.Domain.Entities.JournalEntries;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using ConsensusCore.Domain.Services;

namespace Cindi.Persistence.Serializers
{
    public class NodeStorageSerializer : SerializerBase<NodeStorage>
    {
        public override NodeStorage Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();
            bsonDocument.Remove("_id");
            var result = BsonExtensionMethods.ToJson(bsonDocument);
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return JsonConvert.DeserializeObject<NodeStorage>(bsonDocument.ToString(), jsonSerializerSettings);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, NodeStorage value)
        {
            var jsonDocument = JsonConvert.SerializeObject(value);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }

}
