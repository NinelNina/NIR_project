using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class EdgeSideProfileTests
    {
        private const float ScanTolerance = 0.05f;

        [Test]
        public void SlantedLeftEnd_ProfileFollowsSpriteOutline()
        {
            var points = new List<Vector2>();

            EdgeSideProfile.Append(StraightPath(), SlantedTileSet(), LeftTile(), true, 3f, 0f, new List<float>(), points, out float top, out float bottom);

            Assert.AreEqual(EdgeSideProfile.Rows + 1, points.Count);
            Assert.AreEqual(0f, points[0].x, ScanTolerance);
            Assert.AreEqual(0f, points[0].y, ScanTolerance);
            Assert.AreEqual(1.73f, points[points.Count - 1].x, ScanTolerance);
            Assert.AreEqual(-3f, points[points.Count - 1].y, ScanTolerance);
            Assert.Less(top, bottom);
        }

        [Test]
        public void StraightRightEdge_ProfileIsVertical()
        {
            var points = new List<Vector2>();

            EdgeSideProfile.Append(StraightPath(), SlantedTileSet(), LeftTile(), false, 3f, 0f, new List<float>(), points, out float top, out float bottom);

            Assert.AreEqual(10f, points[0].x, ScanTolerance);
            Assert.AreEqual(10f, points[points.Count - 1].x, ScanTolerance);
            Assert.AreEqual(top, bottom, ScanTolerance);
        }

        private static SurfacePath StraightPath()
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(0f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(20f, 0f), SurfaceNodeType.Corner)
            }, false);
            return path;
        }

        private static EdgeTileSet SlantedTileSet()
        {
            Rect bounds = Rect.MinMaxRect(0f, 0f, 10f, 4f);
            Vector2[] slanted = { new Vector2(2f, 0f), new Vector2(10f, 0f), new Vector2(10f, 3.5f), new Vector2(0f, 3.5f) };
            Vector2[] box = { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 3.5f), new Vector2(0f, 3.5f) };
            var set = new EdgeTileSet();
            set.SetShapes(new SpriteShape(bounds, new[] { slanted }), new[] { new SpriteShape(bounds, new[] { box }) }, new SpriteShape(bounds, new[] { box }));
            set.SetJoining(EdgeSurfaceAlignment.ImageTop, 0f, false, false);
            return set;
        }

        private static EdgeTile LeftTile() => new EdgeTile(EdgeTileRole.Left, 0, 0f, 10f, 1f);
    }
}
