using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared.Extensions;
using PropHunt.Server.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using System.Threading;

namespace PropHunt.Server.Library.Managers
{
    internal static class GameManager
    {
        #region Properties
        public static PlayerList AllPlayers { get { return new PlayerList(); } }
        public static GameStates State { get; set; }
        public static float TimeRemainingInSeconds { get; set; }
        #endregion

        #region Instance Variables
        private const float TIMER_INTERVAL = 250;

        private static PropHunt _pluginInstance;
        private static System.Threading.Timer _updateGameStateTimer;
        #endregion

        public static void Initialize(PropHunt pluginInstance)
        {
            GameManager.State = GameStates.WaitingForPlayers;
            GameManager.TimeRemainingInSeconds = 0;
            _pluginInstance = pluginInstance;
            _updateGameStateTimer = new System.Threading.Timer(UpdateGameState, null, 0, (int)TIMER_INTERVAL);
        }

        public static async Task OnTick() 
        { 
        
        }

        public static void UpdateGameState(object threadState)
        {
            if (GameManager.State == GameStates.WaitingForPlayers)
            {
                if (GameManager.AllPlayers.Count(x => x.State.Get(Constants.StateBagKeys.PlayerInitialSpawn) == false) >= 2)
                {
                    //
                    // Assign hunters and prop players
                    int hunterCount = (int)Math.Floor(GameManager.AllPlayers.Count() / 10.0);
                    for (int i = 0; i < (hunterCount < 1 ? 1 : hunterCount); i++)
                    {
                        Player randomPlayer = GameManager.AllPlayers.Random();
                        randomPlayer.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerState, PlayerTeams.Hunter, true);
                    }

                    foreach (Player player in GameManager.AllPlayers)
                    {
                        if (player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState) != PlayerTeams.Hunter)
                            player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerState, PlayerTeams.Prop, true);
                    }

                    //
                    // Determine weather and time for round
                    int weatherState = Enum.GetValues(typeof(WeatherStates)).Random<int>();
                    int timeState = Enum.GetValues(typeof(TimeOfDayStates)).Random<int>();

                    //
                    // Trigger game state change event
                    PropHunt.TriggerClientEvent(Constants.Events.Client.SyncTimeAndWeather, timeState, weatherState);
                    PropHunt.TriggerClientEvent(Constants.Events.Client.GameStateUpdate, (int)GameStates.PreRound);

                    //
                    // Update state
                    GameManager.State = GameStates.PreRound;
                    GameManager.TimeRemainingInSeconds = 15;
                    Debug.WriteLine($"GameState: {GameManager.State}");
                }
            }

            else if (GameManager.State == GameStates.PreRound)
            {
                if (GameManager.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    PropHunt.TriggerClientEvent(Constants.Events.Client.GameStateUpdate, (int)GameStates.Hiding);

                    //
                    // Update state
                    GameManager.State = GameStates.Hiding;
                    GameManager.TimeRemainingInSeconds = 60;
                    Debug.WriteLine($"GameState: {GameManager.State}");
                }
            }
            else if (GameManager.State == GameStates.Hiding)
            {
                if (GameManager.TimeRemainingInSeconds <= 0 || GameManager.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState) == PlayerTeams.Prop) <= 0 || GameManager.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    PropHunt.TriggerClientEvent(Constants.Events.Client.GameStateUpdate, (int)GameStates.Hunting);

                    //
                    // Update state
                    GameManager.State = GameStates.Hunting;
                    GameManager.TimeRemainingInSeconds = 300;
                    Debug.WriteLine($"GameState: {GameManager.State}");
                }
            }
            
            else if (GameManager.State == GameStates.Hunting)
            {
                if (GameManager.TimeRemainingInSeconds <= 0 || GameManager.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState) == PlayerTeams.Prop) <= 0 || GameManager.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerState) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    PropHunt.TriggerClientEvent(Constants.Events.Client.GameStateUpdate, (int)GameStates.PostRound);

                    //
                    // Update state
                    GameManager.State = GameStates.PostRound;
                    GameManager.TimeRemainingInSeconds = 30;
                    Debug.WriteLine($"GameState: {GameManager.State}");
                }
            }

            else if (GameManager.State == GameStates.PostRound)
            {
                if (GameManager.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    PropHunt.TriggerClientEvent(Constants.Events.Client.GameStateUpdate, (int)GameStates.WaitingForPlayers);

                    //
                    // Update state
                    GameManager.State = GameStates.WaitingForPlayers;
                    Debug.WriteLine($"GameState: {GameManager.State}");
                }
            }

            //
            // Decrement time
            if (GameManager.TimeRemainingInSeconds <= 0)
            {
                GameManager.TimeRemainingInSeconds = 0;
            }
            else
            {
                GameManager.TimeRemainingInSeconds -= TIMER_INTERVAL / 1000f;
            }

            //
            // Send a global update regarding the current state and time remaining
            PropHunt.TriggerClientEvent(Constants.Events.Client.SyncGameManager, (int)GameManager.State, GameManager.TimeRemainingInSeconds);
        }
    }
}
