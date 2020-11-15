using CitizenFX.Core;
using PropHunt.Client.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropHunt.Shared.Enumerations;
using static CitizenFX.Core.Native.API;
using PropHunt.Client.Library.Extensions;
using CitizenFX.Core.UI;
using PropHunt.Shared;

namespace PropHunt.Client.Library.Managers
{
    internal static class GameManager
    {
        private static PropHunt _pluginInstance;
        public static GameStates GameState { get; set; }
        public static float TimeRemainingInSeconds { get; set; }

        public static void Initialize(PropHunt pluginInstance)
        {
            // Handle instance of the client plugin
            _pluginInstance = pluginInstance;

            // Setup the player
            Game.Player.State.Set(Constants.StateBagKeys.PlayerInitialSpawn, true, true);
            Game.Player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerState, PlayerTeams.Unassigned, true);
            Game.Player.State.Set(Constants.StateBagKeys.PlayerPropHandle, null, true);
            Game.Player.RoundReset();
        }

        public static async Task OnTick()
        {
            PlayerTeams playerState;

            //
            // Disable vehicle/pedestrian spawns
            SetPedDensityMultiplierThisFrame(0f);
            SetScenarioPedDensityMultiplierThisFrame(0f, 0f);
            SetRandomVehicleDensityMultiplierThisFrame(0f);
            SetParkedVehicleDensityMultiplierThisFrame(0f);
            SetVehicleDensityMultiplierThisFrame(0f);

            //
            // All player state items
            SetPlayerHealthRechargeMultiplier(Game.Player.Handle, 0f);
            RestorePlayerStamina(PlayerId(), 1f);

            //
            // Is it the right state
            if (GameManager.GameState == GameStates.Hiding || GameManager.GameState == GameStates.Hunting)
            {
                //
                // Handle hunter's view
                playerState = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState);
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

                    List<Entity> entities1 = EntityUtil.GetSurroundingEntities(Game.PlayerPed, 10, EntityUtil.IntersectOptions.IntersectObjects).ToList();
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
                            Game.Player.SetProp(targetEntityHandle);
                    }

                    if (Game.Player.State.Get(Constants.StateBagKeys.PlayerPropHandle) != null)
                        SetPedCapsule(Game.PlayerPed.Handle, 0.01f);
                }
            }
        }

        #region State Helpers
        public static void OnUpdateGameState(GameStates state)
        {
            TextUtil.SendChatMessage($"OnUpdateGameState: {state}");

            PlayerTeams playerState = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState);

            // Enable PvP
            SetPlayerTeam(Game.Player.Handle, (int)playerState);
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);

            // Determine game state
            if (state.Equals(GameStates.WaitingForPlayers))
            {
                SetEntityInvincible(Game.PlayerPed.Handle, true);
            }
            else if (state.Equals(GameStates.PreRound))
            {
                _pluginInstance.SpawnPlayer(-1486, 195, 56);
                SetEntityInvincible(Game.PlayerPed.Handle, true);
            }
            else if (state.Equals(GameStates.Hiding))
            {
                if (playerState == PlayerTeams.Hunter)
                {
                    SetTimecycleModifier("Glasses_BlackOut");
                    FreezeEntityPosition(Game.PlayerPed.Handle, true);
                    SetEntityInvincible(Game.PlayerPed.Handle, true);
                }
                else if (playerState == PlayerTeams.Prop)
                {
                    SetEntityInvincible(Game.PlayerPed.Handle, false);
                }
            }
            else if (state.Equals(GameStates.Hunting))
            {
                SetEntityInvincible(Game.PlayerPed.Handle, false);

                if (playerState == PlayerTeams.Hunter)
                {
                    ClearTimecycleModifier();
                    FreezeEntityPosition(Game.PlayerPed.Handle, false);

                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_PISTOL"), 999, false, true);
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("WEAPON_SMG"), 999, false, true);
                }
            }
            else if (state.Equals(GameStates.PostRound))
            {
                SetEntityInvincible(Game.PlayerPed.Handle, true);
                RemoveAllPedWeapons(Game.PlayerPed.Handle, true);
                Game.Player.RoundReset();
            }
        }
        #endregion
    }
}
