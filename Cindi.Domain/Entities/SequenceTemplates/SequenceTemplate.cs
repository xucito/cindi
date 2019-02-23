using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Cindi.Domain.ValueObjects;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Steps;

namespace Cindi.Domain.Entities.SequencesTemplates
{
    public class SequenceTemplate
    {

        public SequenceTemplate()
        {
            this.LogicBlocks = new List<LogicBlock>();
            // this.StartingMapping = new List<Mapping>();
        }
        /// <summary>
        /// Name of definition
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Version of the definition
        /// </summary>
        public string Version { get; set; }

        public string Description { get; set; }

        public TemplateReference Reference
        {
            get
            {
                return new TemplateReference()
                {
                    Name = this.Name,
                    Version = this.Version
                };
            }
        }


        public List<LogicBlock> LogicBlocks { get; set; }
        //  public TemplateReference StartingStepTemplateReference { get; set; }
        //   public List<Mapping> StartingMapping { get; set; }
        /// <summary>
        /// Input from dependency with input name is the dictionary key and the type as the Dictionary value
        /// </summary>
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; }
    }

    public class LogicBlock
    {
        public LogicBlock()
        {
            PrerequisiteSteps = new List<PrerequisiteStep>();
            SubsequentSteps = new List<SubsequentStep>();
        }

        public int Id { get; set; }
        public string Condition { get; set; }
        public new List<PrerequisiteStep> PrerequisiteSteps { get; set; }
        public new List<SubsequentStep> SubsequentSteps { get; set; }
    }
    /*
    public class Dependency
    {
        public int Id { get; set; }
        /// <summary>
        /// AND or OR
        /// </summary>
        public string Condition { get; set; }
        /// <summary>
        /// Key is the output from the Step, value is the input id for which it is mapped to, type must match
        /// </summary>
        public Dictionary<int, int> OutputMappings { get; set; }
        public List<StatusId> Status { get; set; }
    }*/

    public class Logic
    {
        public static string AND { get { return "AND"; } }
        public static string OR { get { return "OR"; } }
    }

    public class PrerequisiteStep
    {
        /// <summary>
        /// Unique Id of the Step defined within the sequence
        /// </summary>
        public int StepRefId { get; set; }

        public string Description { get; set; }

        // public TemplateReference StepTemplateReference { get; set; }

        private string _status { get; set; }

        public string Status
        {
            get { return _status; }
            set
            {
                if (StepStatuses.IsValid(value))
                {
                    _status = value;
                }
                else
                {
                    throw new InvalidStepStatusInputException();
                }

            }
        }

        public int StatusCode { get; set; }
    }

    public class SubsequentStep
    {
        public SubsequentStep()
        {
            Mappings = new List<Mapping>();
            IsPriority = false;
        }

        public string Description { get; set; }

        public TemplateReference StepTemplateReference { get; set; }
        public List<Mapping> Mappings { get; set; }
        public bool IsPriority { get; set; }
        public int StepRefId { get; set; }
    }

    public class Mapping
    {
        public StepOutputReference[] OutputReferences { get; set; }

        public string Description { get; set; }

        public DefaultValue DefaultValue { get; set; }
        /// <summary>
        /// The field that the Step is mapped to
        /// </summary>
        public string StepInputId { get; set; }
    }

    public class DefaultValue
    {
        public DynamicData Value { get; set; }
        public int Priority = 99999999;
    }

}
