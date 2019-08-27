using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Model.Common
{
    public class TimeRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public override string ToString()
        {
            return $"{Start} - {End}";
        }
    }

    public class TimeRangeList : List<TimeRange>
    {
        public TimeRange FindFirst(DateTime dt)
        {
            return FindFirst(dt.TimeOfDay);
        }

        public TimeRange FindFirst(TimeSpan ts)
        {
            return this.FirstOrDefault(x =>
                x.Start < x.End ? x.Start <= ts && x.End > ts : // Range like 08:00 - 18:00
                x.Start > x.End ? x.Start < ts || x.End >= ts : // Range like 18:00 - 08:00
                x.Start == ts);
        }
    }
}
