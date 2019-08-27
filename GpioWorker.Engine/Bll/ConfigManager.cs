using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioWorker.Bll
{
    public static class ConfigManager
    {
        public static string JobPath
        {
            get { return ConfigurationManager.AppSettings["JobPath"]; }
        }

        public static bool LogConsole
        {
            get { return bool.Parse(ConfigurationManager.AppSettings["LogConsole"]); }
        }

        public static int LogLevel
        {
            get { return int.Parse(ConfigurationManager.AppSettings["LogLevel"]); }
        }

        public static string LogPath
        {
            get { return ConfigurationManager.AppSettings["LogPath"]; }
        }

        public static string StopRequestPath
        {
            get { return ConfigurationManager.AppSettings["StopRequestPath"]; }
        }        
    }
}
