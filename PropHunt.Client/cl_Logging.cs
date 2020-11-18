using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using PropHunt.Shared.Enumerations;

namespace PropHunt.Client
{
    internal static class cl_Logging
    {
        public static void Log(string data, LogLevels level = LogLevels.None)
        {
            if (cl_Init.DebugMode)
            {
                Debug.WriteLine($"[{level.ToString().ToUpper()}] {data}");
            }
        }
    }
}
