using GpioWorker.Model.Gpio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GpioWorker.Bll
{
    internal delegate bool TimeoutDelegate();
    
    public class GpioDriver
    {
        private const string gpioDirectionIn = "in";
        private const string gpioDirectionOut = "out";
        private const string gpioRootDir = "/sys/class/gpio";
        public const int GpioValueHigh = 1;
        public  const int GpioValueLow = 0;

        private string gpioDirectory = string.Empty;
        private string gpioPathActiveLow = string.Empty;
        private string gpioPathDirection = string.Empty;
        private string gpioPathValue = string.Empty;

        public int ActiveLow
        {
            get
            {
                return GetGpioIntValue(gpioPathActiveLow);
            }
            set
            {
                if (ActiveLow != value)
                {
                    SetGpioValue(gpioPathActiveLow, value);
                }
            }
        }

        public bool Export
        {
            get
            {
                return Directory.Exists(gpioDirectory);
            }
            set
            {
                var currentState = Export;
                if ((value && currentState) || (!value && !currentState))
                {
                    TraceManager.Debug($"Export state is already set to {value}");
                }
                else
                {
                    var action = value ? "export" : "unexport";
                    var fp = $"{gpioRootDir}/{action}";
                    var result = ManageTimeout(1000, () => 
                    {
                        File.AppendAllText(fp, gpio.ToString());
                        return Export == value;
                    });
                    if (!result)
                    {
                        throw new GpioException($"Failure to set {this} : {action} (file: {fp})");
                    }
                }
            }
        }

        public bool IsValueHigh
        {
            get { return Value == GpioValueHigh; }
        }

        public bool IsValueLow
        {
            get { return Value == GpioValueLow; }
        }

        private int gpio = 0;
        public int Gpio
        {
            get { return gpio; }
            set
            {
                if (Export)
                {
                    throw new GpioException($"{this} must be unexported before leaving");
                }
                gpio = value;
                gpioDirectory = $"{gpioRootDir}/gpio{gpio}";
                gpioPathActiveLow = $"{gpioDirectory}/active_low";
                gpioPathDirection = $"{gpioDirectory}/direction";
                gpioPathValue = $"{gpioDirectory}/value";
            }
        }

        public GpioDirection Direction
        {
            get
            {
                var fp = gpioPathDirection;
                var data = File.Exists(fp) ?
                    File.ReadAllText(fp).Trim() :
                    string.Empty;
                return
                    data == gpioDirectionIn ? GpioDirection.In :
                    data == gpioDirectionOut ? GpioDirection.Out :
                    GpioDirection.Unknown;
            }
            set
            {
                if (Direction != value)
                {
                    var fp = gpioPathDirection;
                    var result = ManageTimeout(1000, () =>
                    {
                        var valueStr = value == GpioDirection.In ? gpioDirectionIn : gpioDirectionOut;
                        TraceManager.Debug($"{this}.SetDirection({valueStr})");
                        File.AppendAllText(fp, valueStr);
                        return true;
                    });
                    if (!result)
                    {
                        throw new GpioException($"Failure on setting direction value [{value}] to {this}");
                    }
                }
            }
        }

        public int Value {
            get
            {
                return GetGpioIntValue(gpioPathValue);
            }
            set
            {
                SetGpioValue(gpioPathValue, value);
            }
        }

        private int GetGpioIntValue(string filePath)
        {
            var resultStr = GetGpioValue(filePath);
            int result;
            if (!int.TryParse(resultStr, out result))
            {
                throw new GpioException($"GetGpioIntValue error, result is not a valid int : [{resultStr}]");
            }
            return result;
        }

        private string GetGpioValue(string filePath)
        {
            var result = File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new GpioException($"GetGpioValue({filePath}) error. Result is empty");
            }
            return result;
        }

        private bool ManageTimeout(int timeoutMilliseconds, TimeoutDelegate action)
        {
            var timeout = DateTime.Now.AddSeconds(timeoutMilliseconds / 1000);
            var result = false;
            while (!result && DateTime.Now < timeout)
            {
                try
                {
                    result = action();
                }
                catch (Exception e)
                {
                    TraceManager.Trace($"ManageTimeout action error : {e.Message}\r\n{e.StackTrace}");
                }

                if (!result)
                {
                    Thread.Sleep(100);
                }
            }
            return result;
        }

        private void SetGpioValue(string filePath, int value)
        {
            SetGpioValue(filePath, value.ToString());
        }

        private void SetGpioValue(string filePath, string value)
        {
            if (File.Exists(filePath))
            {
                var result = ManageTimeout(1000, () =>
                {
                    //TraceManager.Debug($"{this}.SetGpioValue({filePath}) : {value}");
                    File.AppendAllText(filePath, value.ToString());
                    return GetGpioValue(filePath).ToString() == value;
                });
                if (!result)
                {
                    var fi = new FileInfo(filePath);
                    throw new GpioException($"SetGpioValue error : {this}.{fi.Name} = {ActiveLow} (expected : {value})");
                }
            }
            else
            {
                throw new GpioException($"SetGpioValue error : The file {filePath} does not exist");
            }
        }

        public override string ToString()
        {
            return string.Format("GPIO {0}", gpio.ToString("00"));
        }
    }
}
