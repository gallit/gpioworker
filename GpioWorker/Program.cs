using GpioWorker.Bll;
using GpioWorker.Model.Gpio;
using GpioWorker.Model.Job;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GpioWorker
{
    class Program
    {
        private static WorkerManager worker;
        static void Main(string[] args)
        {                        
            TraceManager.Init(ConfigManager.LogPath, ConfigManager.LogConsole, ConfigManager.LogLevel);
            TraceManager.Trace("*** App.Start ***");

            try
            {
                var stopRequestPath = ConfigManager.StopRequestPath;
                if (File.Exists(stopRequestPath))
                {
                    File.Delete(stopRequestPath);
                }

                worker = new WorkerManager()
                {
                    JobFilePath = ConfigManager.JobPath,
                    Enabled = true
                };

                var t = Thread.CurrentThread;
                while (worker.Enabled && t.ThreadState != ThreadState.Stopped)
                {
                    if (File.Exists(stopRequestPath) || t.ThreadState == ThreadState.StopRequested || t.ThreadState == ThreadState.AbortRequested)
                    {
                        TraceManager.Trace("StopRequest found");
                        worker.Enabled = false;
                        File.Delete(stopRequestPath);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                TraceManager.Trace("Thread.State = " + t.ThreadState);
            }
            catch(Exception e)
            {
                TraceManager.Trace($"*** App.Error ***\r\n{e.Message}\r\n{e.StackTrace}");
            }
            
            TraceManager.Trace("*** App.End ***");

            //#warning READLINE
            //Console.ReadLine();
        }

    }
}