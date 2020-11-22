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
    internal static class sv_Audio
    {
        /// <summary>
        /// Plays a sound from the specified player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void PlayFromPlayer(Player player, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(player, Constants.Events.Audio.PlayFromPlayer, audioName, audioReference);

        /// <summary>
        /// Plays a sound at the coordinate specified
        /// </summary>
        /// <param name="position"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void PlayFromPosition(Vector3 position, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(Constants.Events.Audio.PlayFromPosition, position.X, position.Y, position.Z, audioName, audioReference);

        /// <summary>
        /// Emits the sound for the player to hear
        /// </summary>
        /// <param name="player"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void Play(Player player, string audioName, string audioReference)
            => sv_Init.TriggerClientEvent(Constants.Events.Audio.Play, audioName, audioReference);
    }
}
