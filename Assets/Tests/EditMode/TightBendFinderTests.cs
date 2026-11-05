using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class TightBendFinderTests
    {
        private const float DepthBelow = 1.9f;
        private const float HeightAbove = 0.6f;

        [Test]
        public void Find_GentleArch_ReportsNothing()
        {
            SurfacePath path = Build(SurfaceNodeType.Smooth, new Vector2(-10f, 0f), new Vector2(0f, 2f), new Vector2(10f, 0f));
            var bends = new List<TightBend>();

            TightBendFinder.Find(path, DepthBelow, HeightAbove, bends);

            Assert.AreEqual(0, bends.Count);
        }

        [Test]
        public void Find_TightHump_ReportsHumpBelowRequiredDepth()
        {
            SurfacePath path = Build(SurfaceNodeType.Smooth, new Vector2(-3f, 0f), new Vector2(0f, 1.5f), new Vector2(3f, 0f));
            var bends = new List<TightBend>();

            TightBendFinder.Find(path, DepthBelow, HeightAbove, bends);

            Assert.AreEqual(1, bends.Count);
            Assert.IsFalse(bends[0].Valley);
            Assert.AreEqual(DepthBelow, bends[0].RequiredRadius);
            Assert.Less(bends[0].Radius, DepthBelow);
            Assert.Less(bends[0].Start, bends[0].End);
        }

        [Test]
        public void Find_TightValley_UsesHeightAboveSurface()
        {
            SurfacePath path = Build(SurfaceNodeType.Smooth, new Vector2(-1f, 0f), new Vector2(0f, -1f), new Vector2(1f, 0f));
            var bends = new List<TightBend>();

            TightBendFinder.Find(path, DepthBelow, HeightAbove, bends);

            Assert.AreEqual(1, bends.Count);
            Assert.IsTrue(bends[0].Valley);
            Assert.AreEqual(HeightAbove, bends[0].RequiredRadius);
        }

        [Test]
        public void Find_SharpCorners_AreIgnored()
        {
            SurfacePath path = Build(SurfaceNodeType.Corner, new Vector2(-4f, 0f), new Vector2(-2f, 1f), new Vector2(0f, 0f), new Vector2(2f, 1f), new Vector2(4f, 0f));
            var bends = new List<TightBend>();

            TightBendFinder.Find(path, DepthBelow, HeightAbove, bends);

            Assert.AreEqual(0, bends.Count);
        }

        [Test]
        public void Find_Wave_SplitsHumpsAndValleys()
        {
            SurfacePath path = Build(SurfaceNodeType.Smooth, new Vector2(-3f, 0f), new Vector2(-1.5f, 1f), new Vector2(0f, -0.5f), new Vector2(1.5f, 1f), new Vector2(3f, 0f));
            var bends = new List<TightBend>();

            TightBendFinder.Find(path, DepthBelow, HeightAbove, bends);

            Assert.AreEqual(3, bends.Count);
            Assert.IsFalse(bends[0].Valley);
            Assert.IsTrue(bends[1].Valley);
            Assert.IsFalse(bends[2].Valley);
            Assert.LessOrEqual(bends[0].End, bends[1].Start);
            Assert.LessOrEqual(bends[1].End, bends[2].Start);
        }

        private static SurfacePath Build(SurfaceNodeType type, params Vector2[] positions)
        {
            var nodes = new List<SurfaceNode>();

            foreach (Vector2 position in positions)
                nodes.Add(new SurfaceNode(position, type));

            var path = new SurfacePath();
            path.Build(nodes, false);
            return path;
        }
    }
}
