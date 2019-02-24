using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.StepTests;
using Cindi.Domain.ValueObjects;
using Cindi.Domain.ValueObjects.Journal;
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
            Inputs = new Dictionary<string, object>();
            //Outputs = new List<DynamicData>();
            Tests = new List<TemplateReference>();
            //SuspendedTimes = new List<DateTime>();
        }
        

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// The sequence this step belongs to 
        /// </summary>
        public string SequenceId { get; set; }

        /// <summary>
        /// Used to map to a specific step in a sequence
        /// </summary>
        public int? StepRefId { get; set; }

        public List<TemplateReference> Tests { get; set; }
        public List<StepTestResult> TestResults { get { return Journal.GetLatestValueOrDefault<List<StepTestResult>>("testresults", null); } }

        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        [Required]
        public TemplateReference TemplateReference { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }


        public DateTime CreatedOn { get; set; }

        /*  public DateTime AssignedOn { get; set; }

          /// <summary>
          /// Suspended times
          /// </summary>
          public List<DateTime> SuspendedTimes { get; set; }
          */
        /// <summary>
        /// Completed is the date the step is moved to a completed queue
        /// </summary>
        public DateTime? CompletedOn
        {
            get
            {
                var lastStatusAction = Journal.GetLatestAction("status");
                if (lastStatusAction != null && StepStatuses.IsCompleteStatus((string)lastStatusAction.Update.Value))
                {
                    return lastStatusAction.RecordedOn;
                }
                return null;
            }
        }

        public string Status
        {
            get
            {
                return Journal.GetLatestValueOrDefault("status", StepStatuses.Unassigned);
            }
        }

        /// <summary>
        /// Output from task, the output name is the dictionary key and the value is Dictionary value
        /// </summary>
        public List<DynamicData> Outputs { get { return Journal.GetLatestValueOrDefault<List<DynamicData>>("outputs", null); } }

        /// <summary>
        /// Combined with Status can be used to evaluate dependencies
        /// </summary>
        //public int StatusCode { get; set; }

        //public string Log { get; set; }

        public bool IsComplete
        {
            get
            {
                var lastStatusAction = Journal.GetLatestAction("status");
                if (lastStatusAction != null && StepStatuses.IsCompleteStatus((string)lastStatusAction.Update.Value))
                {
                    return true;
                }
                return false;
            }
        }

        public Journal Journal { get; set; }
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
