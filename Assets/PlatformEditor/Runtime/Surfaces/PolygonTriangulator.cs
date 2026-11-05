using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public static class PolygonTriangulator
    {
        public static void Triangulate(IReadOnlyList<Vector2> points, int firstVertex, List<int> triangles, List<int> scratch)
        {
            int count = points.Count;

            if (count < 3)
                return;

            scratch.Clear();
            bool counterClockwise = SignedArea(points) > 0f;

            for (int i = 0; i < count; i++)
                scratch.Add(counterClockwise ? i : count - 1 - i);

            int cursor = 0;

            while (scratch.Count > 3)
            {
                bool clipped = false;

                for (int step = 0; step < scratch.Count; step++)
                {
                    int i = (cursor + step) % scratch.Count;
                    int previous = scratch[(i + scratch.Count - 1) % scratch.Count];
                    int current = scratch[i];
                    int next = scratch[(i + 1) % scratch.Count];

                    if (!IsEar(points, scratch, previous, current, next))
                        continue;

                    AddTriangle(triangles, firstVertex, previous, current, next);
                    scratch.RemoveAt(i);
                    cursor = Mathf.Max(0, i - 1);
                    clipped = true;
                    break;
                }

                if (!clipped)
                    break;
            }

            for (int i = 1; i < scratch.Count - 1; i++)
                AddTriangle(triangles, firstVertex, scratch[0], scratch[i], scratch[i + 1]);
        }

        public static float SignedArea(IReadOnlyList<Vector2> points)
        {
            float doubled = 0f;
            int count = points.Count;

            for (int i = 0; i < count; i++)
                doubled += Cross(points[i], points[(i + 1) % count]);

            return doubled * 0.5f;
        }

        private static bool IsEar(IReadOnlyList<Vector2> points, List<int> polygon, int previous, int current, int next)
        {
            Vector2 a = points[previous];
            Vector2 b = points[current];
            Vector2 c = points[next];

            if (Cross(b - a, c - b) <= 0f)
                return false;

            for (int i = 0; i < polygon.Count; i++)
            {
                int index = polygon[i];

                if (index == previous || index == current || index == next)
                    continue;

                Vector2 point = points[index];

                if (point == a || point == b || point == c)
                    continue;

                if (InTriangle(point, a, b, c))
                    return false;
            }

            return true;
        }

        private static void AddTriangle(List<int> triangles, int firstVertex, int a, int b, int c)
        {
            triangles.Add(firstVertex + a);
            triangles.Add(firstVertex + b);
            triangles.Add(firstVertex + c);
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static bool InTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) =>
            Cross(b - a, point - a) >= 0f && Cross(c - b, point - b) >= 0f && Cross(a - c, point - c) >= 0f;
    }
}
