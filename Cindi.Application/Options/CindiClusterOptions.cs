using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Options
{
    public class CindiClusterOptions
    {
        public double DefaultSuspensionTime { get; set; } = 1000;
        public string DbConnectionString { get; set; }
    }
}
