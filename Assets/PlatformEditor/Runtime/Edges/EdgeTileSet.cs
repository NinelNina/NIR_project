using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public enum EdgeSurfaceAlignment
    {
        ImageTop,
        DetectedSurface
    }

    public sealed class EdgeTileSet : IEdgeTileMetrics
    {
        private const int SeamScanRows = 16;
        private const float SeamBodyFraction = 0.85f;
        private const float MinScale = 0.05f;

        private readonly List<SpriteShape> _middles = new List<SpriteShape>();

        private SpriteShape _left;
        private SpriteShape _right;
        private Vector2 _leftScale = Vector2.one;
        private Vector2 _middleScale = Vector2.one;
        private Vector2 _rightScale = Vector2.one;
        private EdgeSurfaceAlignment _alignment = EdgeSurfaceAlignment.ImageTop;
        private float _extraOverlap;
        private bool _closeSeams = true;
        private bool _fitEndsToMiddleHeight = true;

        public bool HasEnds => _left != null && _right != null;

        public int MiddleVariantCount => _middles.Count;

        public void SetShapes(SpriteShape left, IReadOnlyList<SpriteShape> middles, SpriteShape right)
        {
            _left = left;
            _right = right;
            _middles.Clear();

            if (middles == null)
                return;

            for (int i = 0; i < middles.Count; i++)
            {
                if (middles[i] != null)
                    _middles.Add(middles[i]);
            }
        }

        public void SetScales(Vector2 left, Vector2 middle, Vector2 right)
        {
            _leftScale = ClampScale(left);
            _middleScale = ClampScale(middle);
            _rightScale = ClampScale(right);
        }

        public void SetJoining(EdgeSurfaceAlignment alignment, float extraOverlap, bool closeSeams, bool fitEndsToMiddleHeight)
        {
            _alignment = alignment;
            _extraOverlap = Mathf.Max(0f, extraOverlap);
            _closeSeams = closeSeams;
            _fitEndsToMiddleHeight = fitEndsToMiddleHeight;
        }

        public SpriteShape Shape(EdgeTileRole role, int variant)
        {
            switch (role)
            {
                case EdgeTileRole.Left:
                    return _left;
                case EdgeTileRole.Right:
                    return _right;
                default:
                    return _middles[variant];
            }
        }

        public Vector2 Scale(EdgeTileRole role)
        {
            switch (role)
            {
                case EdgeTileRole.Left:
                    return _leftScale;
                case EdgeTileRole.Right:
                    return _rightScale;
                default:
                    return _middleScale;
            }
        }

        public float Width(EdgeTileRole role, int variant) => Shape(role, variant).Bounds.width * Scale(role).x;

        public float DepthBelowSurface
        {
            get
            {
                float depth = 0f;

                for (int i = 0; i < _middles.Count; i++)
                    depth = Mathf.Max(depth, (SurfaceY(EdgeTileRole.Middle, i) - _middles[i].Bounds.yMin) * _middleScale.y);

                return depth;
            }
        }

        public float HeightAboveSurface
        {
            get
            {
                float height = 0f;

                for (int i = 0; i < _middles.Count; i++)
                    height = Mathf.Max(height, (_middles[i].Bounds.yMax - SurfaceY(EdgeTileRole.Middle, i)) * _middleScale.y);

                return height;
            }
        }

        public float SurfaceY(EdgeTileRole role, int variant)
        {
            SpriteShape shape = Shape(role, variant);

            if (_alignment == EdgeSurfaceAlignment.DetectedSurface || _middles.Count == 0)
                return shape.SurfaceY;

            SpriteShape reference = _middles[0];
            return shape.Bounds.yMax - (reference.Bounds.yMax - reference.SurfaceY);
        }

        public float BelowSurfaceScale(EdgeTileRole role, int variant)
        {
            float scaleY = Scale(role).y;

            if (!_fitEndsToMiddleHeight || role == EdgeTileRole.Middle || _middles.Count == 0)
                return scaleY;

            float end = BodyThickness(role, variant) * scaleY;
            float middle = BodyThickness(EdgeTileRole.Middle, 0) * _middleScale.y;
            return end > 0f && middle > 0f ? scaleY * middle / end : scaleY;
        }

        public float Thickness => _middles.Count > 0 ? BodyThickness(EdgeTileRole.Middle, 0) * _middleScale.y : 0f;

        public float Overlap(EdgeTileRole leftRole, int leftVariant, EdgeTileRole rightRole, int rightVariant)
        {
            if (!_closeSeams)
                return _extraOverlap;

            return _extraOverlap + SeamGap(leftRole, leftVariant, rightRole, rightVariant);
        }

        private float BodyThickness(EdgeTileRole role, int variant) => SurfaceY(role, variant) - Shape(role, variant).SolidBottomY;

        private float SeamGap(EdgeTileRole leftRole, int leftVariant, EdgeTileRole rightRole, int rightVariant)
        {
            SpriteShape left = Shape(leftRole, leftVariant);
            SpriteShape right = Shape(rightRole, rightVariant);

            if (left == null || right == null || !left.HasOutline || !right.HasOutline)
                return 0f;

            float leftScaleX = Scale(leftRole).x;
            float rightScaleX = Scale(rightRole).x;
            float leftVertical = BelowSurfaceScale(leftRole, leftVariant);
            float rightVertical = BelowSurfaceScale(rightRole, rightVariant);
            float leftSurface = SurfaceY(leftRole, leftVariant);
            float rightSurface = SurfaceY(rightRole, rightVariant);
            float body = Mathf.Min(BodyThickness(leftRole, leftVariant) * leftVertical, BodyThickness(rightRole, rightVariant) * rightVertical) * SeamBodyFraction;
            float gap = 0f;

            for (int row = 0; row < SeamScanRows; row++)
            {
                float depth = body * (row + 1) / SeamScanRows;
                float leftEdge = left.EdgeX(leftSurface - depth / leftVertical, false);
                float rightEdge = right.EdgeX(rightSurface - depth / rightVertical, true);

                if (float.IsNaN(leftEdge) || float.IsNaN(rightEdge))
                    continue;

                float leftGap = (left.Bounds.xMax - leftEdge) * leftScaleX;
                float rightGap = (rightEdge - right.Bounds.xMin) * rightScaleX;
                gap = Mathf.Max(gap, leftGap + rightGap);
            }

            return gap;
        }

        private static Vector2 ClampScale(Vector2 scale) => new Vector2(Mathf.Max(MinScale, scale.x), Mathf.Max(MinScale, scale.y));
    }
}
