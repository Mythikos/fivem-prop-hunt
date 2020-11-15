using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Client.Library.Managers;
using PropHunt.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropHunt.Shared.Enumerations;
using static CitizenFX.Core.Native.API;
using PropHunt.Client.Library.Utils;

namespace PropHunt.Client.Library.Extensions
{
    internal static class PlayerExt
    {
        private const int HEALTH_MAX = 200;
        private const int HEALTH_MIN = 100;

        public static void SetProp(this Player player, int entityHandle)
        {
            int pedHandle = default;
            int propHandle = default;
            Vector3 entityDimensionsMinimum = default;
            Vector3 entityDimensionsMaximum = default;
            float entitySize = 0f;
            Vector3 pedCoords = default;
            Vector3 propRotation = default;
            float propZ = 0f;
            float boneZ = 0f;
            float groundZ = 0f;
            float finalPropZ = 0f;
            int playerCalculatedHealth = 0;

            //
            // Check target entity size to validate
            GetModelDimensions((uint)GetEntityModel(entityHandle), ref entityDimensionsMinimum, ref entityDimensionsMaximum);
            entitySize = Math.Abs(entityDimensionsMaximum.X - entityDimensionsMinimum.X) * Math.Abs(entityDimensionsMaximum.Y - entityDimensionsMinimum.Y) * Math.Abs(entityDimensionsMaximum.Z - entityDimensionsMinimum.Z);
            if (entitySize <= 0.005f)
            {
                TextUtil.SendChatMessage("This prop is too small.", "danger");
                return;
            }

            if (entitySize >= 50.0000f)
            {
                TextUtil.SendChatMessage("This prop is too large.", "danger");
                return;
            }

            if (IsEntityAPed(entityHandle) || IsEntityAVehicle(entityHandle))
            {
                TextUtil.SendChatMessage("This is an invalid prop", "danger");
                return;
            }

            //
            // Handle if the player already has a prop handle
            if (player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
                player.RemoveProp();

            //
            // Process the change
            pedHandle = player.Character.Handle;
            pedCoords = GetEntityCoords(pedHandle, true);
            GetGroundZFor_3dCoord(pedCoords.X, pedCoords.Y, pedCoords.Z, ref groundZ, true);
            propHandle = CreateObject(GetEntityModel(entityHandle), 0f, 0f, 0f, true, true, true);
            SetEntityAsMissionEntity(propHandle, true, true);
            propRotation = GetEntityRotation(propHandle, 2);
            propZ = GetEntityCoords(propHandle, true).Z;
            boneZ = GetPedBoneCoords(pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, 0f).Z;
            finalPropZ = propZ - (boneZ - groundZ);
            AttachEntityToEntity(propHandle, pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, finalPropZ, propRotation.X, propRotation.Y, propRotation.Z, true, false, true, true, 1, true);

            //
            // Change player state
            SetEntityVisible(pedHandle, false, false);
            SetEntityVisible(propHandle, true, false);
            SetEntityCoords(pedHandle, pedCoords.X, pedCoords.Y, pedCoords.Z + 0.25f, true, true, true, false);

            // TODO: This is exploitable - need to consider player's curent hp
            playerCalculatedHealth = (int)Math.Ceiling(entitySize * 10f);
            if (playerCalculatedHealth < HEALTH_MIN)
                playerCalculatedHealth = HEALTH_MIN;
            if (playerCalculatedHealth > HEALTH_MAX)
                playerCalculatedHealth = HEALTH_MAX;
            if (playerCalculatedHealth > GetEntityHealth(player.Character.Handle))
            {
                SetEntityHealth(player.Character.Handle, GetEntityHealth(player.Character.Handle));
            }
            else if (playerCalculatedHealth <= GetEntityHealth(player.Character.Handle))
            {
                SetEntityHealth(player.Character.Handle, playerCalculatedHealth);
            }
            SetEntityMaxHealth(player.Character.Handle, playerCalculatedHealth);

            //
            // Store new prop handle
            player.State.Set(Constants.StateBagKeys.PlayerPropHandle, propHandle, true);
        }

        public static void RemoveProp(this Player player)
        {
            int propHandle = default;

            //
            // Remove the entity handle if it is set
            if (player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
            {
                propHandle = player.State.Get(Constants.StateBagKeys.PlayerPropHandle);
                DetachEntity(propHandle, true, false);
                DeleteObject(ref propHandle);
                player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            }

            //
            // Undo entity changes
            SetEntityVisible(player.Handle, true, true);
            SetEntityMaxHealth(player.Character.Handle, HEALTH_MAX);
            SetEntityHealth(player.Character.Handle, HEALTH_MAX);
        }

        public static void RoundReset(this Player player)
        {
            player.RemoveProp();
            player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerState, PlayerTeams.Unassigned, true);
        }
    }
}
