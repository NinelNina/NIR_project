using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SurfaceColliderBuilderTests
    {
        private const float Tolerance = 0.0001f;
        private const float ColliderTolerance = 0.1f;

        [Test]
        public void Closed_Square_FormsClosedLoop()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.Closed(path, 0f, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(5, points.Count);
            AssertVector(points[0], points[4]);
        }

        [Test]
        public void Closed_SmoothThroughNodes_UsesOnlyNodes()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Smooth, new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.Closed(path, 0f, SurfaceColliderShape.ThroughNodes, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(5, points.Count);
        }

        [Test]
        public void AppendLine_ThroughNodes_UsesOnlyNodes()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Smooth, Wave());
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, 0f, SurfaceColliderShape.ThroughNodes, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(4, points.Count);
            AssertVector(new Vector2(2f, 1f), points[1]);
        }

        [Test]
        public void AppendLine_Simplified_StaysWithinTolerance()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Smooth, Wave());
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, 0f, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.Less(points.Count, path.SampleCount);

            for (int i = 0; i < path.SampleCount; i++)
                Assert.LessOrEqual(DistanceToPolyline(path.Points[i], points), ColliderTolerance + Tolerance);
        }

        [Test]
        public void AppendLine_CornerNode_KeepsCorner()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(2f, 1f), new Vector2(4f, 0f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, 0f, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(3, points.Count);
            AssertVector(new Vector2(2f, 1f), points[1]);
        }

        [Test]
        public void AppendLine_StraightPath_IsTwoPoints()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Smooth, new Vector2(0f, 0f), new Vector2(4f, 0f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0.5f, 3.5f, 0f, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(2, points.Count);
            AssertVector(new Vector2(0.5f, 0f), points[0]);
            AssertVector(new Vector2(3.5f, 0f), points[1]);
        }

        [Test]
        public void ClosedSections_Box_OneChainPerSection()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 2f), new Vector2(0f, 2f));
            var sections = new List<EdgeSection>();
            var chains = new ColliderChainSet();
            EdgeSections.Build(path, 0f, path.Length, true, 45f, 135f, 0.5f, sections);

            SurfaceColliderBuilder.ClosedSections(path, sections, 0f, default, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), new List<Vector2>(), chains);

            Assert.AreEqual(4, chains.Count);
            Assert.AreEqual(1, chains.CountOf(EdgeKind.Top));
            Assert.AreEqual(2, chains.CountOf(EdgeKind.Wall));
            Assert.AreEqual(1, chains.CountOf(EdgeKind.Ceiling));

            for (int i = 0; i < chains.Count; i++)
            {
                Assert.IsTrue(chains.TryGetAdjacent(i, true, out _));
                Assert.IsTrue(chains.TryGetAdjacent(i, false, out _));
            }
        }

        [Test]
        public void ClosedSections_WallOffset_MovesOnlyWalls()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 2f), new Vector2(0f, 2f));
            var sections = new List<EdgeSection>();
            var chains = new ColliderChainSet();
            var points = new List<Vector2>();
            EdgeSections.Build(path, 0f, path.Length, true, 45f, 135f, 0.5f, sections);

            SurfaceColliderBuilder.ClosedSections(path, sections, 0f, new ColliderOffsets(0f, 0.5f, 0f), SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), new List<Vector2>(), chains);

            for (int i = 0; i < chains.Count; i++)
            {
                chains.CopyPoints(i, points);
                float expected = chains[i].Kind == EdgeKind.Wall ? 0.5f : 0f;

                foreach (Vector2 point in points)
                {
                    float outside = Mathf.Max(-point.x, point.x - 4f, -point.y, point.y - 2f);
                    Assert.AreEqual(expected, outside, Tolerance);
                }
            }
        }

        [Test]
        public void ClosedSections_BaseOffset_MovesOnlyTop()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 2f), new Vector2(0f, 2f));
            var sections = new List<EdgeSection>();
            var chains = new ColliderChainSet();
            var points = new List<Vector2>();
            EdgeSections.Build(path, 0f, path.Length, true, 45f, 135f, 0.5f, sections);

            SurfaceColliderBuilder.ClosedSections(path, sections, -0.2f, default, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), new List<Vector2>(), chains);

            bool topLowered = false;

            for (int i = 0; i < chains.Count; i++)
            {
                chains.CopyPoints(i, points);

                foreach (Vector2 point in points)
                {
                    if (chains[i].Kind == EdgeKind.Top)
                        topLowered |= Mathf.Abs(point.y - 1.8f) < Tolerance;
                    else if (chains[i].Kind == EdgeKind.Wall)
                        Assert.AreEqual(0f, Mathf.Min(Mathf.Abs(point.x), Mathf.Abs(point.x - 4f)), Tolerance);
                    else
                        Assert.AreEqual(0f, point.y, Tolerance);
                }
            }

            Assert.IsTrue(topLowered);
        }

        [Test]
        public void ClosedSections_SingleSection_IsClosedLoop()
        {
            SurfacePath path = Build(true, SurfaceNodeType.Corner, new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 2f), new Vector2(0f, 2f));
            var sections = new List<EdgeSection> { new EdgeSection(0f, path.Length, EdgeKind.Top) };
            var chains = new ColliderChainSet();
            var points = new List<Vector2>();

            SurfaceColliderBuilder.ClosedSections(path, sections, 0f, default, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), new List<Vector2>(), chains);
            chains.CopyPoints(0, points);

            Assert.AreEqual(1, chains.Count);
            AssertVector(points[0], points[points.Count - 1]);
        }

        [Test]
        public void AppendLine_InwardOffsetUnderSharpValley_ReachesMiterCorner()
        {
            const float thickness = 1.2f;
            SurfacePath path = Build(false, SurfaceNodeType.Corner, new Vector2(-4f, 4f), new Vector2(0f, 0f), new Vector2(4f, 4f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, -thickness, SurfaceColliderShape.ThroughNodes, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(3, points.Count);
            AssertVector(new Vector2(0f, -thickness * Mathf.Sqrt(2f)), points[1]);
        }

        [Test]
        public void AppendLine_InwardOffsetUnderStep_KeepsSquareCorner()
        {
            const float thickness = 1f;
            SurfacePath path = Build(false, SurfaceNodeType.Corner, new Vector2(-5f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(5f, 2f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, -thickness, SurfaceColliderShape.ThroughNodes, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.IsTrue(points.Exists(point => (point - new Vector2(thickness, -thickness)).sqrMagnitude < Tolerance));
        }

        [Test]
        public void AppendLine_InwardOffsetUnderSharpPeak_MeetsAtOnePoint()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Corner, new Vector2(-4f, 0f), new Vector2(0f, 4f), new Vector2(4f, 0f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, -1f, SurfaceColliderShape.ThroughNodes, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.AreEqual(3, points.Count);
            Assert.IsFalse(CrossesItself(points));
            AssertVector(new Vector2(0f, 4f - Mathf.Sqrt(2f)), points[1]);
        }

        [Test]
        public void AppendLine_InwardOffsetUnderTightCurve_DoesNotCrossItself()
        {
            SurfacePath path = Build(false, SurfaceNodeType.Smooth, new Vector2(-3f, 0f), new Vector2(-1.5f, 1f), new Vector2(0f, -0.5f), new Vector2(1.5f, 1f), new Vector2(3f, 0f));
            var points = new List<Vector2>();

            SurfaceColliderBuilder.AppendLine(path, 0f, path.Length, -1.2f, SurfaceColliderShape.Simplified, ColliderTolerance, new PolylineSimplifier(), points);

            Assert.IsFalse(CrossesItself(points));
        }

        [Test]
        public void TrimCrossing_SideHookingIntoBottom_CutsBothAtCrossing()
        {
            var side = new List<Vector2> { new Vector2(0f, 0f), new Vector2(0f, -2f), new Vector2(-2f, -2f) };
            var bottom = new List<Vector2> { new Vector2(-2f, -2f), new Vector2(1f, -1f), new Vector2(-4f, -1f) };

            SurfaceColliderBuilder.TrimCrossing(side, bottom);

            var joined = new List<Vector2>(side);
            joined.AddRange(bottom.GetRange(1, bottom.Count - 1));
            Assert.IsFalse(CrossesItself(joined));
            AssertVector(side[side.Count - 1], bottom[0]);
        }

        private static bool CrossesItself(List<Vector2> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 2; j < points.Count - 1; j++)
                {
                    Vector2 first = points[i + 1] - points[i];
                    Vector2 second = points[j + 1] - points[j];
                    float denominator = first.x * second.y - first.y * second.x;

                    if (Mathf.Abs(denominator) < 0.0000001f)
                        continue;

                    Vector2 offset = points[j] - points[i];
                    float t = (offset.x * second.y - offset.y * second.x) / denominator;
                    float u = (offset.x * first.y - offset.y * first.x) / denominator;

                    if (t > 0f && t < 1f && u > 0f && u < 1f)
                        return true;
                }
            }

            return false;
        }

        private static Vector2[] Wave() => new[] { new Vector2(0f, 0f), new Vector2(2f, 1f), new Vector2(4f, 0f), new Vector2(6f, 1f) };

        private static float DistanceToPolyline(Vector2 point, List<Vector2> polyline)
        {
            float best = float.MaxValue;

            for (int i = 0; i + 1 < polyline.Count; i++)
                best = Mathf.Min(best, PolylineSimplifier.SquaredDistanceToSegment(point, polyline[i], polyline[i + 1]));

            return Mathf.Sqrt(best);
        }

        private static SurfacePath Build(bool closed, SurfaceNodeType type, params Vector2[] positions)
        {
            var nodes = new List<SurfaceNode>();

            foreach (Vector2 position in positions)
                nodes.Add(new SurfaceNode(position, type));

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
