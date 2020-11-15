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
                public static readonly string ClientAction = "42b19234c24946b99784d72fc8daac31";
                public static readonly string OnRoundStateChanged = "5abd03b79b3442f594ad96d83e8641fd";
                public static readonly string OnRoundSync = "0ffeec9c62804ff6b146dc7289fc044a";
                public static readonly string SyncTimeAndWeather = "3bc0ca4f9f7148b4a2411c1c26e3a3c6";

                public static class Actions
                {
                    public static readonly string Kill = "a493fb7f772b46a9b21c2d22417c64bf";
                    public static readonly string Spawn = "70d23f12e7634b84983f5295270c88ac";
                }
            }

            public static class Server
            {
                public static readonly string ServerAction = "4d8cc9cb4e1847e7992d62c129dafea7";
                public static readonly string OnPlayerInitialSpawn = "fcca7d65805f455b8d2659f45bcb1131";
                public static readonly string OnPlayerSpawn = "6d14dab90b22468991b100855871e221";

                public static class Actions
                {

                }
            }

        }

        public static class StateBagKeys
        {
            public static readonly string PlayerTeam = "2bf609234e7f465eb686a7958f97898c";
            public static readonly string PlayerPropHandle = "6356fdac4d5448e592fa07c0542718e4";
            public static readonly string PlayerInitialSpawn = "9a57e095a6204e11b9fdaaf2aee8f6c7";
        }
    }
}
