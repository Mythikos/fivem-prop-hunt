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
///     X Props need to have limits of what they can and can't become. Both by type (ped vs prop vs car) and size.
///     Need to add locations or "sections" of the map to play on
///     X Remove attached props from player during round reset
///     X Based prop player health on dimensions of prop / disable auto health regeneration
///     When hunters are blinded, weapon wheel removes blind. Need to disable weapon wheel until round is Hunting
///     When the attached prop breaks, the player needs to die
///     X Player's are freely respawning during the game
///     X Player's stamina should be set to 100% permanently 
///     Prop rotation replication is not working
///     Add blip above player's heads that are on the same team
///     Add sound taunting mechanic if player is stationary for over 60 seconds, every 60 seconds
///     OnPlayerDied hook seems to be pretty irregular
/// 
///     TEST THE PLAYER KILLED, DIED, WASTED EVENTS AND WHEN THEY ARE CALLED. DIED IS INCONSISTENT, APPEARS TO BE BECAUSE DIED AND KILLED ARE IN AN ELSE STATEMENT - ONLY 
///     ONE CAN BE CALLED AT A TIME. WATED APPEARS TO BE ALWAYS CALLED IF THE PLAYER DIED.
/// </summary>
namespace PropHunt.Client
{
    public class cl_Init : BaseScript
    {
        internal cl_Rounds Rounds { get; private set; }
        internal cl_Player Player { get; private set; }
        internal cl_Commands Commands { get; private set; }

        public cl_Init()
        {
            try
            {
                //
                // Initialize client elements
                this.Rounds = new cl_Rounds(this);
                this.Player = new cl_Player(this);
                this.Commands = new cl_Commands(this);

                //
                // Subscribe to events
                this.Tick += OnTick;
                this.Tick += this.Rounds.OnTick;
                this.Tick += this.Player.OnTick;
                this.EventHandlers.Add("playerSpawned", new Action(this.Player.OnPlayerSpawned));
                this.EventHandlers.Add("baseevents:onPlayerDied", new Action<Player, int>(this.Player.OnPlayerDied));
                this.EventHandlers.Add("baseevents:onPlayerKilled", new Action<Player, int, List<dynamic>>(this.Player.OnPlayerKilled));
                this.EventHandlers.Add("baseevents:onPlayerWasted", new Action<Player, string>(this.Player.OnPlayerWasted));
                this.EventHandlers.Add("gameEventTriggered", new Action<string, List<dynamic>>(OnGameEventTriggered));
                this.EventHandlers.Add(Constants.Events.Client.OnRoundSync, new Action<int, float>(this.Rounds.OnSync));
                this.EventHandlers.Add(Constants.Events.Client.OnRoundStateChanged, new Action<int>(this.Rounds.OnStateChanged));
                this.EventHandlers.Add(Constants.Events.Client.SyncTimeAndWeather, new Action<int, int>(OnSyncTimeAndWeather));
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
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.1f), $"Player State: {Game.Player.State.Get<PlayerTeams>(Constants.StateBagKeys.PlayerTeam)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.2f), $"Player Initial Spawn: {Game.Player.State.Get(Constants.StateBagKeys.PlayerInitialSpawn)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.3f), $"Player IsInvincible: {Game.PlayerPed.IsInvincible}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.4f), $"Player Health: {Game.PlayerPed.HealthFloat}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.5f), $"Player Armor: {Game.PlayerPed.ArmorFloat}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.6f), $"Time Remaining: {this.Rounds.TimeRemainingInSeconds}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.7f), $"Game State: {this.Rounds.GameState}");
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

        public void OnSyncTimeAndWeather(int timeState, int weatherState)
        {
            WeatherStates weatherStateEnum = (WeatherStates)weatherState;
            TimeOfDayStates timeStateEnum = (TimeOfDayStates)timeState;

            SetWeatherTypePersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNowPersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNow(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetOverrideWeather(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetForcePedFootstepsTracks(false);
            SetForceVehicleTrails(false);
            NetworkOverrideClockTime(timeStateEnum.GetAttribute<NativeValueInt>().NativeValue, 0, 0);
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
    }
}
