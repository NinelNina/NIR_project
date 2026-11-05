using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public enum EdgeTileRole
    {
        Left,
        Middle,
        Right
    }

    public enum EdgeFitMode
    {
        WholeTiles,
        FitLength
    }

    public interface IEdgeTileMetrics
    {
        bool HasEnds { get; }

        int MiddleVariantCount { get; }

        float Width(EdgeTileRole role, int variant);

        float Overlap(EdgeTileRole leftRole, int leftVariant, EdgeTileRole rightRole, int rightVariant);
    }

    public readonly struct EdgeTile
    {
        public EdgeTile(EdgeTileRole role, int variant, float start, float width, float fitScale)
        {
            Role = role;
            Variant = variant;
            Start = start;
            Width = width;
            FitScale = fitScale;
        }

        public EdgeTileRole Role { get; }

        public int Variant { get; }

        public float Start { get; }

        public float Width { get; }

        public float FitScale { get; }

        public float Center => Start + Width * 0.5f;

        public float End => Start + Width;
    }

    public sealed class EdgeLayout
    {
        public const float MaxFitStretch = 0.15f;

        private const int MaxMiddleTiles = 4096;
        private const uint VariationHashPrime = 2654435761u;
        private const float FitEpsilon = 0.0001f;
        private const float MinExactScale = 0.05f;

        private readonly List<EdgeTile> _tiles = new List<EdgeTile>();
        private readonly List<int> _variants = new List<int>();

        private bool _leftEnd = true;
        private bool _rightEnd = true;

        public IReadOnlyList<EdgeTile> Tiles => _tiles;

        public float Start { get; private set; }

        public float CoveredLength { get; private set; }

        public void Build(IEdgeTileMetrics metrics, float start, float length, bool loop, EdgeFitMode mode, int seed, bool leftEnd = true, bool rightEnd = true)
        {
            _leftEnd = leftEnd;
            _rightEnd = rightEnd;

            _tiles.Clear();
            _variants.Clear();
            Start = start;
            CoveredLength = 0f;

            if (metrics == null || metrics.MiddleVariantCount == 0 || length <= 0f)
                return;

            bool withEnds = !loop && metrics.HasEnds && (leftEnd || rightEnd);
            bool fitting = mode == EdgeFitMode.FitLength || loop;
            int minimum = withEnds ? 0 : 1;
            int bestMiddles = minimum;
            bool bestEnds = withEnds;
            float bestError = float.MaxValue;

            for (int middles = minimum; middles <= MaxMiddleTiles; middles++)
            {
                float error = Arrange(metrics, withEnds, middles, loop, length, fitting, seed, start, out float shortest);

                if (error < bestError)
                {
                    bestError = error;
                    bestMiddles = middles;
                }

                if (shortest > length)
                    break;
            }

            if (withEnds && !fitting && Arrange(metrics, false, 1, false, length, false, seed, start, out _) < bestError)
            {
                bestEnds = false;
                bestMiddles = 1;
            }

            Arrange(metrics, bestEnds, bestMiddles, loop, length, fitting, seed, start, out _);
            float fit = fitting ? FitScale(metrics, bestEnds, bestMiddles, loop, length, start) : 1f;
            float endFit = 1f;

            if (fitting)
                FitExactly(metrics, bestEnds, bestMiddles, loop, length, start, ref fit, out endFit);

            CoveredLength = Walk(metrics, bestEnds, bestMiddles, loop, fit, start, true, endFit);
        }

        private void FitExactly(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float length, float start, ref float fit, out float endFit)
        {
            endFit = 1f;
            float middleSum = MiddleWidthSum(metrics, withEnds, middles) * fit;
            float total = middleSum + EndWidthSum(metrics, withEnds, middles);
            float covered = Walk(metrics, withEnds, middles, loop, fit, start, false);

            if (total <= 0f || Mathf.Abs(covered - length) <= FitEpsilon)
                return;

            float overlaps = total - covered;
            float scale = Mathf.Max(MinExactScale, (length + overlaps) / total);
            fit *= scale;
            endFit = scale;
        }

        private float EndWidthSum(IEdgeTileMetrics metrics, bool withEnds, int middles)
        {
            int total = middles + EndCount(withEnds);
            float sum = 0f;

            for (int index = 0; index < total; index++)
            {
                EdgeTileRole role = RoleAt(index, total, withEnds);

                if (role != EdgeTileRole.Middle)
                    sum += metrics.Width(role, _variants[index]);
            }

            return sum;
        }

        private float Arrange(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float length, bool fitting, int seed, float start, out float shortest)
        {
            AssignVariants(metrics, withEnds, middles, seed);
            float error = fitting
                ? ImproveFit(metrics, withEnds, middles, loop, length, start)
                : Mathf.Abs(Walk(metrics, withEnds, middles, loop, 1f, start, false) - length);
            float stretch = fitting ? MiddleWidthSum(metrics, withEnds, middles) * MaxFitStretch : 0f;
            shortest = Walk(metrics, withEnds, middles, loop, 1f, start, false) - stretch;
            return error;
        }

        private void AssignVariants(IEdgeTileMetrics metrics, bool withEnds, int middles, int seed)
        {
            _variants.Clear();
            int total = middles + EndCount(withEnds);
            int count = metrics.MiddleVariantCount;

            for (int index = 0; index < total; index++)
            {
                bool middle = RoleAt(index, total, withEnds) == EdgeTileRole.Middle;
                int variant = middle ? VariantAt(index, seed, count) : 0;

                if (middle && count > 1 && RepeatsNeighbour(index, variant, total, withEnds))
                    variant = (variant + 1) % count;

                _variants.Add(variant);
            }
        }

        private float ImproveFit(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float length, float start)
        {
            int total = _variants.Count;
            int count = metrics.MiddleVariantCount;
            float error = FitResidual(metrics, withEnds, middles, loop, length, start);

            while (error > FitEpsilon && count > 1)
            {
                int bestIndex = -1;
                int bestVariant = 0;
                float bestError = error;

                for (int index = 0; index < total; index++)
                {
                    if (RoleAt(index, total, withEnds) != EdgeTileRole.Middle)
                        continue;

                    int original = _variants[index];

                    for (int variant = 0; variant < count; variant++)
                    {
                        if (variant == original || RepeatsNeighbour(index, variant, total, withEnds))
                            continue;

                        _variants[index] = variant;
                        float candidate = FitResidual(metrics, withEnds, middles, loop, length, start);

                        if (candidate < bestError - FitEpsilon)
                        {
                            bestError = candidate;
                            bestIndex = index;
                            bestVariant = variant;
                        }
                    }

                    _variants[index] = original;
                }

                if (bestIndex < 0)
                    break;

                _variants[bestIndex] = bestVariant;
                error = bestError;
            }

            return error;
        }

        private float FitResidual(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float length, float start)
        {
            float natural = Walk(metrics, withEnds, middles, loop, 1f, start, false);
            float fit = FitScale(metrics, withEnds, middles, loop, length, start);
            return Mathf.Abs(natural + (fit - 1f) * MiddleWidthSum(metrics, withEnds, middles) - length);
        }

        private float FitScale(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float length, float start)
        {
            float middleSum = MiddleWidthSum(metrics, withEnds, middles);

            if (middleSum <= 0f)
                return 1f;

            float natural = Walk(metrics, withEnds, middles, loop, 1f, start, false);
            return Mathf.Clamp(1f + (length - natural) / middleSum, 1f - MaxFitStretch, 1f + MaxFitStretch);
        }

        private bool RepeatsNeighbour(int index, int variant, int total, bool withEnds)
        {
            bool previous = index > 0 && RoleAt(index - 1, total, withEnds) == EdgeTileRole.Middle && _variants[index - 1] == variant;
            bool next = index + 1 < _variants.Count && RoleAt(index + 1, total, withEnds) == EdgeTileRole.Middle && _variants[index + 1] == variant;
            return previous || next;
        }

        private float Walk(IEdgeTileMetrics metrics, bool withEnds, int middles, bool loop, float fit, float start, bool record, float endFit = 1f)
        {
            int total = middles + EndCount(withEnds);
            float cursor = start;
            EdgeTileRole previousRole = EdgeTileRole.Middle;
            int previousVariant = 0;

            for (int index = 0; index < total; index++)
            {
                EdgeTileRole role = RoleAt(index, total, withEnds);
                int variant = _variants[index];
                float tileFit = role == EdgeTileRole.Middle ? fit : endFit;
                float width = metrics.Width(role, variant) * tileFit;

                if (index > 0)
                    cursor -= metrics.Overlap(previousRole, previousVariant, role, variant);

                if (record)
                    _tiles.Add(new EdgeTile(role, variant, cursor, width, tileFit));

                cursor += width;
                previousRole = role;
                previousVariant = variant;
            }

            if (loop && total > 0)
                cursor -= metrics.Overlap(previousRole, previousVariant, RoleAt(0, total, withEnds), _variants[0]);

            return cursor - start;
        }

        private float MiddleWidthSum(IEdgeTileMetrics metrics, bool withEnds, int middles)
        {
            int total = middles + EndCount(withEnds);
            float sum = 0f;

            for (int index = 0; index < total; index++)
            {
                if (RoleAt(index, total, withEnds) == EdgeTileRole.Middle)
                    sum += metrics.Width(EdgeTileRole.Middle, _variants[index]);
            }

            return sum;
        }

        private int EndCount(bool withEnds) => withEnds ? (_leftEnd ? 1 : 0) + (_rightEnd ? 1 : 0) : 0;

        private EdgeTileRole RoleAt(int index, int total, bool withEnds)
        {
            if (withEnds && _leftEnd && index == 0)
                return EdgeTileRole.Left;

            if (withEnds && _rightEnd && index == total - 1)
                return EdgeTileRole.Right;

            return EdgeTileRole.Middle;
        }

        private static int VariantAt(int index, int seed, int variantCount)
        {
            if (variantCount <= 1)
                return 0;

            uint hash = unchecked((uint)(index + 1) * VariationHashPrime ^ (uint)seed);
            hash ^= hash >> 15;
            return (int)(hash % (uint)variantCount);
        }
    }
}
