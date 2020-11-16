using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Client.Extensions;
using PropHunt.Client.Utils;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace PropHunt.Client
{
    internal class cl_Player
    {
        #region Instance Variables / Properties
        private const int HEALTH_MAX = 100;
        private const int HEALTH_MIN = 1;

        private cl_Init _parentInstance;
        #endregion

        public cl_Player(cl_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;

            this.Reset();
            Game.Player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            Game.Player.State.Set(Constants.StateBagKeys.PlayerInitialSpawn, true, true);
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Unassigned, true);
        }

        #region Events
        public async Task OnTick()
        {
            PlayerTeams playerState;

            SetPlayerHealthRechargeMultiplier(Game.Player.Handle, 0f);
            RestorePlayerStamina(Game.Player.Handle, 1f);

            //
            // Is it the right state
            if (this._parentInstance.Rounds.GameState == GameStates.Hiding || this._parentInstance.Rounds.GameState == GameStates.Hunting)
            {
                //
                // Handle hunter's view
                playerState = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam);
                if (playerState == PlayerTeams.Hunter)
                {
                    ShowHudComponentThisFrame((int)HudComponent.WeaponWheel);
                    foreach (int componentValue in Enum.GetValues(typeof(HudComponent)))
                        if (componentValue != (int)HudComponent.Reticle && componentValue != (int)HudComponent.WeaponWheel)
                            HideHudComponentThisFrame(componentValue);
                }

                //
                // Hande the prop's view
                else if (playerState == PlayerTeams.Prop)
                {
                    ShowHudComponentThisFrame((int)HudComponent.Reticle);
                    foreach (int componentValue in Enum.GetValues(typeof(HudComponent)))
                        if (componentValue != (int)HudComponent.Reticle)
                            HideHudComponentThisFrame(componentValue);

                    List<Entity> entities1 = EntityUtil.GetSurroundingEntities(Game.Player.Character, 10, EntityUtil.IntersectOptions.IntersectObjects).ToList();
                    foreach (Entity entity in entities1)
                    {
                        EntityUtil.HighlightEntity(entity, 0.035f, 0.035f, 0.025f, 0.025f, new[] { 0, 0, 0, 255 });
                        EntityUtil.HighlightEntity(entity, 0.035f, 0.035f, 0.015f, 0.015f, new[] { 255, 255, 255, 255 });
                    }

                    int targetEntityHandle = default;
                    GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref targetEntityHandle);
                    if (targetEntityHandle != default)
                    {
                        TextUtil.DrawText3D(GetEntityCoords(targetEntityHandle, true), "Press E");
                        EntityUtil.HighlightEntity(Entity.FromHandle(targetEntityHandle), 0.035f, 0.035f, 0.025f, 0.025f, new[] { 0, 255, 0, 255 });
                        if (IsControlJustPressed(0, 38))
                            this._parentInstance.Player.SetProp(targetEntityHandle);
                    }

                    if (Game.Player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
                        SetPedCapsule(Game.Player.Character.Handle, 0.01f);
                }
            }
        }

        public void OnPlayerSpawned()
        {
            // Prevent auto respawns
            this._parentInstance.SpawnManager_SetAutoSpawn(false);

            // Determine type of spawn
            if (Game.Player.State.Get(Constants.StateBagKeys.PlayerInitialSpawn) == true)
            {
                cl_Init.TriggerServerEvent(Constants.Events.Server.OnPlayerInitialSpawn, Game.Player.ServerId);
                Game.Player.State.Set(Constants.StateBagKeys.PlayerInitialSpawn, false, true);
            }
            cl_Init.TriggerServerEvent(Constants.Events.Server.OnPlayerSpawn, Game.Player.ServerId);
        }

        /// <summary>
        /// Seems to fire if the player was dealt indirect damage
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killerType"></param>
        public void OnPlayerDied([FromSource] Player player, int killerType)
        {
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Unassigned, true);
        }

        /// <summary>
        /// Appears to fire if the palyer had a direct attacker and the attacker is known
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killerId"></param>
        /// <param name="args"></param>
        public void OnPlayerKilled([FromSource] Player player, int killerId, dynamic args)
        {
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Unassigned, true);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets the player's pedestrian as a prop
        /// </summary>
        /// <param name="entityHandle"></param>
        public void SetProp(int entityHandle)
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
            if (Game.Player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
                this.RemoveProp();

            //
            // Process the change
            pedHandle = Game.Player.Character.Handle;
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
            if (playerCalculatedHealth > GetEntityHealth(Game.Player.Character.Handle))
            {
                this.SetHealth(GetEntityHealth(Game.Player.Character.Handle));
            }
            else if (playerCalculatedHealth <= GetEntityHealth(Game.Player.Character.Handle))
            {
                this.SetHealth(playerCalculatedHealth);
            }

            //
            // Store new prop handle
            Game.Player.State.Set(Constants.StateBagKeys.PlayerPropHandle, propHandle, true);
        }

        /// <summary>
        /// Removes the prop from the player's pedestrian and resets their stats
        /// </summary>
        public void RemoveProp()
        {
            int propHandle = default;

            //
            // Remove the entity handle if it is set
            if (Game.Player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
            {
                propHandle = Game.Player.State.Get(Constants.StateBagKeys.PlayerPropHandle);
                DetachEntity(propHandle, true, false);
                DeleteObject(ref propHandle);
                Game.Player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            }

            //
            // Undo entity changes
            this.SetVisible(true);
            this.SetHealth(HEALTH_MAX);
        }

        /// <summary>
        /// Resets the player fully
        /// </summary>
        public void Reset()
        {
            this.RemoveProp();
            this.SetVisible(true);
            this.SetInvincible(true);
            this.SetHealth(HEALTH_MAX);
            this.SetArmor(0);
            RemoveAllPedWeapons(Game.Player.Character.Handle, true);
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Unassigned, true);
        }

        /// <summary>
        /// Sets the player's visibility
        /// </summary>
        /// <param name="state"></param>
        public void SetVisible(bool state)
            => SetEntityVisible(Game.Player.Character.Handle, state, false);

        /// <summary>
        /// Sets the player's invincibility
        /// </summary>
        /// <param name="state"></param>
        public void SetInvincible(bool state)
            => SetEntityInvincible(Game.Player.Character.Handle, state);

        /// <summary>
        /// Sets the player's health
        /// Only use values between 0 and 100.
        /// Note: Takes into consideration that health in GTA V is 200, with min being 100
        /// </summary>
        /// <param name="health"></param>
        public void SetHealth(int health)
            => SetEntityHealth(Game.Player.Character.Handle, health + 100);

        /// <summary>
        /// Sets the player's armor
        /// </summary>
        /// <param name="armor"></param>
        public void SetArmor(int armor)
            => SetPedArmour(Game.Player.Character.Handle, armor);

        public void Freeze(bool state)
            => FreezeEntityPosition(Game.Player.Character.Handle, state);

        public void GiveWeapon(string weaponName, int ammoCount)
            => GiveWeaponToPed(Game.Player.Character.Handle, (uint)GetHashKey(weaponName), ammoCount, false, true);

        public void GiveWeapons(Dictionary<string, int> weaponAndAmmoCounts)
        {
            foreach (KeyValuePair<string, int> pair in weaponAndAmmoCounts)
            {
                this.GiveWeapon(pair.Key, pair.Value);
            }
        }

        public void Blind(bool state)
        {
            if (state == true)
            {
                SetTimecycleModifier("Glasses_BlackOut");
            }
            else
            {
                ClearTimecycleModifier();
            }
        }
        #endregion
    }
}
