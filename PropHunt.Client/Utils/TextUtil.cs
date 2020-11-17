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
        /// Draws text within the 3d world
        /// Limitation: Can only draw 144 elements at once
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        public static void DrawText3D(Vector3 position, string text, int[] color = null)
        {
            bool onScreen = default;
            float screenX = default;
            float screenY = default;
            float scale = default;
            float fov = default;
            float distance = default;
            Vector3 cameraPosition = default;

            if (color == null)
                color = new int[3] { 255, 255, 255 };

            onScreen = World3dToScreen2d(position.X, position.Y, position.Z, ref screenX, ref screenY);
            cameraPosition = GetGameplayCamCoords();
            distance = GetDistanceBetweenCoords(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, position.X, position.Y, position.Z, true);

            fov = (1f / GetGameplayCamFov()) * 100f;
            scale = fov * (1f / distance) * 2f;

            if (onScreen)
            {
                SetTextScale(0f * scale, 0.55f * scale);
                SetTextFont(0);
                SetTextProportional(true);
                SetTextColour(color[0], color[1], color[2], 255);
                SetTextDropshadow(0, 0, 0, 0, 255);
                SetTextEdge(2, 0, 0, 0, 150);
                SetTextDropShadow();
                SetTextOutline();
                SetTextEntry("STRING");
                SetTextCentre(true);
                AddTextComponentString(text);
                DrawText(screenX, screenY);
            }
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
