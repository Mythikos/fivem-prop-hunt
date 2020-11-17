using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Utils
{
    public static class PlayerUtil
    {
        public static string GetSafeName(Player player)
        {
            string safeName = string.Empty;

            if (!string.IsNullOrEmpty(player.Name))
            {
                safeName = player.Name.Replace("^", "").Replace("<", "").Replace(">", "").Replace("~", "");
                safeName = Regex.Replace(safeName, @"[^\u0000-\u007F]+", string.Empty);
                safeName = safeName.Trim(new char[] { '.', ',', ' ', '!', '?' });
                if (string.IsNullOrEmpty(safeName))
                {
                    safeName = "InvalidPlayerName";
                }

                return safeName;
            }

            return "InvalidPlayerName";
        }

        public static async void SpectatePlayer(Player player, bool forceDisable = false)
        {
            if (forceDisable)
            {
                NetworkSetInSpectatorMode(false, 0); // disable spectating.
            }
            else
            {
                if (!NetworkIsPlayerActive(player.Handle))
                    await TeleportToPlayer(player);

                if (player.Handle == Game.Player.Handle)
                {
                    if (NetworkIsInSpectatorMode())
                    {
                        DoScreenFadeOut(500);
                        while (IsScreenFadingOut())
                            await Task.Delay(0);

                        NetworkSetInSpectatorMode(false, 0);
                        DoScreenFadeIn(500);
                        NotificationsUtil.Success("Stopped spectating.", false, true);
                        this._currentlySpectatingPlayer = -1;
                    }
                    else
                    {
                        NotificationsUtil.Error("You can't spectate yourself.", false, true);
                    }
                }
                else
                {
                    if (NetworkIsInSpectatorMode())
                    {
                        if (this._currentlySpectatingPlayer != player.Handle && player.Character != null)
                        {
                            DoScreenFadeOut(500);
                            while (IsScreenFadingOut())
                                await Task.Delay(0);

                            if (player.Character != null)
                            {
                                NetworkSetInSpectatorMode(false, 0);
                                NetworkSetInSpectatorMode(true, player.Character.Handle);
                            }

                            DoScreenFadeIn(500);
                            NotificationsUtil.Success($"You are now spectating ~g~<C>{player.GetSafeName()}</C>~s~.", false, true);
                            this._currentlySpectatingPlayer = player.Handle;
                        }
                        else
                        {
                            DoScreenFadeOut(500);
                            while (IsScreenFadingOut())
                                await Task.Delay(0);

                            NetworkSetInSpectatorMode(false, 0);
                            DoScreenFadeIn(500);
                            NotificationsUtil.Success("Stopped spectating.", false, true);
                            this._currentlySpectatingPlayer = -1;
                        }
                    }
                    else
                    {
                        if (player.Character != null)
                        {
                            DoScreenFadeOut(500);
                            while (IsScreenFadingOut())
                                await Task.Delay(0);

                            if (player.Character != null)
                            {
                                NetworkSetInSpectatorMode(false, 0);
                                NetworkSetInSpectatorMode(true, player.Character.Handle);
                            }

                            DoScreenFadeIn(500);
                            NotificationsUtil.Success($"You are now spectating ~g~<C>{player.GetSafeName()}</C>~s~.", false, true);
                            this._currentlySpectatingPlayer = player.Handle;
                        }
                    }
                }
            }
        }

        public static async Task TeleportToPlayer(this Player player, bool inVehicle = false)
        {
            // If the player exists.
            if (NetworkIsPlayerActive(player.Handle))
            {
                Vector3 playerPos;
                bool wasActive = true;

                if (NetworkIsPlayerActive(player.Handle))
                {
                    Ped playerPedObj = player.Character;
                    if (Game.PlayerPed == playerPedObj)
                    {
                        NotificationsUtil.Error("Sorry, you can ~r~~h~not~h~ ~s~teleport to yourself!");
                        return;
                    }

                    // Get the coords of the other player.
                    playerPos = GetEntityCoords(playerPedObj.Handle, true);
                }
                else
                {
                    playerPos = await MainMenu.RequestPlayerCoordinates(player.ServerId);
                    wasActive = false;
                }

                // Then await the proper loading/teleporting.
                await TeleportToCoords(playerPos);

                // Wait until the player has been created.
                while (player.Character == null)
                    await Task.Delay(0);

                var playerId = player.Handle;
                var playerPed = player.Character.Handle;

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Wait until the target player vehicle has loaded, if they weren't active beforehand.
                    if (!wasActive)
                    {
                        var startWait = GetGameTimer();

                        while (!IsPedInAnyVehicle(playerPed, false))
                        {
                            await Task.Delay(0);

                            if ((GetGameTimer() - startWait) > 1500)
                            {
                                break;
                            }
                        }
                    }

                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        Vehicle vehicle = GetVehicle(new Player(playerId), false);
                        if (vehicle != null)
                        {
                            int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle.Handle));

                            // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                            if (vehicle.Exists() && !vehicle.IsDead && IsAnyVehicleSeatEmpty(vehicle.Handle))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, (int)VehicleSeat.Any);
                                Notify.Success("Teleported into ~g~<C>" + GetPlayerName(playerId) + "</C>'s ~s~vehicle.");
                            }
                            // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                            else
                            {
                                // If there's only one seat on this vehicle, tell them that it's a one-seater.
                                if (totalVehicleSeats == 1)
                                {
                                    Notify.Error("This vehicle only has room for 1 player!");
                                }
                                // Otherwise, tell them there's not enough empty seats remaining.
                                else
                                {
                                    Notify.Error("Not enough empty vehicle seats remaining!");
                                }
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to ~y~<C>" + GetPlayerName(playerId) + "</C>~s~.");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error(CommonErrors.PlayerNotFound, placeholderValue: "So the teleport has been cancelled.");
            }
        }

        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }
    }
}
