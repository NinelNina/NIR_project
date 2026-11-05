using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class EdgeCornerBuilderTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void LeftCorner_FaceAndSurfaceMeetAtCorner()
        {
            List<Vector3> vertices = Build(true, out _, out _);

            AssertVector(new Vector2(-0.5f, 0.5f), vertices[6]);
            AssertVector(new Vector2(1f, -1f), vertices[2]);
        }

        [Test]
        public void RightCorner_IsMirrored()
        {
            List<Vector3> vertices = Build(false, out _, out _);

            AssertVector(new Vector2(0.5f, 0.5f), vertices[8]);
            AssertVector(new Vector2(-1f, -1f), vertices[0]);
        }

        [TestCase(true, 2, 6)]
        [TestCase(false, 0, 8)]
        public void InnerEdges_FadeOut(bool faceLeft, int innerVertex, int outerVertex)
        {
            Build(faceLeft, out List<Color32> colors, out _);

            Assert.AreEqual(0, colors[innerVertex].a);
            Assert.AreEqual(0, colors[innerVertex + 3].a);
            Assert.AreEqual(255, colors[outerVertex].a);
        }

        [Test]
        public void Grid_HasNineVerticesAndEightTriangles()
        {
            List<Vector3> vertices = Build(true, out _, out List<int> triangles);

            Assert.AreEqual(9, vertices.Count);
            Assert.AreEqual(24, triangles.Count);
        }

        private static List<Vector3> Build(bool faceLeft, out List<Color32> colors, out List<int> triangles)
        {
            var art = new EdgeCornerArt(Rect.MinMaxRect(-2f, -2f, 2f, 2f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 1.5f, faceLeft ? -1.5f : 1.5f, faceLeft);
            var vertices = new List<Vector3>();
            colors = new List<Color32>();
            triangles = new List<int>();

            Vector2 alongTop = faceLeft ? Vector2.right : Vector2.left;
            EdgeCornerBuilder.Append(art, Vector2.zero, alongTop, Vector2.down, 1f, 1f, 0.2f, vertices, new List<Vector2>(), colors, triangles);
            return vertices;
        }

        private static void AssertVector(Vector2 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
