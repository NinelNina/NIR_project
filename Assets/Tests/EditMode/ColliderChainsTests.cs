using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class ColliderChainsTests
    {
        [Test]
        public void AllSides_SplitIntoKindsAroundPlatform()
        {
            ColliderChainSet chains = Assemble(true, true, true);

            Assert.AreEqual(4, chains.Count);
            Assert.AreEqual(EdgeKind.Wall, chains[0].Kind);
            Assert.AreEqual(EdgeKind.Top, chains[1].Kind);
            Assert.AreEqual(EdgeKind.Wall, chains[2].Kind);
            Assert.AreEqual(EdgeKind.Ceiling, chains[3].Kind);
        }

        [Test]
        public void AllSides_EveryChainHasBothNeighbours()
        {
            ColliderChainSet chains = Assemble(true, true, true);

            for (int i = 0; i < chains.Count; i++)
            {
                Assert.IsTrue(chains.TryGetAdjacent(i, true, out _));
                Assert.IsTrue(chains.TryGetAdjacent(i, false, out _));
            }
        }

        [Test]
        public void LeftSideOff_TopHasNoStartNeighbour()
        {
            ColliderChainSet chains = Assemble(false, true, true);
            int top = chains.IndexOf(EdgeKind.Top);

            Assert.AreEqual(3, chains.Count);
            Assert.IsFalse(chains.TryGetAdjacent(top, true, out _));
            Assert.IsTrue(chains.TryGetAdjacent(top, false, out Vector2 next));
            Assert.AreEqual(new Vector2(4f, -1f), next);
        }

        [Test]
        public void BothSidesOff_TopAndBottomOnly()
        {
            ColliderChainSet chains = Assemble(false, false, true);

            Assert.AreEqual(2, chains.Count);
            Assert.AreEqual(1, chains.CountOf(EdgeKind.Top));
            Assert.AreEqual(1, chains.CountOf(EdgeKind.Ceiling));
            Assert.AreEqual(0, chains.CountOf(EdgeKind.Wall));
        }

        [Test]
        public void BottomOff_NoCeiling()
        {
            ColliderChainSet chains = Assemble(true, true, false);

            Assert.AreEqual(3, chains.Count);
            Assert.AreEqual(0, chains.CountOf(EdgeKind.Ceiling));
        }

        private static ColliderChainSet Assemble(bool left, bool right, bool bottom)
        {
            var chains = new ColliderChainSet();

            ColliderChains.Assemble(
                new List<Vector2> { new Vector2(0f, -1f), new Vector2(0f, 0f) },
                new List<Vector2> { new Vector2(0f, 0f), new Vector2(4f, 0f) },
                new List<Vector2> { new Vector2(4f, 0f), new Vector2(4f, -1f) },
                new List<Vector2> { new Vector2(4f, -1f), new Vector2(0f, -1f) },
                left,
                right,
                bottom,
                chains);
            return chains;
        }
    }
}
