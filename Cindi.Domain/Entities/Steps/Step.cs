using Cindi.Domain.Converters;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.StepTests;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Cindi.Domain.Utilities.SecurityUtility;

namespace Cindi.Domain.Entities.Steps
{
    public class Step : TrackedEntity
    {

        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// The workflow this step belongs to 
        /// </summary>
        public Guid? WorkflowId { get; set; }

        //public new Guid Id { get; set; }

        [Required]
        public string StepTemplateId { get; set; }

        [Text]
        [JsonIgnore]
        public string _Inputs { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        [Text(Ignore = true)]
        public Dictionary<string, object> Inputs { get { return JsonConvert.DeserializeObject<Dictionary<string, object>>(_Inputs); } set { _Inputs = JsonConvert.SerializeObject(value); } }

        /*  public DateTime AssignedOn { get; set; }

          /// <summary>
          /// Suspended times
          /// </summary>
          public List<DateTime> SuspendedTimes { get; set; }
          */
        /// <summary>
        /// Completed is the date the step is moved to a completed queue
        /// </summary>
        public DateTimeOffset? CompletedOn { get; set; }

        public string Status { get; set; }


        [Text]
        public string _Outputs { get; set; }

        /// <summary>
        /// Output from task, the output name is the dictionary key and the value is Dictionary value
        /// </summary>
        [Text(Ignore = true)]
        public Dictionary<string, object> Outputs { get { 
                return (_Outputs != null ? JsonConvert.DeserializeObject<Dictionary<string, object>>(_Outputs) : null); } set { _Outputs = JsonConvert.SerializeObject(value); } }

        /// <summary>
        /// Combined with Status can be used to evaluate dependencies
        /// </summary>
        public int StatusCode { get; set; }

        public List<StepLog> Logs { get; set; } = new List<StepLog>();

        public bool IsComplete()
        {
            if (Status != null && StepStatuses.IsCompleteStatus((string)Status))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The bot that has the step assigned to
        /// </summary>
        public Guid? AssignedTo { get; set; }

        public DateTimeOffset? SuspendedUntil { get; set; }

        public Guid? ExecutionTemplateId { get; set; }
        public Guid? ExecutionScheduleId { get; set; }

        public void RemoveDelimiters()
        {
            Dictionary<string, object> convertedInput = new Dictionary<string, object>();

            foreach (var input in Inputs)
            {
                if (input.Value is string && ((string)input.Value).Length > 1)
                {
                    var convertedStringInput = (string)input.Value;
                    if (convertedStringInput.Length > 2 && convertedStringInput[0] == '\\' && convertedStringInput[1] == '$')
                    {
                        convertedInput.Add(input.Key, convertedStringInput.Substring(1, convertedStringInput.Length - 1));
                    }
                    else
                    {
                        convertedInput.Add(input.Key, input.Value);
                    }
                }
                else
                {
                    convertedInput.Add(input.Key, input.Value);
                }
            }

            Inputs = convertedInput;
        }
    }
}
