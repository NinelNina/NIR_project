using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class EdgeSectionsTests
    {
        private const float Tolerance = 0.0001f;
        private const float WallAngle = 45f;
        private const float CeilingAngle = 135f;
        private const float MinLength = 0.5f;

        [TestCase(0f, 1f, EdgeKind.Top)]
        [TestCase(0.5f, 1f, EdgeKind.Top)]
        [TestCase(1f, 0f, EdgeKind.Wall)]
        [TestCase(-1f, 0.2f, EdgeKind.Wall)]
        [TestCase(0f, -1f, EdgeKind.Ceiling)]
        public void Classify_ByAngleToUp(float x, float y, EdgeKind expected)
        {
            EdgeKind kind = EdgeSections.Classify(new Vector2(x, y).normalized, WallAngle, CeilingAngle);

            Assert.AreEqual(expected, kind);
        }

        [Test]
        public void FlatOpenPath_IsSingleTop()
        {
            SurfacePath path = Build(false, new Vector2(0f, 0f), new Vector2(10f, 0f));
            var sections = new List<EdgeSection>();

            EdgeSections.Build(path, 0f, path.Length, false, WallAngle, CeilingAngle, MinLength, sections);

            Assert.AreEqual(1, sections.Count);
            Assert.AreEqual(EdgeKind.Top, sections[0].Kind);
            Assert.AreEqual(10f, sections[0].Length, Tolerance);
        }

        [Test]
        public void ShortStep_MergesIntoTop()
        {
            SurfacePath path = Build(false, new Vector2(0f, 0f), new Vector2(5f, 0f), new Vector2(5f, 0.3f), new Vector2(10f, 0.3f));
            var sections = new List<EdgeSection>();

            EdgeSections.Build(path, 0f, path.Length, false, WallAngle, CeilingAngle, MinLength, sections);

            Assert.AreEqual(1, sections.Count);
            Assert.AreEqual(EdgeKind.Top, sections[0].Kind);
            Assert.AreEqual(path.Length, sections[0].Length, Tolerance);
        }

        [Test]
        public void ClosedBox_HasTopTwoWallsAndCeiling()
        {
            SurfacePath path = Build(true, new Vector2(-4f, -4f), new Vector2(4f, -4f), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            var sections = new List<EdgeSection>();

            EdgeSections.Build(path, 0f, path.Length, true, WallAngle, CeilingAngle, MinLength, sections);

            Assert.AreEqual(4, sections.Count);
            Assert.AreEqual(1, Count(sections, EdgeKind.Top));
            Assert.AreEqual(2, Count(sections, EdgeKind.Wall));
            Assert.AreEqual(1, Count(sections, EdgeKind.Ceiling));
            Assert.AreEqual(path.Length, sections[sections.Count - 1].End - sections[0].Start, Tolerance);
        }

        [Test]
        public void ClosedBox_TopSectionSpansTopSide()
        {
            SurfacePath path = Build(true, new Vector2(-4f, -4f), new Vector2(4f, -4f), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            var sections = new List<EdgeSection>();

            EdgeSections.Build(path, 0f, path.Length, true, WallAngle, CeilingAngle, MinLength, sections);

            foreach (EdgeSection section in sections)
            {
                if (section.Kind == EdgeKind.Top)
                    Assert.AreEqual(8f, section.Length, Tolerance);
                else if (section.Kind == EdgeKind.Wall)
                    Assert.AreEqual(4f, section.Length, Tolerance);
            }
        }

        private static int Count(List<EdgeSection> sections, EdgeKind kind)
        {
            int count = 0;

            foreach (EdgeSection section in sections)
            {
                if (section.Kind == kind)
                    count++;
            }

            return count;
        }

        private static SurfacePath Build(bool closed, params Vector2[] points)
        {
            var nodes = new List<SurfaceNode>();

            foreach (Vector2 point in points)
                nodes.Add(new SurfaceNode(point, SurfaceNodeType.Corner));

            var path = new SurfacePath();
            path.Build(nodes, closed);
            return path;
        }
    }
}
