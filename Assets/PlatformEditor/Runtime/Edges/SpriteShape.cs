using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public sealed class SpriteShape
    {
        public const float SolidCoverage = 0.6f;

        private const int CoverageScanRows = 160;
        private const float MinBodyThickness = 0.05f;

        private readonly List<Vector2[]> _polygons = new List<Vector2[]>();
        private readonly List<float> _intersections = new List<float>();

        public SpriteShape(Rect bounds, IReadOnlyList<Vector2[]> polygons, Sprite sprite = null)
        {
            Bounds = bounds;
            Sprite = sprite;

            if (polygons != null)
            {
                for (int i = 0; i < polygons.Count; i++)
                {
                    if (polygons[i] != null && polygons[i].Length >= 3)
                        _polygons.Add(polygons[i]);
                }
            }

            FindSolidRange();
        }

        public Sprite Sprite { get; }

        public Rect Bounds { get; }

        public float SurfaceY { get; private set; }

        public float SolidBottomY { get; private set; }

        public bool HasOutline => _polygons.Count > 0;

        public static SpriteShape FromSprite(Sprite sprite)
        {
            Bounds spriteBounds = sprite.bounds;
            Rect bounds = Rect.MinMaxRect(spriteBounds.min.x, spriteBounds.min.y, spriteBounds.max.x, spriteBounds.max.y);
            int shapeCount = sprite.GetPhysicsShapeCount();
            var polygons = new Vector2[shapeCount][];
            var buffer = new List<Vector2>();

            for (int shape = 0; shape < shapeCount; shape++)
            {
                sprite.GetPhysicsShape(shape, buffer);
                polygons[shape] = buffer.ToArray();
            }

            return new SpriteShape(bounds, polygons, sprite);
        }

        public static SpriteShape FromSpriteFlipped(Sprite sprite)
        {
            Bounds spriteBounds = sprite.bounds;
            Rect bounds = Rect.MinMaxRect(spriteBounds.min.x, spriteBounds.min.y, spriteBounds.max.x, spriteBounds.max.y);
            int shapeCount = sprite.GetPhysicsShapeCount();
            var polygons = new Vector2[shapeCount][];
            var buffer = new List<Vector2>();

            for (int shape = 0; shape < shapeCount; shape++)
            {
                sprite.GetPhysicsShape(shape, buffer);
                polygons[shape] = buffer.ToArray();
            }

            return Flipped(bounds, polygons, sprite);
        }

        public static SpriteShape Flipped(Rect bounds, IReadOnlyList<Vector2[]> polygons, Sprite sprite = null)
        {
            Rect flippedBounds = Rect.MinMaxRect(bounds.xMin, -bounds.yMax, bounds.xMax, -bounds.yMin);
            var flipped = new Vector2[polygons.Count][];

            for (int polygon = 0; polygon < polygons.Count; polygon++)
            {
                Vector2[] points = polygons[polygon];
                flipped[polygon] = new Vector2[points.Length];

                for (int i = 0; i < points.Length; i++)
                    flipped[polygon][i] = new Vector2(points[i].x, -points[i].y);
            }

            return new SpriteShape(flippedBounds, flipped, sprite);
        }

        public static SpriteShape FromSpriteRotated(Sprite sprite, bool faceLeft = true)
        {
            Bounds spriteBounds = sprite.bounds;
            Rect bounds = faceLeft
                ? Rect.MinMaxRect(spriteBounds.min.y, -spriteBounds.max.x, spriteBounds.max.y, -spriteBounds.min.x)
                : Rect.MinMaxRect(-spriteBounds.max.y, spriteBounds.min.x, -spriteBounds.min.y, spriteBounds.max.x);
            int shapeCount = sprite.GetPhysicsShapeCount();
            var polygons = new Vector2[shapeCount][];
            var buffer = new List<Vector2>();

            for (int shape = 0; shape < shapeCount; shape++)
            {
                sprite.GetPhysicsShape(shape, buffer);
                var rotated = new Vector2[buffer.Count];

                for (int i = 0; i < buffer.Count; i++)
                    rotated[i] = faceLeft ? new Vector2(buffer[i].y, -buffer[i].x) : new Vector2(-buffer[i].y, buffer[i].x);

                polygons[shape] = rotated;
            }

            return new SpriteShape(bounds, polygons, sprite);
        }

        public float Coverage(float y)
        {
            if (Bounds.width <= 0f)
                return 0f;

            float covered = 0f;

            for (int polygon = 0; polygon < _polygons.Count; polygon++)
            {
                Vector2[] points = _polygons[polygon];
                _intersections.Clear();

                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 a = points[i];
                    Vector2 b = points[(i + 1) % points.Length];

                    if ((a.y > y) == (b.y > y))
                        continue;

                    _intersections.Add(a.x + (y - a.y) * (b.x - a.x) / (b.y - a.y));
                }

                _intersections.Sort();

                for (int i = 0; i + 1 < _intersections.Count; i += 2)
                    covered += _intersections[i + 1] - _intersections[i];
            }

            return covered / Bounds.width;
        }

        public float EdgeX(float y, bool leftEdge)
        {
            float edge = float.NaN;

            for (int polygon = 0; polygon < _polygons.Count; polygon++)
            {
                Vector2[] points = _polygons[polygon];

                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 a = points[i];
                    Vector2 b = points[(i + 1) % points.Length];

                    if ((a.y > y) == (b.y > y))
                        continue;

                    float x = a.x + (y - a.y) * (b.x - a.x) / (b.y - a.y);

                    if (float.IsNaN(edge) || (leftEdge ? x < edge : x > edge))
                        edge = x;
                }
            }

            return edge;
        }

        private void FindSolidRange()
        {
            float top = Bounds.yMax;
            float bottom = Bounds.yMin;
            float surface = top;
            float solidBottom = bottom;

            if (_polygons.Count > 0)
            {
                float step = (top - bottom) / CoverageScanRows;

                for (int row = 0; row <= CoverageScanRows; row++)
                {
                    float y = top - step * row;

                    if (Coverage(y) < SolidCoverage)
                        continue;

                    surface = y;
                    break;
                }

                for (int row = 0; row <= CoverageScanRows; row++)
                {
                    float y = bottom + step * row;

                    if (Coverage(y) < SolidCoverage)
                        continue;

                    solidBottom = y;
                    break;
                }
            }

            SurfaceY = surface;
            SolidBottomY = Mathf.Min(solidBottom, surface - MinBodyThickness);
        }
    }
}
