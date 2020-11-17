using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Utils
{
    public static class NotificationsUtil
    {
        public static void Custom(string message, bool blink = true, bool saveToBrief = true)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON");
            foreach (string part in StringToArray(message))
                AddTextComponentSubstringPlayerName(part);
            DrawNotification(blink, saveToBrief);
        }

        public static void Alert(string message, bool blink = true, bool saveToBrief = true)
            => Custom("~y~~h~Alert~h~~s~: " + message, blink, saveToBrief);

        public static void Error(string message, bool blink = true, bool saveToBrief = true)
            => Custom("~r~~h~Error~h~~s~: " + message, blink, saveToBrief);

        public static void Info(string message, bool blink = true, bool saveToBrief = true)
            => Custom("~b~~h~Info~h~~s~: " + message, blink, saveToBrief);

        public static void Success(string message, bool blink = true, bool saveToBrief = true)
            => Custom("~g~~h~Success~h~~s~: " + message, blink, saveToBrief);
    }
}
