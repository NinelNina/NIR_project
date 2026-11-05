using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class PolygonTriangulatorTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Square_GivesTwoTriangles()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f) };
            var triangles = new List<int>();

            PolygonTriangulator.Triangulate(points, 0, triangles, new List<int>());

            Assert.AreEqual(6, triangles.Count);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ConcaveLShape_TrianglesCoverExactArea(bool reversed)
        {
            List<Vector2> points = LShape();

            if (reversed)
                points.Reverse();

            var triangles = new List<int>();

            PolygonTriangulator.Triangulate(points, 0, triangles, new List<int>());

            Assert.AreEqual(12, triangles.Count);
            Assert.AreEqual(3f, TotalArea(points, triangles), Tolerance);
        }

        [Test]
        public void FirstVertex_OffsetsAllIndices()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f) };
            var triangles = new List<int>();

            PolygonTriangulator.Triangulate(points, 10, triangles, new List<int>());

            foreach (int index in triangles)
                Assert.GreaterOrEqual(index, 10);
        }

        private static List<Vector2> LShape() => new List<Vector2>
        {
            new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 2f), new Vector2(0f, 2f)
        };

        private static float TotalArea(List<Vector2> points, List<int> triangles)
        {
            float area = 0f;

            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector2 a = points[triangles[i]];
                Vector2 b = points[triangles[i + 1]];
                Vector2 c = points[triangles[i + 2]];
                area += Mathf.Abs((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) * 0.5f;
            }

            return area;
        }
    }
}
