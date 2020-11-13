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
                public const string ClientAction = "PropHunt:Events:Client:ClientAction";
                public const string GameSync = "PropHunt:Events:Client:GameSync";
                public const string GameStateUpdate = "PropHunt:Events:Client:GameStateUpdate";

                public static class Actions
                {
                    public const string Kill = "PropHunt:Events:Client:Action:Kill";
                    public const string Spawn = "PropHunt:Events:Client:Action:Spawn";
                }
            }

            public static class Server
            {
                public const string ServerAction = "PropHunt:Events:Server:ServerAction";
                public const string PlayerInitialSpawn = "PropHunt:Events:Server:PlayerInitialSpawn";
                public const string PlayerSpawn = "PropHunt:Events:Server:PlayerSpawn";

                public static class Actions
                {
                }
            }

        }

        public static class StateBagKeys
        {
            public const string PlayerState = "PropHunt:StateBagKeys:PlayerState";
            public const string PlayerPropHandle = "PropHunt:StateBagKeys:PlayerPropHandle";
            public const string PlayerInitialSpawn = "PropHunt:StateBagKeys:PlayerInitialSpawn";
        }
    }
}
