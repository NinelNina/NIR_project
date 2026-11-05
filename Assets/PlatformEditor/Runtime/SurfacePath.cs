using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public sealed class SurfacePath
    {
        public const int MinOpenNodes = 2;
        public const int MinClosedNodes = 3;

        private const int DefaultSamplesPerSegment = 16;
        private const float HandleEpsilon = 0.000001f;
        private const float DirectionEpsilon = 0.0000001f;
        private const float ParallelEpsilon = 0.0000001f;
        private const float OneThird = 1f / 3f;
        private const float SharpAngleDegrees = 20f;
        private const float MinMiterCosine = 0.25f;

        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<Vector2> _tangents = new List<Vector2>();
        private readonly List<Vector2> _normals = new List<Vector2>();
        private readonly List<bool> _sharp = new List<bool>();
        private readonly List<bool> _nodeSamples = new List<bool>();
        private readonly List<float> _distances = new List<float>();
        private readonly List<int> _nodeSampleIndices = new List<int>();
        private readonly int _samplesPerSegment;

        private bool _closed;
        private int _nodeCount;
        private float _length;
        private float _signedArea;
        private bool _reversed;

        public SurfacePath(int samplesPerSegment = DefaultSamplesPerSegment)
        {
            _samplesPerSegment = Mathf.Max(1, samplesPerSegment);
        }

        public bool IsValid => _points.Count >= MinOpenNodes;

        public bool IsClosed => _closed;

        public int NodeCount => _nodeCount;

        public int SegmentCount => _nodeCount == 0 ? 0 : _closed ? _nodeCount : _nodeCount - 1;

        public float Length => _length;

        public float SignedArea => _signedArea;

        public bool IsCounterClockwise => _signedArea > 0f;

        public int SampleCount => _points.Count;

        public IReadOnlyList<Vector2> Points => _points;

        public IReadOnlyList<Vector2> Tangents => _tangents;

        public IReadOnlyList<Vector2> Normals => _normals;

        public IReadOnlyList<float> Distances => _distances;

        public int NodeSampleIndex(int node) => _nodeSampleIndices[node];

        public bool IsSharpSample(int sample) => _sharp[sample];

        public bool IsReversed => _reversed;

        public bool IsNodeSample(int sample) => _nodeSamples[sample];

        public void Build(IReadOnlyList<SurfaceNode> nodes, bool closed, bool flipped = false)
        {
            Clear();
            _closed = closed;

            int count = nodes == null ? 0 : nodes.Count;

            if (count < (closed ? MinClosedNodes : MinOpenNodes))
                return;

            _nodeCount = count;
            _points.Add(nodes[0].Position);
            _nodeSampleIndices.Add(0);

            int segmentCount = SegmentCount;

            for (int segment = 0; segment < segmentCount; segment++)
            {
                int to = (segment + 1) % count;
                Vector2 start = nodes[segment].Position;
                Vector2 end = nodes[to].Position;
                Vector2 startControl = start + OutHandle(nodes, segment);
                Vector2 endControl = end + InHandle(nodes, to);
                bool straight = (startControl - start).sqrMagnitude < HandleEpsilon && (endControl - end).sqrMagnitude < HandleEpsilon;
                int steps = straight ? 1 : _samplesPerSegment;
                bool closing = closed && to == 0;
                int lastStep = closing ? steps - 1 : steps;

                for (int step = 1; step <= lastStep; step++)
                    _points.Add(CubicBezier(start, startControl, endControl, end, step / (float)steps));

                if (!closing)
                    _nodeSampleIndices.Add(_points.Count - 1);
            }

            bool outwardOnRight = closed && ComputeSignedArea() > 0f;

            if (outwardOnRight != flipped)
                ReverseSamples();

            MarkNodeSamples();
            ComputeDistances();
            _signedArea = closed ? ComputeSignedArea() : 0f;
            ComputeFrames();
        }

        public void SegmentRange(int segment, out float start, out float end)
        {
            float from = _distances[_nodeSampleIndices[segment]];
            float to = _distances[_nodeSampleIndices[(segment + 1) % _nodeCount]];

            start = _reversed ? to : from;
            end = _reversed ? from : to;

            if (_closed && end <= start)
                end += _length;
        }

        public void Evaluate(float distance, out Vector2 point, out Vector2 normal)
        {
            int count = _points.Count;

            if (count < MinOpenNodes)
            {
                point = count > 0 ? _points[0] : Vector2.zero;
                normal = Vector2.up;
                return;
            }

            if (_closed)
            {
                distance = Mathf.Repeat(distance, _length);
            }
            else
            {
                float end = _distances[count - 1];

                if (distance <= 0f)
                {
                    point = _points[0] + _tangents[0] * distance;
                    normal = _normals[0];
                    return;
                }

                if (distance >= end)
                {
                    point = _points[count - 1] + _tangents[count - 1] * (distance - end);
                    normal = _normals[count - 1];
                    return;
                }
            }

            int index = FindChord(distance);
            int next = (index + 1) % count;
            float chordStart = _distances[index];
            float chordEnd = index + 1 < count ? _distances[index + 1] : _length;
            float t = chordEnd > chordStart ? (distance - chordStart) / (chordEnd - chordStart) : 0f;
            Vector2 chordNormal = ChordNormal(index, next);
            Vector2 startNormal = _sharp[index] ? chordNormal : _normals[index];
            Vector2 endNormal = _sharp[next] ? chordNormal : _normals[next];
            Vector2 blended = Vector2.Lerp(startNormal, endNormal, t);

            point = Vector2.Lerp(_points[index], _points[next], t);
            normal = blended.sqrMagnitude > DirectionEpsilon ? blended.normalized : chordNormal;
        }

        public Vector2 MiterNormal(int sample)
        {
            int count = _points.Count;
            bool hasPrevious = _closed || sample > 0;
            bool hasNext = _closed || sample < count - 1;

            if (!hasPrevious || !hasNext)
                return _normals[sample];

            Vector2 incoming = ChordNormal((sample - 1 + count) % count, sample);
            Vector2 outgoing = ChordNormal(sample, (sample + 1) % count);
            Vector2 sum = incoming + outgoing;

            if (sum.sqrMagnitude < DirectionEpsilon)
                return _normals[sample];

            Vector2 direction = sum.normalized;
            return direction / Mathf.Max(Vector2.Dot(direction, incoming), MinMiterCosine);
        }

        public bool TryFindSelfIntersection(out Vector2 point)
        {
            point = default;

            int count = _points.Count;
            int chordCount = _closed ? count : count - 1;

            for (int first = 0; first < chordCount; first++)
            {
                Vector2 firstStart = _points[first];
                Vector2 firstEnd = _points[(first + 1) % count];

                for (int second = first + 2; second < chordCount; second++)
                {
                    if (_closed && first == 0 && second == chordCount - 1)
                        continue;

                    if (TryIntersect(firstStart, firstEnd, _points[second], _points[(second + 1) % count], out point))
                        return true;
                }
            }

            return false;
        }

        private void Clear()
        {
            _points.Clear();
            _tangents.Clear();
            _normals.Clear();
            _sharp.Clear();
            _nodeSamples.Clear();
            _distances.Clear();
            _nodeSampleIndices.Clear();
            _nodeCount = 0;
            _length = 0f;
            _signedArea = 0f;
            _reversed = false;
        }

        private void MarkNodeSamples()
        {
            for (int i = 0; i < _points.Count; i++)
                _nodeSamples.Add(false);

            for (int node = 0; node < _nodeSampleIndices.Count; node++)
                _nodeSamples[_nodeSampleIndices[node]] = true;
        }

        private void ReverseSamples()
        {
            int count = _points.Count;
            _reversed = true;

            if (_closed)
                _points.Reverse(1, count - 1);
            else
                _points.Reverse();

            for (int node = 0; node < _nodeSampleIndices.Count; node++)
            {
                int sample = _nodeSampleIndices[node];
                _nodeSampleIndices[node] = _closed ? (count - sample) % count : count - 1 - sample;
            }
        }

        private Vector2 OutHandle(IReadOnlyList<SurfaceNode> nodes, int index)
        {
            SurfaceNode node = nodes[index];

            switch (node.Type)
            {
                case SurfaceNodeType.Bezier:
                    return node.OutTangent;
                case SurfaceNodeType.Smooth:
                    GetNeighbours(nodes, index, out Vector2 previous, out Vector2 next);
                    return SmoothOutHandle(previous, node.Position, next);
                default:
                    return Vector2.zero;
            }
        }

        private Vector2 InHandle(IReadOnlyList<SurfaceNode> nodes, int index)
        {
            SurfaceNode node = nodes[index];

            switch (node.Type)
            {
                case SurfaceNodeType.Bezier:
                    return node.InTangent;
                case SurfaceNodeType.Smooth:
                    GetNeighbours(nodes, index, out Vector2 previous, out Vector2 next);
                    return SmoothInHandle(previous, node.Position, next);
                default:
                    return Vector2.zero;
            }
        }

        private void GetNeighbours(IReadOnlyList<SurfaceNode> nodes, int index, out Vector2 previous, out Vector2 next)
        {
            int count = nodes.Count;
            Vector2 current = nodes[index].Position;
            bool hasPrevious = _closed || index > 0;
            bool hasNext = _closed || index < count - 1;

            next = hasNext ? nodes[(index + 1) % count].Position : current;
            previous = hasPrevious ? nodes[(index - 1 + count) % count].Position : current;

            if (!hasPrevious)
                previous = current * 2f - next;

            if (!hasNext)
                next = current * 2f - previous;
        }

        private static Vector2 SmoothOutHandle(Vector2 previous, Vector2 current, Vector2 next)
        {
            float before = Mathf.Sqrt(Vector2.Distance(previous, current));
            float after = Mathf.Sqrt(Vector2.Distance(current, next));

            if (before < HandleEpsilon || after < HandleEpsilon)
                return (next - current) * OneThird;

            return (before * before * (next - current) + after * after * (current - previous)) / (3f * before * (before + after));
        }

        private static Vector2 SmoothInHandle(Vector2 previous, Vector2 current, Vector2 next)
        {
            float before = Mathf.Sqrt(Vector2.Distance(previous, current));
            float after = Mathf.Sqrt(Vector2.Distance(current, next));

            if (before < HandleEpsilon || after < HandleEpsilon)
                return (previous - current) * OneThird;

            return (after * after * (previous - current) + before * before * (current - next)) / (3f * after * (before + after));
        }

        private static Vector2 CubicBezier(Vector2 start, Vector2 startControl, Vector2 endControl, Vector2 end, float t)
        {
            float u = 1f - t;
            return u * u * u * start + 3f * u * u * t * startControl + 3f * u * t * t * endControl + t * t * t * end;
        }

        private void ComputeDistances()
        {
            float distance = 0f;

            for (int i = 0; i < _points.Count; i++)
            {
                if (i > 0)
                    distance += Vector2.Distance(_points[i - 1], _points[i]);

                _distances.Add(distance);
            }

            _length = _closed ? distance + Vector2.Distance(_points[_points.Count - 1], _points[0]) : distance;
        }

        private float ComputeSignedArea()
        {
            float doubled = 0f;
            int count = _points.Count;

            for (int i = 0; i < count; i++)
                doubled += Cross(_points[i], _points[(i + 1) % count]);

            return doubled * 0.5f;
        }

        private void ComputeFrames()
        {
            int count = _points.Count;
            Vector2 previousTangent = Vector2.right;

            for (int i = 0; i < count; i++)
            {
                Vector2 current = _points[i];
                Vector2 before = i > 0 ? _points[i - 1] : _closed ? _points[count - 1] : current;
                Vector2 after = i < count - 1 ? _points[i + 1] : _closed ? _points[0] : current;
                Vector2 direction = after - before;
                Vector2 tangent = direction.sqrMagnitude > DirectionEpsilon ? direction.normalized : previousTangent;
                Vector2 incoming = current - before;
                Vector2 outgoing = after - current;
                bool sharp = incoming.sqrMagnitude > DirectionEpsilon
                    && outgoing.sqrMagnitude > DirectionEpsilon
                    && Vector2.Angle(incoming, outgoing) > SharpAngleDegrees;

                previousTangent = tangent;
                _tangents.Add(tangent);
                _normals.Add(new Vector2(-tangent.y, tangent.x));
                _sharp.Add(sharp);
            }
        }

        private int FindChord(float distance)
        {
            int low = 0;
            int high = _distances.Count - 1;

            while (low < high)
            {
                int middle = (low + high + 1) >> 1;

                if (_distances[middle] <= distance)
                    low = middle;
                else
                    high = middle - 1;
            }

            return low;
        }

        private Vector2 ChordNormal(int from, int to)
        {
            Vector2 direction = _points[to] - _points[from];

            if (direction.sqrMagnitude < DirectionEpsilon)
                return _normals[from];

            direction.Normalize();
            return new Vector2(-direction.y, direction.x);
        }

        private static bool TryIntersect(Vector2 firstStart, Vector2 firstEnd, Vector2 secondStart, Vector2 secondEnd, out Vector2 point)
        {
            point = default;

            Vector2 first = firstEnd - firstStart;
            Vector2 second = secondEnd - secondStart;
            float denominator = Cross(first, second);

            if (Mathf.Abs(denominator) < ParallelEpsilon)
                return false;

            Vector2 offset = secondStart - firstStart;
            float firstT = Cross(offset, second) / denominator;
            float secondT = Cross(offset, first) / denominator;

            if (firstT <= 0f || firstT >= 1f || secondT <= 0f || secondT >= 1f)
                return false;

            point = firstStart + first * firstT;
            return true;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
