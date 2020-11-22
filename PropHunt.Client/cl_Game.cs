using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using PropHunt.Client.Extensions;
using PropHunt.Client.Utils;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    internal static class cl_Game
    {
        #region Instance Variables / Properties
        public static GameStates GameState { get; private set; }
        public static float TimeRemainingInSeconds { get; private set; }
        #endregion

        #region Events
        public static async Task OnTick()
        {
            //
            // Disable vehicle/pedestrian spawns
            SetPedDensityMultiplierThisFrame(0f);
            SetScenarioPedDensityMultiplierThisFrame(0f, 0f);
            SetRandomVehicleDensityMultiplierThisFrame(0f);
            SetParkedVehicleDensityMultiplierThisFrame(0f);
            SetVehicleDensityMultiplierThisFrame(0f);
        }

        public static void OnSyncGameState(int gameState, float timeRemainingInSeconds)
        {
            cl_Game.GameState = (GameStates)gameState;
            cl_Game.TimeRemainingInSeconds = timeRemainingInSeconds;
        }

        public static void OnGameStateChanged(int state)
        {

            GameStates gameState = (GameStates)state;
            PlayerTeams playerState = Game.Player.State.Get<PlayerTeams>(Constants.State.Player.Team);

            // Enable PvP
            SetPlayerTeam(Game.Player.Handle, (int)playerState);
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);

            // Determine game state
            if (gameState.Equals(GameStates.WaitingForPlayers))
            {
                cl_Player.SetInvincible(true);
            }
            else if (gameState.Equals(GameStates.PreRound))
            {
                cl_Init.TriggerEvent(Constants.Actions.Player.Spawn, -1486, 195, 56);
                cl_Player.SetInvincible(true);
            }
            else if (gameState.Equals(GameStates.Hiding))
            {
                if (playerState == PlayerTeams.Hunter)
                {
                    cl_Player.Blind(true);
                    cl_Player.Freeze(true);
                    cl_Player.SetInvincible(true);
                }
                else if (playerState == PlayerTeams.Prop)
                {
                    cl_Player.SetInvincible(false);
                }
            }
            else if (gameState.Equals(GameStates.Hunting))
            {
                cl_Player.SetInvincible(false);

                if (playerState == PlayerTeams.Hunter)
                {
                    cl_Player.Blind(false);
                    cl_Player.Freeze(false);
                    cl_Player.GiveWeapons(new Dictionary<string, int>()
                    {
                        { "WEAPON_PISTOL", 9999 },
                        { "WEAPON_SMG", 9999 }
                    });
                }
            }
            else if (gameState.Equals(GameStates.PostRound))
            {
                cl_Player.Reset();
            }
        }
        #endregion
    }
}
