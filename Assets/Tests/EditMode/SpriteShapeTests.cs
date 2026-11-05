using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SpriteShapeTests
    {
        private const float ScanTolerance = 0.05f;

        [Test]
        public void SparseGrassAboveBody_SurfaceIsBodyTop()
        {
            SpriteShape shape = PlatformWithGrassAndRoots();

            Assert.AreEqual(4f, shape.SurfaceY, ScanTolerance);
        }

        [Test]
        public void SparseRootsBelowBody_SolidBottomIsBodyBottom()
        {
            SpriteShape shape = PlatformWithGrassAndRoots();

            Assert.AreEqual(0f, shape.SolidBottomY, ScanTolerance);
        }

        [TestCase(true, 0f)]
        [TestCase(false, 10f)]
        public void EdgeX_InsideBody_ReturnsBodySide(bool leftEdge, float expected)
        {
            SpriteShape shape = PlatformWithGrassAndRoots();

            float edge = shape.EdgeX(2f, leftEdge);

            Assert.AreEqual(expected, edge, ScanTolerance);
        }

        [Test]
        public void NoOutline_SurfaceIsBoundsTop()
        {
            var shape = new SpriteShape(Rect.MinMaxRect(0f, 0f, 10f, 4f), null);

            Assert.AreEqual(4f, shape.SurfaceY, ScanTolerance);
        }

        [Test]
        public void Flipped_RootsBecomeSurfaceSide()
        {
            Vector2[] body = Box(0f, 0f, 10f, 4f);
            Vector2[] grass = Box(1f, 4f, 2f, 5f);
            Vector2[] roots = Box(3f, -1f, 4f, 0f);

            SpriteShape shape = SpriteShape.Flipped(Rect.MinMaxRect(0f, -1f, 10f, 5f), new[] { body, grass, roots });

            Assert.AreEqual(0f, shape.SurfaceY, ScanTolerance);
            Assert.AreEqual(-4f, shape.SolidBottomY, ScanTolerance);
        }

        private static SpriteShape PlatformWithGrassAndRoots()
        {
            Vector2[] body = Box(0f, 0f, 10f, 4f);
            Vector2[] grass = Box(1f, 4f, 2f, 5f);
            Vector2[] roots = Box(3f, -1f, 4f, 0f);
            return new SpriteShape(Rect.MinMaxRect(0f, -1f, 10f, 5f), new[] { body, grass, roots });
        }

        private static Vector2[] Box(float xMin, float yMin, float xMax, float yMax) =>
            new[] { new Vector2(xMin, yMin), new Vector2(xMax, yMin), new Vector2(xMax, yMax), new Vector2(xMin, yMax) };
    }
}
