using Cindi.Domain.Exceptions.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cindi.Domain.Utilities
{
    public static class DateTimeMathsUtility
    {
        public static readonly char[] ValidTimeUnits = new char[]
        {
            'y',
            'M',
            'w',
            'd',
            'h',
            'H',
            'm',
            's'
        };
        /// <summary>
        /// Uses similar maths to elasticsearch
        /// https://www.elastic.co/guide/en/elasticsearch/reference/current/common-options.html#date-math
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static double GetMs(string duration)
        {
            var digits = double.Parse(Regex.Match(duration, @"^[0-9]*").Value.Trim());

            var timeUnit = duration.Trim().Last();

            if(!IsValidTimeUnit(timeUnit))
            {
                throw new InvalidTimeUnitException(timeUnit + " is not a valid time unit.");
            }

            switch(timeUnit)
            {
                case 'y':
                    return 365 * 24.0 * 60 * 60 * 1000.0 * digits;
                case 'M':
                    return 30.0 * 24.0 * 60 * 60 * 1000.0 * digits;
                case 'w':
                    return 7 * 24.0 * 60 * 60 * 1000.0 * digits;
                case 'd':
                    return 24.0 * 60 * 60 * 1000.0 * digits;
                case 'h':
                    return 60 * 60 * 1000.0 * digits;
                case 'H':
                    return 60 * 60 * 1000.0 * digits;
                case 'm':
                    return 60 * 1000.0 * digits;
                case 's':
                    return 1000.0 * digits;
                default:
                    return 1000.0 * digits;
            }
        }

        public static bool IsValidTimeUnit(char timeUnit)
        {
            if (ValidTimeUnits.Contains(timeUnit))
            {
                return true;
            }
            return false;
        }
    }
}
