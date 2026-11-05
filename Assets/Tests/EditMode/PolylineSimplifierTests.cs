using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class PolylineSimplifierTests
    {
        [Test]
        public void CollinearPoints_ReduceToEnds()
        {
            var points = new List<Vector2>();

            for (int i = 0; i <= 10; i++)
                points.Add(new Vector2(i, 0f));

            new PolylineSimplifier().Simplify(points, 0, 0.1f);

            Assert.AreEqual(2, points.Count);
        }

        [Test]
        public void SharpCorner_IsKept()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f), new Vector2(2f, 2f) };

            new PolylineSimplifier().Simplify(points, 0, 0.1f);

            Assert.AreEqual(3, points.Count);
            Assert.AreEqual(new Vector2(2f, 0f), points[1]);
        }

        [Test]
        public void StartOffset_LeavesEarlierPointsUntouched()
        {
            var points = new List<Vector2> { new Vector2(-5f, 3f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 0f) };

            new PolylineSimplifier().Simplify(points, 1, 0.1f);

            Assert.AreEqual(3, points.Count);
            Assert.AreEqual(new Vector2(-5f, 3f), points[0]);
        }

        [Test]
        public void SmallBump_BelowTolerance_IsRemoved()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 0.05f), new Vector2(2f, 0f) };

            new PolylineSimplifier().Simplify(points, 0, 0.1f);

            Assert.AreEqual(2, points.Count);
        }
    }
}
