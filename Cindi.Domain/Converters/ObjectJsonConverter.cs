using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Domain.Converters
{
    public class ObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var test = reader.ReadAsString();
            try
            {
                return JsonConvert.DeserializeObject<object>(test);
            }
            catch (JsonReaderException ex)
            {
                return test;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }
    }
}
