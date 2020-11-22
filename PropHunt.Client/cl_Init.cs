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
using System.Threading;
using PropHunt.Shared.Attributes;
using PropHunt.Shared.Extensions;
using Newtonsoft.Json;

namespace PropHunt.Client
{
    public class cl_Init : BaseScript
    {
        internal static readonly bool DebugMode = true;
        private Timer _garbageCollectorTimer;
        
        public cl_Init()
        {
            try
            {
                //
                // Get them static bois woke
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Player).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Logging).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Game).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Environment).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Commands).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_Audio).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(cl_World).TypeHandle);

                //
                // Assign instance stuff
                this._garbageCollectorTimer = new Timer(GarbageCollectorTimerCallback, null, 0, 60000);
                this.SpawnManager_SetAutoSpawn(false);
                PlayerList.SetInstance(this);

                //
                // Subscribe to events
                this.Tick += OnTick;
                this.Tick += cl_Game.OnTick;
                this.Tick += cl_Player.OnTick;
                this.Tick += cl_Player.OnTick_DrawGamerTags;
                this.Tick += cl_Player.OnTick_DrawComponents;
                this.Tick += cl_Player.Spectate.OnTick;

                this.EventHandlers.Add(Constants.Events.Player.Kill, new Action(() => { Game.Player.Character.Kill(); }));
                this.EventHandlers.Add(Constants.Events.Player.Spawn, new Action<float, float, float>((float x, float y, float z) => { this.SpawnManager_SpawnPlayer(x, y, z); }));
                
                this.EventHandlers.Add("gameEventTriggered", new Action<string, List<dynamic>>(OnEntityDamage));
                this.EventHandlers.Add("playerSpawned", new Action(cl_Player.OnPlayerSpawned));
                this.EventHandlers.Add("baseevents:onPlayerDied", new Action<Player, int>(cl_Player.OnPlayerDied));
                this.EventHandlers.Add("baseevents:onPlayerKilled", new Action<Player, int, dynamic>(cl_Player.OnPlayerKilled));
                
                this.EventHandlers.Add(Constants.Events.GameManager.OnSyncGameState, new Action<int, float>(cl_Game.OnSyncGameState));
                this.EventHandlers.Add(Constants.Events.GameManager.OnGameStateChanged, new Action<int>(cl_Game.OnGameStateChanged));
                
                this.EventHandlers.Add(Constants.Events.Environment.SetTime, new Action<int>(cl_Environment.SetTime));
                this.EventHandlers.Add(Constants.Events.Environment.SetWeather, new Action<int>(cl_Environment.SetWeather));
                this.EventHandlers.Add(Constants.Events.Environment.SetWeatherAndTime, new Action<int, int>(cl_Environment.SetWeatherAndTime));
                
                this.EventHandlers.Add(Constants.Events.Audio.Play, new Action<string, string>(cl_Audio.Play));
                this.EventHandlers.Add(Constants.Events.Audio.PlayFromPlayer, new Action<string, string>(cl_Audio.PlayFromPlayer));
                this.EventHandlers.Add(Constants.Events.Audio.PlayFromPosition, new Action<float, float, float, string, string>(cl_Audio.PlayFromPosition));

                this.EventHandlers.Add(Constants.Events.World.Setup, new Action<string>((string json) => { cl_World.Setup(JsonConvert.DeserializeObject<cl_World.Zone>(json)); }));
                this.EventHandlers.Add(Constants.Events.World.Cleanup, new Action<string>((string json) => { cl_World.Cleanup(JsonConvert.DeserializeObject<cl_World.Zone>(json)); }));

                Debug.WriteLine($"PropHunt.Client was loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PropHunt.Client failed to load: {ex.Message}");
            }
        }

        #region Events
        private async Task OnTick()
        {
            //
            // Draw debugging
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.0f), $"SID: {Game.Player.ServerId} - Handle: {Game.Player.Handle}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.1f), $"Player Team: {Game.Player.State.Get<PlayerTeams>(Constants.State.Player.Team)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.2f), $"Player Initial Spawn: {Game.Player.State.Get(Constants.State.Player.InitialSpawn)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.3f), $"Player IsInvincible: {Game.PlayerPed.IsInvincible}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.4f), $"Time Remaining: {cl_Game.TimeRemainingInSeconds}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.5f), $"Game State: {cl_Game.GameState}");
        }

        /// <summary>
        /// Fires when the CEventNetworkEntityDamage game event is triggered 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        private static void OnEntityDamage(string name, List<dynamic> args)
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
        #endregion

        #region Spawn Manager Exports
        public void SpawnManager_SetAutoSpawn(bool enable)
            => this.Exports["spawnmanager"].setAutoSpawn(enable);

        private void SpawnManager_ForceRespawn()
            => this.Exports["spawnmanager"].forceRespawn();

        private void SpawnManager_SetAutoSpawnCallback(ExportDictionary export)
            => this.Exports["spawnmanager"].setAutoSpawnCallback(export);

        public void SpawnManager_SpawnPlayer(float x, float y, float z, string modelName = "a_m_y_hipster_01")
        {
            dynamic spawnInfo = new ExpandoObject();
            spawnInfo.x = x;
            spawnInfo.y = y;
            spawnInfo.z = z;
            spawnInfo.heading = 0;
            spawnInfo.model = GetHashKey(modelName);

            SpawnManager_SetAutoSpawnCallback(this.Exports["spawnmanager"].spawnPlayer(spawnInfo));
            SpawnManager_ForceRespawn();
            SpawnManager_SetAutoSpawn(false);
        }
        #endregion

        #region Timer Callbacks
        /// <summary>
        /// Task for clearing unused memory periodically.
        /// </summary>
        /// <returns></returns>
        private void GarbageCollectorTimerCallback(object state)
            => GC.Collect();
        #endregion

        internal static class PlayerList
        {
            private static cl_Init _parentInstance;
            public static void SetInstance(cl_Init parentInstance)
                => _parentInstance = parentInstance;

            public static List<Player> GetAllPlayers()
                => _parentInstance?.Players?.ToList() ?? new List<Player>();

            public static List<Player> GetAllActivePlayers()
                => _parentInstance?.Players?.Where(x => x.State.Get<bool>(Constants.State.Player.InitialSpawn) == false).ToList() ?? new List<Player>();

            public static Player GetPlayer(int serverId)
                => _parentInstance?.Players?[serverId];

            public static Player GetPlayer(string name)
                => _parentInstance?.Players?.FirstOrDefault(x => x.Name.Equals(name));
        }
    }
}
