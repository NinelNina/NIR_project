using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TiledPlatform : SurfaceBase
    {
        private const float DefaultHalfLength = 8f;
        private const float MinThickness = 0.05f;
        private const float MinBodyThickness = 0.6f;
        private const float FillUnderCrust = 0.7f;
        private const float FillBottomCornerInset = 0.5f;
        private const float PassableSurfaceArc = 160f;

        [Header("Платформа")]
        [SerializeField] private bool _flipSide;
        [SerializeField] private bool _passable;

        [Header("Стороны коллайдера")]
        [SerializeField] private bool _colliderLeftSide = true;
        [SerializeField] private bool _colliderRightSide = true;
        [SerializeField] private bool _colliderBottom = true;
        [SerializeField] private bool _sidesFollowSprite = true;
        [SerializeField] private bool _autoThickness = true;
        [SerializeField, Min(MinThickness)] private float _thickness = 0.5f;
        [SerializeField, Min(0f)] private float _endInset = 0.1f;

        [Header("Толщина")]
        [SerializeField] private bool _thick;
        [SerializeField, Min(MinBodyThickness)] private float _bodyThickness = 2f;

        [Header("Заливка под платформой")]
        [SerializeField] private bool _fillBelow;
        [SerializeField] private float _fillBaseY = -5f;

        private readonly List<Vector2> _leftSide = new List<Vector2>();
        private readonly List<Vector2> _top = new List<Vector2>();
        private readonly List<Vector2> _rightSide = new List<Vector2>();
        private readonly List<Vector2> _bottom = new List<Vector2>();
        private readonly List<float> _profileScratch = new List<float>();
        private readonly List<Vector2> _fillBottom = new List<Vector2>();

        public override bool IsClosed => false;

        public bool IsThick => _thick;

        public float BodyThickness => _bodyThickness;

        public void SetBodyThickness(float thickness)
        {
            _bodyThickness = Mathf.Max(MinBodyThickness, thickness);
            Rebuild();
        }

        public bool TryGetThicknessHandle(out Vector2 surface, out Vector2 normal)
        {
            surface = default;
            normal = Vector2.up;

            if (!_thick || !Path.IsValid)
                return false;

            Path.Evaluate(Path.Length * 0.5f, out surface, out normal);
            return true;
        }

        protected override bool EdgeByAngleByDefault => false;

        protected override bool IsFlipped => _flipSide != DrawnRightToLeft;

        private bool DrawnRightToLeft => NodeCount >= SurfacePath.MinOpenNodes && GetNode(0).Position.x > GetNode(NodeCount - 1).Position.x;

        protected override bool FillEnabled => _thick || _fillBelow;

        protected override float LayeredThickness => _thick ? _bodyThickness : 0f;

        protected override bool RootUsesEffector => IsPassable;

        public bool IsPassable => _passable && !_thick;

        protected override void SyncComponents()
        {
            bool hasEffector = TryGetComponent(out PlatformEffector2D effector);
            bool hasPassage = TryGetComponent(out PassablePlatform passage);

            if (IsPassable)
            {
                if (!hasEffector)
                {
                    effector = gameObject.AddComponent<PlatformEffector2D>();
                    effector.useOneWay = true;
                    effector.useColliderMask = true;
                    effector.colliderMask = ~0;
                    effector.surfaceArc = PassableSurfaceArc;
                }

                if (!hasPassage)
                    gameObject.AddComponent<PassablePlatform>().Configure(effector);

                return;
            }

            if (Application.isPlaying)
                return;

            if (hasPassage)
                DestroyImmediate(passage);

            if (hasEffector)
                DestroyImmediate(effector);
        }

        protected override void CreateDefaultNodes(List<SurfaceNode> nodes)
        {
            nodes.Add(new SurfaceNode(new Vector2(-DefaultHalfLength, 0f), SurfaceNodeType.Smooth));
            nodes.Add(new SurfaceNode(new Vector2(DefaultHalfLength, 0f), SurfaceNodeType.Smooth));
        }

        protected override void BuildFillOutline(List<Vector2> outline)
        {
            if (!_thick)
            {
                SurfaceOutline.OpenFill(Path, FillInset, _fillBaseY, outline);
                return;
            }

            outline.Clear();

            if (!TryGetLayeredBody(out float start, out float end, out float crustDepth, out float underHeight))
                return;

            start = Mathf.Max(0f, start);
            end = Mathf.Min(Path.Length, end);
            float top = -crustDepth * FillUnderCrust;
            float bottom = -_bodyThickness + underHeight * FillUnderCrust;
            _fillBottom.Clear();
            AddFillPoint(start, top, outline);
            float chamferTop = Mathf.Min(top, bottom + FillBottomCornerInset);
            AddFillPoint(start, chamferTop, _fillBottom);
            AddFillPoint(Mathf.Min(start + FillBottomCornerInset, end), bottom, _fillBottom);

            for (int i = 0; i < Path.SampleCount; i++)
            {
                float distance = Path.Distances[i];

                if (distance <= start || distance >= end)
                    continue;

                outline.Add(Path.Points[i] + Path.MiterNormal(i) * top);

                if (distance > start + FillBottomCornerInset && distance < end - FillBottomCornerInset)
                    _fillBottom.Add(Path.Points[i] + Path.MiterNormal(i) * bottom);
            }

            AddFillPoint(end, top, outline);
            AddFillPoint(Mathf.Max(end - FillBottomCornerInset, start), bottom, _fillBottom);
            AddFillPoint(end, chamferTop, _fillBottom);
            SurfaceColliderBuilder.RemoveLoops(outline, 0);
            SurfaceColliderBuilder.RemoveLoops(_fillBottom, 0);

            for (int i = _fillBottom.Count - 1; i >= 0; i--)
                outline.Add(_fillBottom[i]);
        }

        private void AddFillPoint(float distance, float offset, List<Vector2> points)
        {
            Path.Evaluate(distance, out Vector2 point, out Vector2 normal);
            points.Add(point + normal * offset);
        }

        protected override void BuildColliderChains(ColliderChainSet chains)
        {
            float thickness = _thick ? _bodyThickness : Mathf.Max(MinThickness, _autoThickness && EdgeThickness > 0f ? EdgeThickness : _thickness);
            bool sliced = _thick && TileSet.MiddleVariantCount > 0 && TileSet.DepthBelowSurface > 0f;

            if (sliced)
                thickness = Mathf.Max(MinThickness, _bodyThickness - Mathf.Max(0f, TileSet.DepthBelowSurface - EdgeThickness));

            ColliderOffsets offsets = KindOffsets;
            float topOffset = ColliderOffset + offsets.Top;
            float bottomOffset = -thickness - offsets.Ceiling;
            float sideSpan = Mathf.Max(MinThickness, topOffset - bottomOffset);
            float topStart;
            float topEnd;
            float bottomStart;
            float bottomEnd;

            _leftSide.Clear();
            _top.Clear();
            _rightSide.Clear();
            _bottom.Clear();

            if (_sidesFollowSprite && TryGetArtEnds(out EdgeTile firstTile, out EdgeTile lastTile))
            {
                EndSlice firstSlice = sliced ? EndSliceFor(firstTile, _bodyThickness) : default;
                EndSlice lastSlice = sliced ? EndSliceFor(lastTile, _bodyThickness) : default;
                EdgeSideProfile.Append(Path, TileSet, firstTile, true, sideSpan, topOffset, _profileScratch, _leftSide, out topStart, out bottomStart, firstSlice);
                EdgeSideProfile.Append(Path, TileSet, lastTile, false, sideSpan, topOffset, _profileScratch, _rightSide, out topEnd, out bottomEnd, lastSlice);
                _leftSide.Reverse();
            }
            else
            {
                float start = HasArt ? ArtStart : 0f;
                float end = HasArt ? ArtEnd : Path.Length;
                float middle = (start + end) * 0.5f;
                topStart = bottomStart = Mathf.Min(start + _endInset, middle);
                topEnd = bottomEnd = Mathf.Max(end - _endInset, middle);
                AddStraightSide(topStart, topOffset, sideSpan, true, _leftSide);
                AddStraightSide(topEnd, topOffset, sideSpan, false, _rightSide);
            }

            if (topEnd <= topStart)
            {
                chains.Clear();
                return;
            }

            SurfaceColliderBuilder.AppendLine(Path, topStart, topEnd, topOffset, ColliderShape, ColliderTolerance, Simplifier, _top);
            SurfaceColliderBuilder.AppendLine(Path, bottomStart, bottomEnd, bottomOffset, ColliderShape, ColliderTolerance, Simplifier, _bottom);
            _bottom.Reverse();
            SurfaceColliderBuilder.TrimCrossing(_rightSide, _bottom);
            SurfaceColliderBuilder.TrimCrossing(_bottom, _leftSide);

            bool solid = !IsPassable;
            ColliderChains.Assemble(_leftSide, _top, _rightSide, _bottom, solid && _colliderLeftSide, solid && _colliderRightSide, solid && _colliderBottom, chains);
        }

        private void AddStraightSide(float distance, float offset, float thickness, bool upward, List<Vector2> side)
        {
            Path.Evaluate(distance, out Vector2 point, out Vector2 normal);
            Vector2 top = point + normal * offset;
            Vector2 bottom = top - normal * thickness;
            side.Add(upward ? bottom : top);
            side.Add(upward ? top : bottom);
        }
    }
}
