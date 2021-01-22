using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.Conditions
{
    public class ConditionSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ConditionSerializer).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            // Load JObject from stream 
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject 
            Condition target = Create(objectType, jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected Condition Create(Type objectType, Newtonsoft.Json.Linq.JObject jObject)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(Condition)))
                    {
                        var name = jObject.ContainsKey("name") ? jObject.Value<string>("name") : jObject.Value<string>("Name");
                        if (!t.IsGenericTypeDefinition && name == ((Condition)Activator.CreateInstance(t)).Name)
                        {
                            return (Condition)Activator.CreateInstance(t);
                        }
                    }
                }
            }

            throw new Exception("Routed request configuration was not found");
        }

    }
}
