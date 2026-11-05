using System.Collections.Generic;
using Level.Platforms;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SurfacePathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void StraightOpenPath_LengthEqualsNodeDistance()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(3f, 4f));
            var path = new SurfacePath();

            path.Build(nodes, false);

            Assert.AreEqual(5f, path.Length, Tolerance);
            Assert.AreEqual(2, path.SampleCount);
        }

        [Test]
        public void ClosedCornerSquare_LengthEqualsPerimeter()
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();

            path.Build(nodes, true);

            Assert.AreEqual(8f, path.Length, Tolerance);
            Assert.AreEqual(4, path.SampleCount);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClosedSquare_NormalsPointOutwardForAnyWinding(bool reversed)
        {
            List<SurfaceNode> nodes = Square(reversed);
            var path = new SurfacePath();
            var center = new Vector2(1f, 1f);

            path.Build(nodes, true);

            for (int i = 0; i < path.SampleCount; i++)
                Assert.Greater(Vector2.Dot(path.Normals[i], path.Points[i] - center), 0f);
        }

        [Test]
        public void OpenPath_LeftToRight_NormalPointsUp()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var path = new SurfacePath();

            path.Build(nodes, false);

            AssertVector(Vector2.up, path.Normals[0]);
        }

        [Test]
        public void OpenPath_ReversedNodes_NormalPointsDown()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(4f, 0f), new Vector2(0f, 0f));
            var path = new SurfacePath();

            path.Build(nodes, false);

            AssertVector(Vector2.down, path.Normals[0]);
        }

        [Test]
        public void OpenPath_Flipped_NormalPointsDown()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var path = new SurfacePath();

            path.Build(nodes, false, true);

            AssertVector(Vector2.down, path.Normals[0]);
        }

        [Test]
        public void SmoothPath_PassesThroughEveryNode()
        {
            List<SurfaceNode> nodes = Wave(SurfaceNodeType.Smooth);
            var path = new SurfacePath();

            path.Build(nodes, false);

            for (int node = 0; node < nodes.Count; node++)
                AssertVector(nodes[node].Position, path.Points[path.NodeSampleIndex(node)]);
        }

        [Test]
        public void SmoothNode_HasNoVisibleCorner()
        {
            List<SurfaceNode> nodes = Wave(SurfaceNodeType.Smooth);
            var path = new SurfacePath(64);

            path.Build(nodes, false);

            Assert.Less(ChordAngleAtNode(path, 1), 5f);
        }

        [Test]
        public void CornerNode_KeepsSharpCorner()
        {
            List<SurfaceNode> nodes = Wave(SurfaceNodeType.Corner);
            var path = new SurfacePath(64);

            path.Build(nodes, false);

            Assert.Greater(ChordAngleAtNode(path, 1), 30f);
        }

        [Test]
        public void ClosedPathWithTwoNodes_IsInvalid()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(1f, 0f));
            var path = new SurfacePath();

            path.Build(nodes, true);

            Assert.IsFalse(path.IsValid);
        }

        [Test]
        public void FigureEight_FindsSelfIntersection()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(2f, 2f), new Vector2(2f, 0f), new Vector2(0f, 2f));
            var path = new SurfacePath();
            path.Build(nodes, true);

            bool found = path.TryFindSelfIntersection(out Vector2 point);

            Assert.IsTrue(found);
            AssertVector(new Vector2(1f, 1f), point);
        }

        [Test]
        public void Square_HasNoSelfIntersection()
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();
            path.Build(nodes, true);

            bool found = path.TryFindSelfIntersection(out _);

            Assert.IsFalse(found);
        }

        [Test]
        public void Evaluate_MiddleOfStraightPath_ReturnsMidpointAndUpNormal()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var path = new SurfacePath();
            path.Build(nodes, false);

            path.Evaluate(2f, out Vector2 point, out Vector2 normal);

            AssertVector(new Vector2(2f, 0f), point);
            AssertVector(Vector2.up, normal);
        }

        [TestCase(-1f, -1f)]
        [TestCase(5f, 5f)]
        public void Evaluate_BeyondOpenEnds_ExtrapolatesAlongEndTangent(float distance, float expectedX)
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var path = new SurfacePath();
            path.Build(nodes, false);

            path.Evaluate(distance, out Vector2 point, out _);

            AssertVector(new Vector2(expectedX, 0f), point);
        }

        [Test]
        public void Evaluate_ClosedPathPastLength_WrapsAround()
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();
            path.Build(nodes, true);

            path.Evaluate(path.Length + 1f, out Vector2 wrapped, out _);
            path.Evaluate(1f, out Vector2 direct, out _);

            AssertVector(direct, wrapped);
        }

        [Test]
        public void Evaluate_NearSquareCorner_UsesSideNormalNotBisector()
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();
            path.Build(nodes, true);

            path.Evaluate(0.5f, out _, out Vector2 normal);

            AssertVector(Vector2.left, normal);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClosedSquare_AnyWinding_SamplesRunClockwise(bool reversed)
        {
            List<SurfaceNode> nodes = Square(reversed);
            var path = new SurfacePath();

            path.Build(nodes, true);

            Assert.Less(path.SignedArea, 0f);
        }

        [TestCase(0, 6f, 8f)]
        [TestCase(3, 0f, 2f)]
        public void SegmentRange_CounterClockwiseSquare_CoversSegment(int segment, float expectedStart, float expectedEnd)
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();
            path.Build(nodes, true);

            path.SegmentRange(segment, out float start, out float end);

            Assert.AreEqual(expectedStart, start, Tolerance);
            Assert.AreEqual(expectedEnd, end, Tolerance);
        }

        [Test]
        public void OpenFlipped_NodeSampleIndex_StillPointsToSameNode()
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var path = new SurfacePath();

            path.Build(nodes, false, true);

            AssertVector(nodes[0].Position, path.Points[path.NodeSampleIndex(0)]);
        }

        [Test]
        public void MiterNormal_SquareCorner_KeepsEqualOffsetFromBothSides()
        {
            List<SurfaceNode> nodes = Square(false);
            var path = new SurfacePath();
            path.Build(nodes, true);

            Vector2 miter = path.MiterNormal(path.NodeSampleIndex(1));

            AssertVector(new Vector2(1f, -1f), miter);
        }

        [Test]
        public void IsNodeSample_MarksExactlyTheNodes()
        {
            List<SurfaceNode> nodes = Wave(SurfaceNodeType.Smooth);
            var path = new SurfacePath();
            path.Build(nodes, false);
            int marked = 0;

            for (int i = 0; i < path.SampleCount; i++)
            {
                if (path.IsNodeSample(i))
                    marked++;
            }

            Assert.AreEqual(nodes.Count, marked);
        }

        private static float ChordAngleAtNode(SurfacePath path, int node)
        {
            int sample = path.NodeSampleIndex(node);
            Vector2 incoming = path.Points[sample] - path.Points[sample - 1];
            Vector2 outgoing = path.Points[sample + 1] - path.Points[sample];
            return Vector2.Angle(incoming, outgoing);
        }

        private static List<SurfaceNode> Square(bool reversed)
        {
            List<SurfaceNode> nodes = Nodes(SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));

            if (reversed)
                nodes.Reverse();

            return nodes;
        }

        private static List<SurfaceNode> Wave(SurfaceNodeType type) =>
            Nodes(type, new Vector2(0f, 0f), new Vector2(2f, 1f), new Vector2(4f, 0f), new Vector2(6f, 1f));

        private static List<SurfaceNode> Nodes(SurfaceNodeType type, params Vector2[] positions)
        {
            var nodes = new List<SurfaceNode>(positions.Length);

            foreach (Vector2 position in positions)
                nodes.Add(new SurfaceNode(position, type));

            return nodes;
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
