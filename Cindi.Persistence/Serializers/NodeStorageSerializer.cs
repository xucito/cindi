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
using Cindi.Domain.Entities.States;

namespace Cindi.Persistence.Serializers
{
    public class NodeStorageSerializer : SerializerBase<NodeStorage<CindiClusterState>>
    {
        public override NodeStorage<CindiClusterState> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();
            bsonDocument.Remove("_id");
            bsonDocument.Remove("_saveLocker");
            bsonDocument.Remove("_saveThread");
            bsonDocument.Remove("_locker");
            var result = BsonExtensionMethods.ToJson(bsonDocument);
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return JsonConvert.DeserializeObject<NodeStorage<CindiClusterState>>(bsonDocument.ToString(), jsonSerializerSettings);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, NodeStorage<CindiClusterState> value)
        {
            NodeStorage<CindiClusterState> tempCache = value;
            var jsonDocument = JsonConvert.SerializeObject(tempCache);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }

}
