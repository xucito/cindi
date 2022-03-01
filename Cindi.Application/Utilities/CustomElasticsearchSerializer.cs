using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.Utilities
{
    public class CustomElasticsearchSerializer : ConnectionSettingsAwareSerializerBase
    {
        public CustomElasticsearchSerializer(IElasticsearchSerializer builtinSerializer, IConnectionSettingsValues connectionSettings) : base(builtinSerializer, connectionSettings)
        {
        }

        protected override JsonSerializerSettings CreateJsonSerializerSettings() =>
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Converters = new List<JsonConverter>()
                {
                    //new ObjectJsonConverter()
                }
            };
    }
}
