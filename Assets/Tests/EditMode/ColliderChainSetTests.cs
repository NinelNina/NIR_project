using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class ColliderChainSetTests
    {
        [Test]
        public void Add_DropsRepeatedPoints()
        {
            var chains = new ColliderChainSet();
            var points = new List<Vector2>();

            chains.Add(EdgeKind.Top, new List<Vector2> { Vector2.zero, Vector2.zero, Vector2.right, Vector2.right });
            chains.CopyPoints(0, points);

            Assert.AreEqual(2, points.Count);
        }

        [Test]
        public void Add_SinglePoint_IsIgnored()
        {
            var chains = new ColliderChainSet();

            chains.Add(EdgeKind.Top, new List<Vector2> { Vector2.zero, Vector2.zero });

            Assert.AreEqual(0, chains.Count);
        }

        [Test]
        public void TryGetAdjacent_ConsecutiveChains_ReturnNeighbourPoints()
        {
            var chains = new ColliderChainSet();
            chains.Add(EdgeKind.Top, new List<Vector2> { new Vector2(0f, 0f), new Vector2(2f, 0f) });
            chains.Add(EdgeKind.Wall, new List<Vector2> { new Vector2(2f, 0f), new Vector2(2f, -3f) });

            Assert.IsTrue(chains.TryGetAdjacent(0, false, out Vector2 afterTop));
            Assert.IsTrue(chains.TryGetAdjacent(1, true, out Vector2 beforeWall));
            Assert.IsFalse(chains.TryGetAdjacent(0, true, out _));
            Assert.AreEqual(new Vector2(2f, -3f), afterTop);
            Assert.AreEqual(new Vector2(0f, 0f), beforeWall);
        }

        [Test]
        public void TryGetAdjacent_ClosedChain_WrapsOntoItself()
        {
            var chains = new ColliderChainSet();
            chains.Add(EdgeKind.Top, new List<Vector2> { new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 0f) });

            Assert.IsTrue(chains.TryGetAdjacent(0, true, out Vector2 before));
            Assert.IsTrue(chains.TryGetAdjacent(0, false, out Vector2 after));
            Assert.AreEqual(new Vector2(2f, 2f), before);
            Assert.AreEqual(new Vector2(2f, 0f), after);
        }

        [TestCase(EdgeKind.Top, 1f)]
        [TestCase(EdgeKind.Wall, 2f)]
        [TestCase(EdgeKind.Ceiling, 3f)]
        public void ColliderOffsets_For_PicksKind(EdgeKind kind, float expected)
        {
            var offsets = new ColliderOffsets(1f, 2f, 3f);

            Assert.AreEqual(expected, offsets.For(kind));
        }
    }
}
