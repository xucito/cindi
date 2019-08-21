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
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Enums;

namespace Cindi.Domain.Entities.WorkflowsTemplates
{
    public class WorkflowTemplate : TrackedEntity
    {
        public WorkflowTemplate()
        {
            this.LogicBlocks = new Dictionary<string, LogicBlock>();
            ShardType = typeof(WorkflowTemplate).Name;
        }

        public WorkflowTemplate(Journal journal) : base(journal)
        {
            ShardType = typeof(WorkflowTemplate).Name;
        }

        public WorkflowTemplate(
            Guid id,
            string referenceId,
            string description,
            Dictionary<string, DynamicDataDescription> inputDefinitions,
            Dictionary<string, LogicBlock> logicBlocks,
            string createdBy,
            DateTime createdOn
            ) : base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "referenceid",
                        Value = referenceId,
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
            Id = id;
            ShardType = typeof(WorkflowTemplate).Name;
        }

        public string ReferenceId { get; set; }
        public string Name { get { return ReferenceId.Split(':')[0]; } }
        public string Version { get { return ReferenceId.Split(':')[1]; } }

        public string Description { get; set; }
        public Dictionary<string, LogicBlock> LogicBlocks { get; set; }

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
                    throw new InvalidMappingException("All starting mappings must have one output reference as the workflow output is always available");
                }

                foreach (var reference in map.OutputReferences)
                {
                    if (reference.StepName.ToLower() != ReservedValues.WorkflowStartStepName)
                    {
                        throw new InvalidMappingException("All starting mappings must be mapping from step start to step 0");
                    }
                }
            }

            if (map.DefaultValue == null && (map.OutputReferences == null || map.OutputReferences.Count() == 0))
            {
                throw new InvalidMappingException("Both Value and Output reference are not specified for mapping.");
            }

            return true;
        }
    }
}
