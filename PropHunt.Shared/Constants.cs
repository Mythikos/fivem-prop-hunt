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
                public const string OnRoundStateChanged = "5abd03b79b3442f594ad96d83e8641fd";
                public const string OnRoundSync = "0ffeec9c62804ff6b146dc7289fc044a";

                public const string OnEnvironmentTimeChanged = "a5b27550766741c892fbf3c769e1353f";
                public const string OnEnvironmentWeatherChanged = "31b74599a1024e56a1879e16b0f83bf4";
                public const string OnEnvironmentWeatherAndTimeChanged = "864e944976ac45f9b81124d7a4167671";

                public const string OnAudioPlayFromPlayer = "0ba8e3580d3941859fac3cbcd775baa3";
                public const string OnAudioPlayFromPosition = "3ec525f5811549248440c36d22712384";
                public const string OnAudioPlay = "542f69c47c53427dbf9f227784b4aa5f";

                public const string Spawn = "70d23f12e7634b84983f5295270c88ac";
                public const string Kill = "a493fb7f772b46a9b21c2d22417c64bf";
            }

            public static class Server
            {
                public const string OnPlayerInitialSpawn = "fcca7d65805f455b8d2659f45bcb1131";
                public const string OnPlayerSpawn = "6d14dab90b22468991b100855871e221";

                public const string GetPlayerCoords = "82c78ce2337448cdb026597b7fab8120";
            }

        }

        public static class StateBagKeys
        {
            public const string PlayerTeam = "2bf609234e7f465eb686a7958f97898c";
            public const string PlayerPropHandle = "6356fdac4d5448e592fa07c0542718e4";
            public const string PlayerInitialSpawn = "9a57e095a6204e11b9fdaaf2aee8f6c7";
            public const string TauntLastPositionX = "293cbf21a1ca47d990ace47a75280d21";
            public const string TauntLastPositionY = "3db612de10014e79b46f7594ae6c01ec";
            public const string TauntLastPositionZ = "3b94de9a648f454aa198352ed9d6491d";
            public const string TauntLastTime = "d722c4a5eae24ed89086995189069f8f";

        }
    }
}
