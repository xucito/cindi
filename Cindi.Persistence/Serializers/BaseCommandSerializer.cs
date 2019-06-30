using ConsensusCore.Domain.BaseClasses;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Cindi.Persistence.Serializers
{
    public class BaseCommandSerializer : SerializerBase<BaseCommand>
    {
        public override BaseCommand Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();

            var result = BsonExtensionMethods.ToJson(bsonDocument);
            var test = JsonConvert.DeserializeObject<BaseCommand>(result);
            return JsonConvert.DeserializeObject<BaseCommand>(result);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BaseCommand value)
        {
            var jsonDocument = JsonConvert.SerializeObject(value);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }
}
