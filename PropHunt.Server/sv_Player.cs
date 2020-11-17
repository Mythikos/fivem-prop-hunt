using CitizenFX.Core;
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
    internal class sv_Player
    {
        #region Properties
        private sv_Init _parentInstance;

        private const float TIMER_TAUNT_CHECK_INTERVAL = 1000f;
        private Timer _tauntCheckTimer;
        private readonly Dictionary<string, string> _tauntSounds = new Dictionary<string, string>()
        {
            { "SPEECH_RELATED_SOUNDS", "Franklin_Whistle_For_Chop" },
            { "DLC_Apt_Yacht_Ambient_Soundset", "HORN" }
        };
        #endregion

        public sv_Player(sv_Init parentInstance)
        {
            if (parentInstance == null)
                throw new ArgumentNullException("parentInstance");

            this._parentInstance = parentInstance;

            // prep taunt timer
            this._tauntCheckTimer = new Timer(TauntCheckCallback, null, 0, (int)TIMER_TAUNT_CHECK_INTERVAL);
        }

        #region Events
        [EventHandler(Constants.Events.Server.OnPlayerInitialSpawn)]
        public void OnPlayerInitialSpawn(int playerServerId)
        {
            Player player;

            player = this._parentInstance.Rounds.AllPlayers[playerServerId];
            if (player != null)
            {
                if (this._parentInstance.Rounds.State == GameStates.Hiding || this._parentInstance.Rounds.State == GameStates.Hunting)
                    sv_Init.TriggerClientEvent(player, Constants.Events.Client.Kill);
            }
        }

        [EventHandler(Constants.Events.Server.OnPlayerSpawn)]
        public void OnPlayerSpawn(int playerServerId)
        {
            Player player;

            player = this._parentInstance.Rounds.AllPlayers[playerServerId];
            if (player != null)
            {

            }
        }

        [EventHandler(Constants.Events.Server.GetPlayerCoords)]
        private void GetPlayerCoords([FromSource] Player source, int playerId, NetworkCallbackDelegate callback)
        {
            if (IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.Teleport") || IsPlayerAceAllowed(source.Handle, "vMenu.Everything") ||
                IsPlayerAceAllowed(source.Handle, "vMenu.OnlinePlayers.All"))
            {
                var coords = new PlayerList[playerId]?.Character?.Position ?? Vector3.Zero;

                _ = callback(coords);

                return;
            }

            _ = callback(Vector3.Zero);
        }
        #endregion

        #region Timer Callbacks
        private void TauntCheckCallback(object state)
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
            if (this._parentInstance.Rounds.State != GameStates.Hunting)
                return;

            // Iterate over players and check their last position and check count
            foreach (Player player in new PlayerList())
            {
                if (player != null && player.Character != null)
                {
                    if (player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam) == PlayerTeams.Prop)
                    {
                        currentX = (float)Math.Floor(player.Character.Position.X);
                        currentY = (float)Math.Floor(player.Character.Position.Y);
                        currentZ = (float)Math.Floor(player.Character.Position.Z);

                        lastX = (float)Math.Floor(player.State.Get<float>(Constants.StateBagKeys.TauntLastPositionX));
                        lastY = (float)Math.Floor(player.State.Get<float>(Constants.StateBagKeys.TauntLastPositionY));
                        lastZ = (float)Math.Floor(player.State.Get<float>(Constants.StateBagKeys.TauntLastPositionZ));

                        tauntTime = player.State.Get<long>(Constants.StateBagKeys.TauntLastTime);
                        if (tauntTime == default) // Handle initial assignment
                            tauntTime = GetGameTimer();

                        if (currentX != lastX || currentY != lastY || currentZ != lastZ)
                            tauntTime = GetGameTimer();

                        if (GetGameTimer() - 30 > tauntTime)
                        {
                            selectedTaunt = _tauntSounds.Random();
                            this._parentInstance.Audio.PlayFromPlayer(player, selectedTaunt.Value, selectedTaunt.Key);
                            tauntTime = GetGameTimer();
                        }

                        player.State.Set<long>(Constants.StateBagKeys.TauntLastTime, tauntTime, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.StateBagKeys.TauntLastPositionX, player.Character.Position.X, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.StateBagKeys.TauntLastPositionY, player.Character.Position.Y, false); // Dont replicate to client, no need
                        player.State.Set<float>(Constants.StateBagKeys.TauntLastPositionZ, player.Character.Position.Z, false); // Dont replicate to client, no need
                    }
                }
            }
        }
        #endregion
    }
}
