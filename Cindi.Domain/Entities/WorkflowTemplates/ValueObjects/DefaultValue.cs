using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class DefaultValue
    {
        public object Value { get; set; }
        public int Priority = 99999999;
    }
}
