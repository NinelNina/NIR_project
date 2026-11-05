using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public readonly struct ColliderChain
    {
        public ColliderChain(EdgeKind kind, int start, int count)
        {
            Kind = kind;
            Start = start;
            Count = count;
        }

        public EdgeKind Kind { get; }

        public int Start { get; }

        public int Count { get; }
    }

    public readonly struct ColliderOffsets
    {
        public ColliderOffsets(float top, float wall, float ceiling)
        {
            Top = top;
            Wall = wall;
            Ceiling = ceiling;
        }

        public float Top { get; }

        public float Wall { get; }

        public float Ceiling { get; }

        public float For(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall:
                    return Wall;
                case EdgeKind.Ceiling:
                    return Ceiling;
                default:
                    return Top;
            }
        }
    }

    public sealed class ColliderChainSet
    {
        private const float JoinEpsilon = 0.000001f;
        private const int MinChainPoints = 2;

        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<ColliderChain> _chains = new List<ColliderChain>();

        public int Count => _chains.Count;

        public ColliderChain this[int index] => _chains[index];

        public void Clear()
        {
            _points.Clear();
            _chains.Clear();
        }

        public void Add(EdgeKind kind, List<Vector2> source)
        {
            int start = _points.Count;

            for (int i = 0; i < source.Count; i++)
            {
                if (_points.Count > start && (_points[_points.Count - 1] - source[i]).sqrMagnitude <= JoinEpsilon)
                    continue;

                _points.Add(source[i]);
            }

            int count = _points.Count - start;

            if (count < MinChainPoints)
            {
                _points.RemoveRange(start, count);
                return;
            }

            _chains.Add(new ColliderChain(kind, start, count));
        }

        public int IndexOf(EdgeKind kind)
        {
            for (int i = 0; i < _chains.Count; i++)
            {
                if (_chains[i].Kind == kind)
                    return i;
            }

            return -1;
        }

        public int CountOf(EdgeKind kind)
        {
            int count = 0;

            for (int i = 0; i < _chains.Count; i++)
            {
                if (_chains[i].Kind == kind)
                    count++;
            }

            return count;
        }

        public void CopyPoints(int chain, List<Vector2> target)
        {
            target.Clear();
            ColliderChain range = _chains[chain];

            for (int i = 0; i < range.Count; i++)
                target.Add(_points[range.Start + i]);
        }

        public bool TryGetAdjacent(int chain, bool atStart, out Vector2 point)
        {
            ColliderChain range = _chains[chain];
            Vector2 joint = atStart ? First(range) : Last(range);

            for (int other = 0; other < _chains.Count; other++)
            {
                ColliderChain candidate = _chains[other];

                if (other == chain && !IsClosed(range))
                    continue;

                Vector2 candidateJoint = atStart ? Last(candidate) : First(candidate);

                if ((candidateJoint - joint).sqrMagnitude > JoinEpsilon)
                    continue;

                point = atStart ? _points[candidate.Start + candidate.Count - 2] : _points[candidate.Start + 1];
                return true;
            }

            point = default;
            return false;
        }

        private bool IsClosed(in ColliderChain range) => range.Count > MinChainPoints && (First(range) - Last(range)).sqrMagnitude <= JoinEpsilon;

        private Vector2 First(in ColliderChain range) => _points[range.Start];

        private Vector2 Last(in ColliderChain range) => _points[range.Start + range.Count - 1];
    }
}
