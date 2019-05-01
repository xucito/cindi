using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class CreateGlobalValueVM
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
    }
}
