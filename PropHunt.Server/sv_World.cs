using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using PropHunt.Shared;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Server
{
    /// <summary>
    /// What do we want it to do?
    ///     Load zones (defined by coordinates (min,max) and generate a wall at that boundary. 
    ///     Remove zone walls
    ///     Load props for specific zones
    ///     Cleanup all props within a zone
    /// </summary>
    internal static class sv_World
    {
        public static List<Zone> Zones { get; set; }

        static sv_World()
        {
            sv_World.Zones = new List<Zone>();
        }

        /// <summary>
        /// Creates the zone which includes its boundary walls and props
        /// </summary>
        /// <returns></returns>
        public static async void Setup(Zone zone)
        {
            sv_World.Cleanup(zone);
            sv_World.GenerateWalls(zone);
            sv_World.GenerateProps();
        }

        /// <summary>
        /// Cleans the zone of all props excluding boundaries.
        /// </summary>
        /// <returns></returns>
        public static async void Cleanup(Zone zone)
        {
            Vector3 center = zone.GetCenter();
            float radius = zone.GetRadius();
            sv_Init.TriggerClientEvent(Constants.Actions.World.Cleanup, center.X, center.Y, center.Z, radius);
        }

        /// <summary>
        /// Generates the boundary walls for any given area
        /// </summary>
        public static async void GenerateWalls(Zone zone)
        {
            int handle = 0;
            float lerpValue = 0f;
            float distance = 0f;
            
            int cellSegments = 0;
            float cellWidth = 1.1f;
            float cellHeight = 0.8f;

            // Iterate over each point
            for (int p1Index = 0; p1Index < zone.Points.Count; p1Index++)
            {
                // Wrap to beginning of collection
                int p2Index = p1Index + 1;
                if (p2Index >= zone.Points.Count)
                    p2Index = 0;

                // Get points
                var p1 = zone.Points[p1Index];
                var p2 = zone.Points[p2Index];
                sv_Logging.Log($"p1: {p1}");
                sv_Logging.Log($"p2: {p2}");

                // Calculate
                cellSegments = (int)Math.Ceiling(Vector3.Distance(p1, p2) / cellWidth);
                sv_Logging.Log($"segments: {cellSegments}");

                lerpValue = 0f;
                distance = 1f / cellSegments;
                sv_Logging.Log($"distance: {distance}");
                for (int i = 0; i < cellSegments; i++)
                {
                    lerpValue += distance;
                    var pos = Vector3.Lerp(p1, p2, lerpValue);
                    handle = CreateObjectNoOffset((uint)GetHashKey("prop_box_ammo03a_set2"/*"prop_ld_fragwall_01a"*/), pos.X, pos.Y, 60, true, true, true);
                    FreezeEntityPosition(handle, true);
                }
                sv_Logging.Log($"--------------------------");
            }
        }

        public static void GenerateProps()
        {

        }

        #region Sub Classes
        public class Zone
        {
            public List<Vector3> Points { get; set; }
            public List<Prop> Props { get; set; }

            public Zone()
            {
                this.Points = new List<Vector3>();
                this.Props = new List<Prop>();
            }

            public float GetRadius()
                => (float)Math.Round(Math.Sqrt(Math.Pow(this.Points.Sum(point => Math.Abs(point.X)), 2f) + Math.Pow(this.Points.Sum(point => Math.Abs(point.Y)), 2f)), 3);

            public Vector3 GetCenter()
                => new Vector3(this.Points.Average(point => point.X), this.Points.Average(point => point.Y), this.Points.Average(point => point.Z));

            public float LowestZ()
                => this.Points.Min(point => point.Z);
        }
        #endregion
    }
}
