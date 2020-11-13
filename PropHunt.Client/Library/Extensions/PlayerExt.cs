using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Client.Library.Managers;
using PropHunt.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Library.Extensions
{
    internal static class PlayerExt
    {
        public static void SetProp(this Player player, int entityHandle)
        {
            int pedHandle = default;
            int propHandle = default;
            Vector3 pedCoords = default;
            Vector3 propRotation = default;
            float propZ = 0f;
            float boneZ = 0f;
            float groundZ = 0f;
            float finalPropZ = 0f;

            if (player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
            {
                DetachEntity(player.State.Get(Constants.StateBagKeys.PlayerPropHandle), true, false);
                DeleteObject(ref propHandle);
                propHandle = default;
                player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            }

            pedHandle = Game.PlayerPed.Handle;
            pedCoords = GetEntityCoords(pedHandle, true);
            GetGroundZFor_3dCoord(pedCoords.X, pedCoords.Y, pedCoords.Z, ref groundZ, true);
            propHandle = CreateObject(GetEntityModel(entityHandle), 0f, 0f, 0f, true, true, true);
            SetEntityAsMissionEntity(propHandle, true, true);
            propRotation = GetEntityRotation(propHandle, 2);
            propZ = GetEntityCoords(propHandle, true).Z;
            boneZ = GetPedBoneCoords(pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, 0f).Z;
            finalPropZ = propZ - (boneZ - groundZ);

            AttachEntityToEntity(propHandle, pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, finalPropZ, propRotation.X, propRotation.Y, propRotation.Z, true, false, true, true, 1, true);
            SetEntityVisible(pedHandle, false, false);
            SetEntityVisible(propHandle, true, false);
            SetEntityCoords(pedHandle, pedCoords.X, pedCoords.Y, pedCoords.Z + 0.25f, true, true, true, false);

            player.State.Set(Constants.StateBagKeys.PlayerPropHandle, propHandle, true);
        }
    }
}
