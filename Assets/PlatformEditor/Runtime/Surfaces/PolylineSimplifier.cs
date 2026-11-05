using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public sealed class PolylineSimplifier
    {
        private readonly List<int> _stack = new List<int>();
        private readonly List<bool> _keep = new List<bool>();
        private readonly List<Vector2> _buffer = new List<Vector2>();

        public void Simplify(List<Vector2> points, int start, float tolerance)
        {
            int count = points.Count - start;

            if (count < 3 || tolerance <= 0f)
                return;

            _keep.Clear();

            for (int i = 0; i < count; i++)
                _keep.Add(false);

            _keep[0] = true;
            _keep[count - 1] = true;
            _stack.Clear();
            _stack.Add(0);
            _stack.Add(count - 1);

            float squaredTolerance = tolerance * tolerance;

            while (_stack.Count > 0)
            {
                int last = _stack[_stack.Count - 1];
                int first = _stack[_stack.Count - 2];
                _stack.RemoveRange(_stack.Count - 2, 2);

                float farthest = squaredTolerance;
                int farthestIndex = -1;

                for (int i = first + 1; i < last; i++)
                {
                    float distance = SquaredDistanceToSegment(points[start + i], points[start + first], points[start + last]);

                    if (distance <= farthest)
                        continue;

                    farthest = distance;
                    farthestIndex = i;
                }

                if (farthestIndex < 0)
                    continue;

                _keep[farthestIndex] = true;
                _stack.Add(first);
                _stack.Add(farthestIndex);
                _stack.Add(farthestIndex);
                _stack.Add(last);
            }

            _buffer.Clear();

            for (int i = 0; i < count; i++)
            {
                if (_keep[i])
                    _buffer.Add(points[start + i]);
            }

            points.RemoveRange(start, count);
            points.AddRange(_buffer);
        }

        public static float SquaredDistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 segment = b - a;
            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared <= 0f)
                return (point - a).sqrMagnitude;

            float t = Mathf.Clamp01(Vector2.Dot(point - a, segment) / lengthSquared);
            return (point - (a + segment * t)).sqrMagnitude;
        }
    }
}
