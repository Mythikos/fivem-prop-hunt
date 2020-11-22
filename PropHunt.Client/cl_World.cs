using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    internal static class cl_World
    {
        public static void Setup()
        {

        }

        public static void Cleanup(float x, float y, float z, float radius)
        {
            // Delete all props that are within the distance of our zone
            foreach (Prop prop in World.GetAllProps().Where(prop => GetDistanceBetweenCoords(prop.Position.X, prop.Position.Y, prop.Position.Z, x, y, z, true) <= radius))
                prop.Delete();
        }
    }
}
