using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
        /*public int? LockTimeoutMs { get; set; }
        public Guid? LockId { get; set; }
        [Date]
        public DateTimeOffset? LockCreatedOn { get; set; }
        [Date]
        public DateTimeOffset? LockExpiresOn { get; set; }
        public bool IsExpired() { return (DateTimeOffset.UtcNow - LockCreatedOn.Value).TotalMilliseconds > LockTimeoutMs; }*/
    }
}
