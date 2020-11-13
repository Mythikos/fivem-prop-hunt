using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Server.Library.Managers;
using PropHunt.Server.Library.Extensions;
using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using PropHunt.Shared;
using System.ComponentModel;

/// <summary>
/// Good hunt locations:
///     732 129 360
///     1668 -22 173
///     -1486 195 56
/// </summary>
namespace PropHunt.Server
{
    public class PropHunt : BaseScript
    {
        public PropHunt()
        {
            //
            // Initialize managers
            GameManager.Initialize(this);

            //
            // Subscribe to events
            this.Tick += OnTick;
            this.EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            this.EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDisconnected);
            this.EventHandlers[Constants.Events.Server.PlayerInitialSpawn] += new Action<int>(OnPlayerInitialSpawn);
            this.EventHandlers[Constants.Events.Server.PlayerSpawn] += new Action<int>(OnPlayerSpawn);

            //
            // Output plugin was loaded?
            Debug.WriteLine("PropHunt.Server was loaded successfully");
        }

        #region Native Events
        public async Task OnTick()
        {
            //
            // Execute manager ticks
            GameManager.OnTick();
        }

        private async void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            deferrals.defer();
            await Delay(0);
            deferrals.done();
        }

        private void OnPlayerDisconnected([FromSource] Player player, string reason)
        {
            Debug.WriteLine($"Player {player.Name} dropped (Reason: {reason}).");
        }
        #endregion

        #region Custom Events
        private void OnPlayerInitialSpawn(int playerServerId)
        {
            Player player;

            player = GameManager.AllPlayers[playerServerId];
            if (player != null)
            {
                if (GameManager.State == GameStates.PreRound || GameManager.State == GameStates.Hiding || GameManager.State == GameStates.Hunting)
                    TriggerClientEvent(player, Constants.Events.Client.ClientAction, Constants.Events.Client.Actions.Kill);

                Debug.WriteLine($"OnPlayerInitialSpawn: {player.Name}");
            }
        }

        private void OnPlayerSpawn(int playerServerId)
        {
            Player player;

            player = GameManager.AllPlayers[playerServerId];
            if (player != null)
            {
                Debug.WriteLine($"OnPlayerSpawn: {player.Name}");
            }
        }
        #endregion
    }
}
