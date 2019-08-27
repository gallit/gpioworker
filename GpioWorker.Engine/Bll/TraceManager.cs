using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Bll
{
    public static class TraceManager
    {
        public const int LogLevelDebug = 1;
        public const int LogLevelDebugVerbose = 2;
        public const int LogLevelDebugAll = 3;
        private static bool console = false;
        private static string logFolder = "";
        private static int logLevel= 1;
        public static void Init(string logFolder, bool console, int logLevel)
        {
            TraceManager.console = console;
            TraceManager.logLevel = logLevel;
            TraceManager.logFolder = logFolder;
        }
        
        public static void Debug(string message, int logLevel = LogLevelDebug)
        {
            if (logLevel <= TraceManager.logLevel)
            {
                Trace($"{message} | DEBUG{logLevel}");
            }
        }

        public static void Trace(string message)
        {
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var now = DateTime.Now;
            var fileName = "Bot1Worker_" + now.ToString("yyyyMMdd") + ".log";
            var filePath = Path.Combine(logFolder, fileName);
            message = now.ToString("HH:mm:ss") + " : " + message + "\r\n";
            if (console)
            {
                Console.Write(message);
            }
            File.AppendAllText(filePath, message);
        }
    }
}
