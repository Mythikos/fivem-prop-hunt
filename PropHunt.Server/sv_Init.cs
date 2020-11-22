using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Server.Extensions;
using PropHunt.Shared.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using PropHunt.Shared;
using System.ComponentModel;

namespace PropHunt.Server
{
    public class sv_Init : BaseScript
    {
        internal static readonly bool DebugMode = true;

        public sv_Init()
        {
            try
            {
                //
                // Get them static bois woke
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_Player).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_Logging).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_Game).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_Environment).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_Audio).TypeHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(sv_World).TypeHandle);

                //
                // Assign instance stuff
                PlayerList.SetInstance(this);

                //
                // Subscribe to events
                this.EventHandlers.Add(Constants.Events.Player.OnInitialSpawn, new Action<int>(sv_Player.OnPlayerInitialSpawn));
                this.EventHandlers.Add(Constants.Events.Player.OnSpawn, new Action<int>(sv_Player.OnPlayerSpawn));

                 Debug.WriteLine("PropHunt.Server was loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PropHunt.Server failed to load: {ex.Message}");
            }
        }

        internal static class PlayerList 
        {
            private static sv_Init _parentInstance;
            public static void SetInstance(sv_Init parentInstance)
                => _parentInstance = parentInstance;

            public static List<Player> GetAllPlayers()
                => _parentInstance?.Players?.ToList() ?? new List<Player>();

            public static List<Player> GetAllActivePlayers()
                => _parentInstance?.Players?.Where(x => x?.Character != null).ToList() ?? new List<Player>();

            public static Player GetPlayer(int serverId)
                => _parentInstance?.Players?[serverId];

            public static Player GetPlayer(string name)
                => _parentInstance?.Players?.FirstOrDefault(x => x.Name.Equals(name));
        }
    }
}
