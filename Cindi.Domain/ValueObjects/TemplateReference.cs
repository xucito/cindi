using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class TemplateReference
    {
        public string Name { get; set; }
        public int Version { get; set; }

        [JsonIgnore]
        public string TemplateId { get { return Name + ":" + Version; } }
    }
}
