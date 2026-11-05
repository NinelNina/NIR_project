using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public enum SurfaceColliderShape
    {
        Simplified,
        ThroughNodes
    }

    public static class SurfaceColliderBuilder
    {
        private const float ParallelEpsilon = 0.0000001f;

        public static void Closed(SurfacePath path, float offset, SurfaceColliderShape shape, float tolerance, PolylineSimplifier simplifier, List<Vector2> points)
        {
            points.Clear();

            if (!path.IsValid)
                return;

            for (int i = 0; i < path.SampleCount; i++)
            {
                if (shape == SurfaceColliderShape.Simplified || path.IsNodeSample(i))
                    points.Add(path.Points[i] + path.MiterNormal(i) * offset);
            }

            points.Add(points[0]);

            if (shape == SurfaceColliderShape.Simplified)
                simplifier.Simplify(points, 0, tolerance);
        }

        public static void ClosedSections(
            SurfacePath path,
            IReadOnlyList<EdgeSection> sections,
            float baseOffset,
            in ColliderOffsets offsets,
            SurfaceColliderShape shape,
            float tolerance,
            PolylineSimplifier simplifier,
            List<Vector2> scratch,
            ColliderChainSet chains)
        {
            chains.Clear();

            if (!path.IsValid)
                return;

            if (sections.Count <= 1)
            {
                EdgeKind kind = sections.Count == 1 ? sections[0].Kind : EdgeKind.Top;
                Closed(path, (kind == EdgeKind.Top ? baseOffset : 0f) + offsets.For(kind), shape, tolerance, simplifier, scratch);
                chains.Add(kind, scratch);
                return;
            }

            float length = path.Length;

            for (int i = 0; i < sections.Count; i++)
            {
                EdgeSection section = sections[i];
                float offset = (section.Kind == EdgeKind.Top ? baseOffset : 0f) + offsets.For(section.Kind);
                scratch.Clear();

                if (section.End <= length)
                {
                    AppendLine(path, section.Start, section.End, offset, shape, tolerance, simplifier, scratch);
                }
                else
                {
                    AppendLine(path, section.Start, length, offset, shape, tolerance, simplifier, scratch);
                    AppendLine(path, 0f, section.End - length, offset, shape, tolerance, simplifier, scratch);
                }

                chains.Add(section.Kind, scratch);
            }
        }

        public static void AppendLine(
            SurfacePath path,
            float start,
            float end,
            float offset,
            SurfaceColliderShape shape,
            float tolerance,
            PolylineSimplifier simplifier,
            List<Vector2> points)
        {
            if (!path.IsValid)
                return;

            int first = points.Count;
            AddEvaluated(path, start, offset, points);

            for (int i = 0; i < path.SampleCount; i++)
            {
                float distance = path.Distances[i];

                if (distance <= start || distance >= end)
                    continue;

                if (shape == SurfaceColliderShape.Simplified || path.IsNodeSample(i))
                    points.Add(path.Points[i] + path.MiterNormal(i) * offset);
            }

            AddEvaluated(path, end, offset, points);
            RemoveLoops(points, first);

            if (shape == SurfaceColliderShape.Simplified)
                simplifier.Simplify(points, first, tolerance);
        }

        public static void TrimCrossing(List<Vector2> leading, List<Vector2> trailing)
        {
            for (int i = 0; i < leading.Count - 1; i++)
            {
                for (int j = trailing.Count - 2; j >= 0; j--)
                {
                    if (i == leading.Count - 2 && j == 0)
                        continue;

                    if (!TryIntersect(leading[i], leading[i + 1], trailing[j], trailing[j + 1], out Vector2 crossing))
                        continue;

                    leading.RemoveRange(i + 1, leading.Count - i - 1);
                    leading.Add(crossing);
                    trailing.RemoveRange(0, j + 1);
                    trailing.Insert(0, crossing);
                    return;
                }
            }
        }

        public static void RemoveLoops(List<Vector2> points, int first)
        {
            for (int i = first; i < points.Count - 3; i++)
            {
                for (int j = points.Count - 2; j > i + 1; j--)
                {
                    if (!TryIntersect(points[i], points[i + 1], points[j], points[j + 1], out Vector2 crossing))
                        continue;

                    points.RemoveRange(i + 1, j - i);
                    points.Insert(i + 1, crossing);
                    break;
                }
            }
        }

        private static bool TryIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 crossing)
        {
            crossing = default;
            Vector2 first = b - a;
            Vector2 second = d - c;
            float denominator = first.x * second.y - first.y * second.x;

            if (Mathf.Abs(denominator) < ParallelEpsilon)
                return false;

            Vector2 offset = c - a;
            float t = (offset.x * second.y - offset.y * second.x) / denominator;
            float u = (offset.x * first.y - offset.y * first.x) / denominator;

            if (t <= 0f || t >= 1f || u <= 0f || u >= 1f)
                return false;

            crossing = a + first * t;
            return true;
        }

        private static void AddEvaluated(SurfacePath path, float distance, float offset, List<Vector2> points)
        {
            path.Evaluate(distance, out Vector2 point, out Vector2 normal);
            points.Add(point + normal * offset);
        }
    }
}
