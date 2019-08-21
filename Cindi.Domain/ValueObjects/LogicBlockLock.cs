using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class LogicBlockLock
    {
        public Guid WorkflowId { get; set; }
        public string LogicBlockId { get; set; }
        //Randomly generated GUID to identify the thread that locked the logic block
        public Guid LockerCode { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
