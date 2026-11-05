using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public static class EdgeSideProfile
    {
        public const int Rows = 8;

        private const float MinVerticalScale = 0.05f;

        public static void Append(
            SurfacePath path,
            EdgeTileSet tileSet,
            in EdgeTile tile,
            bool startSide,
            float thickness,
            float offset,
            List<float> scratch,
            List<Vector2> points,
            out float topDistance,
            out float bottomDistance,
            in EndSlice slice = default)
        {
            SpriteShape shape = tileSet.Shape(tile.Role, tile.Variant);
            float surfaceY = tileSet.SurfaceY(tile.Role, tile.Variant);
            float vertical = Mathf.Max(MinVerticalScale, tileSet.BelowSurfaceScale(tile.Role, tile.Variant));
            float horizontal = tileSet.Scale(tile.Role).x * tile.FitScale;
            float centerX = shape.Bounds.center.x;
            float firstValid = float.NaN;

            scratch.Clear();

            for (int row = 0; row <= Rows; row++)
            {
                float x = shape.EdgeX(surfaceY - slice.ArtDepth(thickness * row / Rows - offset) / vertical, startSide);
                scratch.Add(x);

                if (float.IsNaN(firstValid) && !float.IsNaN(x))
                    firstValid = x;
            }

            float previous = float.IsNaN(firstValid) ? (startSide ? shape.Bounds.xMin : shape.Bounds.xMax) : firstValid;
            topDistance = 0f;
            bottomDistance = 0f;

            for (int row = 0; row <= Rows; row++)
            {
                float x = float.IsNaN(scratch[row]) ? previous : scratch[row];
                float depth = thickness * row / Rows;
                float distance = tile.Center + (x - centerX) * horizontal;
                previous = x;

                path.Evaluate(distance, out Vector2 point, out Vector2 normal);
                points.Add(point + normal * (offset - depth));

                if (row == 0)
                    topDistance = distance;

                if (row == Rows)
                    bottomDistance = distance;
            }
        }
    }
}
