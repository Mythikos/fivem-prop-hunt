using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client.Library.Utils
{
    internal static class EntityUtil
    {
        #region Raycast Helpers
        public enum IntersectOptions
        {
            IntersectEverything = -1,
            IntersectMap = 1,
            IntersectMissionEntityAndTrain = 2,
            IntersectPeds1 = 4,
            IntersectPeds2 = 8,
            IntersectVehicles = 10,
            IntersectObjects = 16,
            IntersectVegetation = 256
        };

        public static List<Entity> GetSurroundingEntities(Entity fromEntity, int radius, IntersectOptions flags)
        {
            const int DEGREES = 1;

            List<Entity> entities = new List<Entity>();
            double angle = default;
            float startX = default;
            float startY = default;
            float endX = default;
            float endY = default;
            int raycastHandle = default;
            bool raycastHit = false;
            Vector3 raycastEndCoords = default;
            Vector3 raycastSurfaceNormal = default;
            int raycastEntityHitHandle = default;
            Entity raycastFoundEntity = default;

            // Wait so it isn't firing off too quickly
            Wait(10);

            // Start lookin
            for (int i = 0; i < (360 / DEGREES); i++)
            {
                angle = (Math.PI / 180) * (i * DEGREES);
                startX = fromEntity.Position.X;
                startY = fromEntity.Position.Y;
                endX = fromEntity.Position.X + (radius * (float)Math.Cos(angle));
                endY = fromEntity.Position.Y + (radius * (float)Math.Sin(angle));

                raycastHandle = StartShapeTestCapsule(startX, startY, fromEntity.Position.Z, endX, endY, fromEntity.Position.Z, 20, (int)flags, fromEntity.Handle, 7);
                GetShapeTestResult(raycastHandle, ref raycastHit, ref raycastEndCoords, ref raycastSurfaceNormal, ref raycastEntityHitHandle);
                if (raycastHit)
                {
                    raycastFoundEntity = Entity.FromHandle(raycastEntityHitHandle);
                    if (raycastFoundEntity != null && entities.Contains(raycastFoundEntity) == false)
                        entities.Add(raycastFoundEntity);
                }
            }

            return entities;
        }
        #endregion

        #region Drawables
        public static void HighlightEntity(Entity entity, float offsetX = 0.01f, float offsetY = 0.01f, float width = 0.006f, float height = 0.006f, int[] color = null)
        {
            if (color == null)
                color = new[] { 255, 255, 255, 255 };

            SetDrawOrigin(entity.Position.X, entity.Position.Y, entity.Position.Z, 0);
            RequestStreamedTextureDict("helicopterhud", false);
            DrawSprite("helicopterhud", "hud_corner", -offsetX, -offsetY, width, height, 0.0f, color[0], color[1], color[2], color[3]);
            DrawSprite("helicopterhud", "hud_corner", offsetX, -offsetY, width, height, 90.0f, color[0], color[1], color[2], color[3]);
            DrawSprite("helicopterhud", "hud_corner", -offsetX, offsetY, width, height, 270.0f, color[0], color[1], color[2], color[3]);
            DrawSprite("helicopterhud", "hud_corner", offsetX, offsetY, width, height, 180.0f, color[0], color[1], color[2], color[3]);
            ClearDrawOrigin();
        }
        #endregion
    }
}
