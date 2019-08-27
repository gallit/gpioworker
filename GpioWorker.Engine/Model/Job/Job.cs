using GpioWorker.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Model.Job
{
    public class Job
    {
        public JobActionList Actions { get; set; }
        public int JobDurationSeconds { get; set; }
        public int JobId { get; set; }
        public string JobLabel { get; set; }
        public int JobTimeoutSeconds { get; set; }
        public DateTime LastExecution { get; set; }

        public TimeRangeList SleepRange { get; set; }
        public override string ToString()
        {
            return $"{JobId} - {JobLabel}";
        }
    }

    public class JobList : List<Job> { }
}
