using Cindi.Domain.ValueObjects;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace Cindi.Domain.Entities.ExecutionTemplates
{
    public class ExecutionTemplate: TrackedEntity
    {
        public string Name { get; set; }

        [Text]
        [JsonIgnore]
        public string _Inputs { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        [Text(Ignore = true)]
        public Dictionary<string, object> Inputs { get {
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(_Inputs);
                foreach (var input in values)
                {
                    if(input.Value is JsonValueKind.String)
                    {
                        values[input.Key] = input.Value.ToString();
                    }
                }
                
                return values; } set {
                Dictionary<string, object> finalList = new Dictionary<string, object>();

                foreach(var kv in value)
                {
                    if (kv.Value is JsonElement)
                    {
                        finalList.Add(kv.Key, kv.Value.ToString());
                    }
                    else
                    {
                        finalList.Add(kv.Key, kv.Value);
                    }
                }

                _Inputs = JsonConvert.SerializeObject(finalList); } }
        public string ReferenceId { get; set; }
        public string ExecutionTemplateType { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }
    }
}
