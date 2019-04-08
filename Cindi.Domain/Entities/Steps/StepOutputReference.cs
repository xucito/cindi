using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Steps
{
    public class StepOutputReference
    {
        public int StepRefId { get; set; }

        public string OutputId { get; set; }

        private int _priority;

        /// <summary>
        /// Will map first available step output based on highest priority logic
        /// </summary>
        public int? Priority { get { return _priority; } set {
                if(!value.HasValue)
                {
                    _priority = 0;
                }
                else
                {
                    _priority = value.Value;
                }
            } }
    }
}
