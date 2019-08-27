using GpioWorker.Model.Job;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GpioWorker.Bll
{
    public class WorkerManager
    {
        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;

                if (enabled)
                {
                    worker.Start();
                }
                else
                {
                    TraceManager.Trace("Arrêt en cours");
                    worker.Join();
                }
            }
        }

        private List<GpioDriver> gpioDrivers = new List<GpioDriver>();

        private string jobFilePath;
        public string JobFilePath
        {
            get
            {
                return jobFilePath;
            }
            set
            {
                if (!File.Exists(value))
                {
                    throw new ArgumentException($"JobFilePath.Set error : The file {value} does not exist or is not accessible");
                }
                jobFilePath = value;
            }
        }

        private JobList jobs { get; set; }
        internal JobList Jobs
        {
            get
            {
                if (jobs == null)
                {
                    jobs = Newtonsoft.Json.JsonConvert.DeserializeObject<JobList>(File.ReadAllText(JobFilePath));
                }
                return jobs;
            }
        }

        private Thread worker;

        private GpioDriver GetGpioDriver(int gpio)
        {
            var result = gpioDrivers.FirstOrDefault(x => x.Gpio == gpio);
            if (result == null)
            {
                TraceManager.Debug($"Connecting to GPIO {gpio}", TraceManager.LogLevelDebugVerbose);
                result = new GpioDriver()
                {
                    Gpio = gpio,
                    Export = true
                };
                result.ActiveLow = GpioDriver.GpioValueHigh;
                result.Direction = Model.Gpio.GpioDirection.Out;
                gpioDrivers.Add(result);
            }
            return result;
        }

        private void SaveJobs()
        {
            var timeout = DateTime.Now.AddSeconds(10);
            var ok = false;
            while(!ok && DateTime.Now < timeout)
            {
                try
                {
                    File.WriteAllText(JobFilePath, Newtonsoft.Json.JsonConvert.SerializeObject(jobs, Newtonsoft.Json.Formatting.Indented));
                    ok = true;
                }
                catch(Exception e)
                {
                    TraceManager.Trace($"SaveJobs error : {e.Message}");
                    Thread.Sleep(100);
                }                
            }            
        }

        public WorkerManager()
        {
            worker = new Thread(new ThreadStart(WorkerRun));
            worker.Priority = ThreadPriority.Lowest;
        }

        private void WorkerRun()
        {
            while (Enabled)
            {
                var now = DateTime.Now;
                var job = Jobs.OrderBy(x => x.LastExecution).FirstOrDefault(x => x.LastExecution.AddSeconds(x.JobTimeoutSeconds) < now);
                var sleep = job == null || job.SleepRange == null ? null : job.SleepRange.FindFirst(now);
                if (sleep != null)
                {
                    TraceManager.Debug($"Sleep mode [{job}] : [{sleep}]");
                    job.LastExecution = job.LastExecution.AddSeconds(job.JobTimeoutSeconds);
                    SaveJobs();
                }
                else if (job != null)
                {
                    TraceManager.Trace($"Starting job [{job}]");
                    var dtStart = DateTime.Now;
                    var actions = job.Actions.ToList();

                    TraceManager.Debug(
                        string.Join("\r\n          * ", actions.Select(x => $"Job action = {x.Label}, offset = {x.OffsetSeconds}")), 
                        TraceManager.LogLevelDebugAll);

                    while(Enabled && actions != null && actions.Count > 0)
                    {
                        var action = actions.OrderBy(x => x.OffsetSeconds).FirstOrDefault(x => dtStart.AddSeconds(x.OffsetSeconds) < DateTime.Now);
                        if (action != null)
                        {
                            TraceManager.Trace($"Executing action [{action.Label}]");
                            var driver = GetGpioDriver(action.Gpio);
                            driver.Value = action.PinValue;
                            actions.Remove(action);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                    if (Enabled)
                    {
                        while(job.LastExecution < dtStart.Subtract(new TimeSpan(0, 0, job.JobTimeoutSeconds)))
                        {
                            if (job.LastExecution < dtStart.Subtract(new TimeSpan(0, 0, job.JobTimeoutSeconds * 2)))
                            {
                                TraceManager.Debug($"Ignoring job [{job}] execution of {job.LastExecution}, time's up", TraceManager.LogLevelDebugVerbose);
                            }                            
                            job.LastExecution = job.LastExecution.AddSeconds(job.JobTimeoutSeconds);
                        }
                        
                        SaveJobs();
                        TraceManager.Trace($"Job [{job}] complete");
                    }
                    else
                    {
                        TraceManager.Trace($"Job [{job}] aborded");
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            TraceManager.Trace("Stopping worker");
            foreach(var d in gpioDrivers)
            {
                TraceManager.Trace($"Disabling driver {d.ToString()}");
                d.Value = GpioDriver.GpioValueLow;
                d.Export = false;
            }
        }
    }
}
