using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PropHunt.Shared.Extensions;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Server
{
    internal static class sv_Player
    {
        #region Properties
        private const float TIMER_TAUNT_CHECK_INTERVAL = 1000f;

        private static Timer _tauntCheckTimer;
        private static readonly Dictionary<string, string> _tauntSounds = new Dictionary<string, string>()
        {
            { "SPEECH_RELATED_SOUNDS", "Franklin_Whistle_For_Chop" },
            { "DLC_Apt_Yacht_Ambient_Soundset", "HORN" }
        };
        #endregion

        static sv_Player()
        {
            // prep taunt timer
            _tauntCheckTimer = new Timer(TauntCheckTimerCallback, null, 0, (int)TIMER_TAUNT_CHECK_INTERVAL);
        }

        #region Events
        public static void OnPlayerInitialSpawn(int playerServerId)
        {
            Player player;

            player = sv_Init.PlayerList.GetPlayer(playerServerId);
            if (player != null)
            {
                if (sv_Game.State == GameStates.Hiding || sv_Game.State == GameStates.Hunting)
                    sv_Init.TriggerClientEvent(player, Constants.Events.Player.Kill);
                sv_Logging.Log("OnPlayerInitialSpawn called.");
            }
            else
            {
                sv_Logging.Log("Player was not found during OnPlayerInitialSpawn... you fucked up.");
            }
        }

        public static void OnPlayerSpawn(int playerServerId)
        {
            Player player;

            player = sv_Init.PlayerList.GetPlayer(playerServerId);
            if (player != null)
            {
                sv_Logging.Log("OnPlayerSpawned called.");
            }
            else
            {
                sv_Logging.Log("Player was not found during OnPlayerSpawnEvent... you fucked up.");
            }
        }
        #endregion

        #region Timer Callbacks
        private static void TauntCheckTimerCallback(object state)
        {
            string lastPositionString = string.Empty;
            long tauntTime = 0;
            float currentX = 0f;
            float currentY = 0f;
            float currentZ = 0f;
            float lastX = 0f;
            float lastY = 0f;
            float lastZ = 0f;
            KeyValuePair<string, string> selectedTaunt;

            // We only want to make a taunt check during the hiding phase
            if (sv_Game.State != GameStates.Hunting)
                return;

            // Iterate over players and check their last position and check count
            foreach (Player player in sv_Init.PlayerList.GetAllActivePlayers())
            {
                if (player != null && player.Character != null)
                {
                    if (player.State.Get<PlayerTeams>(Constants.State.Player.Team) == PlayerTeams.Prop)
                    {
                        currentX = (float)Math.Floor(player.Character.Position.X);
                        currentY = (float)Math.Floor(player.Character.Position.Y);
                        currentZ = (float)Math.Floor(player.Character.Position.Z);

                        lastX = (float)Math.Floor(player.State.Get<float>(Constants.State.Player.TauntLastPositionX));
                        lastY = (float)Math.Floor(player.State.Get<float>(Constants.State.Player.TauntLastPositionY));
                        lastZ = (float)Math.Floor(player.State.Get<float>(Constants.State.Player.TauntLastPositionZ));

                        tauntTime = player.State.Get<long>(Constants.State.Player.TauntLastTime);
                        if (tauntTime == default) // Handle initial assignment
                            tauntTime = GetGameTimer();

                        if (currentX != lastX || currentY != lastY || currentZ != lastZ)
                            tauntTime = GetGameTimer();

                        if (GetGameTimer() - 30000 > tauntTime)
                        {
                            selectedTaunt = _tauntSounds.Random();
                            sv_Audio.PlayFromPlayer(player, selectedTaunt.Value, selectedTaunt.Key);
                            tauntTime = GetGameTimer();
                        }

                        player.State.Set<long>(Constants.State.Player.TauntLastTime, tauntTime, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.State.Player.TauntLastPositionX, player.Character.Position.X, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.State.Player.TauntLastPositionY, player.Character.Position.Y, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.State.Player.TauntLastPositionZ, player.Character.Position.Z, false); // Dont replicate to client, no need
                    }
                }
            }
        }
        #endregion
    }
}
