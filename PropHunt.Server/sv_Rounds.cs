using CitizenFX.Core;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using PropHunt.Server.Library.Extensions;

namespace PropHunt.Server
{
    internal class sv_Rounds
    {
        #region Instance Variables / Properties
        private const float TIMER_UPDATE_INTERVAL = 250;

        private sv_Init _parentInstance;
        private System.Threading.Timer _updateGameStateTimer;

        public PlayerList AllPlayers { get { return new PlayerList(); } }
        public GameStates State { get; private set; }
        public float TimeRemainingInSeconds { get; private set; }
        #endregion

        #region Constructors
        public sv_Rounds(sv_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this.State = GameStates.WaitingForPlayers;
            this.TimeRemainingInSeconds = 0;

            this._parentInstance = parentInstance;
            this._updateGameStateTimer = new System.Threading.Timer(UpdateGameStateCallback, null, 0, (int)TIMER_UPDATE_INTERVAL);
        }
        #endregion

        #region Timer Callbacks
        public void UpdateGameStateCallback(object threadState)
        {
            if (this.State == GameStates.WaitingForPlayers)
            {
                if (this.AllPlayers.Count(x => x.State.Get(Constants.StateBagKeys.PlayerInitialSpawn) == false) >= 2)
                {
                    //
                    // Assign hunters and prop players
                    int hunterCount = (int)Math.Floor(this.AllPlayers.Count() / 10.0);
                    for (int i = 0; i < (hunterCount < 1 ? 1 : hunterCount); i++)
                    {
                        Player randomPlayer = this.AllPlayers.Random();
                        randomPlayer.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Hunter, true);
                    }

                    foreach (Player player in this.AllPlayers)
                    {
                        if (player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) != PlayerTeams.Hunter)
                            player.State.Set<PlayerTeams>(Constants.StateBagKeys.PlayerTeam, PlayerTeams.Prop, true);
                    }

                    //
                    // Trigger game state change event
                    this._parentInstance.Environment.RandomizeWeatherAndTime();
                    sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundStateChanged, (int)GameStates.PreRound);

                    //
                    // Update state
                    this.State = GameStates.PreRound;
                    this.TimeRemainingInSeconds = 3;// 15;
                    Debug.WriteLine($"GameState: {this.State}");
                }
            }

            else if (this.State == GameStates.PreRound)
            {
                if (this.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundStateChanged, (int)GameStates.Hiding);

                    //
                    // Update state
                    this.State = GameStates.Hiding;
                    this.TimeRemainingInSeconds = 3;// 60;
                    Debug.WriteLine($"GameState: {this.State}");
                }
            }
            else if (this.State == GameStates.Hiding)
            {
                if (this.TimeRemainingInSeconds <= 0
                    || this.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) == PlayerTeams.Prop) <= 0
                    || this.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundStateChanged, (int)GameStates.Hunting);

                    //
                    // Update state
                    this.State = GameStates.Hunting;
                    this.TimeRemainingInSeconds = 600;// 300;
                    Debug.WriteLine($"GameState: {this.State}");
                }
            }

            else if (this.State == GameStates.Hunting)
            {
                if (this.TimeRemainingInSeconds <= 0
                    || this.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) == PlayerTeams.Prop) <= 0
                    || this.AllPlayers.Count(x => x.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) == PlayerTeams.Hunter) <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundStateChanged, (int)GameStates.PostRound);

                    //
                    // Update state
                    this.State = GameStates.PostRound;
                    this.TimeRemainingInSeconds = 3;// 30;
                    Debug.WriteLine($"GameState: {this.State}");
                }
            }

            else if (this.State == GameStates.PostRound)
            {
                if (this.TimeRemainingInSeconds <= 0)
                {
                    //
                    // Trigger game state change event
                    sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundStateChanged, (int)GameStates.WaitingForPlayers);

                    //
                    // Update state
                    this.State = GameStates.WaitingForPlayers;
                    Debug.WriteLine($"GameState: {this.State}");
                }
            }

            //
            // Decrement time
            this.TimeRemainingInSeconds = (this.TimeRemainingInSeconds <= 0) ? 0 : this.TimeRemainingInSeconds - TIMER_UPDATE_INTERVAL / 1000f;

            //
            // Send a global update regarding the current state and time remaining
            sv_Init.TriggerClientEvent(Constants.Events.Client.OnRoundSync, (int)this.State, this.TimeRemainingInSeconds);
        }
        #endregion
    }
}
