using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
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
            if (step.StepTemplateReference.TemplateId == Reference.TemplateId)
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
    }
}
