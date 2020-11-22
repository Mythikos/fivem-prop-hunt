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
            public static class GameManager
            {
                public const string OnSyncGameState = "0ffeec9c62804ff6b146dc7289fc044a";
                public const string OnGameStateChanged = "5abd03b79b3442f594ad96d83e8641fd";
            }

            public static class Player
            {
                public const string OnInitialSpawn = "fcca7d65805f455b8d2659f45bcb1131";
                public const string OnSpawn = "6d14dab90b22468991b100855871e221";
            }

        }
        public static class Actions
        {
            public static class Player
            {
                public const string Spawn = "70d23f12e7634b84983f5295270c88ac";
                public const string Kill = "a493fb7f772b46a9b21c2d22417c64bf";
                public const string GetCoords = "82c78ce2337448cdb026597b7fab8120";
            }

            public static class Environment
            {
                public const string SetTime = "a5b27550766741c892fbf3c769e1353f";
                public const string SetWeather = "31b74599a1024e56a1879e16b0f83bf4";
                public const string SetWeatherAndTime = "864e944976ac45f9b81124d7a4167671";
            }

            public static class Audio
            {
                public const string PlayFromPlayer = "0ba8e3580d3941859fac3cbcd775baa3";
                public const string PlayFromPosition = "3ec525f5811549248440c36d22712384";
                public const string Play = "542f69c47c53427dbf9f227784b4aa5f";
            }

            public static class World
            {
                public const string Setup = "c654cfc4504c48069516a05765ce409b";
                public const string Cleanup = "28940621d06a4ca38b023bbd40bd96a3";
            }
        }

        public static class State
        {
            public static class Player
            {
                public const string Team = "2bf609234e7f465eb686a7958f97898c";
                public const string PropHandle = "6356fdac4d5448e592fa07c0542718e4";
                public const string InitialSpawn = "9a57e095a6204e11b9fdaaf2aee8f6c7";
                public const string TauntLastPositionX = "293cbf21a1ca47d990ace47a75280d21";
                public const string TauntLastPositionY = "3db612de10014e79b46f7594ae6c01ec";
                public const string TauntLastPositionZ = "3b94de9a648f454aa198352ed9d6491d";
                public const string TauntLastTime = "d722c4a5eae24ed89086995189069f8f";
            }
        }
    }
}
