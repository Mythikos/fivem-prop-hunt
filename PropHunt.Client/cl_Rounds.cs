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
    internal class cl_Rounds
    {
        #region Instance Variables / Properties
        private cl_Init _parentInstance;

        public GameStates GameState { get; private set; }
        public float TimeRemainingInSeconds { get; private set; }
        #endregion


        public cl_Rounds(cl_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;
        }

        #region Events
        public async Task OnTick()
        {
            //
            // Disable vehicle/pedestrian spawns
            SetPedDensityMultiplierThisFrame(0f);
            SetScenarioPedDensityMultiplierThisFrame(0f, 0f);
            SetRandomVehicleDensityMultiplierThisFrame(0f);
            SetParkedVehicleDensityMultiplierThisFrame(0f);
            SetVehicleDensityMultiplierThisFrame(0f);
        }

        public void OnSync(int gameState, float timeRemainingInSeconds)
        {
            this.GameState = (GameStates)gameState;
            this.TimeRemainingInSeconds = timeRemainingInSeconds;
        }

        public void OnStateChanged(int state)
        {

            GameStates gameState = (GameStates)state;
            PlayerTeams playerState = Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam);

            TextUtil.SendChatMessage($"OnUpdateGameState: {gameState}");

            // Enable PvP
            SetPlayerTeam(Game.Player.Handle, (int)playerState);
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);

            // Determine game state
            if (gameState.Equals(GameStates.WaitingForPlayers))
            {
                this._parentInstance.Player.SetInvincible(true);
            }
            else if (gameState.Equals(GameStates.PreRound))
            {
                this._parentInstance.SpawnManager_SpawnPlayer(-1486, 195, 56);
                this._parentInstance.Player.SetInvincible(true);
            }
            else if (gameState.Equals(GameStates.Hiding))
            {
                if (playerState == PlayerTeams.Hunter)
                {
                    this._parentInstance.Player.Blind(true);
                    this._parentInstance.Player.Freeze(true);
                    this._parentInstance.Player.SetInvincible(true);
                }
                else if (playerState == PlayerTeams.Prop)
                {
                    this._parentInstance.Player.SetInvincible(false);
                }
            }
            else if (gameState.Equals(GameStates.Hunting))
            {
                this._parentInstance.Player.SetInvincible(false);

                if (playerState == PlayerTeams.Hunter)
                {
                    this._parentInstance.Player.Blind(false);
                    this._parentInstance.Player.Freeze(false);
                    this._parentInstance.Player.GiveWeapons(new Dictionary<string, int>()
                    {
                        { "WEAPON_PISTOL", 9999 },
                        { "WEAPON_SMG", 9999 }
                    });
                }
            }
            else if (gameState.Equals(GameStates.PostRound))
            {
                this._parentInstance.Player.Reset();
            }
        }
        #endregion
    }
}
