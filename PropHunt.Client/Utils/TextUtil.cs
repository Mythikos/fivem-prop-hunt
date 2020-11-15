using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Utils
{
    internal static class TextUtil
    {
        /// <summary>
        /// Limitation: Can only draw 144 elements at once
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        public static void DrawText3D(Vector3 position, string text)
        {
            float screenX = default;
            float screenY = default;

            World3dToScreen2d(position.X, position.Y, position.Z, ref screenX, ref screenY);

            SetTextScale(0.35f, 0.35f);
            SetTextFont(4);
            SetTextProportional(true);
            SetTextColour(255, 255, 255, 215);
            SetTextEntry("STRING");
            SetTextCentre(true);
            AddTextComponentString(text);
            DrawText(screenX, screenY);
        }

        public static void SendChatMessage(string message, string state = "info")
        {
            int[] color;
            switch (state)
            {
                case "danger":
                    color = new[] { 220, 53, 69 };
                    break;
                case "success":
                    color = new[] { 40, 167, 69 };
                    break;
                case "warning":
                    color = new[] { 255, 193, 7 };
                    break;
                case "info":
                default:
                    color = new[] { 23, 162, 184 };
                    break;
            }

            cl_Init.TriggerEvent("chat:addMessage", new { color = color, args = new[] { "[PropHunt]", message } });
        }
    }
}
