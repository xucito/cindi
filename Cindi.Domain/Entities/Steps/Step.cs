using Cindi.Domain.Entities.StepTests;
using Cindi.Domain.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Domain.Entities.Steps
{
    public class Step
    {
        public Step()
        {
            Inputs = new List<DynamicData>();
            Outputs = new List<DynamicData>();
            Tests = new List<TemplateReference>();
            SuspendedTimes = new List<DateTimeOffset>();
        }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// The sequence this step belongs to 
        /// </summary>
        public int? SequenceId { get; set; }

        /// <summary>
        /// Used to map to a specific step in a sequence
        /// </summary>
        public int StepRefId { get; set; }

        public List<TemplateReference> Tests { get; set; }
        public List<StepTestResult> TestResults { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        [Required]
        public TemplateReference StepTemplateReference { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public List<DynamicData> Inputs { get; set; }

        /// <summary>
        /// Output from task, the output name is the dictionary key and the value is Dictionary value
        /// </summary>
        public List<DynamicData> Outputs { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset AssignedOn { get; set; }

        /// <summary>
        /// Suspended times
        /// </summary>
        public List<DateTimeOffset> SuspendedTimes { get; set; }

        /// <summary>
        /// Completed is the date the step is moved to a completed queue
        /// </summary>
        public DateTimeOffset CompletedOn { get; set; }

        private string _status { get; set; }
        public string Status
        {
            get { return _status; }
            set
            {
                if (!StepStatuses.AllStatuses.Contains(value))
                {
                    throw new InvalidOperationException("Status " + value + " is not a valid step status");
                }
                _status = value;
            }
        }

        /// <summary>
        /// Combined with Status can be used to evaluate dependencies
        /// </summary>
        public int StatusCode { get; set; }

        public string Log { get; set; }

        public bool IsComplete
        {
            get
            {
                if (_status == StepStatuses.Warning ||
                    _status == StepStatuses.Successful ||
                    _status == StepStatuses.Error
                    )
                {
                    return true;
                }
                return false;
            }
        }
    }



    /*
    public class DataTypes
    {
        public static string Int { get { return "int"; } }
        public static string String { get { return "string"; } }
        public static string Bool { get { return "bool"; } }
        public static string Object { get { return "object"; } }
    };*/

}
