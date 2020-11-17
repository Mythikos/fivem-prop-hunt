using CitizenFX.Core;
using CitizenFX.Core.UI;
using PropHunt.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared;
using PropHunt.Client.Extensions;
using System.Dynamic;
using PropHunt.Shared.Attributes;
using PropHunt.Shared.Extensions;

/// <summary>
/// NOTES:
///     prop_mil_crate_02 is good for a crate
///     1593 3219 40 is a good position
/// 
/// TODO:
///     Need to add locations or "sections" of the map to play on
///     When hunters are blinded, weapon wheel removes blind. Need to disable weapon wheel until round is Hunting
///     When the attached prop breaks, the player needs to die
///     Prop rotation replication is not working
///     Add blip above player's heads that are on the same team
/// </summary>
namespace PropHunt.Client
{
    public class cl_Init : BaseScript
    {
        internal cl_Rounds Rounds { get; private set; }
        internal cl_Player Player { get; private set; }
        internal cl_Environment Environment { get; private set; }
        internal cl_Audio Audio { get; private set; }
        internal cl_Commands Commands { get; private set; }

        public cl_Init()
        {
            try
            {
                //
                // Initialize client elements
                this.Rounds = new cl_Rounds(this);
                this.Player = new cl_Player(this);
                this.Environment = new cl_Environment(this);
                this.Audio = new cl_Audio(this);
                this.Commands = new cl_Commands(this);

                //
                // Subscribe to events
                this.Tick += OnTick;
                this.Tick += GcTick;
                this.Tick += this.Rounds.OnTick;
                this.Tick += this.Player.OnTick;
                this.Tick += this.Player.OnTick_DrawGamerTags;
                this.Tick += this.Player.OnTick_DrawComponents;

                this.EventHandlers.Add("playerSpawned", new Action(this.Player.OnPlayerSpawned));
                this.EventHandlers.Add("baseevents:onPlayerDied", new Action<Player, int>(this.Player.OnPlayerDied));
                this.EventHandlers.Add("baseevents:onPlayerKilled", new Action<Player, int, dynamic>(this.Player.OnPlayerKilled));
                this.EventHandlers.Add("gameEventTriggered", new Action<string, List<dynamic>>(OnGameEventTriggered));

                this.EventHandlers.Add(Constants.Events.Client.OnRoundSync, new Action<int, float>(this.Rounds.OnSync));
                this.EventHandlers.Add(Constants.Events.Client.OnRoundStateChanged, new Action<int>(this.Rounds.OnStateChanged));

                this.EventHandlers.Add(Constants.Events.Client.OnEnvironmentTimeChanged, new Action<int>(this.Environment.OnTimeChanged));
                this.EventHandlers.Add(Constants.Events.Client.OnEnvironmentWeatherChanged, new Action<int>(this.Environment.OnWeatherChanged));
                this.EventHandlers.Add(Constants.Events.Client.OnEnvironmentWeatherAndTimeChanged, new Action<int, int>(this.Environment.OnWeatherAndTimeChanged));

                this.EventHandlers.Add(Constants.Events.Client.OnAudioPlay, new Action<string, string>(this.Audio.Play));
                this.EventHandlers.Add(Constants.Events.Client.OnAudioPlayFromPlayer, new Action<string, string>(this.Audio.PlayFromPlayer));
                this.EventHandlers.Add(Constants.Events.Client.OnAudioPlayFromPosition, new Action<float, float, float, string, string>(this.Audio.PlayFromPosition));

                this.EventHandlers.Add(Constants.Events.Client.ClientAction, new Action<string>(OnClientAction));

                Debug.WriteLine($"PropHunt.Client was loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PropHunt.Client failed to load: {ex.Message}");
            }
        }

        #region Client Events
        private async Task OnTick()
        {
            //
            // Draw debugging
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.0f), $"SID: {Game.Player.ServerId} - Handle: {Game.Player.Handle}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.1f), $"Player Team: {Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.2f), $"Player Initial Spawn: {Game.Player.State.Get(Constants.StateBagKeys.PlayerInitialSpawn)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.3f), $"Player IsInvincible: {Game.PlayerPed.IsInvincible}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.4f), $"Time Remaining: {this.Rounds.TimeRemainingInSeconds}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.5f), $"Game State: {this.Rounds.GameState}");
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            Entity target = null;
            Entity attacker = null;
            bool targetDied = false;
            uint weaponHash = 0;

            if (name.Equals("CEventNetworkEntityDamage"))
            {
                if (args[0] != null)
                    target = Entity.FromHandle(int.Parse(args[0].ToString()));
                if (args[1] != null)
                    attacker = Entity.FromHandle(int.Parse(args[1].ToString()));
                if (args[2] != null)
                    targetDied = int.Parse(args[3].ToString()) == 1;
                if (args[3] != null)
                    weaponHash = (uint)int.Parse(args[4].ToString());

                if (target != null && attacker != null)
                {
                    if (!target.Model.IsVehicle && !(target is Ped))
                    {
                        if (targetDied == true)
                        {
                            if (target.IsAttached() == true)
                            {
                                Entity attachedEntity = target.GetEntityAttachedTo();
                                if (attachedEntity != null && attachedEntity is Ped)
                                {
                                    attachedEntity.HasBeenDamagedBy(attacker);
                                    ApplyDamageToPed(attachedEntity.Handle, 9999, false);
                                }
                                else
                                {
                                    if (!(target is Ped))
                                    {
                                        // Damage is being dealt to a prop that isn't a player, rek them
                                        if (weaponHash != 0)
                                        {
                                            ApplyDamageToPed(attacker.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                                        }
                                        else
                                        {
                                            ApplyDamageToPed(attacker.Handle, 10, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!(target is Ped))
                                {
                                    // Damage is being dealt to a prop that isn't a player, rek them
                                    if (weaponHash != 0)
                                    {
                                        ApplyDamageToPed(attacker.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                                    }
                                    else
                                    {
                                        ApplyDamageToPed(attacker.Handle, 10, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (target.IsAttached() == true)
                            {
                                Entity attachedEntity = target.GetEntityAttachedTo();
                                if (attachedEntity != null && attachedEntity is Ped)
                                {
                                    attachedEntity.HasBeenDamagedBy(attacker);
                                    ApplyDamageToPed(attachedEntity.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                                }
                                else
                                {
                                    if (!(target is Ped))
                                    {
                                        // Damage is being dealt to a prop that isn't a player, rek them
                                        if (weaponHash != 0)
                                        {
                                            ApplyDamageToPed(attacker.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                                        }
                                        else
                                        {
                                            ApplyDamageToPed(attacker.Handle, 10, true);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!(target is Ped))
                                {
                                    // Damage is being dealt to a prop that isn't a player, rek them
                                    if (weaponHash != 0)
                                    {
                                        ApplyDamageToPed(attacker.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                                    }
                                    else
                                    {
                                        ApplyDamageToPed(attacker.Handle, 10, true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!(target is Ped))
                        {
                            // Damage is being dealt to a prop that isn't a player, rek them
                            if (weaponHash != 0)
                            {
                                ApplyDamageToPed(attacker.Handle, (int)Math.Ceiling(GetWeaponDamage(weaponHash, 0)), true);
                            }
                            else
                            {
                                ApplyDamageToPed(attacker.Handle, 10, true);
                            }
                        }
                    }
                }
            }
        }

        public void OnClientAction(string action)
        {
            if (action.Equals(Constants.Events.Client.Actions.Kill))
            {
                Game.PlayerPed.Kill();
            }
            else if (action.Equals(Constants.Events.Client.Actions.Spawn))
            {
                this.SpawnManager_SpawnPlayer(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
            }
        }
        #endregion

        #region Spawn Manager Exports
        public void SpawnManager_SetAutoSpawn(bool enable)
        {
            this.Exports["spawnmanager"].setAutoSpawn(enable);
        }

        private void SpawnManager_ForceRespawn()
        {
            this.Exports["spawnmanager"].forceRespawn();
        }

        private void SpawnManager_SetAutoSpawnCallback(ExportDictionary export)
        {
            this.Exports["spawnmanager"].setAutoSpawnCallback(export);
        }

        public void SpawnManager_SpawnPlayer(float x, float y, float z, string modelName = "a_m_y_hipster_01")
        {
            dynamic spawnInfo = new ExpandoObject();
            spawnInfo.x = x;
            spawnInfo.y = y;
            spawnInfo.z = z;
            spawnInfo.heading = 0;
            spawnInfo.model = GetHashKey(modelName);

            SpawnManager_SetAutoSpawnCallback(this.Exports["spawnmanager"].spawnPlayer(spawnInfo));
            SpawnManager_SetAutoSpawn(false);
            SpawnManager_ForceRespawn();
        }
        #endregion

        #region gc thread
        /// <summary>
        /// Task for clearing unused memory periodically.
        /// </summary>
        /// <returns></returns>
        int gcTickTimer = GetGameTimer();
        private async Task GcTick()
        {
            if (GetGameTimer() - gcTickTimer > 60000)
            {
                gcTickTimer = GetGameTimer();
                GC.Collect();
            }

            await Delay(1000);
        }
        #endregion
    }
}
