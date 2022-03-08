
using Cindi.Domain.ValueObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cindi.Domain.Entities
{
    public class TrackedEntity : BaseEntity
    {
        public string CreatedBy { get; set; }
    }
}
