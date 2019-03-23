using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.StepTests;
using Cindi.Domain.Exceptions.Steps;
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
    public class Step: TrackedEntity
    {
        public Step()
        {
            Inputs = new Dictionary<string, object>();
            //Outputs = new List<DynamicData>();
            Tests = new List<string>();
            //SuspendedTimes = new List<DateTime>();
            Journal = new Journal(new List<JournalEntry>());
        }


        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// The sequence this step belongs to 
        /// </summary>
        public Guid? SequenceId { get; set; }

        /// <summary>
        /// Used to map to a specific step in a sequence
        /// </summary>
        public int? StepRefId { get; set; }
        public List<string> Tests { get; set; }
        public List<StepTestResult> TestResults { get { return Journal.GetLatestValueOrDefault<List<StepTestResult>>("testresults", new List<StepTestResult>()); } }

        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        [Required]
        public string StepTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

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
                    return lastStatusAction.CreatedOn;
                }
                return null;
            }
        }

        public string Status
        {
            get
            {
                var status = Journal.GetLatestValueOrDefault<string>("status", null);
                if (status == null)
                {
                    return StepStatuses.Unknown;
                    //throw new InvalidStepStatusInputException("Status for step " + Id + " was not found.");
                }
                return status;
            }
        }

        /// <summary>
        /// Output from task, the output name is the dictionary key and the value is Dictionary value
        /// </summary>
        public Dictionary<string, object> Outputs { get { return Journal.GetLatestValueOrDefault<Dictionary<string, object>>("outputs", new Dictionary<string, object>()); } }

        /// <summary>
        /// Combined with Status can be used to evaluate dependencies
        /// </summary>
        public int StatusCode { get { return Journal.GetLatestValueOrDefault<int>("statuscode", 0); } }

        public List<StepLog> Logs
        {
            get
            {
                List<string> allLogs = new List<string>();
                return Journal.GetAllUpdates("logs").Select(l => new StepLog()
                {
                    Message = (string)l.Update.Value,
                    CreatedOn = l.CreatedOn
                }).OrderBy(l => l.CreatedOn).ToList();
            }
        }

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

        public StepMetadata Metadata { get {
                return new StepMetadata()
                {
                    StepId = Id,
                    Status = Status,
                    StepTemplateId = StepTemplateId,
                    CreatedOn = DateTime.UtcNow
                };
            } }

        public DateTime? SuspendedUntil
        {
            get
            {
                var lastAction = Journal.GetLatestAction("suspendedUntil");

                if(lastAction == null)
                {
                    return null;
                }

                var lastSuspension = (DateTime)lastAction.Update.Value;
                if (Status == StepStatuses.Suspended)
                {
                    return lastSuspension;
                }
                return null;
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
