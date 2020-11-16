using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    /// <summary>
    /// Handles playing sound on the client
    /// Audio reference: https://github.com/DurtyFree/gta-v-data-dumps/blob/master/soundNames.json
    /// </summary>
    internal class cl_Audio
    {
        private cl_Init _parentInstance;

        public cl_Audio(cl_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;
        }

        #region Play Methods
        public void PlayFromPlayer(string audioName, string audioReference)
            => PlaySoundFromEntity(-1, audioName, Game.Player.Character.Handle, audioReference, true, 0);

        public void PlayFromPosition(float x, float y, float z, string audioName, string audioReference)
            => PlaySoundFromCoord(-1, audioName, x, y, z, audioReference, true, 0, true);

        public void Play(string audioName, string audioReference)
            => PlaySound(-1, audioName, audioReference, true, 0, true);
        #endregion
    }
}
