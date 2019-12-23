using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class PutBotKeyVM
    {
        /// <summary>
        /// Set to null to not update the value
        /// </summary>
        public bool? IsDisabled { get; set; }
        /// <summary>
        /// Set to null to not update
        /// </summary>
        public string BotName { get; set; }
    }
}
