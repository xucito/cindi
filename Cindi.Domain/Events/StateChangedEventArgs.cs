using Cindi.Domain.Entities.States;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Events
{
    public class StateChangedEventArgs : EventArgs
    {
        public CindiClusterState NewState { get; set; }
    }
}
