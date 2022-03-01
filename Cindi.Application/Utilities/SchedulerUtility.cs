using Cronos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Utilities
{
    public static class SchedulerUtility
    {
        public static DateTimeOffset NextOccurence(string[] schedules, DateTime? lastRun = null)
        {
            DateTimeOffset? nextDate = null;

            foreach (var schedule in schedules)
            {
                DateTimeOffset? candidateDateTime = null;
                var includeSeconds = schedule.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                CronExpression expression = includeSeconds.Length == 6 ? CronExpression.Parse(schedule, CronFormat.IncludeSeconds) : CronExpression.Parse(schedule, CronFormat.Standard);
                candidateDateTime = expression.GetNextOccurrence(lastRun == null ? DateTime.UtcNow : lastRun.Value);

                if (nextDate == null || candidateDateTime.Value < nextDate)
                {
                    nextDate = candidateDateTime;
                }
            }

            return nextDate.Value;
        }

        public static bool IsValidScheduleString(string scheduleString)
        {
            try
            {
                var includeSeconds = scheduleString.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                CronExpression expression = includeSeconds.Length == 6 ? CronExpression.Parse(scheduleString, CronFormat.IncludeSeconds) : CronExpression.Parse(scheduleString, CronFormat.Standard);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
