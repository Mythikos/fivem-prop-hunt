using PropHunt.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Server
{
    /// <summary>
    /// Handle's playing audio from the server.
    /// Audio reference: https://github.com/DurtyFree/gta-v-data-dumps/blob/master/soundNames.json
    /// </summary>
    internal class sv_Audio
    {
        #region Properties
        private sv_Init _parentInstance;
        #endregion

        public sv_Audio(sv_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;
        }

        #region Play Methods
        /// <summary>
        /// Plays a sound from the specified player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public void PlayFromPlayer(Player player, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(player, Constants.Events.Client.OnAudioPlayFromPlayer, audioName, audioReference);

        /// <summary>
        /// Plays a sound at the coordinate specified
        /// </summary>
        /// <param name="position"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public void PlayFromPosition(Vector3 position, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(Constants.Events.Client.OnAudioPlayFromPosition, position.X, position.Y, position.Z, audioName, audioReference);

        /// <summary>
        /// Emits the sound for the player to hear
        /// </summary>
        /// <param name="player"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public void Play(Player player, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(Constants.Events.Client.OnAudioPlay, audioName, audioReference);
        #endregion
    }
}
