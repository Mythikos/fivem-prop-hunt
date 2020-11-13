using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Enumerations
{
    public enum GameStates
    {
        /// <summary>
        /// Waiting for more players
        /// </summary>
        WaitingForPlayers = 0,

        /// <summary>
        /// Syncing up player information, setting hunters, etc
        /// </summary>
        PreRound = 1,

        /// <summary>
        /// Hunters are blinded, props are hiding
        /// </summary>
        Hiding = 2,

        /// <summary>
        /// Hunters are actively looking for props
        /// </summary>
        Hunting = 3,

        /// <summary>
        /// The game has concluded, show scoreboards, etc
        /// </summary>
        PostRound = 4
    }
}
