using CitizenFX.Core;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Server
{
    internal class sv_Player
    {
        private sv_Init _parentInstance;
        public sv_Player(sv_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;
        }

        #region Events
        public void OnPlayerInitialSpawn(int playerServerId)
        {
            Player player;

            player = this._parentInstance.Rounds.AllPlayers[playerServerId];
            if (player != null)
            {
                if (this._parentInstance.Rounds.State == GameStates.Hiding || this._parentInstance.Rounds.State == GameStates.Hunting)
                    sv_Init.TriggerClientEvent(player, Constants.Events.Client.ClientAction, Constants.Events.Client.Actions.Kill);

                Debug.WriteLine($"OnPlayerInitialSpawn: {player.Name}");
            }
        }

        public void OnPlayerSpawn(int playerServerId)
        {
            Player player;

            player = this._parentInstance.Rounds.AllPlayers[playerServerId];
            if (player != null)
            {
                Debug.WriteLine($"OnPlayerSpawn: {player.Name}");
            }
        }
        #endregion
    }
}
