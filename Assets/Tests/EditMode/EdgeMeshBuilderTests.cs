using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class EdgeMeshBuilderTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void StraightPath_FirstColumnSitsOnSurfaceLine()
        {
            List<Vector3> vertices = BuildStraightTile(out _, out _);

            AssertVector(new Vector2(4f, -0.8f), vertices[0]);
            AssertVector(new Vector2(4f, 0f), vertices[1]);
            AssertVector(new Vector2(4f, 0.2f), vertices[2]);
        }

        [Test]
        public void StraightPath_LastColumnEndsAtTileEnd()
        {
            List<Vector3> vertices = BuildStraightTile(out _, out _);

            AssertVector(new Vector2(6f, 0.2f), vertices[vertices.Count - 1]);
        }

        [Test]
        public void Tile_HasExpectedVertexAndTriangleCounts()
        {
            List<Vector3> vertices = BuildStraightTile(out List<Vector2> uvs, out List<int> triangles);

            Assert.AreEqual((EdgeMeshBuilder.SegmentsPerTile + 1) * 3, vertices.Count);
            Assert.AreEqual(uvs.Count, vertices.Count);
            Assert.AreEqual(EdgeMeshBuilder.SegmentsPerTile * 2 * 2 * 3, triangles.Count);
        }

        [Test]
        public void SurfaceRow_UvMatchesSurfaceHeight()
        {
            BuildStraightTile(out List<Vector2> uvs, out _);

            Assert.AreEqual(0.8f, uvs[1].y, Tolerance);
        }

        [Test]
        public void FlippedArt_BottomRowSamplesImageTop()
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(0f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(10f, 0f), SurfaceNodeType.Corner)
            }, false);
            var art = new EdgeTileArt(Rect.MinMaxRect(-1f, -0.5f, 1f, 0.5f), Rect.MinMaxRect(0f, 0.2f, 1f, 0.6f), 0.3f, Vector2.one, 1f, false, true);
            var tile = new EdgeTile(EdgeTileRole.Middle, 0, 4f, 2f, 1f);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();

            EdgeMeshBuilder.AppendTile(path, art, tile, 0f, vertices, uvs, new List<Color32>(), new List<int>());

            Assert.AreEqual(0.6f, uvs[0].y, Tolerance);
            Assert.AreEqual(0.2f, uvs[2].y, Tolerance);
        }

        [TestCase(false, 0.6f)]
        [TestCase(true, 0.4f)]
        public void CropAboveSurface_CutsFringeAndUv(bool flipped, float expectedTopV)
        {
            var art = new EdgeTileArt(Rect.MinMaxRect(-1f, -0.5f, 1f, 0.5f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 0f, Vector2.one, 1f, false, flipped);

            EdgeTileArt cropped = art.CropAboveSurface(0.1f);
            float topV = flipped ? cropped.UvRect.yMin : cropped.UvRect.yMax;

            Assert.AreEqual(0.1f, cropped.LocalRect.yMax, Tolerance);
            Assert.AreEqual(expectedTopV, topV, Tolerance);
            Assert.IsTrue(cropped.FadeTop);
        }

        [Test]
        public void CropToRange_CutsTileAndTextureAtRangeEnd()
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(0f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(20f, 0f), SurfaceNodeType.Corner)
            }, false);
            var art = new EdgeTileArt(Rect.MinMaxRect(-2f, -0.5f, 2f, 0.5f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 0f, Vector2.one, 1f);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();

            EdgeMeshBuilder.AppendTile(path, art, new EdgeTile(EdgeTileRole.Middle, 0, 7f, 4f, 1f), 0f, vertices, uvs, new List<Color32>(), new List<int>(), 0f, 10f, true);

            Assert.AreEqual(10f, vertices[vertices.Count - 2].x, 0.01f);
            Assert.AreEqual(0.75f, uvs[uvs.Count - 2].x, Tolerance);
        }

        [Test]
        public void FadeTop_TopRowIsTransparent()
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(0f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(10f, 0f), SurfaceNodeType.Corner)
            }, false);
            var art = new EdgeTileArt(Rect.MinMaxRect(-1f, -0.5f, 1f, 0.5f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 0f, Vector2.one, 1f, false, false, true);
            var colors = new List<Color32>();

            EdgeMeshBuilder.AppendTile(path, art, new EdgeTile(EdgeTileRole.Middle, 0, 4f, 2f, 1f), 0f, new List<Vector3>(), new List<Vector2>(), colors, new List<int>());

            Assert.AreEqual(255, colors[1].a);
            Assert.AreEqual(0, colors[2].a);
        }

        [Test]
        public void TileOverSharpPeak_HasColumnOnPeakAndNoFoldedTriangles()
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(-4f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(0f, 4f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(4f, 0f), SurfaceNodeType.Corner)
            }, false);
            var art = new EdgeTileArt(Rect.MinMaxRect(-2.5f, -1.5f, 2.5f, 0.5f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 0f, Vector2.one, 1f);
            float peak = path.Length * 0.5f;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            EdgeMeshBuilder.AppendTile(path, art, new EdgeTile(EdgeTileRole.Middle, 0, peak - 2.3f, 5f, 1f), 0f, vertices, new List<Vector2>(), new List<Color32>(), triangles);

            bool hasPeakColumn = false;

            for (int i = 1; i < vertices.Count; i += EdgeMeshBuilder.RowsPerColumn)
                hasPeakColumn |= ((Vector2)vertices[i] - new Vector2(0f, 4f)).sqrMagnitude < Tolerance;

            Assert.IsTrue(hasPeakColumn);

            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector2 a = vertices[triangles[i]];
                Vector2 b = vertices[triangles[i + 1]];
                Vector2 c = vertices[triangles[i + 2]];
                Assert.LessOrEqual((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x), Tolerance);
            }
        }

        private static List<Vector3> BuildStraightTile(out List<Vector2> uvs, out List<int> triangles)
        {
            var path = new SurfacePath();
            path.Build(new List<SurfaceNode>
            {
                new SurfaceNode(new Vector2(0f, 0f), SurfaceNodeType.Corner),
                new SurfaceNode(new Vector2(10f, 0f), SurfaceNodeType.Corner)
            }, false);

            var art = new EdgeTileArt(Rect.MinMaxRect(-1f, -0.5f, 1f, 0.5f), Rect.MinMaxRect(0f, 0f, 1f, 1f), 0.3f, Vector2.one, 1f);
            var tile = new EdgeTile(EdgeTileRole.Middle, 0, 4f, 2f, 1f);
            var vertices = new List<Vector3>();
            var colors = new List<Color32>();
            uvs = new List<Vector2>();
            triangles = new List<int>();

            EdgeMeshBuilder.AppendTile(path, art, tile, 0f, vertices, uvs, colors, triangles);
            return vertices;
        }

        private static void AssertVector(Vector2 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
