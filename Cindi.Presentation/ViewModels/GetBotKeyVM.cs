using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class GetBotKeyVM
    {
        public string BotName { get; set; }
        public bool IsDisabled { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset RegisteredOn { get; set; }
    }
}
