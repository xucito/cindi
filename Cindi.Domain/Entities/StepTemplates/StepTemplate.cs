using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Cindi.Domain.Entities.StepTemplates
{
    public class StepTemplate
    {
        /// <summary>
        /// Name of definition
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Version of the definition
        /// </summary>
        public int Version { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Dynamic inputs will default type string
        /// </summary>
        public bool AllowDynamicInputs = false;
        

        public TemplateReference Reference { get {
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
        public Dictionary<string, DataDescription> InputDefinitions { get; set; }

        /// <summary>
        ///  Output from task, the output name is the dictionary key and the type is Dictionary value
        ///  Value is the object type based on serialized string i.e.
        ///  {
        ///    name: string,
        ///    value: number
        ///  }
        /// </summary>
        public Dictionary<string, DataDescription> OutputDefinitions { get; set; }

        /// <summary>
        /// Checks whether the step matches the step definition
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool StepMatches(Step step)
        {
            if(step.StepTemplateReference.TemplateId == Reference.TemplateId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
