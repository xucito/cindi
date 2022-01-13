using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Locks
{
    public class Lock : BaseEntity
    {
        public string Name { get; set; }
        public int LockTimeoutMs { get; set; }
        /// <summary>
        /// Key used
        /// </summary>
        public string UniqueKey { get; set; }
        public bool IsExpired() { return (DateTime.Now - CreatedOn).TotalMilliseconds > LockTimeoutMs; }
    }
}
