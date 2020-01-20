using Cindi.Domain.Entities.Steps;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Serializers
{
    public class ObjectSerializerProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(BsonDocument))
            {
                return new ObjectSerializer();
            }

            return null;
        }
    }

    public class ObjectSerializer : IBsonSerializer
    {
        /*public Type ValueType => typeof(BsonDocument);

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = BsonDocumentSerializer.Instance.Deserialize(context, args);
            BsonValue outputted;
            if (value.TryGetValue("value", out outputted))
            {
                return JsonConvert.DeserializeObject<BsonDocument>(outputted.AsString);
            }
            else
            {
                throw new Exception("Failed to deserialize bson value Object Type");
            }
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            BsonStringSerializer.Instance.Serialize(BsonSerializationContext.CreateRoot(context.Writer), args, "test");//(BsonDocument)value);
                                                                                                                        context.Writer.WriteStartDocument();
                                                                                                                        context.Writer.WriteName("_t");
                                                                                                                        context.Writer.WriteString("System.Object");
                                                                                                                        context.Writer.WriteName("value");
                                                                                                                       //      context.Writer.WriteString(JsonConvert.SerializeObject(value));
                                                                                                                       //  context.Writer.WriteEndDocument();
        }*/

        public Type ValueType => typeof(object);

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            /*var type = context.Reader.GetCurrentBsonType();
            switch (type)
            {
                case BsonType.Binary:
                    var binaryData = context.Reader.ReadBinaryData();
                    var subType = binaryData.SubType;
                    if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
                    {
                        return binaryData.ToGuid();
                    }
                    else
                    {
                        return binaryData.AsByteArray;
                    }
                    break;
            }*/
            return BsonSerializer.Deserialize<object>(context.Reader);
            /*var value = BsonValueSerializer.Instance.Deserialize(context, args);
            BsonValue typing;
            if (value.IsBsonDocument && (value.AsBsonDocument.TryGetValue("_t", out typing)))
            {
                if (typing.AsString == "System.Object")
                {
                    BsonValue rawValue;
                    value.AsBsonDocument.TryGetValue("value", out rawValue);
                    return JObject.Parse(rawValue.AsString);
                }
            }*/
            return BsonSerializer.Deserialize<object>(context.Reader);
            //return BsonTypeMapper.MapToDotNetValue(value);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value != null)
            {
                var valueType = value.GetType();
                var manual = (new object()).GetType();
                if (valueType.FullName == typeof(System.Object).FullName)
                {
                    //context.Writer.WriteBinaryData(BsonBinaryData.Create(value));
                    context.Writer.WriteStartDocument();
                    context.Writer.WriteName("_t");
                    context.Writer.WriteString("System.Object");
                    context.Writer.WriteName("value");
                    context.Writer.WriteString(JsonConvert.SerializeObject(value));
                    context.Writer.WriteName("_id");
                    context.Writer.WriteObjectId(ObjectId.GenerateNewId());
                    context.Writer.WriteEndDocument();
                    return;
                }
                BsonSerializer.Serialize(context.Writer, value.GetType(), value);
            }
            else
            {
                BsonSerializer.Serialize(context.Writer, typeof(object), value);
            }
        }
    }
}
