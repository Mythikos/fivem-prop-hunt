using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace PropHunt.Server
{
    internal static class sv_Logging
    {
        public static void Log(dynamic data, LogLevels level = LogLevels.None)
        {
            string prefix = string.Empty;

            if (sv_Init.DebugMode)
            {
                switch(level)
                {
                    case LogLevels.Error:
                        prefix = $"^1[PropHunt] [{level.ToString().ToUpper()}]^7 ";
                        break;
                    case LogLevels.Info:
                        prefix = $"^5[PropHunt] [{level.ToString().ToUpper()}]^7 ";
                        break;
                    case LogLevels.Success:
                        prefix = $"^2[PropHunt] [{level.ToString().ToUpper()}]^7 ";
                        break;
                    case LogLevels.Warning:
                        prefix = $"^3[PropHunt] [{level.ToString().ToUpper()}]^7 ";
                        break;
                    case LogLevels.None:
                    default:
                        prefix = $"[PropHunt] ";
                        break;
                }

                Debug.WriteLine($"{prefix} {data.ToString()}");
            }
        }
    }
}
