using CitizenFX.Core;
using PropHunt.Client.Library;
using PropHunt.Client.Library.Extensions;
using PropHunt.Client.Library.Utils;
using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Library.Managers
{
    internal static class CommandManager
    {
        private static PropHunt _pluginInstance;

        public static void Initialize(PropHunt pluginInstance)
        {
            _pluginInstance = pluginInstance;

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

            RegisterCommand("ts", new Action<int, List<object>, string>((source, args, raw) =>
            {
                ClearTimecycleModifier();

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

            RegisterCommand("spawn", new Action<int, List<object>, string>((source, args, raw) =>
            {
                _pluginInstance.SpawnPlayer(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
            }), false);

            //RegisterCommand("spawnidiot", new Action<int, List<object>, string>((source, args, raw) =>
            //{
            //    Vector3 entityCoords = default;
            //    int newPedHandle = default;
            //    int propHandle = default;
            //    Vector3 pedCoords = default;

            //    entityCoords = GetEntityCoords(Game.PlayerPed.Handle, true);
            //    newPedHandle = CreatePed(0, (uint)GetHashKey("a_m_y_hipster_01"), entityCoords.X, entityCoords.Y, entityCoords.Z, Game.PlayerPed.Heading, true, false);
            //    pedCoords = GetEntityCoords(newPedHandle, true);
            //    propHandle = CreateObject(GetHashKey("prop_amb_handbag_01"), 0f, 0f, 0f, true, true, true);
            //    AttachEntityToEntity(propHandle, newPedHandle, GetEntityBoneIndexByName(newPedHandle, "IX_ROOT"), 3f, 0f, 0f, 0f, 0f, 0f, false, false, true, true, 1, true);// true, false, true, true, 1, true);
            //    FreezeEntityPosition(newPedHandle, true);
            //}), false);
        }
    }
}
