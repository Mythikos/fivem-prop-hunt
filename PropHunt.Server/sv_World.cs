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
            int point1Index, point2Index;
            Vector3 point1, point2;
            Vector3 propPosition;
            int propHandle;
            float cellWidth = 4;
            int cellSegmentsWidth = 0;
            float lerpValue = 0f;

            // Iterate over each point
            for (point1Index = 0; point1Index < zone.Points.Count; point1Index++)
            {
                // Wrap to beginning of collection
                point2Index = point1Index + 1;
                if (point2Index >= zone.Points.Count)
                    point2Index = 0;

                // Get points
                point1 = zone.Points[point1Index];
                point2 = zone.Points[point2Index];

                // Calculate segments cell
                cellSegmentsWidth = (int)Math.Ceiling(Vector3.Distance(point1, point2) / cellWidth);

                // Reset lerp value
                lerpValue = 0f;

                // Iterate over n number of segments and do the thing
                for (int i = 0; i < cellSegmentsWidth; i++)
                {
                    lerpValue += 1f / cellSegmentsWidth;
                    propPosition = Vector3.Lerp(point1, point2, lerpValue);
                    propHandle = CreateObjectNoOffset((uint)GetHashKey("prop_mp_cone_04"), propPosition.X, propPosition.Y, propPosition.Z, true, true, true);
                    FreezeEntityPosition(propHandle, true);
                }
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
        }
        #endregion
    }
}
