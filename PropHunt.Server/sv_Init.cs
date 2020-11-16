using CitizenFX.Core;
using CitizenFX.Core.Native;
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

namespace PropHunt.Server
{
    public class sv_Init : BaseScript
    {
        internal sv_Rounds Rounds { get; private set; }
        internal sv_Player Player { get; private set; }
        internal sv_Environment Environment { get; private set; }

        public sv_Init()
        {
            try
            {
                //
                // Initialize server elements
                this.Rounds = new sv_Rounds(this);
                this.Player = new sv_Player(this);
                this.Environment = new sv_Environment(this);

                //
                // Subscribe to events
                this.EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
                this.EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDisconnected);
                this.EventHandlers[Constants.Events.Server.OnPlayerInitialSpawn] += new Action<int>(this.Player.OnPlayerInitialSpawn);
                this.EventHandlers[Constants.Events.Server.OnPlayerSpawn] += new Action<int>(this.Player.OnPlayerSpawn);

                Debug.WriteLine("PropHunt.Server was loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PropHunt.Server failed to load: {ex.Message}");
            }
        }

        #region Native Events
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
    }
}
