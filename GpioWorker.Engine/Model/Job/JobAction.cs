using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Model.Job
{
    public class JobAction
    {
        public int Gpio { get; set; }
        public string Label { get; set; }

        public int OffsetSeconds { get; set; }
        public int PinValue { get; set; }

        public override string ToString()
        {
            return Label;
        }
    }

    public class JobActionList : List<JobAction> { }
}
