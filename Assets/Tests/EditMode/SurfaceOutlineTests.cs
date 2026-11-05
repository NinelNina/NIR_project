using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SurfaceOutlineTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ClosedFill_SquareInset_ShrinksEvenly()
        {
            SurfacePath path = Build(true, new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));
            var outline = new List<Vector2>();

            SurfaceOutline.ClosedFill(path, 0.1f, outline);

            Assert.AreEqual(1.8f * 1.8f, Mathf.Abs(PolygonTriangulator.SignedArea(outline)), Tolerance);
        }

        [Test]
        public void OpenFill_StraightPlatform_ReachesBase()
        {
            SurfacePath path = Build(false, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var outline = new List<Vector2>();

            SurfaceOutline.OpenFill(path, 0.1f, -2f, outline);

            Assert.AreEqual(4, outline.Count);
            AssertVector(new Vector2(0f, -0.1f), outline[0]);
            AssertVector(new Vector2(4f, -0.1f), outline[1]);
            AssertVector(new Vector2(4f, -2f), outline[2]);
            AssertVector(new Vector2(0f, -2f), outline[3]);
        }

        [Test]
        public void ClosedFill_ChamferedCorner_ReplacedByTwoPoints()
        {
            SurfacePath path = Build(true, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 4f), new Vector2(0f, 4f));
            var chamfer = new List<bool> { true, false, false, false };
            var outline = new List<Vector2>();

            SurfaceOutline.ClosedFill(path, 0f, chamfer, 0.5f, outline);

            Assert.AreEqual(5, outline.Count);
            Assert.AreEqual(0.5f, Vector2.Distance(outline[0], path.Points[0]), 0.0001f);
            Assert.AreEqual(0.5f, Vector2.Distance(outline[1], path.Points[0]), 0.0001f);
        }

        [Test]
        public void IsConvex_SquareCorners_AreConvex()
        {
            SurfacePath path = Build(true, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 4f), new Vector2(0f, 4f));

            for (int i = 0; i < path.SampleCount; i++)
                Assert.IsTrue(SurfaceOutline.IsConvex(path, i));
        }

        [Test]
        public void IsConvex_NotchCorner_IsConcave()
        {
            SurfacePath path = Build(true, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 4f), new Vector2(2f, 2f), new Vector2(0f, 4f));
            int concave = 0;

            for (int i = 0; i < path.SampleCount; i++)
            {
                if (!SurfaceOutline.IsConvex(path, i))
                    concave++;
            }

            Assert.AreEqual(1, concave);
        }

        private static SurfacePath Build(bool closed, params Vector2[] positions)
        {
            var nodes = new List<SurfaceNode>();

            foreach (Vector2 position in positions)
                nodes.Add(new SurfaceNode(position, SurfaceNodeType.Corner));

            var path = new SurfacePath();
            path.Build(nodes, closed);
            return path;
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
