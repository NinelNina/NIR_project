using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public readonly struct TightBend
    {
        public TightBend(float start, float end, float radius, float requiredRadius, Vector2 point, bool valley)
        {
            Start = start;
            End = end;
            Radius = radius;
            RequiredRadius = requiredRadius;
            Point = point;
            Valley = valley;
        }

        public float Start { get; }

        public float End { get; }

        public float Radius { get; }

        public float RequiredRadius { get; }

        public Vector2 Point { get; }

        public bool Valley { get; }
    }

    public static class TightBendFinder
    {
        private const float MinTurn = 0.0000001f;

        public static void Find(SurfacePath path, float depthBelowSurface, float heightAboveSurface, List<TightBend> bends)
        {
            bends.Clear();

            if (!path.IsValid)
                return;

            int count = path.SampleCount;
            bool open = false;
            bool openValley = false;
            float openStart = 0f;
            float openRadius = float.MaxValue;
            Vector2 openPoint = default;

            for (int i = 0; i < count; i++)
            {
                bool tight = TryMeasure(path, i, depthBelowSurface, heightAboveSurface, out float radius, out bool valley);

                if (open && (!tight || valley != openValley))
                {
                    bends.Add(new TightBend(openStart, Distance(path, i), openRadius, Required(openValley, depthBelowSurface, heightAboveSurface), openPoint, openValley));
                    open = false;
                }

                if (!tight)
                    continue;

                if (!open)
                {
                    open = true;
                    openValley = valley;
                    openStart = Distance(path, i - 1);
                    openRadius = float.MaxValue;
                }

                if (radius < openRadius)
                {
                    openRadius = radius;
                    openPoint = path.Points[i];
                }
            }

            if (open)
                bends.Add(new TightBend(openStart, path.Length, openRadius, Required(openValley, depthBelowSurface, heightAboveSurface), openPoint, openValley));
        }

        private static bool TryMeasure(SurfacePath path, int sample, float depthBelowSurface, float heightAboveSurface, out float radius, out bool valley)
        {
            radius = float.MaxValue;
            valley = false;

            int count = path.SampleCount;
            bool hasPrevious = path.IsClosed || sample > 0;
            bool hasNext = path.IsClosed || sample < count - 1;

            if (!hasPrevious || !hasNext || path.IsSharpSample(sample))
                return false;

            Vector2 previous = path.Points[(sample - 1 + count) % count];
            Vector2 current = path.Points[sample];
            Vector2 next = path.Points[(sample + 1) % count];
            Vector2 incoming = current - previous;
            Vector2 outgoing = next - current;
            float turn = incoming.x * outgoing.y - incoming.y * outgoing.x;

            if (Mathf.Abs(turn) < MinTurn)
                return false;

            radius = incoming.magnitude * outgoing.magnitude * (next - previous).magnitude / (2f * Mathf.Abs(turn));
            valley = turn > 0f;
            return radius < Required(valley, depthBelowSurface, heightAboveSurface);
        }

        private static float Required(bool valley, float depthBelowSurface, float heightAboveSurface) => valley ? heightAboveSurface : depthBelowSurface;

        private static float Distance(SurfacePath path, int sample)
        {
            int count = path.SampleCount;

            if (sample < 0)
                return path.IsClosed ? path.Distances[count - 1] : 0f;

            return sample < count ? path.Distances[sample] : path.Length;
        }
    }
}
