using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared
{
    public static class Constants
    {
        public static class Events
        {
            public static class Client
            {
                public static readonly string ClientAction = Guid.NewGuid().ToString();
                public static readonly string OnRoundStateChanged = Guid.NewGuid().ToString();
                public static readonly string OnRoundSync = Guid.NewGuid().ToString();
                public static readonly string SyncTimeAndWeather = Guid.NewGuid().ToString();

                public static class Actions
                {
                    public static readonly string Kill = Guid.NewGuid().ToString();
                    public static readonly string Spawn = Guid.NewGuid().ToString();
                }
            }

            public static class Server
            {
                public static readonly string ServerAction = Guid.NewGuid().ToString();
                public static readonly string OnPlayerInitialSpawn = Guid.NewGuid().ToString();
                public static readonly string OnPlayerSpawn = Guid.NewGuid().ToString();

                public static class Actions
                {
                    
                }
            }

        }

        public static class StateBagKeys
        {
            public static readonly string PlayerTeam = Guid.NewGuid().ToString();
            public static readonly string PlayerPropHandle = Guid.NewGuid().ToString();
            public static readonly string PlayerInitialSpawn = Guid.NewGuid().ToString();
        }
    }
}
