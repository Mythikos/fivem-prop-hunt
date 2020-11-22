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
    internal static class sv_Game
    {
        #region Instance Variables / Properties
        private const float TIMER_UPDATE_INTERVAL = 250f;
        private static System.Threading.Timer _updateGameStateTimer;

        public static GameStates State { get; private set; }
        public static float TimeRemainingInSeconds { get; private set; }
        #endregion

        #region Constructors
        static sv_Game()
        {
            sv_Game.State = GameStates.WaitingForPlayers;
            sv_Game.TimeRemainingInSeconds = 0;
            _updateGameStateTimer = new System.Threading.Timer(UpdateGameStateCallback, null, 0, (int)TIMER_UPDATE_INTERVAL);
        }
        #endregion

        #region Timer Callbacks
        public static void UpdateGameStateCallback(object threadState)
        {
            var allActivePlayers = sv_Init.PlayerList.GetAllActivePlayers();
            if (sv_Game.State == GameStates.WaitingForPlayers)
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
                    // Pick zone
                    if (sv_World.CurrentZone != default)
                        sv_World.Cleanup(sv_World.CurrentZone);
                    sv_World.Zone zone = sv_World.Zones.Random();
                    sv_World.Setup(zone);

                    //
                    // Trigger game state change event
                    sv_Environment.RandomizeWeatherAndTime();
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.PreRound);

                    //
                    // Update state
                    sv_Game.State = GameStates.PreRound;
                    sv_Game.TimeRemainingInSeconds = 15;
                }
            }

            else if (sv_Game.State == GameStates.PreRound)
            {
                if (sv_Game.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.Hiding);

                    //
                    // Update state
                    sv_Game.State = GameStates.Hiding;
                    sv_Game.TimeRemainingInSeconds = 60;
                }
            }
            else if (sv_Game.State == GameStates.Hiding)
            {
                if (sv_Game.TimeRemainingInSeconds <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Prop) <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.Hunting);

                    //
                    // Update state
                    sv_Game.State = GameStates.Hunting;
                    sv_Game.TimeRemainingInSeconds = 300;
                }
            }

            else if (sv_Game.State == GameStates.Hunting)
            {
                if (sv_Game.TimeRemainingInSeconds <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Prop) <= 0
                    || allActivePlayers.Count(x => x.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.PostRound);

                    //
                    // Update state
                    sv_Game.State = GameStates.PostRound;
                    sv_Game.TimeRemainingInSeconds = 30;
                }
            }

            else if (sv_Game.State == GameStates.PostRound)
            {
                if (sv_Game.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnGameStateChanged, (int)GameStates.WaitingForPlayers);

                    //
                    // Update state
                    sv_Game.State = GameStates.WaitingForPlayers;
                }
            }

            //
            // Decrement time
            sv_Game.TimeRemainingInSeconds = (sv_Game.TimeRemainingInSeconds <= 0) ? 0 : sv_Game.TimeRemainingInSeconds - TIMER_UPDATE_INTERVAL / 1000f;

            //
            // Send a global update regarding the current state and time remaining
            sv_Init.TriggerClientEvent(Constants.Events.GameManager.OnSyncGameState, (int)sv_Game.State, sv_Game.TimeRemainingInSeconds);
        }
        #endregion
    }
}
