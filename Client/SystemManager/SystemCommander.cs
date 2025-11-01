using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.SystemManager
{
    internal static class SystemCommander
    {
        public static void Shutdown() => Process.Start("shutdown", "/s /t 0");
        public static void Restart() => Process.Start("shutdown", "/r /t 0");
    }
}
