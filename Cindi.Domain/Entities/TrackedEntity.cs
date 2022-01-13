
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
        public DateTime? LockExpiryDate { get; set; }
        public Guid? LockId { get; set; }
        public void Unlock()
        {
            LockExpiryDate = null;
            LockId = null;
        }
    }
}
