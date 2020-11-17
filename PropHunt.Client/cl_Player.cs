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
using PropHunt.Shared.Extensions;
using PropHunt.Shared.Attributes;

namespace PropHunt.Client
{
    internal class cl_Player
    {
        #region Instance Variables / Properties
        private const int HEALTH_MAX = 100;
        private const int HEALTH_MIN = 1;

        private cl_Init _parentInstance;
        private Dictionary<Player, int> _playerTags;
        private int _currentlySpectatingPlayer = -1;
        #endregion

        public cl_Player(cl_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;
            this._playerTags = new Dictionary<Player, int>();

            this.Reset();
            Game.Player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            Game.Player.State.Set(Constants.StateBagKeys.PlayerInitialSpawn, true, true);
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Spectator, true);
        }

        #region Events
        public async Task OnTick()
        {
            SetPlayerHealthRechargeMultiplier(Game.Player.Handle, 0f);
            RestorePlayerStamina(Game.Player.Handle, 1f);
        }

        public async Task OnTick_DrawGamerTags()
        {
            float distance;
            bool canSee;
            PlayerTeams playerTeam;
            PlayerList playerList;
            PlayerTeams localPlayerTeam;

            localPlayerTeam = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam);
            playerList = new PlayerList();

            foreach (Player player in playerList)
            {
                if (player != null)// && !player.Equals(Game.Player))
                {
                    // Get vars 
                    playerTeam = player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam);
                    distance = GetDistanceBetweenCoords(player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, true);
                    canSee = distance < 250 && HasEntityClearLosToEntity(Game.Player.Character.Handle, player.Character.Handle, 17) && (localPlayerTeam.Equals(playerTeam) || localPlayerTeam != PlayerTeams.Hunter);

                    // Handle tag visible state
                    if (this._playerTags.ContainsKey(player))
                    {
                        if (!canSee)
                        {
                            RemoveMpGamerTag(this._playerTags[player]);
                            this._playerTags.Remove(player);
                        }
                        else
                        {
                            this._playerTags[player] = CreateMpGamerTag(player.Character.Handle, player.Name, false, false, string.Empty, 0);
                        }
                    }
                    else if (canSee)
                    {
                        this._playerTags.Add(player, CreateMpGamerTag(player.Character.Handle, player.Name, false, false, string.Empty, 0));
                    }

                    // Display the tag if we are of the right team and within the right distance
                    if (canSee && this._playerTags.ContainsKey(player))
                    {
                        SetMpGamerTagVisibility(this._playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, true);
                        SetMpGamerTagVisibility(this._playerTags[player], GamerTagComponents.HealthAndArmor.GetAttribute<NativeValueInt>().NativeValue, true);
                        SetMpGamerTagVisibility(this._playerTags[player], GamerTagComponents.AudioIcon.GetAttribute<NativeValueInt>().NativeValue, NetworkIsPlayerTalking(player.Handle));

                        SetMpGamerTagAlpha(this._playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 255);
                        SetMpGamerTagAlpha(this._playerTags[player], GamerTagComponents.HealthAndArmor.GetAttribute<NativeValueInt>().NativeValue, 255);
                        SetMpGamerTagAlpha(this._playerTags[player], GamerTagComponents.AudioIcon.GetAttribute<NativeValueInt>().NativeValue, 255);

                        if (playerTeam == PlayerTeams.Hunter)
                            SetMpGamerTagColour(this._playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 125);
                        else
                            SetMpGamerTagColour(this._playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 0);
                    }
                }
            }

            // await Task.Delay(0);
        }

        public async Task OnTick_DrawComponents()
        {
            //
            // Is it the right state
            if (this._parentInstance.Rounds.GameState == GameStates.Hiding || this._parentInstance.Rounds.GameState == GameStates.Hunting)
            {
                var playerState = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam);

                //
                // Handle hunter's view
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

            //await Task.Delay(0);
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
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Spectator, true);
        }

        /// <summary>
        /// Appears to fire if the palyer had a direct attacker and the attacker is known
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killerId"></param>
        /// <param name="args"></param>
        public void OnPlayerKilled([FromSource] Player player, int killerId, dynamic args)
        {
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Spectator, true);

        }
        #endregion

        #region Teleport
        public static async Task TeleportToPlayer(this Player player, bool inVehicle = false)
        {
            // If the player exists.
            if (NetworkIsPlayerActive(player.Handle))
            {
                Vector3 playerPos;
                bool wasActive = true;

                if (NetworkIsPlayerActive(player.Handle))
                {
                    Ped playerPedObj = player.Character;
                    if (Game.PlayerPed == playerPedObj)
                    {
                        NotificationsUtil.Error("Sorry, you can ~r~~h~not~h~ ~s~teleport to yourself!");
                        return;
                    }

                    // Get the coords of the other player.
                    playerPos = GetEntityCoords(playerPedObj.Handle, true);
                }
                else
                {
                    playerPos = await MainMenu.RequestPlayerCoordinates(player.ServerId);
                    wasActive = false;
                }

                // Then await the proper loading/teleporting.
                await TeleportToCoords(playerPos);

                // Wait until the player has been created.
                while (player.Character == null)
                    await Task.Delay(0);

                var playerId = player.Handle;
                var playerPed = player.Character.Handle;

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Wait until the target player vehicle has loaded, if they weren't active beforehand.
                    if (!wasActive)
                    {
                        var startWait = GetGameTimer();

                        while (!IsPedInAnyVehicle(playerPed, false))
                        {
                            await Task.Delay(0);

                            if ((GetGameTimer() - startWait) > 1500)
                            {
                                break;
                            }
                        }
                    }

                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        Vehicle vehicle = GetVehicle(new Player(playerId), false);
                        if (vehicle != null)
                        {
                            int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle.Handle));

                            // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                            if (vehicle.Exists() && !vehicle.IsDead && IsAnyVehicleSeatEmpty(vehicle.Handle))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, (int)VehicleSeat.Any);
                                Notify.Success("Teleported into ~g~<C>" + GetPlayerName(playerId) + "</C>'s ~s~vehicle.");
                            }
                            // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                            else
                            {
                                // If there's only one seat on this vehicle, tell them that it's a one-seater.
                                if (totalVehicleSeats == 1)
                                {
                                    Notify.Error("This vehicle only has room for 1 player!");
                                }
                                // Otherwise, tell them there's not enough empty seats remaining.
                                else
                                {
                                    Notify.Error("Not enough empty vehicle seats remaining!");
                                }
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to ~y~<C>" + GetPlayerName(playerId) + "</C>~s~.");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error(CommonErrors.PlayerNotFound, placeholderValue: "So the teleport has been cancelled.");
            }
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
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Spectator, true);
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
