using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using PropHunt.Server.Extensions;

namespace PropHunt.Server
{
    internal static class sv_GameManager
    {
        #region Instance Variables / Properties
        private const float TIMER_UPDATE_INTERVAL = 250f;
        private static System.Threading.Timer _updateGameStateTimer;

        public static GameStates State { get; private set; }
        public static float TimeRemainingInSeconds { get; private set; }
        #endregion

        #region Constructors
        static sv_GameManager()
        {
            sv_GameManager.State = GameStates.WaitingForPlayers;
            sv_GameManager.TimeRemainingInSeconds = 0;
            _updateGameStateTimer = new System.Threading.Timer(UpdateGameStateCallback, null, 0, (int)TIMER_UPDATE_INTERVAL);
        }
        #endregion

        #region Timer Callbacks
        public static void UpdateGameStateCallback(object threadState)
        {
            var allActivePlayers = sv_Init.PlayerList.GetAllActivePlayers();
            if (sv_GameManager.State == GameStates.WaitingForPlayers)
            {
                if (allActivePlayers.Count() >= 2)
                {
                    //
                    // Assign hunters and prop players
                    int hunterCount = (int)Math.Floor(allActivePlayers.Count() / 10.0);
                    for (int i = 0; i < (hunterCount < 1 ? 1 : hunterCount); i++)
                    {
                        Player randomPlayer = allActivePlayers.Random();
                        randomPlayer.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Hunter, true);
                    }

                    foreach (Player player in allActivePlayers)
                    {
                        if (player.State.Get<PlayerTeams>(Constants.State.Player.Team) != PlayerTeams.Hunter)
                            player.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Prop, true);
                    }

                    //
                    // Trigger game state change event
                    sv_Environment.RandomizeWeatherAndTime();
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.PreRound);

                    //
                    // Update state
                    sv_GameManager.State = GameStates.PreRound;
                    sv_GameManager.TimeRemainingInSeconds = 15;
                }
            }

            else if (sv_GameManager.State == GameStates.PreRound)
            {
                if (sv_GameManager.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.Hiding);

                    //
                    // Update state
                    sv_GameManager.State = GameStates.Hiding;
                    sv_GameManager.TimeRemainingInSeconds = 60;
                }
            }
            else if (sv_GameManager.State == GameStates.Hiding)
            {
                if (sv_GameManager.TimeRemainingInSeconds <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Prop) <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.Hunting);

                    //
                    // Update state
                    sv_GameManager.State = GameStates.Hunting;
                    sv_GameManager.TimeRemainingInSeconds = 300;
                }
            }

            else if (sv_GameManager.State == GameStates.Hunting)
            {
                if (sv_GameManager.TimeRemainingInSeconds <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Prop) <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.PostRound);

                    //
                    // Update state
                    sv_GameManager.State = GameStates.PostRound;
                    sv_GameManager.TimeRemainingInSeconds = 30;
                }
            }

            else if (sv_GameManager.State == GameStates.PostRound)
            {
                if (sv_GameManager.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.WaitingForPlayers);

                    //
                    // Update state
                    sv_GameManager.State = GameStates.WaitingForPlayers;
                }
            }

            //
            // Decrement time
            sv_GameManager.TimeRemainingInSeconds = (sv_GameManager.TimeRemainingInSeconds <= 0) ? 0 : sv_GameManager.TimeRemainingInSeconds - TIMER_UPDATE_INTERVAL / 1000f;

            //
            // Send a global update regarding the current state and time remaining
            sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnSyncGameState, (int)sv_GameManager.State, sv_GameManager.TimeRemainingInSeconds);
        }
        #endregion
    }
}
