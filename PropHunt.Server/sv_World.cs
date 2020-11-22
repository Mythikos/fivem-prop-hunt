using CitizenFX.Core;
using Newtonsoft.Json;
using PropHunt.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Server
{
    internal static class sv_World
    {
        public static List<Zone> Zones { get; set; }
        public static Zone CurrentZone { get; private set; }

        static sv_World()
        {
            sv_World.Zones = new List<Zone>();
            
            sv_World.Zones.Add(new sv_World.Zone()
            {
                Points = new List<Vector3>()
                {
                    new Vector3(-1420, 207, 58),
                    new Vector3(-1450, 243, 60),
                    new Vector3(-1468, 234, 59),
                    new Vector3(-1496, 199, 57),
                    new Vector3(-1499, 169, 54),
                    new Vector3(-1457, 135, 52),
                    new Vector3(-1455, 151, 54),
                    new Vector3(-1439, 180, 56),
                }
            });
        }

        public static void Setup(Zone zone)
        {
            sv_World.CurrentZone = zone;
            sv_Init.TriggerClientEvent(Constants.Events.World.Setup, JsonConvert.SerializeObject(sv_World.CurrentZone));
        }

        public static void Cleanup(Zone zone)
        {
            sv_Init.TriggerClientEvent(Constants.Events.World.Cleanup, JsonConvert.SerializeObject(sv_World.CurrentZone));
        }

        #region Sub Classes
        public class Zone
        {
            public List<Vector3> Points { get; set; }

            public Zone()
                => this.Points = new List<Vector3>();

            public float GetRadius()
                => (float)Math.Round(Math.Sqrt(Math.Pow(this.Points.Sum(point => Math.Abs(point.X)), 2f) + Math.Pow(this.Points.Sum(point => Math.Abs(point.Y)), 2f)), 3);

            public Vector3 GetCenter()
                => new Vector3(this.Points.Average(point => point.X), this.Points.Average(point => point.Y), this.Points.Average(point => point.Z));
        }
        #endregion
    }
}
