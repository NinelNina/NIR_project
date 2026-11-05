using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class EdgeTileSetTests
    {
        private const float ScanTolerance = 0.05f;

        [Test]
        public void TransparentMarginOnLeftEnd_OverlapClosesSeam()
        {
            EdgeTileSet set = TileSet(Box(0f, 0f, 9f, 3.5f));
            set.SetJoining(EdgeSurfaceAlignment.ImageTop, 0f, true, false);

            float overlap = set.Overlap(EdgeTileRole.Left, 0, EdgeTileRole.Middle, 0);

            Assert.AreEqual(1f, overlap, ScanTolerance);
        }

        [Test]
        public void SeamClosingOff_OverlapIsExtraOnly()
        {
            EdgeTileSet set = TileSet(Box(0f, 0f, 9f, 3.5f));
            set.SetJoining(EdgeSurfaceAlignment.ImageTop, 0.02f, false, false);

            float overlap = set.Overlap(EdgeTileRole.Left, 0, EdgeTileRole.Middle, 0);

            Assert.AreEqual(0.02f, overlap, 0.0001f);
        }

        [Test]
        public void RaggedTopRow_DoesNotWidenSeam()
        {
            var bumpy = new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 3.4f), new Vector2(6f, 3.6f), new Vector2(4f, 3.6f), new Vector2(0f, 3.4f) };
            EdgeTileSet set = TileSet(bumpy);
            set.SetJoining(EdgeSurfaceAlignment.ImageTop, 0f, true, false);

            float overlap = set.Overlap(EdgeTileRole.Left, 0, EdgeTileRole.Middle, 0);

            Assert.AreEqual(0f, overlap, ScanTolerance);
        }

        [Test]
        public void ThinnerEnd_BelowSurfaceScaleStretchesToMiddleThickness()
        {
            EdgeTileSet set = TileSet(Box(0f, 1.5f, 10f, 3.5f));
            set.SetJoining(EdgeSurfaceAlignment.ImageTop, 0f, true, true);

            float scale = set.BelowSurfaceScale(EdgeTileRole.Left, 0);

            Assert.AreEqual(1.75f, scale, ScanTolerance);
        }

        [Test]
        public void MiddleScale_MultipliesWidth()
        {
            EdgeTileSet set = TileSet(Box(0f, 0f, 10f, 3.5f));
            set.SetScales(Vector2.one, new Vector2(2f, 1f), Vector2.one);

            float width = set.Width(EdgeTileRole.Middle, 0);

            Assert.AreEqual(20f, width, 0.0001f);
        }

        private static EdgeTileSet TileSet(Vector2[] leftBody)
        {
            Rect bounds = Rect.MinMaxRect(0f, 0f, 10f, 4f);
            var left = new SpriteShape(bounds, new[] { leftBody });
            var middle = new SpriteShape(bounds, new[] { Box(0f, 0f, 10f, 3.5f) });
            var right = new SpriteShape(bounds, new[] { Box(0f, 0f, 10f, 3.5f) });
            var set = new EdgeTileSet();
            set.SetShapes(left, new[] { middle }, right);
            return set;
        }

        private static Vector2[] Box(float xMin, float yMin, float xMax, float yMax) =>
            new[] { new Vector2(xMin, yMin), new Vector2(xMax, yMin), new Vector2(xMax, yMax), new Vector2(xMin, yMax) };
    }
}
