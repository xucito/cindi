using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.StepTemplates
{
    public class StepTemplate : TrackedEntity
    {
        public StepTemplate()
        {
            ShardType = typeof(StepTemplate).Name;
        }

        public StepTemplate(Journal journal): base (journal)
        {
            ShardType = typeof(StepTemplate).Name;
        }

        public StepTemplate(
            Guid id,
            string referenceId,
            string description,
            bool allowDynamicInputs,
            Dictionary<string, DynamicDataDescription> inputDefinitions,
            Dictionary<string, DynamicDataDescription> outputDefinitions,
            string CreatedBy,
            DateTime CreatedOn) : base(
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
                        FieldName = "allowdynamicinputs",
                        Value = allowDynamicInputs,
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
                        FieldName = "outputdefinitions",
                        Value = outputDefinitions,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdon",
                        Value = CreatedOn,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdby",
                        Value = CreatedBy,
                        Type = UpdateType.Create
                    }
                }
            })
            )
        {
            Id = id;
            ShardType = typeof(StepTemplate).Name;
        }

        public string ReferenceId { get; set; }

        public string Name { get { return ReferenceId.Split(':')[0]; } }
        public string Version { get { return ReferenceId.Split(':')[1]; } }

        public string Description { get; set; }

        /// <summary>
        /// Dynamic inputs will default type string
        /// </summary>
        public bool AllowDynamicInputs { get; set; }

        /*public TemplateReference Reference
        {
            get
            {
                return new TemplateReference(Id);
            }
        }*/

        /// <summary>
        /// Input from dependency with input name is the dictionary key and the type as the Dictionary value
        /// </summary>
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; }

        /// <summary>
        ///  Output from task, the output name is the dictionary key and the type is Dictionary value
        ///  Value is the object type based on serialized string i.e.
        ///  {
        ///    name: string,
        ///    value: number
        ///  }
        /// </summary>
        public Dictionary<string, DynamicDataDescription> OutputDefinitions { get; set; }

        /// <summary>
        /// Checks whether the step matches the step definition
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool StepMatches(Step step)
        {
            if (step.StepTemplateId == ReferenceId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsEqual(StepTemplate stepTemplate, out BaseException exception)
        {
            if (stepTemplate.InputDefinitions.Count() != InputDefinitions.Count())
            {
                exception = new ConflictingStepTemplateException("Found existing template with conflicting inputs, the number of inputs is different.");
                return false;
            }

            if(stepTemplate.AllowDynamicInputs != AllowDynamicInputs)
            {
                exception = new ConflictingStepTemplateException("Found existing template with conflicting settings, Allow dynamics input is different.");
                return false;
            }

            if ((stepTemplate.OutputDefinitions == null && OutputDefinitions != null) ||
                (stepTemplate.OutputDefinitions != null && OutputDefinitions == null)
                || (stepTemplate.OutputDefinitions.Count() != OutputDefinitions.Count()))
            {
                exception = new ConflictingStepTemplateException("Found existing template with conflicting inputs, the number of inputs is different.");
                return false;
            }

            foreach (var input in stepTemplate.InputDefinitions)
            {

                var foundInputs = InputDefinitions.Where(i => i.Key == input.Key);
                if (foundInputs.Count() == 0)
                {
                    exception = new ConflictingStepTemplateException("Missing input " + input.Key + " in existing template.");
                    return false;
                }


                if (foundInputs.First().Value.Type != input.Value.Type)
                {
                    exception = new ConflictingStepTemplateException("Non matching type for input " + input.Key + ".");
                    return false;
                }
            }

            foreach (var output in stepTemplate.OutputDefinitions)
            {

                var foundOutputs = OutputDefinitions.Where(i => i.Key == output.Key);
                if (foundOutputs.Count() == 0)
                {
                    exception = new ConflictingStepTemplateException("Missing output " + output.Key + " in existing template.");
                    return false;
                }


                if (foundOutputs.First().Value.Type != output.Value.Type)
                {
                    exception = new ConflictingStepTemplateException("Non matching type for output " + output.Key + ".");
                    return false;
                }
            }

            exception = null;
            return true;
        }

        public Step GenerateStep(string stepTemplateId, string createdBy, string name = "", string description = "", Dictionary<string, object> inputs = null, List<string> stepTestTemplateIds = null, int? stepRefId = null, Guid? sequenceId = null, string encryptionKey = "")
        {
            var verifiedInputs = new Dictionary<string, object>();

            if (inputs != null)
            {
                if (inputs.Count() > InputDefinitions.Count() && !AllowDynamicInputs)
                {
                    throw new InvalidStepInputException("Too many step inputs for step template " + Id + " were given, expected " + InputDefinitions.Count() + " got " + inputs.Count());
                }

                if (InputDefinitions.Count() > inputs.Count())
                {
                    string missingInputs = "";
                    foreach (var id in InputDefinitions)
                    {
                        if (!inputs.Select(i => i.Key).Contains(id.Key))
                        {
                            missingInputs += id.Key + " ";
                        }
                    }
                    throw new InvalidStepInputException("Missing step inputs for step template " + Id + ", expected " + InputDefinitions.Count() + " got " + inputs.Count() + " missing ");
                }

                foreach (var input in inputs)
                {
                    var foundTemplate = InputDefinitions.Where(tp => tp.Key == input.Key).FirstOrDefault();

                    if (!IsStepInputValid(input))
                    {
                        throw new InvalidStepInputException("Step input " + input.Key + " is not found in the template definition.");
                    }

                    if ((AllowDynamicInputs && !InputDefinitions.ContainsKey(input.Key)) || InputDefinitions.ContainsKey(input.Key))
                    {
                        if (InputDefinitions.ContainsKey(input.Key) && InputDefinitions[input.Key].Type == InputDataTypes.Secret && !InputDataUtility.IsInputReference(input, out _, out _))
                        {
                            verifiedInputs.Add(input.Key.ToLower(), SecurityUtility.SymmetricallyEncrypt((string)input.Value, encryptionKey));
                        }
                        else
                        {
                            verifiedInputs.Add(input.Key.ToLower(), input.Value);
                        }
                    }
                }
            }
            else if (inputs == null && InputDefinitions.Count() > 0)
            {
                throw new InvalidStepInputException("No inputs were specified however step template " + Id + " has " + InputDefinitions.Count() + " inputs.");
            }

            var newStep = new Step(Guid.NewGuid(), name, description, stepTemplateId, createdBy, verifiedInputs, encryptionKey, stepRefId, sequenceId);
            return newStep;
        }

        public string GetInputType(string input)
        {
            foreach (var inputDef in InputDefinitions)
            {
                if (inputDef.Key.ToLower() == input.ToLower())
                {
                    return inputDef.Value.Type;
                }
            }
            throw new InputDefinitionNotFoundException("Input " + input + " does not exist in step template " + Id);
        }

        private bool IsStepInputValid(KeyValuePair<string, object> input)
        {
            try
            {
                var foundTemplate = InputDefinitions.Where(tp => tp.Key == input.Key).First();

                if (!InputDefinitions.ContainsKey(input.Key) && !AllowDynamicInputs)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
