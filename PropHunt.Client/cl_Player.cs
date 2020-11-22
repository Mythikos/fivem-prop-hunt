using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Client.Extensions;
using PropHunt.Client.Utils;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using PropHunt.Shared.Extensions;
using PropHunt.Shared.Attributes;
using System.Text.RegularExpressions;
using PropHunt.Shared.Implementations;

namespace PropHunt.Client
{
    internal static class cl_Player
    {
        #region Instance Variables / Properties
        private const int HEALTH_MAX = 100;
        private const int HEALTH_MIN = 1;

        private static Dictionary<Player, int> _playerTags;
        #endregion

        static cl_Player()
        {
            _playerTags = new Dictionary<Player, int>();

            cl_Player.Reset();
            Game.Player.State.Set(Constants.State.Player.PropHandle, null, true);
            Game.Player.State.Set(Constants.State.Player.InitialSpawn, true, true);
            Game.Player.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Unassigned, true);
        }

        #region Events
        /// <summary>
        /// This handles general player natives that are required per frame
        /// </summary>
        /// <returns></returns>
        public static async Task OnTick()
        {
            SetPlayerHealthRechargeMultiplier(Game.Player.Handle, 0f);
            RestorePlayerStamina(Game.Player.Handle, 1f);
        }

        /// <summary>
        /// This handles the draw of gamer tags
        /// </summary>
        /// <returns></returns>
        public static async Task OnTick_DrawGamerTags()
        {
            float distance;
            bool canSee;
            List<Player> playerList;
            PlayerTeams playerTeam;
            PlayerTeams localPlayerTeam;

            localPlayerTeam = Game.Player.State.Get<PlayerTeams>(Constants.State.Player.Team);
            playerList = cl_Init.PlayerList.GetAllActivePlayers();

            foreach (Player player in playerList)
            {
                if (player != null && !player.Equals(Game.Player))
                {
                    // Get vars 
                    playerTeam = player.State.Get<PlayerTeams>(Constants.State.Player.Team);
                    distance = GetDistanceBetweenCoords(player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, true);
                    canSee = distance < 250 && HasEntityClearLosToEntity(Game.Player.Character.Handle, player.Character.Handle, 17) && (localPlayerTeam.Equals(playerTeam) || localPlayerTeam != PlayerTeams.Hunter);

                    // Handle tag visible state
                    if (_playerTags.ContainsKey(player))
                    {
                        if (!canSee)
                        {
                            RemoveMpGamerTag(_playerTags[player]);
                            _playerTags.Remove(player);
                        }
                        else
                        {
                            _playerTags[player] = CreateMpGamerTag(player.Character.Handle, cl_Player.GetSafeName(player), false, false, string.Empty, 0);
                        }
                    }
                    else if (canSee)
                    {
                        _playerTags.Add(player, CreateMpGamerTag(player.Character.Handle, cl_Player.GetSafeName(player), false, false, string.Empty, 0));
                    }

                    // Display the tag if we are of the right team and within the right distance
                    if (canSee && _playerTags.ContainsKey(player))
                    {
                        SetMpGamerTagVisibility(_playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, true);
                        SetMpGamerTagVisibility(_playerTags[player], GamerTagComponents.HealthAndArmor.GetAttribute<NativeValueInt>().NativeValue, true);
                        SetMpGamerTagVisibility(_playerTags[player], GamerTagComponents.AudioIcon.GetAttribute<NativeValueInt>().NativeValue, NetworkIsPlayerTalking(player.Handle));

                        SetMpGamerTagAlpha(_playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 255);
                        SetMpGamerTagAlpha(_playerTags[player], GamerTagComponents.HealthAndArmor.GetAttribute<NativeValueInt>().NativeValue, 255);
                        SetMpGamerTagAlpha(_playerTags[player], GamerTagComponents.AudioIcon.GetAttribute<NativeValueInt>().NativeValue, 255);

                        if (playerTeam == PlayerTeams.Hunter)
                            SetMpGamerTagColour(_playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 125);
                        else
                            SetMpGamerTagColour(_playerTags[player], GamerTagComponents.GamerName.GetAttribute<NativeValueInt>().NativeValue, 0);
                    }
                }
            }

            await BaseScript.Delay(100);
        }

        /// <summary>
        /// Handles the draw for ui components
        /// </summary>
        /// <returns></returns>
        public static async Task OnTick_DrawComponents()
        {
            //
            // Is it the right state
            if (cl_Game.GameState == GameStates.Hiding || cl_Game.GameState == GameStates.Hunting)
            {
                var playerState = Game.Player.State.Get<PlayerTeams>(Constants.State.Player.Team);

                //
                // Handle hunter's view
                if (playerState == PlayerTeams.Hunter)
                {
                    ShowHudComponentThisFrame((int)HudComponent.WeaponWheel);
                    foreach (int componentValue in Enum.GetValues(typeof(HudComponent)))
                        if (componentValue != (int)HudComponent.Reticle && componentValue != (int)HudComponent.WeaponWheel)
                            HideHudComponentThisFrame(componentValue);
                }

                //
                // Hande the prop's view
                else if (playerState == PlayerTeams.Prop)
                {
                    ShowHudComponentThisFrame((int)HudComponent.Reticle);
                    foreach (int componentValue in Enum.GetValues(typeof(HudComponent)))
                        if (componentValue != (int)HudComponent.Reticle)
                            HideHudComponentThisFrame(componentValue);
                    BlockWeaponWheelThisFrame();

                    List<Entity> entities1 = EntityUtil.GetSurroundingEntities(Game.Player.Character, 10, EntityUtil.IntersectOptions.IntersectObjects).ToList();
                    foreach (Entity entity in entities1)
                    {
                        EntityUtil.HighlightEntity(entity, 0.035f, 0.035f, 0.025f, 0.025f, new[] { 0, 0, 0, 255 });
                        EntityUtil.HighlightEntity(entity, 0.035f, 0.035f, 0.015f, 0.015f, new[] { 255, 255, 255, 255 });
                    }

                    int targetEntityHandle = default;
                    GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref targetEntityHandle);
                    if (targetEntityHandle != default)
                    {
                        TextUtil.DrawText3D(GetEntityCoords(targetEntityHandle, true), "Press E");
                        EntityUtil.HighlightEntity(Entity.FromHandle(targetEntityHandle), 0.035f, 0.035f, 0.025f, 0.025f, new[] { 0, 255, 0, 255 });
                        if (IsControlJustPressed(0, 38))
                            cl_Player.SetPlayerProp(targetEntityHandle);
                    }

                    if (Game.Player.State.Get(Constants.State.Player.PropHandle) != null)
                        SetPedCapsule(Game.Player.Character.Handle, 0.01f);
                }
            }
        }

        /// <summary>
        /// Fires when the player spawns
        /// </summary>
        public static async void OnPlayerSpawned()
        {
            //
            // Unspectate
            cl_Player.Spectate.End();

            //
            // Determine type of spawn
            if (Game.Player.State.Get(Constants.State.Player.InitialSpawn) == true)
            {
                cl_Init.TriggerServerEvent(Constants.Events.Player.OnInitialSpawn, Game.Player.ServerId);
                Game.Player.State.Set(Constants.State.Player.InitialSpawn, false, true);
            }
            cl_Init.TriggerServerEvent(Constants.Events.Player.OnSpawn, Game.Player.ServerId);
        }

        /// <summary>
        /// Seems to fire if the player was dealt indirect damage
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killerType"></param>
        public static async void OnPlayerDied([FromSource] Player player, int killerType)
        {
            //
            // Set player state
            Game.Player.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Unassigned, true);

            //
            // Begin spectate
            cl_Player.Spectate.Start();
        }

        /// <summary>
        /// Appears to fire if the palyer had a direct attacker and the attacker is known
        /// </summary>
        /// <param name="player"></param>
        /// <param name="killerId"></param>
        /// <param name="args"></param>
        public static async void OnPlayerKilled([FromSource] Player player, int killerId, dynamic args)
        {
            //
            // Set player state
            Game.Player.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Unassigned, true);

            //
            // Begin spectate
            cl_Player.Spectate.Start();

        }
        #endregion

        #region Spectator Mode
        public static class Spectate
        {
            private static Player _target;
            private static Camera _camera;
            private static Vector3 _cameraAngle;

            static Spectate()
            {
                _target = null;
                _camera = null;
                _cameraAngle = Vector3.Zero;
            }

            /// <summary>
            /// Handle the spectator mode
            /// </summary>
            /// <returns></returns>
            public static async Task OnTick()
            {
                Vector3 newPosition;

                if (_camera != null && _target != null)
                {
                    //
                    // Disable UI elements
                    DisableFirstPersonCamThisFrame();
                    BlockWeaponWheelThisFrame();

                    //
                    // Move between players
                    if (IsControlJustPressed(0, 24)) // Left mouse
                    {
                        cl_Logging.Log("Changing target forward");
                        await Spectate.ChangeTarget(1);
                    }
                    else if (IsControlJustPressed(0, 25)) // Right mouse
                    {
                        cl_Logging.Log("Changing target backward");
                        await Spectate.ChangeTarget(-1);
                    }

                    //
                    // Handle camera position/angle
                    newPosition = Spectate.ProcessPosition();
                    SetFocusArea(newPosition.X, newPosition.Y, newPosition.Z, 0f, 0f, 0f);
                    _camera.Position = newPosition;
                    _camera.PointAt(_target.Character.Position + new Vector3(0f, 0f, 0.5f));
                }
            }

            public static void Start()
            {
                if (_camera == null)
                {
                    ClearFocus();
                    _camera = World.CreateCamera(Game.Player.Character.Position, Vector3.Zero, GetGameplayCamFov());
                    _camera.IsActive = true;
                    RenderScriptCams(true, false, 0, true, false);
                    Spectate.ChangeTarget(0);
                }
            }

            public static async Task ChangeTarget(int direction = 1)
            {
                Player player = null;
                List<Player> playerList;
                WrappingIterator<Player> playerListIterator;

                // Get alive players that aren't the local player
                playerList = cl_Init.PlayerList.GetAllActivePlayers().Where(x => x.IsAlive && x.Equals(Game.Player) == false).ToList();
                if (playerList.Count() > 0)
                {
                    // Do we have an existing target?
                    if (_target != null && _target.Character != null)
                    {
                        // Lets move from this target's index
                        if (direction > 0) // Forward
                        {
                            playerListIterator = WrappingIterator<Player>.CreateAt(playerList, _target);
                            player = playerListIterator.GetNext();
                        }
                        else if (direction < 0) // Backwards
                        {
                            playerListIterator = WrappingIterator<Player>.CreateAt(playerList, _target);
                            player = playerListIterator.GetPrevious();
                        }
                        else if (direction == 0) // Randomize
                        {
                            player = playerList.Random();
                        }
                    }
                    else
                    {
                        // Randomize the target
                        player = playerList.Random();
                    }

                    // Verify we found a target and the target is different
                    if (player != null && player.Equals(_target) == false)
                    {
                        DoScreenFadeOut(500);
                        await BaseScript.Delay(500);

                        _target = player;
                        RequestCollisionAtCoord(_target.Character.Position.X, _target.Character.Position.Y, _target.Character.Position.Z);

                        DoScreenFadeIn(500);
                        await BaseScript.Delay(500);
                    }
                }
            }

            public static async Task End()
            {
                if (_camera != null)
                {
                    DoScreenFadeOut(500);
                    await BaseScript.Delay(500);

                    ClearFocus();
                    RenderScriptCams(false, false, 0, true, false);
                    _camera.Delete();

                    _camera = null;
                    _target = null;

                    DoScreenFadeIn(500);
                    await BaseScript.Delay(500);
                }
            }

            private static Vector3 ProcessPosition()
            {
                const float RADIUS = 6f;
                float mouseX = default;
                float mouseY = default;
                Vector3 behindCam = default;
                Vector3 offset = default;
                Vector3 position = default;
                int rayHandle = default;
                bool rayHit = default;
                Vector3 rayEndCoords = default;
                Vector3 raySurfaceNormal = default;
                int rayEntityHit = default;

                mouseX = GetDisabledControlNormal(1, 1);
                mouseY = GetDisabledControlNormal(1, 2);

                _cameraAngle.Z = _cameraAngle.Z - mouseX;
                _cameraAngle.Y = _cameraAngle.Y + mouseY;
                if (_cameraAngle.Y > 89f)
                    _cameraAngle.Y = 89f;
                else if (_cameraAngle.Y < -89f)
                    _cameraAngle.Y = -89f;

                behindCam = new Vector3();
                behindCam.X = _target.Character.Position.X + (((float)Math.Cos(_cameraAngle.Z) * (float)Math.Cos(_cameraAngle.Y)) + ((float)Math.Cos(_cameraAngle.Y) * (float)Math.Cos(_cameraAngle.Z))) / 2f * (RADIUS + 0.5f);
                behindCam.Y = _target.Character.Position.Y + (((float)Math.Sin(_cameraAngle.Z) * (float)Math.Cos(_cameraAngle.Y)) + ((float)Math.Cos(_cameraAngle.Y) * (float)Math.Sin(_cameraAngle.Z))) / 2f * (RADIUS + 0.5f);
                behindCam.Z = _target.Character.Position.Z + (((float)Math.Sin(_cameraAngle.Y))) * (RADIUS + 0.5f);

                rayHandle = StartShapeTestRay(_target.Character.Position.X, _target.Character.Position.Y, _target.Character.Position.Z + 0.5f, behindCam.X, behindCam.Y, behindCam.Z, -1, _target.Character.Handle, 0);
                GetShapeTestResult(rayHandle, ref rayHit, ref rayEndCoords, ref raySurfaceNormal, ref rayEntityHit);

                var maxRadius = RADIUS;
                if (rayHit && Vdist(_target.Character.Position.X, _target.Character.Position.Y, _target.Character.Position.Z + 0.5f, rayEndCoords.X, rayEndCoords.Y, rayEndCoords.Z) < RADIUS + 0.5)
                    maxRadius = Vdist(_target.Character.Position.X, _target.Character.Position.Y, _target.Character.Position.Z + 0.5f, rayEndCoords.X, rayEndCoords.Y, rayEndCoords.Z);

                offset = new Vector3();
                offset.X = (((float)Math.Cos(_cameraAngle.Z) * (float)Math.Cos(_cameraAngle.Y)) + ((float)Math.Cos(_cameraAngle.Y) * (float)Math.Cos(_cameraAngle.Z))) / 2 * maxRadius;
                offset.Y = (((float)Math.Sin(_cameraAngle.Z) * (float)Math.Cos(_cameraAngle.Y)) + ((float)Math.Cos(_cameraAngle.Y) * (float)Math.Sin(_cameraAngle.Z))) / 2 * maxRadius;
                offset.Z = (((float)Math.Sin(_cameraAngle.Y))) * maxRadius;

                position = new Vector3();
                position.X = _target.Character.Position.X + offset.X;
                position.Y = _target.Character.Position.Y + offset.Y;
                position.Z = _target.Character.Position.Z + offset.Z;

                return position;
            }
        }
        #endregion

        #region Player Helper Methods
        /// <summary>
        /// Returns the safe name of the player to prevent formatting shenanigans
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string GetSafeName(Player player)
        {
            string safeName = string.Empty;

            if (!string.IsNullOrEmpty(player.Name))
            {
                safeName = player.Name.Replace("^", "").Replace("<", "").Replace(">", "").Replace("~", "");
                //safeName = Regex.Replace(safeName, @"[^\u0000-\u007F]+", string.Empty); // Can't use regex on the client side: https://forum.cfx.re/t/c-regex-doesnt-work-on-client-side/911766
                safeName = safeName.Trim(new char[] { '.', ',', ' ', '!', '?' });
                if (string.IsNullOrEmpty(safeName))
                {
                    safeName = "InvalidPlayerName";
                }

                return safeName;
            }

            return "InvalidPlayerName";
        }

        /// <summary>
        /// Sets the player's pedestrian as a prop
        /// </summary>
        /// <param name="entityHandle"></param>
        public static void SetPlayerProp(int entityHandle)
        {
            int pedHandle = default;
            int propHandle = default;
            Vector3 entityDimensionsMinimum = default;
            Vector3 entityDimensionsMaximum = default;
            float entitySize = 0f;
            Vector3 pedCoords = default;
            Vector3 propRotation = default;
            float propZ = 0f;
            float boneZ = 0f;
            float groundZ = 0f;
            float finalPropZ = 0f;
            int playerCalculatedHealth = 0;

            //
            // Check target entity size to validate
            GetModelDimensions((uint)GetEntityModel(entityHandle), ref entityDimensionsMinimum, ref entityDimensionsMaximum);
            entitySize = Math.Abs(entityDimensionsMaximum.X - entityDimensionsMinimum.X) * Math.Abs(entityDimensionsMaximum.Y - entityDimensionsMinimum.Y) * Math.Abs(entityDimensionsMaximum.Z - entityDimensionsMinimum.Z);
            if (entitySize <= 0.005f)
            {
                TextUtil.SendChatMessage("This prop is too small.", "danger");
                return;
            }

            if (entitySize >= 50.0000f)
            {
                TextUtil.SendChatMessage("This prop is too large.", "danger");
                return;
            }

            if (IsEntityAPed(entityHandle) || IsEntityAVehicle(entityHandle))
            {
                TextUtil.SendChatMessage("This is an invalid prop", "danger");
                return;
            }

            //
            // Handle if the player already has a prop handle
            if (Game.Player.State.Get(Constants.State.Player.PropHandle) != null)
                cl_Player.RemovePlayerProp();

            //
            // Process the change
            pedHandle = Game.Player.Character.Handle;
            pedCoords = GetEntityCoords(pedHandle, true);
            GetGroundZFor_3dCoord(pedCoords.X, pedCoords.Y, pedCoords.Z, ref groundZ, true);
            propHandle = CreateObject(GetEntityModel(entityHandle), 0f, 0f, 0f, true, true, true);
            SetEntityAsMissionEntity(propHandle, true, true);
            propRotation = GetEntityRotation(propHandle, 2);
            propZ = GetEntityCoords(propHandle, true).Z;
            boneZ = GetPedBoneCoords(pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, 0f).Z;
            finalPropZ = propZ - (boneZ - groundZ);
            AttachEntityToEntity(propHandle, pedHandle, GetEntityBoneIndexByName(pedHandle, "IX_ROOT"), 0f, 0f, finalPropZ, propRotation.X, propRotation.Y, propRotation.Z, true, false, true, true, 1, true);

            //
            // Change player state
            SetEntityVisible(pedHandle, false, false);
            SetEntityVisible(propHandle, true, false);
            SetEntityCoords(pedHandle, pedCoords.X, pedCoords.Y, pedCoords.Z + 0.25f, true, true, true, false);

            // TODO: This is exploitable - need to consider player's curent hp
            playerCalculatedHealth = (int)Math.Ceiling(entitySize * 10f);
            if (playerCalculatedHealth < HEALTH_MIN)
                playerCalculatedHealth = HEALTH_MIN;
            if (playerCalculatedHealth > HEALTH_MAX)
                playerCalculatedHealth = HEALTH_MAX;
            if (playerCalculatedHealth > GetEntityHealth(Game.Player.Character.Handle))
            {
                cl_Player.SetHealth(GetEntityHealth(Game.Player.Character.Handle));
            }
            else if (playerCalculatedHealth <= GetEntityHealth(Game.Player.Character.Handle))
            {
                cl_Player.SetHealth(playerCalculatedHealth);
            }

            //
            // Store new prop handle
            Game.Player.State.Set(Constants.State.Player.PropHandle, propHandle, true);
        }

        /// <summary>
        /// Removes the prop from the player's pedestrian and resets their stats
        /// </summary>
        public static void RemovePlayerProp()
        {
            int propHandle = default;

            //
            // Remove the entity handle if it is set
            if (Game.Player.State.Get(Constants.State.Player.PropHandle) != null)
            {
                propHandle = Game.Player.State.Get(Constants.State.Player.PropHandle);
                DetachEntity(propHandle, true, false);
                DeleteObject(ref propHandle);
                Game.Player.State.Set(Constants.State.Player.PropHandle, null, true);
            }

            //
            // Undo entity changes
            cl_Player.SetVisible(true);
            cl_Player.SetHealth(HEALTH_MAX);
        }

        /// <summary>
        /// Resets the player fully
        /// </summary>
        public static void Reset()
        {
            cl_Player.RemovePlayerProp();
            cl_Player.SetVisible(true);
            cl_Player.SetInvincible(true);
            cl_Player.SetHealth(HEALTH_MAX);
            cl_Player.SetArmor(0);
            RemoveAllPedWeapons(Game.Player.Character.Handle, true);
            Game.Player.State.Set<PlayerTeams>(Constants.State.Player.Team, PlayerTeams.Unassigned, true);
        }

        /// <summary>
        /// Sets the player's visibility
        /// </summary>
        /// <param name="state"></param>
        public static void SetVisible(bool state)
            => SetEntityVisible(Game.Player.Character.Handle, state, false);

        /// <summary>
        /// Sets the player's invincibility
        /// </summary>
        /// <param name="state"></param>
        public static void SetInvincible(bool state)
            => SetEntityInvincible(Game.Player.Character.Handle, state);

        /// <summary>
        /// Sets the player's health
        /// Only use values between 0 and 100.
        /// Note: Takes into consideration that health in GTA V is 200, with min being 100
        /// </summary>
        /// <param name="health"></param>
        public static void SetHealth(int health)
            => SetEntityHealth(Game.Player.Character.Handle, health + 100);

        /// <summary>
        /// Sets the player's armor
        /// </summary>
        /// <param name="armor"></param>
        public static void SetArmor(int armor)
            => SetPedArmour(Game.Player.Character.Handle, armor);

        /// <summary>
        /// Freezes and unfreezes the player
        /// </summary>
        /// <param name="state"></param>
        public static void Freeze(bool state)
            => FreezeEntityPosition(Game.Player.Character.Handle, state);

        /// <summary>
        /// Gives the player the specified weapon with the specified ammo count
        /// </summary>
        /// <param name="weaponName"></param>
        /// <param name="ammoCount"></param>
        public static void GiveWeapon(string weaponName, int ammoCount)
            => GiveWeaponToPed(Game.Player.Character.Handle, (uint)GetHashKey(weaponName), ammoCount, false, true);

        /// <summary>
        /// Gives the player the weapon/ammo items defined in the dictionary
        /// </summary>
        /// <param name="weaponAndAmmoCounts"></param>
        public static void GiveWeapons(Dictionary<string, int> weaponAndAmmoCounts)
        {
            foreach (KeyValuePair<string, int> pair in weaponAndAmmoCounts)
            {
                cl_Player.GiveWeapon(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Toggles the blinding effect for the user
        /// </summary>
        /// <param name="state"></param>
        public static void Blind(bool state)
        {
            if (state == true)
            {
                SetTimecycleModifier("Glasses_BlackOut");
                SetExtraTimecycleModifier("prologue_ending_fog");
            }
            else
            {
                ClearTimecycleModifier();
                ClearExtraTimecycleModifier();
            }
        }
        #endregion
    }
}
