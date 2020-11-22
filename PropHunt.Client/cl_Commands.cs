using CitizenFX.Core;
using PropHunt.Client.Utils;
using PropHunt.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    internal static class cl_Commands
    {
        static cl_Commands()
        {
            RegisterCommand("spawncar", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // account for the argument not being passed
                var model = "adder";
                if (args.Count > 0)
                {
                    model = args[0].ToString();
                }

                // check if the model actually exists
                // assumes the directive 'using static CitizenFX.Core.Native.API;'
                var hash = (uint)GetHashKey(model);
                if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                {
                    TextUtil.SendChatMessage($"{model} does not exist.");
                    return;
                }

                // create the vehicle
                var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                SetRadioToStationName("OFF");

                // set the player ped into the vehicle and driver seat
                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            }), false);

            RegisterCommand("spawnprop", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // account for the argument not being passed
                var model = string.Empty;
                if (args.Count != 1)
                {
                    TextUtil.SendChatMessage($"Must specify a model to spawn.");
                    return;
                }
                model = args[0].ToString();

                var hash = (uint)GetHashKey(model);
                if (!IsModelInCdimage(hash))
                    return;

                // create the prop
                var prop = await World.CreateProp(model, GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0, 0, 0), true, true);
            }), false);

            RegisterCommand("wrist", new Action<int, List<object>, string>((source, args, raw) =>
            {
                ApplyDamageToPed(PlayerPedId(), 99999, false);
            }), false);

            RegisterCommand("setpos", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (args.Count != 3)
                {
                    TextUtil.SendChatMessage($"Must specify an x, y, and z coordinate to teleport to");
                    return;
                }

                SetEntityCoords(PlayerPedId(), int.Parse(args[0]?.ToString()), int.Parse(args[1]?.ToString()), int.Parse(args[2]?.ToString()), true, true, true, false);
                TextUtil.SendChatMessage($"Current Position: {args[0]}, {args[1]}, {args[2]}");
            }), false);

            RegisterCommand("getpos", new Action<int, List<object>, string>((source, args, raw) =>
            {
                var coords = GetEntityCoords(PlayerPedId(), true);
                TextUtil.SendChatMessage($"Current Position: {coords.X}, {coords.Y}, {coords.Z}");
            }), false);

            RegisterCommand("giveweapon", new Action<int, List<object>, string>((source, args, raw) =>
            {
                GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey(args[0].ToString()), 999, false, true);
            }), false);

            RegisterCommand("spawn", new Action<int, List<object>, string>((source, args, raw) =>
            {
                cl_Init.TriggerEvent(Constants.Events.Player.Spawn, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
            }), false);

            //
            // Test commands
            RegisterCommand("test.timecycle", new Action<int, List<object>, string>((source, args, raw) =>
            {
                ClearTimecycleModifier();
                ClearExtraTimecycleModifier();

                if (args.Count == 1)
                {
                    SetTimecycleModifier(args[0].ToString());
                }
                else if (args.Count == 2)
                {
                    SetTimecycleModifier(args[0].ToString());
                    SetTimecycleModifierStrength((float)args[1]);
                }
            }), false);

            RegisterCommand("test.audio", new Action<int, List<object>, string>((source, args, raw) =>
            {
                cl_Audio.PlayFromPlayer(args[0].ToString(), args[1].ToString());
            }), false);

            RegisterCommand("test.notifications", new Action<int, List<object>, string>((source, args, raw) =>
            {
                NotificationsUtil.Custom(args[0].ToString(), true, true);
                NotificationsUtil.Alert(args[0].ToString(), true, true);
                NotificationsUtil.Error(args[0].ToString(), true, true);
                NotificationsUtil.Info(args[0].ToString(), true, true);
                NotificationsUtil.Success(args[0].ToString(), true, true);
            }), false);

            RegisterCommand("test.spectate.start", new Action<int, List<object>, string>((source, args, raw) =>
            {
                cl_Player.Spectate.Start();
            }), false);

            RegisterCommand("test.spectate.changetarget", new Action<int, List<object>, string>((source, args, raw) =>
            {
                cl_Player.Spectate.ChangeTarget(int.Parse(args[0]?.ToString() ?? "0"));
            }), false);

            RegisterCommand("test.spectate.end", new Action<int, List<object>, string>((source, args, raw) =>
            {
                cl_Player.Spectate.End();
            }), false);


            RegisterCommand("test.propdims", new Action<int, List<object>, string>((source, args, raw) =>
            {
                Model model = new Model(args[0].ToString());
                var dims = model.GetDimensions();

                cl_Logging.Log($"Dims: {dims.ToString()}");

            }), false);
        }
    }
}
//prop_box_tea01a