using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Domain.Converters
{
    public class DataJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> ReadJson(JsonReader reader, Type objectType, Dictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var test = reader.ReadAsString();
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(test);
        }

        public override void WriteJson(JsonWriter writer, Dictionary<string, object> value, JsonSerializer serializer)
        {
            var json = "asd";// JsonConvert.SerializeObject(value).Replace("\"", "\\\"");
            writer.WriteValue(json);
        }
    }
}
