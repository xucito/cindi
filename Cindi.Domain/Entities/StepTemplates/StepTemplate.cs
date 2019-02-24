using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.StepTemplates
{
    public class StepTemplate
    {
        // Will always be set to Name:Version
        public string Id { get { return Reference.TemplateId; } }
        /// <summary>
        /// Name of definition
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Version of the definition
        /// </summary>
        public string Version { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Dynamic inputs will default type string
        /// </summary>
        public bool AllowDynamicInputs = false;

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
            if (step.TemplateReference.TemplateId == Reference.TemplateId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DateTime CreatedOn { get; set; }

        public bool IsEqual(StepTemplate stepTemplate, out BaseException exception)
        {
            if (stepTemplate.InputDefinitions.Count() != InputDefinitions.Count())
            {
                exception = new ConflictingStepTemplateException("Found existing template with conflicting inputs, the number of inputs is different.");
                return false;
            }

            if (stepTemplate.OutputDefinitions.Count() != OutputDefinitions.Count())
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

        public Step GenerateStep(TemplateReference templateReference, string name = "", string description = "", Dictionary<string, object> inputs = null, List<TemplateReference> tests = null, int? stepRefId = null, string sequenceId = null)
        {
            var newStep = new Step();
            newStep.Name = name;
            newStep.Description = description;
            newStep.TemplateReference = templateReference;
            newStep.Inputs = new Dictionary<string, object>();

            if (inputs != null)
            {
                if(inputs.Count() > InputDefinitions.Count() && !AllowDynamicInputs)
                {
                    throw new InvalidStepInputException("Too many step inputs for step template " + Id + " were given, expected " + InputDefinitions.Count() + " got " + inputs.Count());
                }

                if(InputDefinitions.Count() > inputs.Count())
                {
                    string missingInputs = "";
                    foreach(var id in InputDefinitions)
                    {
                        if(!inputs.Select(i => i.Key).Contains(id.Key))
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

                    if (AllowDynamicInputs && !InputDefinitions.ContainsKey(input.Key))
                    {
                        newStep.Inputs.Add(input.Key, input.Value);
                    }
                    else
                    {
                        newStep.Inputs.Add(input.Key, input.Value);
                    }
                }
            }
            else if (inputs == null && InputDefinitions.Count() > 0)
            {
                throw new InvalidStepInputException("No inputs were specified however step template " + Id + " has " + InputDefinitions.Count() + " inputs.");
            }
            newStep.Tests = tests;
            newStep.StepRefId = stepRefId;
            newStep.SequenceId = sequenceId;
            newStep.CreatedOn = DateTime.Now;
            return newStep;
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
