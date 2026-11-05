using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public enum EdgeKind
    {
        Top,
        Wall,
        Ceiling
    }

    public readonly struct EdgeSection
    {
        public EdgeSection(float start, float end, EdgeKind kind)
        {
            Start = start;
            End = end;
            Kind = kind;
        }

        public float Start { get; }

        public float End { get; }

        public EdgeKind Kind { get; }

        public float Length => End - Start;
    }

    public static class EdgeSections
    {
        public static EdgeKind Classify(Vector2 normal, float wallAngle, float ceilingAngle)
        {
            float angle = Vector2.Angle(normal, Vector2.up);

            if (angle < wallAngle)
                return EdgeKind.Top;

            return angle > ceilingAngle ? EdgeKind.Ceiling : EdgeKind.Wall;
        }

        public static void Build(SurfacePath path, float start, float end, bool loop, float wallAngle, float ceilingAngle, float minLength, List<EdgeSection> sections)
        {
            sections.Clear();

            int count = path.SampleCount;

            if (count < 2 || end <= start)
                return;

            float length = path.Length;
            int chords = path.IsClosed ? count : count - 1;
            int copies = path.IsClosed ? 2 : 1;

            for (int copy = 0; copy < copies; copy++)
            {
                float offset = copy * length;

                for (int i = 0; i < chords; i++)
                {
                    int next = (i + 1) % count;
                    float from = Mathf.Max(path.Distances[i] + offset, start);
                    float to = Mathf.Min((next == 0 ? length : path.Distances[next]) + offset, end);
                    Vector2 chord = path.Points[next] - path.Points[i];

                    if (to <= from || chord.sqrMagnitude <= 0f)
                        continue;

                    Vector2 normal = new Vector2(-chord.y, chord.x).normalized;
                    Append(sections, from, to, Classify(normal, wallAngle, ceilingAngle));
                }
            }

            if (loop)
                MergeWrap(sections, length);

            MergeShort(sections, minLength);

            if (loop)
                MergeWrap(sections, length);
        }

        private static void Append(List<EdgeSection> sections, float from, float to, EdgeKind kind)
        {
            int last = sections.Count - 1;

            if (last >= 0 && sections[last].Kind == kind)
            {
                sections[last] = new EdgeSection(sections[last].Start, to, kind);
                return;
            }

            sections.Add(new EdgeSection(last >= 0 ? sections[last].End : from, to, kind));
        }

        private static void MergeShort(List<EdgeSection> sections, float minLength)
        {
            while (sections.Count > 1)
            {
                int shortest = -1;
                float shortestLength = minLength;

                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Length < shortestLength)
                    {
                        shortest = i;
                        shortestLength = sections[i].Length;
                    }
                }

                if (shortest < 0)
                    return;

                int neighbour = Neighbour(sections, shortest);
                EdgeSection kept = sections[neighbour];
                EdgeSection merged = sections[shortest];
                sections[neighbour] = new EdgeSection(Mathf.Min(kept.Start, merged.Start), Mathf.Max(kept.End, merged.End), kept.Kind);
                sections.RemoveAt(shortest);
                Coalesce(sections);
            }
        }

        private static int Neighbour(List<EdgeSection> sections, int index)
        {
            if (index == 0)
                return 1;

            if (index == sections.Count - 1)
                return index - 1;

            return sections[index - 1].Length >= sections[index + 1].Length ? index - 1 : index + 1;
        }

        private static void Coalesce(List<EdgeSection> sections)
        {
            for (int i = sections.Count - 1; i > 0; i--)
            {
                if (sections[i].Kind != sections[i - 1].Kind)
                    continue;

                sections[i - 1] = new EdgeSection(sections[i - 1].Start, sections[i].End, sections[i].Kind);
                sections.RemoveAt(i);
            }
        }

        private static void MergeWrap(List<EdgeSection> sections, float length)
        {
            int last = sections.Count - 1;

            if (last < 1 || sections[0].Kind != sections[last].Kind)
                return;

            sections[last] = new EdgeSection(sections[last].Start, sections[0].End + length, sections[last].Kind);
            sections.RemoveAt(0);
        }
    }
}
