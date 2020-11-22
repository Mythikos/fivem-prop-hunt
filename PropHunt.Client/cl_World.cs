using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    internal static class cl_World
    {
        public static Zone CurrentZone { get; private set; }

        public static async Task Setup(Zone zone)
        {
            cl_World.CurrentZone = zone;
            await cl_World.Cleanup(zone);
            await cl_World.GenerateBoundary(zone);
        }

        public static async Task Cleanup(Zone zone)
        {
            // Delete all props that are within the distance of our zone
            var center = zone.GetCenter();
            foreach (Prop prop in World.GetAllProps().Where(prop => GetDistanceBetweenCoords(prop.Position.X, prop.Position.Y, prop.Position.Z, center.X, center.Y, center.Z, true) <= zone.GetRadius()))
                prop.Delete();
        }

        public static async Task GenerateBoundary(Zone zone)
        {
            int point1Index, point2Index;
            Vector3 point1, point2;
            Vector3 propPosition;
            Prop prop;
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
                    prop = await World.CreateProp(new Model("prop_mp_cone_04"), propPosition, false, true);
                    prop.IsPositionFrozen = true;
                    prop.IsCollisionEnabled = false;
                }
            }
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
