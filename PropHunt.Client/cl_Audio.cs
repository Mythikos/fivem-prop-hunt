using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using PropHunt.Shared;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    /// <summary>
    /// Handles playing sound on the client
    /// Audio reference: https://github.com/DurtyFree/gta-v-data-dumps/blob/master/soundNames.json
    /// </summary>
    internal static class cl_Audio
    {
        /// <summary>
        /// Plays audio from the position of a player
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void PlayFromPlayer(string audioName, string audioReference)
            => PlaySoundFromEntity(-1, audioName, Game.Player.Character.Handle, audioReference, true, 0);

        /// <summary>
        /// Plays audio from the targeted position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void PlayFromPosition(float x, float y, float z, string audioName, string audioReference)
            => PlaySoundFromCoord(-1, audioName, x, y, z, audioReference, true, 0, true);

        /// <summary>
        /// Plays audio for local player
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="audioReference"></param>
        public static void Play(string audioName, string audioReference)
            => PlaySound(-1, audioName, audioReference, true, 0, true);
    }
}