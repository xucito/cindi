using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Cindi.Domain.ValueObjects;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Entities.JournalEntries;

namespace Cindi.Domain.Entities.SequencesTemplates
{
    public class SequenceTemplate : TrackedEntity
    {
        public SequenceTemplate()
        {
            this.LogicBlocks = new List<LogicBlock>();

            ShardType = typeof(SequenceTemplate).Name;
        }

        public SequenceTemplate(Journal journal) : base(journal)
        {
            ShardType = typeof(SequenceTemplate).Name;
        }

        public SequenceTemplate(
            string id,
            string description,
            Dictionary<string, DynamicDataDescription> inputDefinitions,
            List<LogicBlock> logicBlocks,
            string createdBy,
            DateTime createdOn
            ) : base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "id",
                        Value = id,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "description",
                        Value = description,
                        Type = UpdateType.Create
                    },
                   new Update()
                    {
                        FieldName = "inputdefinitions",
                        Value = inputDefinitions,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "logicblocks",
                        Value = logicBlocks,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdon",
                        Value = createdOn,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdby",
                        Value = createdBy,
                        Type = UpdateType.Create
                    }
                }
            }))
        {
            ShardType = typeof(SequenceTemplate).Name;
        }

        public string Id { get; set; }
        public string Name { get { return Id.Split(':')[0]; } }
        public string Version { get { return Id.Split(':')[1]; } }

        public string Description { get; set; }
        public List<LogicBlock> LogicBlocks { get; set; }

        /// <summary>
        /// Input from dependency with input name is the dictionary key and the type as the Dictionary value
        /// </summary>
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; }

        public static bool ValidateMapping(Mapping map, bool isStartingMapping = false)
        {
            if (isStartingMapping)
            {
                if (map.OutputReferences.Count() == 0 || map.OutputReferences.Count() > 1)
                {
                    throw new InvalidMappingException("All starting mappings must have one output reference as the sequence output is always available");
                }

                foreach (var reference in map.OutputReferences)
                {
                    if (reference.StepRefId != -1)
                    {
                        throw new InvalidMappingException("All starting mappings must be mapping from step -1 to step 0");
                    }
                }
            }

            if (map.DefaultValue == null && (map.OutputReferences == null || map.OutputReferences.Count() == 0))
            {
                throw new InvalidMappingException("Both Value and Output reference are not specified for mapping " + map.StepInputId);
            }

            return true;
        }
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

        public string StepTemplateReferenceId { get; set; }

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
        public string StepTemplateId { get; set; }
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
        public object Value { get; set; }
        public int Priority = 99999999;
    }

}
