using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public static class SurfaceOutline
    {
        private const float MaxChamferFraction = 0.45f;

        public static void ClosedFill(SurfacePath path, float inset, List<Vector2> outline)
        {
            outline.Clear();

            if (!path.IsValid)
                return;

            for (int i = 0; i < path.SampleCount; i++)
                outline.Add(path.Points[i] - path.MiterNormal(i) * inset);
        }

        public static void ClosedFill(SurfacePath path, float inset, IReadOnlyList<bool> chamferSamples, float chamfer, List<Vector2> outline)
        {
            outline.Clear();

            if (!path.IsValid)
                return;

            int count = path.SampleCount;

            for (int i = 0; i < count; i++)
            {
                Vector2 point = path.Points[i] - path.MiterNormal(i) * inset;

                if (chamfer <= 0f || i >= chamferSamples.Count || !chamferSamples[i])
                {
                    outline.Add(point);
                    continue;
                }

                Vector2 previous = path.Points[(i - 1 + count) % count];
                Vector2 next = path.Points[(i + 1) % count];
                Vector2 toPrevious = previous - path.Points[i];
                Vector2 toNext = next - path.Points[i];
                float cut = Mathf.Min(chamfer, toPrevious.magnitude * MaxChamferFraction, toNext.magnitude * MaxChamferFraction);
                outline.Add(point + toPrevious.normalized * cut);
                outline.Add(point + toNext.normalized * cut);
            }
        }

        public static bool IsConvex(SurfacePath path, int sample)
        {
            int count = path.SampleCount;
            Vector2 point = path.Points[sample];
            Vector2 incoming = point - path.Points[(sample - 1 + count) % count];
            Vector2 outgoing = path.Points[(sample + 1) % count] - point;
            Vector2 incomingNormal = new Vector2(-incoming.y, incoming.x);
            return Vector2.Dot(outgoing, incomingNormal) < 0f;
        }

        public static void OpenFill(SurfacePath path, float inset, float baseY, List<Vector2> outline)
        {
            outline.Clear();

            if (!path.IsValid)
                return;

            for (int i = 0; i < path.SampleCount; i++)
                outline.Add(path.Points[i] - path.MiterNormal(i) * inset);

            outline.Add(new Vector2(outline[outline.Count - 1].x, baseY));
            outline.Add(new Vector2(outline[0].x, baseY));
        }
    }
}
