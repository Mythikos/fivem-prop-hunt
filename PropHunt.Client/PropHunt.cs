using CitizenFX.Core;
using CitizenFX.Core.UI;
using PropHunt.Client.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropHunt.Client.Library;
using PropHunt.Client.Library.Managers;
using static CitizenFX.Core.Native.API;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared;
using PropHunt.Client.Library.Extensions;
using System.Dynamic;

/// <summary>
/// NOTES:
///     prop_mil_crate_02 is good for a crate
///     1593 3219 40 is a good position
/// 
/// TODO:
///     Props need to have limits of what they can and can't become. Both by type (ped vs prop vs car) and size.
///     Need to add locations or "sections" of the map to play on
///     Remove attached props from player during round reset
///     Add taunting mechanic for props
///     Based prop player health on dimensions of prop / disable auto health regeneration
///     When hunters are blinded, weapon wheel removes blind. Need to disable weapon wheel until round is Hunting
///     When the attached prop breaks, the player needs to die
///     Shooting props that aren't a player doesn't seem to apply damage to the attacker.
/// </summary>
namespace PropHunt.Client
{
    public class PropHunt : BaseScript
    {
        public PropHunt()
        {
            //
            // Initialize managers
            GameManager.Initialize(this); // Initialize first first
            CommandManager.Initialize(this);

            //
            // Subscribe to events
            this.Tick += OnTick;
            this.EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);
            this.EventHandlers["onPlayerDied"] += new Action<int, int[]>(OnPlayerDied);
            this.EventHandlers[Constants.Events.Client.GameSync] += new Action<int, float>(OnGameSync);
            this.EventHandlers[Constants.Events.Client.GameStateUpdate] += new Action<int>(OnGameStateUpdate);
            this.EventHandlers[Constants.Events.Client.ClientAction] += new Action<string>(OnClientAction);
            this.EventHandlers["gameEventTriggered"] += new Action<string, List<dynamic>>(OnGameEventTriggered);

            //
            // Setup exports
            this.SetAutoSpawn(false);

            //
            // Output plugin was loaded?
            Debug.WriteLine($"PropHunt.Client was loaded successfully.");
            TextUtil.SendChatMessage($"PropHunt.Client was loaded successfully.");
        }

        #region Native Events
        private async Task OnTick()
        {
            //
            // Execute manager ticks
            GameManager.OnTick();

            //
            // Draw debugging
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.1f), $"Player State: {Game.Player.State.Get<PlayerStates>(Constants.StateBagKeys.PlayerState)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.2f), $"Player Initial Spawn: {Game.Player.State.Get(Constants.StateBagKeys.PlayerInitialSpawn)}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.3f), $"Time Remaining: {GameManager.TimeRemainingInSeconds}");
            TextUtil.DrawText3D(Game.PlayerPed.Position + new Vector3(0f, 0f, 1.4f), $"Game State: {GameManager.GameState}");
        }

        private void OnPlayerSpawned()
        {
            // Enable PvP
            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(PlayerPedId(), true, true);

            // Determine type of spawn
            if (Game.Player.State.Get(Constants.StateBagKeys.PlayerInitialSpawn) == true)
            {
                TriggerServerEvent(Constants.Events.Server.PlayerInitialSpawn, Game.Player.ServerId);
                Game.Player.State.Set(Constants.StateBagKeys.PlayerInitialSpawn, false, true);
            }
            TriggerServerEvent(Constants.Events.Server.PlayerSpawn, Game.Player.ServerId);
        }

        private void OnPlayerDied(int killerType, int[] deathCoords)
        {
            Game.Player.State.Set<PlayerStates>(Constants.StateBagKeys.PlayerState, PlayerStates.Dead, true);
            TextUtil.SendChatMessage("Im ded nigga");
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            if (name.Equals("CEventNetworkEntityDamage"))
            {
                Entity target = Entity.FromHandle(int.Parse(args[0].ToString()));
                Entity attacker = Entity.FromHandle(int.Parse(args[1].ToString()));
                bool targetDied = int.Parse(args[3].ToString()) == 1;
                uint weaponHash = (uint)int.Parse(args[4].ToString());

                if (target != null)
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
                            }
                            else
                            {
                                ApplyDamageToPed(attacker.Handle, 5, false);
                            }
                        }
                    }
                    else
                    {
                        ApplyDamageToPed(attacker.Handle, 5, false);
                    }
                }
            }
        }
        #endregion

        #region PropHunt Events
        public void OnGameSync(int gameState, float timeRemainingInSeconds)
        {
            GameManager.GameState = (GameStates)gameState;
            GameManager.TimeRemainingInSeconds = timeRemainingInSeconds;
        }

        public void OnGameStateUpdate(int gameState)
        {
            GameManager.OnUpdateGameState((GameStates)gameState);
        }

        public void OnClientAction(string action)
        {
            switch (action)
            {
                case Constants.Events.Client.Actions.Kill:
                    Game.PlayerPed.Kill();
                    break;
                case Constants.Events.Client.Actions.Spawn:
                    this.SpawnPlayer(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
                    break;
            }
        }
        #endregion

        #region Player Spawn Exports
        public void SetAutoSpawn(bool enable)
        {
            this.Exports["spawnmanager"].setAutoSpawn(enable);
        }

        private void ForceRespawn()
        {
            this.Exports["spawnmanager"].forceRespawn();
        }

        private void SetAutoSpawnCallback(ExportDictionary export)
        {
            this.Exports["spawnmanager"].setAutoSpawnCallback(export);
        }

        public void SpawnPlayer(float x, float y, float z, string modelName = "a_m_y_hipster_01")
        {
            dynamic spawnInfo = new ExpandoObject();
            spawnInfo.x = x;
            spawnInfo.y = y;
            spawnInfo.z = z;
            spawnInfo.heading = 0;
            spawnInfo.model = GetHashKey(modelName);

            SetAutoSpawn(false);
            SetAutoSpawnCallback(this.Exports["spawnmanager"].spawnPlayer(spawnInfo));
            ForceRespawn();
        }
        #endregion
    }
}
