using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Model.Gpio
{
    public class GpioException : Exception
    {
        public GpioException() { }
        public GpioException(string message) : base(message) { }
        public GpioException(string message, params object[] args) : base(string.Format(message, args)) { }
    }
}
