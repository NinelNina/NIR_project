using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public static class EdgeMeshBuilder
    {
        public const int SegmentsPerTile = 12;

        public const int RowsPerColumn = 3;
        private const float RangeEpsilon = 0.001f;
        private const float CornerEpsilon = 0.01f;
        private const float ClipEpsilon = 0.000001f;
        private const float MaxMiterReach = 4f;

        private static readonly float[] ColumnX = new float[SegmentsPerTile + 1];
        private static readonly float[] ColumnDistance = new float[SegmentsPerTile + 1];
        private static readonly int[] ColumnCorner = new int[SegmentsPerTile + 1];
        private static readonly List<int> ClipCorners = new List<int>();

        public static void AppendTile(
            SurfacePath path,
            in EdgeTileArt art,
            in EdgeTile tile,
            float surfaceOffset,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles,
            float rangeStart = float.NegativeInfinity,
            float rangeEnd = float.PositiveInfinity,
            bool cropToRange = false)
        {
            Rect local = art.LocalRect;
            Rect uv = art.UvRect;
            float surfaceY = Mathf.Clamp(art.SurfaceY, local.yMin, local.yMax);
            float surfaceFraction = Mathf.InverseLerp(local.yMin, local.yMax, surfaceY);
            float horizontalScale = art.Scale.x * tile.FitScale;
            float bottomOffset = (local.yMin - surfaceY) * art.BelowSurfaceScale + surfaceOffset;
            float topOffset = (local.yMax - surfaceY) * art.Scale.y + surfaceOffset;
            int firstVertex = vertices.Count;
            var white = new Color32(255, 255, 255, 255);
            Color32 topColor = art.FadeTop ? new Color32(255, 255, 255, 0) : white;

            PlanColumns(path, local, tile, horizontalScale, cropToRange, rangeStart, rangeEnd);

            for (int column = 0; column <= SegmentsPerTile; column++)
            {
                float x = ColumnX[column];
                float distance = ColumnDistance[column];
                int corner = ColumnCorner[column];
                float along = Mathf.InverseLerp(local.xMin, local.xMax, x);
                Vector2 point;
                Vector2 normal;

                if (corner >= 0)
                {
                    point = path.Points[corner];
                    normal = path.MiterNormal(corner);
                }
                else
                {
                    path.Evaluate(KeepInsideRange(distance, rangeStart, rangeEnd), out point, out normal);
                }

                Vector2 surface = point + normal * surfaceOffset;
                Vector2 bottom = surface + normal * (bottomOffset - surfaceOffset);
                Vector2 top = surface + normal * (topOffset - surfaceOffset);

                if (corner < 0)
                {
                    bottom = ClipToCorners(path, surface, normal, surfaceOffset, bottomOffset - surfaceOffset, bottom);
                    top = ClipToCorners(path, surface, normal, surfaceOffset, topOffset - surfaceOffset, top);
                }

                vertices.Add(bottom);
                vertices.Add(surface);
                vertices.Add(top);
                uvs.Add(Uv(art, uv, along, 0f));
                uvs.Add(Uv(art, uv, along, surfaceFraction));
                uvs.Add(Uv(art, uv, along, 1f));
                colors.Add(white);
                colors.Add(white);
                colors.Add(topColor);
            }

            for (int column = 0; column < SegmentsPerTile; column++)
            {
                for (int row = 0; row < RowsPerColumn - 1; row++)
                {
                    int bottom = firstVertex + column * RowsPerColumn + row;
                    int top = bottom + 1;
                    int nextBottom = bottom + RowsPerColumn;
                    int nextTop = nextBottom + 1;
                    triangles.Add(bottom);
                    triangles.Add(top);
                    triangles.Add(nextTop);
                    triangles.Add(bottom);
                    triangles.Add(nextTop);
                    triangles.Add(nextBottom);
                }
            }
        }

        private static void PlanColumns(SurfacePath path, Rect local, in EdgeTile tile, float horizontalScale, bool cropToRange, float rangeStart, float rangeEnd)
        {
            for (int column = 0; column <= SegmentsPerTile; column++)
            {
                float x = Mathf.Lerp(local.xMin, local.xMax, column / (float)SegmentsPerTile);
                float distance = tile.Center + (x - local.center.x) * horizontalScale;

                if (cropToRange && horizontalScale > 0f)
                {
                    float clamped = Mathf.Clamp(distance, rangeStart, rangeEnd);
                    x += (clamped - distance) / horizontalScale;
                    distance = clamped;
                }

                ColumnX[column] = x;
                ColumnDistance[column] = distance;
                ColumnCorner[column] = -1;
            }

            ClipCorners.Clear();

            if (horizontalScale <= 0f || !path.IsValid)
                return;

            float spanStart = ColumnDistance[0];
            float spanEnd = ColumnDistance[SegmentsPerTile];
            float reach = local.height * Mathf.Max(1f, horizontalScale) * MaxMiterReach;

            for (int sample = 0; sample < path.SampleCount; sample++)
            {
                if (!path.IsSharpSample(sample))
                    continue;

                float distance = NearestCopy(path, path.Distances[sample], (spanStart + spanEnd) * 0.5f);

                if (distance < spanStart - reach || distance > spanEnd + reach)
                    continue;

                ClipCorners.Add(sample);

                if (distance <= spanStart + CornerEpsilon || distance >= spanEnd - CornerEpsilon || distance <= rangeStart + CornerEpsilon || distance >= rangeEnd - CornerEpsilon)
                    continue;

                SnapColumn(local, tile, horizontalScale, sample, distance);
            }
        }

        private static float NearestCopy(SurfacePath path, float distance, float target)
        {
            if (!path.IsClosed || path.Length <= 0f)
                return distance;

            return distance + Mathf.Round((target - distance) / path.Length) * path.Length;
        }

        private static void SnapColumn(Rect local, in EdgeTile tile, float horizontalScale, int sample, float distance)
        {
            int nearest = -1;
            float best = float.MaxValue;

            for (int column = 1; column < SegmentsPerTile; column++)
            {
                float gap = Mathf.Abs(ColumnDistance[column] - distance);

                if (gap < best && ColumnCorner[column] < 0)
                {
                    best = gap;
                    nearest = column;
                }
            }

            if (nearest < 0 || distance <= ColumnDistance[nearest - 1] || distance >= ColumnDistance[nearest + 1])
                return;

            ColumnCorner[nearest] = sample;
            ColumnDistance[nearest] = distance;
            ColumnX[nearest] = local.center.x + (distance - tile.Center) / horizontalScale;
        }

        private static Vector2 ClipToCorners(SurfacePath path, Vector2 surface, Vector2 normal, float surfaceOffset, float offset, Vector2 vertex)
        {
            if (Mathf.Abs(offset) < ClipEpsilon)
                return vertex;

            for (int i = 0; i < ClipCorners.Count; i++)
            {
                int corner = ClipCorners[i];
                Vector2 miter = path.MiterNormal(corner);
                Vector2 bisector = miter.normalized;
                Vector2 basePoint = path.Points[corner] + miter * surfaceOffset;
                float side = Cross(bisector, surface - basePoint);
                float tipSide = Cross(bisector, surface + normal * offset - basePoint);

                if (Mathf.Abs(side) < ClipEpsilon || side * tipSide > 0f)
                    continue;

                float turn = Cross(bisector, normal);

                if (Mathf.Abs(turn) < ClipEpsilon)
                    continue;

                float reach = -side / turn;
                Vector2 crossing = surface + normal * reach;

                if (reach * offset <= 0f || Vector2.Dot(crossing - basePoint, bisector) * offset < 0f)
                    continue;

                return basePoint + miter * offset;
            }

            return vertex;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static float KeepInsideRange(float distance, float rangeStart, float rangeEnd)
        {
            if (Mathf.Abs(distance - rangeStart) < RangeEpsilon)
                return rangeStart + RangeEpsilon;

            if (Mathf.Abs(distance - rangeEnd) < RangeEpsilon)
                return rangeEnd - RangeEpsilon;

            return distance;
        }

        private static Vector2 Uv(in EdgeTileArt art, Rect uv, float along, float across) =>
            art.Rotated
                ? new Vector2(Mathf.Lerp(uv.xMax, uv.xMin, across), Mathf.Lerp(uv.yMin, uv.yMax, along))
                : new Vector2(Mathf.Lerp(uv.xMin, uv.xMax, along), art.Flipped ? Mathf.Lerp(uv.yMax, uv.yMin, across) : Mathf.Lerp(uv.yMin, uv.yMax, across));
    }
}
