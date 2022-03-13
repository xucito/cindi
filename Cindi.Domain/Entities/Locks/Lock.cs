using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Locks
{
    public class Lock : BaseEntity
    {
        public string Name { get; set; }
        public int LockTimeoutMs { get; set; }
        public Guid LockId { get; set; }
        public DateTimeOffset ExpireOn { get; set; }
    }
}
