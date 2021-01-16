using System;

namespace Cindi.Domain.Entities
{
    public class TrackedEntity : BaseEntity
    {
        public TrackedEntity() { }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int Version { get; set; }
    }
}
