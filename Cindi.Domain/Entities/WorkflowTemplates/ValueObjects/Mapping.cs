﻿using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class Mapping
    {
        public StepOutputReference[] OutputReferences { get; set; }
        public string Description { get; set; }
        public DefaultValue DefaultValue { get; set; }
    }
}
