using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Level.Platforms
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public abstract class SurfaceBase : MonoBehaviour
    {
        public const string GroundLayerName = "Ground";

        private const string GeneratedPrefix = "[Generated] Part ";
        private const float SliceEpsilon = 0.0001f;
        private const float EndFillReach = 0.5f;
        private const float EndSideOverlap = 0.05f;
        private const float EndCrustOverlap = 0.75f;
        private const float EndUnderOverlap = 0.88f;
        private const float MaxWholeEndStretch = 0.6f;
        private const string ColliderHolderPrefix = "[Generated] Collider ";
        private const float DefaultWallAngle = 45f;
        private const float DefaultCeilingAngle = 135f;
        private const float DefaultMinSectionLength = 0.5f;
        private const string GeneratedMeshName = "SurfacePart_";
        private const string FallbackShaderName = "Sprites/Default";
        private const float RunJoinTolerance = 0.0001f;
        private const int RunSeedStep = 7919;
        private const float MinColliderTolerance = 0.001f;
        private const float DefaultTangentFraction = 0.25f;
        private const float MinTangentSquared = 0.0001f;
        private const float WallStepEpsilon = 0.001f;
        private const float CornerProbe = 0.01f;
        private const float CornerDirectionProbe = 0.2f;
        private const float CornerStraightStep = 0.1f;
        private const float CornerStraightTolerance = 0.08f;
        private const float MinCornerSpan = 0.6f;
        private const float MinCornerAngle = 60f;
        private const float MaxCornerAngle = 120f;
        private const float CornerSpanFraction = 0.5f;
        private const float CornerFade = 0.4f;
        private const float MaxTaperInterior = 179f;
        private const float MinTrimmedLength = 0.05f;
        private const float EarthFringe = 0.08f;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly Comparison<Vector2> CompareRunStarts = (a, b) => a.x.CompareTo(b.x);

        [Header("Узлы")]
        [SerializeField] private List<SurfaceNode> _nodes = new List<SurfaceNode>();

        [Header("Биом")]
        [SerializeField] private GroundTheme _theme;

        [Header("Кромка")]
        [SerializeField] private EdgeFitMode _fitMode = EdgeFitMode.FitLength;
        [SerializeField] private EdgeSurfaceAlignment _alignment = EdgeSurfaceAlignment.ImageTop;
        [SerializeField] private Vector2 _leftScale = Vector2.one;
        [SerializeField] private Vector2 _middleScale = Vector2.one;
        [SerializeField] private Vector2 _rightScale = Vector2.one;
        [SerializeField, Min(0f)] private float _tileOverlap = 0.02f;
        [SerializeField] private bool _closeSeams = true;
        [SerializeField] private bool _fitEndsToMiddleHeight = true;
        [SerializeField] private int _variationSeed;
        [SerializeField] private bool _edgeByAngle = true;

        [Header("Коллайдер")]
        [SerializeField] private SurfaceColliderShape _colliderShape = SurfaceColliderShape.Simplified;
        [SerializeField, Min(MinColliderTolerance)] private float _colliderTolerance = 0.1f;
        [SerializeField] private float _colliderOffset;
        [SerializeField, Min(0f)] private float _edgeRadius = 0.02f;
        [SerializeField, HideInInspector] private EdgeCollider2D _secondaryCollider;
        [SerializeField] private SurfaceProfile _surfaceProfile;

        [Header("Заливка")]
        [SerializeField, Min(0f)] private float _fillInset = 0.1f;

        [Header("Отрисовка")]
        [SerializeField] private Material _material;
        [SerializeField] private int _sortingLayerId;
        [SerializeField] private int _sortingOrder;
        [SerializeField] private int _fillSortingOffset = -2;
        [SerializeField] private int _wallSortingOffset = -1;
        [SerializeField] private int _cornerSortingOffset = 1;

        private readonly SurfacePath _path = new SurfacePath();
        private readonly SpriteShapeCache _shapeCache = new SpriteShapeCache();
        private readonly EdgeTileSet _tileSet = new EdgeTileSet();
        private readonly EdgeTileSet _crustSet = new EdgeTileSet();
        private readonly EdgeTileSet _underSet = new EdgeTileSet();
        private readonly List<SpriteShape> _shapeScratch = new List<SpriteShape>();
        private readonly EdgeLayout _layout = new EdgeLayout();
        private readonly List<SpriteShape> _middleShapes = new List<SpriteShape>();
        private readonly List<Vector2> _runs = new List<Vector2>();
        private readonly List<MeshPart> _parts = new List<MeshPart>();
        private readonly List<Vector2> _outline = new List<Vector2>();
        private readonly ColliderChainSet _colliderChains = new ColliderChainSet();
        private readonly List<Vector2> _chainPoints = new List<Vector2>();
        private readonly List<EdgeCollider2D> _holderColliders = new List<EdgeCollider2D>();
        private readonly List<int> _triangulationScratch = new List<int>();
        private readonly PolylineSimplifier _simplifier = new PolylineSimplifier();
        private readonly List<EdgeCollider2D> _edgeColliders = new List<EdgeCollider2D>();
        private readonly List<EdgeSection> _sections = new List<EdgeSection>();
        private readonly List<SectionTrim> _trims = new List<SectionTrim>();
        private readonly List<CornerPlacement> _corners = new List<CornerPlacement>();
        private readonly List<TileSpan> _tileSpans = new List<TileSpan>();

        private EdgeCollider2D _collider;
        private string _ownMeshName;
        private MaterialPropertyBlock _propertyBlock;
        private Material _fallbackMaterial;
        private int _usedParts;
        private bool _tileSetReady;
        private bool _hasArt;
        private float _artStart;
        private float _artEnd;
        private EdgeTile _firstTile;
        private EdgeTile _lastTile;
        private bool _firstTileIsEnd;
        private bool _lastTileIsEnd;
        private SpriteShape _wallShape;
        private bool _crustReady;
        private bool _underReady;
        private float _layeredMiddleStart;
        private float _layeredMiddleEnd;
        private float _layeredCrustDepth;
        private float _layeredUnderHeight;

        protected SurfaceBase()
        {
            _edgeByAngle = EdgeByAngleByDefault;
        }

        public abstract bool IsClosed { get; }

        protected virtual bool EdgeByAngleByDefault => true;

        public IReadOnlyList<SurfaceNode> Nodes => _nodes;

        public GroundTheme Theme => _theme;

        public SurfacePath Path => _path;

        protected virtual bool IsFlipped => false;

        protected virtual bool UsesBlockEdges => false;

        protected virtual float LayeredThickness => 0f;

        protected virtual bool RootUsesEffector => TryGetComponent(out PlatformEffector2D _);

        private bool Layered => LayeredThickness > 0f && !UsesBlockEdges && _crustReady && _underReady;

        protected abstract bool FillEnabled { get; }

        protected float FillInset => _fillInset;

        protected float ColliderOffset => _colliderOffset;

        protected ColliderOffsets KindOffsets => _theme != null ? _theme.ColliderOffsets : default;

        protected float EdgeThickness => _tileSetReady ? _tileSet.Thickness : 0f;

        protected bool HasArt => _hasArt;

        protected float ArtStart => _artStart;

        protected float ArtEnd => _artEnd;

        protected SurfaceColliderShape ColliderShape => _colliderShape;

        protected float ColliderTolerance => _colliderTolerance;

        protected PolylineSimplifier Simplifier => _simplifier;

        protected EdgeTileSet TileSet => _tileSet;

        protected bool TryGetArtEnds(out EdgeTile first, out EdgeTile last)
        {
            first = _firstTile;
            last = _lastTile;
            return _hasArt && _tileSetReady && _firstTileIsEnd && _lastTileIsEnd;
        }

        public int NodeCount => _nodes.Count;

        public int MinNodeCount => IsClosed ? SurfacePath.MinClosedNodes : SurfacePath.MinOpenNodes;

        public SurfaceNode GetNode(int index) => _nodes[index];

        public void Reshuffle()
        {
            _variationSeed = unchecked(_variationSeed * 1103515245 + 12345);
            Rebuild();
        }

        public void SetNodePosition(int index, Vector2 position)
        {
            _nodes[index].Position = position;
            Rebuild();
        }

        public void SetNodeType(int index, SurfaceNodeType type)
        {
            SurfaceNode node = _nodes[index];

            if (node.Type == type)
                return;

            node.Type = type;

            if (type == SurfaceNodeType.Bezier)
                ResetTangents(index);

            Rebuild();
        }

        public void SetNodeTangentMode(int index, SurfaceTangentMode mode)
        {
            _nodes[index].SetTangentMode(mode);
            Rebuild();
        }

        public void SetNodeInTangent(int index, Vector2 tangent)
        {
            _nodes[index].SetInTangent(tangent);
            Rebuild();
        }

        public void SetNodeOutTangent(int index, Vector2 tangent)
        {
            _nodes[index].SetOutTangent(tangent);
            Rebuild();
        }

        public void SetNodeEdgeVisible(int index, bool visible)
        {
            _nodes[index].EdgeVisible = visible;
            Rebuild();
        }

        public void InsertNode(int index, Vector2 position, SurfaceNodeType type)
        {
            index = Mathf.Clamp(index, 0, _nodes.Count);

            var node = new SurfaceNode(position, type);

            if (_nodes.Count > 0)
                node.EdgeVisible = _nodes[Mathf.Max(0, index - 1)].EdgeVisible;

            _nodes.Insert(index, node);

            if (type == SurfaceNodeType.Bezier)
                ResetTangents(index);

            Rebuild();
        }

        public bool RemoveNode(int index)
        {
            if (_nodes.Count <= MinNodeCount || index < 0 || index >= _nodes.Count)
                return false;

            _nodes.RemoveAt(index);
            Rebuild();
            return true;
        }

        public void FindTightBends(List<TightBend> bends)
        {
            if (!_tileSetReady)
            {
                bends.Clear();
                return;
            }

            EdgeTileSet set = UsesBlockEdges && _crustReady ? _crustSet : _tileSet;
            TightBendFinder.Find(_path, set.DepthBelowSurface, set.HeightAboveSurface, bends);
        }

        public int FindSegmentAt(float distance)
        {
            int segments = _path.SegmentCount;

            for (int segment = 0; segment < segments; segment++)
            {
                _path.SegmentRange(segment, out float start, out float end);

                if (distance >= start && distance <= end)
                    return segment;

                if (IsClosed && distance + _path.Length >= start && distance + _path.Length <= end)
                    return segment;
            }

            return Mathf.Max(0, segments - 1);
        }

        private void ResetTangents(int index)
        {
            int count = _nodes.Count;
            Vector2 current = _nodes[index].Position;
            bool hasPrevious = IsClosed || index > 0;
            bool hasNext = IsClosed || index < count - 1;
            Vector2 previous = hasPrevious ? _nodes[(index - 1 + count) % count].Position : current;
            Vector2 next = hasNext ? _nodes[(index + 1) % count].Position : current;
            Vector2 tangent = (next - previous) * DefaultTangentFraction;

            if (tangent.sqrMagnitude < MinTangentSquared)
                tangent = Vector2.right;

            SurfaceNode node = _nodes[index];
            node.SetTangentMode(SurfaceTangentMode.Symmetric);
            node.SetOutTangent(tangent);
        }

        public void Rebuild()
        {
            EnsureNodes();
            SyncComponents();
            SyncSurfaceProfile(gameObject);
            ApplyGroundLayer();
            _path.Build(_nodes, IsClosed, IsFlipped);
            CollectParts();
            _usedParts = 0;
            BuildEdges();

            if (FillEnabled)
                BuildFill();

            FinishParts();
            UpdateCollider();
        }

        protected abstract void CreateDefaultNodes(List<SurfaceNode> nodes);

        protected abstract void BuildFillOutline(List<Vector2> outline);

        protected abstract void BuildColliderChains(ColliderChainSet chains);


        protected virtual void SyncComponents()
        {
        }

        public SurfaceProfile SurfaceProfile => _surfaceProfile != null ? _surfaceProfile : _theme != null ? _theme.SurfaceProfile : null;

        private void SyncSurfaceProfile(GameObject target)
        {
            SurfaceProfile profile = SurfaceProfile;
            bool has = target.TryGetComponent(out GroundSurface surface);

            if (profile != null)
            {
                if (!has)
                    surface = target.AddComponent<GroundSurface>();

                surface.SetProfile(profile);
                return;
            }

            if (has && !Application.isPlaying)
                DestroyImmediate(surface);
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
                Rebuild();
            else
                ScheduleRebuild();
        }

        private void OnValidate() => ScheduleRebuild();

        private void OnDestroy()
        {
            CollectParts();

            for (int i = 0; i < _parts.Count; i++)
                DestroyPartMesh(_parts[i]);

            _parts.Clear();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying || !FillEnabled || !transform.hasChanged)
                return;

            transform.hasChanged = false;
            Rebuild();
        }
#endif

        private void ScheduleRebuild()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            UnityEditor.EditorApplication.delayCall -= RebuildDeferred;
            UnityEditor.EditorApplication.delayCall += RebuildDeferred;
#endif
        }

        private void RebuildDeferred()
        {
            if (this != null && isActiveAndEnabled)
                Rebuild();
        }

        private void EnsureNodes()
        {
            _nodes ??= new List<SurfaceNode>();
            _nodes.RemoveAll(node => node == null);

            if (_nodes.Count >= (IsClosed ? SurfacePath.MinClosedNodes : SurfacePath.MinOpenNodes))
                return;

            _nodes.Clear();
            CreateDefaultNodes(_nodes);
        }

        private void BuildEdges()
        {
            _layeredMiddleStart = float.MaxValue;
            _layeredMiddleEnd = float.MinValue;
            _layeredCrustDepth = 0f;
            _layeredUnderHeight = 0f;

            _hasArt = false;
            _firstTileIsEnd = false;
            _lastTileIsEnd = false;
            _tileSetReady = _theme != null && _path.IsValid && ConfigureTileSet();

            if (!_tileSetReady)
                return;

            bool loop = CollectRuns();
            bool spriteWalls = !UsesBlockEdges
                || _theme.BlockWallArt == BlockWallArt.WallSprite
                || (_theme.BlockWallArt == BlockWallArt.Crust && !_crustReady)
                || (_theme.BlockWallArt == BlockWallArt.Earth && !_underReady);
            _wallShape = _edgeByAngle && _theme.Wall != null && spriteWalls ? SpriteShape.FromSpriteRotated(_theme.Wall) : null;

            for (int run = 0; run < _runs.Count; run++)
            {
                CollectSections(_runs[run], loop);
                PlanCorners(loop);
                bool sectionLoop = loop && _sections.Count == 1;

                for (int index = 0; index < _sections.Count; index++)
                {
                    EdgeSection section = _sections[index];
                    SectionTrim trim = _trims[index];
                    var trimmed = new EdgeSection(section.Start + trim.Start, section.End - trim.End, section.Kind);
                    bool first = run == 0 && index == 0;
                    bool wall = section.Kind == EdgeKind.Wall && _wallShape != null;
                    _tileSpans.Clear();

                    if (trimmed.Length > MinTrimmedLength)
                    {
                        if (wall)
                            AppendWall(trimmed);
                        else
                            AppendEdgeRun(trimmed, sectionLoop, _variationSeed + (run + index * _runs.Count) * RunSeedStep, first, HasEnd(trim.CornerAtStart, loop, index == 0), HasEnd(trim.CornerAtEnd, loop, index == _sections.Count - 1));
                    }

                    if (UsesBlockEdges && !sectionLoop)
                        TaperSpans(section);

                    bool innerStart = UsesBlockEdges && !sectionLoop && (loop || index > 0);
                    bool innerEnd = UsesBlockEdges && !sectionLoop && (loop || index < _sections.Count - 1);

                    if ((trim.CornerAtStart || innerStart) && _tileSpans.Count > 0)
                        HideColumn(_tileSpans[0].Part.Colors, _tileSpans[0].First);

                    if ((trim.CornerAtEnd || innerEnd) && _tileSpans.Count > 0)
                        HideColumn(_tileSpans[_tileSpans.Count - 1].Part.Colors, _tileSpans[_tileSpans.Count - 1].End - EdgeMeshBuilder.RowsPerColumn);

                    _lastTileIsEnd = !wall;

                    if (first)
                        _firstTileIsEnd = !wall;
                }

                for (int i = 0; i < _corners.Count; i++)
                    AppendCorner(_corners[i]);
            }
        }

        private void CollectSections(Vector2 range, bool loop)
        {
            if (_edgeByAngle)
            {
                EdgeSections.Build(_path, range.x, range.y, loop, _theme.WallAngle, _theme.CeilingAngle, _theme.MinSectionLength, _sections);
                return;
            }

            _sections.Clear();
            _sections.Add(new EdgeSection(range.x, range.y, EdgeKind.Top));
        }

        private bool HasEnd(bool corner, bool loop, bool runBoundary)
        {
            if (corner)
                return false;

            return !UsesBlockEdges || (!loop && runBoundary);
        }

        private void AppendEdgeRun(in EdgeSection section, bool loop, int seed, bool first, bool leftEnd, bool rightEnd)
        {
            EdgeTileSet set = SetFor(section.Kind, out bool flipped, out bool trimFringe);
            _layout.Build(set, section.Start, section.Length, loop, _fitMode, seed, leftEnd, rightEnd);
            IReadOnlyList<EdgeTile> tiles = _layout.Tiles;

            if (tiles.Count == 0)
                return;

            float rangeStart = loop ? float.NegativeInfinity : section.Start;
            float rangeEnd = loop ? float.PositiveInfinity : section.End;

            for (int i = 0; i < tiles.Count; i++)
            {
                if (Layered && set == _tileSet)
                    AppendLayeredTile(tiles[i], rangeStart, rangeEnd);
                else
                    AppendTile(set, flipped, trimFringe, tiles[i], rangeStart, rangeEnd);
            }

            if (first || !_hasArt)
                _firstTile = tiles[0];

            _lastTile = tiles[tiles.Count - 1];
            ExtendArt(tiles[0].Start, _layout.Start + _layout.CoveredLength);
        }

        private EdgeTileSet SetFor(EdgeKind kind, out bool flipped, out bool trimFringe)
        {
            flipped = false;
            trimFringe = false;

            switch (ArtFor(kind))
            {
                case EdgeArt.Crust:
                    return _crustSet;
                case EdgeArt.Earth:
                    flipped = true;
                    trimFringe = kind == EdgeKind.Wall;
                    return _underSet;
                default:
                    return _tileSet;
            }
        }

        private EdgeArt ArtFor(EdgeKind kind)
        {
            if (!UsesBlockEdges)
                return EdgeArt.Main;

            switch (kind)
            {
                case EdgeKind.Top:
                    return _crustReady ? EdgeArt.Crust : EdgeArt.Main;
                case EdgeKind.Wall:
                    if (_theme.BlockWallArt == BlockWallArt.Crust && _crustReady)
                        return EdgeArt.Crust;

                    if (_theme.BlockWallArt == BlockWallArt.Earth && _underReady)
                        return EdgeArt.Earth;

                    return _wallShape != null ? EdgeArt.WallSprite : EdgeArt.Main;
                default:
                    if (_theme.BlockBottomArt == BlockBottomArt.Crust && _crustReady)
                        return EdgeArt.Crust;

                    return _underReady ? EdgeArt.Earth : EdgeArt.Main;
            }
        }

        private void AppendWall(in EdgeSection section)
        {
            Sprite sprite = _theme.Wall;
            float height = _wallShape.Bounds.width;

            if (height <= 0f)
                return;

            MeshPart part = AcquirePart(sprite.texture, _sortingOrder + _wallSortingOffset);
            float depth = _tileSet.Thickness > 0f ? _tileSet.Thickness : float.PositiveInfinity;
            int firstVertex = part.Vertices.Count;

            for (float cursor = section.Start; cursor < section.End - WallStepEpsilon; cursor += height)
            {
                float span = Mathf.Min(height, section.End - cursor);
                EdgeTileArt art = EdgeTileArt.FromSpriteRotated(sprite, _wallShape.SurfaceY, span / height, depth);
                var tile = new EdgeTile(EdgeTileRole.Middle, 0, cursor, span, 1f);
                EdgeMeshBuilder.AppendTile(_path, art, tile, 0f, part.Vertices, part.Uvs, part.Colors, part.Triangles, section.Start, section.End);
            }

            _tileSpans.Add(new TileSpan(part, firstVertex, part.Vertices.Count));
            ExtendArt(section.Start, section.End);
        }

        private static void HideColumn(List<Color32> colors, int firstVertex)
        {
            var hidden = new Color32(255, 255, 255, 0);

            for (int row = 0; row < EdgeMeshBuilder.RowsPerColumn; row++)
                colors[firstVertex + row] = hidden;
        }

        private void TaperSpans(in EdgeSection section)
        {
            for (int i = 0; i < _tileSpans.Count; i++)
            {
                TileSpan span = _tileSpans[i];
                TaperTowardsCorner(span, section.Start, section.Start + CornerProbe, section.Start - CornerProbe);
                TaperTowardsCorner(span, section.End, section.End - CornerProbe, section.End + CornerProbe);
            }
        }

        private void TaperTowardsCorner(in TileSpan span, float cornerDistance, float inside, float outside)
        {
            _path.Evaluate(inside, out _, out Vector2 normal);
            _path.Evaluate(outside, out _, out Vector2 neighbour);
            float interior = 180f - Vector2.Angle(normal, neighbour);

            if (interior >= MaxTaperInterior)
                return;

            _path.Evaluate(cornerDistance, out Vector2 corner, out _);
            float slope = Mathf.Tan(interior * 0.5f * Mathf.Deg2Rad);
            List<Vector3> vertices = span.Part.Vertices;
            List<Vector2> uvs = span.Part.Uvs;

            for (int bottom = span.First; bottom + 1 < span.End; bottom += EdgeMeshBuilder.RowsPerColumn)
            {
                Vector2 surface = vertices[bottom + 1];
                Vector2 inward = (Vector2)vertices[bottom] - surface;
                float depth = inward.magnitude;
                float limit = Vector2.Distance(surface, corner) * slope;

                if (depth <= limit || depth <= 0f)
                    continue;

                float kept = limit / depth;
                vertices[bottom] = surface + inward * kept;
                uvs[bottom] = Vector2.Lerp(uvs[bottom + 1], uvs[bottom], kept);
            }
        }

        private void PlanCorners(bool loop)
        {
            _corners.Clear();
            _trims.Clear();

            int count = _sections.Count;

            for (int i = 0; i < count; i++)
                _trims.Add(default);

            if (!UsesBlockEdges || !_edgeByAngle)
                return;

            int boundaries = loop && count > 1 ? count : count - 1;

            for (int i = 0; i < boundaries; i++)
            {
                int next = (i + 1) % count;
                EdgeKind before = _sections[i].Kind;
                EdgeKind after = _sections[next].Kind;

                if (before == after || !TakesCorner(before) || !TakesCorner(after))
                    continue;

                if (Rank(before) <= Rank(after))
                    PlanCorner(i, next, _sections[i].End, false);
                else
                    PlanCorner(next, i, _sections[next].Start, true);
            }
        }

        private bool TakesCorner(EdgeKind kind)
        {
            EdgeArt art = ArtFor(kind);
            return art == EdgeArt.Crust || (kind == EdgeKind.Wall && art == EdgeArt.WallSprite);
        }

        private static int Rank(EdgeKind kind) => kind == EdgeKind.Top ? 0 : kind == EdgeKind.Wall ? 1 : 2;

        private void PlanCorner(int topIndex, int wallIndex, float distance, bool topAfter)
        {
            EdgeSection top = _sections[topIndex];
            EdgeSection wall = _sections[wallIndex];
            _path.Evaluate(topAfter ? distance + CornerProbe : distance - CornerProbe, out _, out Vector2 topNormal);
            _path.Evaluate(topAfter ? distance - CornerProbe : distance + CornerProbe, out _, out Vector2 wallNormal);
            float angle = Vector2.Angle(topNormal, wallNormal);

            if (angle < MinCornerAngle || angle > MaxCornerAngle)
                return;

            bool faceLeft = Vector2.Dot(wallNormal, new Vector2(topNormal.y, -topNormal.x)) < 0f;
            Sprite sprite = faceLeft ? _theme.SharpCornerLeft : _theme.SharpCornerRight;

            if (sprite == null)
                return;

            SpriteShape side = SpriteShape.FromSpriteRotated(sprite, faceLeft);
            EdgeTileArt frame = EdgeTileArt.FromSprite(sprite, 0f, Vector2.one, 1f);
            var art = new EdgeCornerArt(frame.LocalRect, frame.UvRect, _shapeCache.Get(sprite).SurfaceY, faceLeft ? -side.SurfaceY : side.SurfaceY, faceLeft);
            float topSpan = StraightSpan(distance, topAfter ? 1f : -1f, Mathf.Min(top.Length * CornerSpanFraction, art.MaxTopSpan));
            float wallSpan = StraightSpan(distance, topAfter ? -1f : 1f, Mathf.Min(wall.Length * CornerSpanFraction, art.MaxWallSpan));
            _path.Evaluate(distance, out Vector2 corner, out _);
            _path.Evaluate(topAfter ? distance + CornerDirectionProbe : distance - CornerDirectionProbe, out Vector2 topPoint, out _);
            _path.Evaluate(topAfter ? distance - CornerDirectionProbe : distance + CornerDirectionProbe, out Vector2 facePoint, out _);

            if (Vector2.Dot(facePoint - corner, topNormal) >= 0f || topSpan < MinCornerSpan || wallSpan < MinCornerSpan)
                return;
            _corners.Add(new CornerPlacement(sprite, art, corner, (topPoint - corner).normalized, (facePoint - corner).normalized, topSpan, wallSpan));

            float topTrim = Mathf.Max(0f, topSpan - CornerFade);
            float wallTrim = Mathf.Max(0f, wallSpan - CornerFade);
            _trims[topIndex] = topAfter ? _trims[topIndex].WithStart(topTrim) : _trims[topIndex].WithEnd(topTrim);
            _trims[wallIndex] = topAfter ? _trims[wallIndex].WithEnd(wallTrim) : _trims[wallIndex].WithStart(wallTrim);
        }

        private float StraightSpan(float distance, float direction, float maxSpan)
        {
            _path.Evaluate(distance, out Vector2 corner, out _);
            _path.Evaluate(distance + direction * CornerDirectionProbe, out Vector2 probe, out _);
            Vector2 along = (probe - corner).normalized;

            for (float span = CornerStraightStep; span < maxSpan; span += CornerStraightStep)
            {
                _path.Evaluate(distance + direction * span, out Vector2 point, out _);
                Vector2 offset = point - corner;

                if (Mathf.Abs(offset.x * along.y - offset.y * along.x) > CornerStraightTolerance)
                    return Mathf.Max(0f, span - CornerStraightStep);
            }

            return maxSpan;
        }

        private void AppendCorner(in CornerPlacement placement)
        {
            MeshPart part = AcquirePart(placement.Sprite.texture, _sortingOrder + _cornerSortingOffset);
            EdgeCornerBuilder.Append(placement.Art, placement.Corner, placement.AlongTop, placement.AlongFace, placement.TopSpan, placement.WallSpan, CornerFade, part.Vertices, part.Uvs, part.Colors, part.Triangles);
        }

        private void ExtendArt(float start, float end)
        {
            _artStart = _hasArt ? Mathf.Min(_artStart, start) : start;
            _artEnd = _hasArt ? Mathf.Max(_artEnd, end) : end;
            _hasArt = true;
        }

        private bool ConfigureTileSet()
        {
            _shapeCache.Clear();
            _middleShapes.Clear();

            Sprite[] middle = _theme.Middle;

            if (middle != null)
            {
                for (int i = 0; i < middle.Length; i++)
                {
                    SpriteShape shape = _shapeCache.Get(middle[i]);

                    if (shape != null)
                        _middleShapes.Add(shape);
                }
            }

            if (_middleShapes.Count == 0)
                return false;

            _tileSet.SetShapes(_shapeCache.Get(_theme.Left), _middleShapes, _shapeCache.Get(_theme.Right));
            _tileSet.SetScales(_leftScale, _middleScale, _rightScale);
            _tileSet.SetJoining(_alignment, _tileOverlap, _closeSeams, _fitEndsToMiddleHeight);
            bool layered = UsesBlockEdges || LayeredThickness > 0f;
            _crustReady = layered && ConfigureBlockSet(_crustSet, _theme.CrustLeft, _theme.CrustMiddle, _theme.CrustRight, false);
            _underReady = layered && ConfigureBlockSet(_underSet, _theme.UnderLeft, _theme.UnderMiddle, _theme.UnderRight, UsesBlockEdges);
            return true;
        }

        private bool ConfigureBlockSet(EdgeTileSet set, Sprite left, Sprite[] middle, Sprite right, bool flipped)
        {
            _shapeScratch.Clear();

            if (middle != null)
            {
                for (int i = 0; i < middle.Length; i++)
                {
                    SpriteShape shape = _shapeCache.Get(middle[i], flipped);

                    if (shape != null)
                        _shapeScratch.Add(shape);
                }
            }

            if (_shapeScratch.Count == 0)
                return false;

            set.SetShapes(_shapeCache.Get(left, flipped), _shapeScratch, _shapeCache.Get(right, flipped));
            set.SetScales(_leftScale, _middleScale, _rightScale);
            set.SetJoining(_alignment, _tileOverlap, _closeSeams, _fitEndsToMiddleHeight);
            return true;
        }

        private bool CollectRuns()
        {
            _runs.Clear();

            int segments = _path.SegmentCount;
            bool allVisible = true;

            for (int segment = 0; segment < segments; segment++)
            {
                if (!_nodes[segment].EdgeVisible)
                {
                    allVisible = false;
                    break;
                }
            }

            if (allVisible && IsClosed)
            {
                _runs.Add(new Vector2(0f, _path.Length));
                return true;
            }

            for (int segment = 0; segment < segments; segment++)
            {
                if (!_nodes[segment].EdgeVisible)
                    continue;

                _path.SegmentRange(segment, out float start, out float end);
                _runs.Add(new Vector2(start, end));
            }

            _runs.Sort(CompareRunStarts);
            MergeTouchingRuns();
            return false;
        }

        private void MergeTouchingRuns()
        {
            for (int i = _runs.Count - 1; i > 0; i--)
            {
                Vector2 previous = _runs[i - 1];
                Vector2 current = _runs[i];

                if (current.x - previous.y > RunJoinTolerance)
                    continue;

                _runs[i - 1] = new Vector2(previous.x, Mathf.Max(previous.y, current.y));
                _runs.RemoveAt(i);
            }

            if (!IsClosed || _runs.Count < 2)
                return;

            Vector2 first = _runs[0];
            Vector2 last = _runs[_runs.Count - 1];

            if (first.x > RunJoinTolerance || _path.Length - last.y > RunJoinTolerance)
                return;

            _runs[_runs.Count - 1] = new Vector2(last.x, _path.Length + first.y);
            _runs.RemoveAt(0);
        }

        private void AppendTile(EdgeTileSet set, bool flipped, bool trimFringe, in EdgeTile tile, float rangeStart, float rangeEnd)
        {
            Sprite sprite = set.Shape(tile.Role, tile.Variant).Sprite;

            if (sprite == null)
                return;

            EdgeTileArt art = EdgeTileArt.FromSprite(
                sprite,
                set.SurfaceY(tile.Role, tile.Variant),
                set.Scale(tile.Role),
                set.BelowSurfaceScale(tile.Role, tile.Variant),
                flipped);

            if (trimFringe)
                art = art.CropAboveSurface(EarthFringe);
            MeshPart part = AcquirePart(sprite.texture, _sortingOrder);
            int firstVertex = part.Vertices.Count;
            EdgeMeshBuilder.AppendTile(_path, art, tile, 0f, part.Vertices, part.Uvs, part.Colors, part.Triangles, rangeStart, rangeEnd, UsesBlockEdges);
            _tileSpans.Add(new TileSpan(part, firstVertex, part.Vertices.Count));
        }

        private void AppendLayeredTile(in EdgeTile tile, float rangeStart, float rangeEnd)
        {
            float thickness = LayeredThickness;

            if (tile.Role != EdgeTileRole.Middle)
            {
                AppendSlicedEnd(tile, thickness, rangeStart, rangeEnd);
                return;
            }

            int crustVariant = tile.Variant % _crustSet.MiddleVariantCount;
            int underVariant = tile.Variant % _underSet.MiddleVariantCount;
            SpriteShape under = _underSet.Shape(EdgeTileRole.Middle, underVariant);
            float underHeight = under.Bounds.height * _underSet.Scale(EdgeTileRole.Middle).y;
            AppendScaledTile(_crustSet, tile, _crustSet.SurfaceY(EdgeTileRole.Middle, crustVariant), _crustSet.Scale(EdgeTileRole.Middle).y, 0f, rangeStart, rangeEnd, _sortingOrder, crustVariant);
            AppendScaledTile(_underSet, tile, under.Bounds.yMax, _underSet.Scale(EdgeTileRole.Middle).y, underHeight - thickness, rangeStart, rangeEnd, _sortingOrder, underVariant);
            _layeredMiddleStart = Mathf.Min(_layeredMiddleStart, tile.Start);
            _layeredMiddleEnd = Mathf.Max(_layeredMiddleEnd, tile.End);
            _layeredCrustDepth = Mathf.Max(_layeredCrustDepth, (_crustSet.SurfaceY(EdgeTileRole.Middle, crustVariant) - _crustSet.Shape(EdgeTileRole.Middle, crustVariant).Bounds.yMin) * _crustSet.Scale(EdgeTileRole.Middle).y);
            _layeredUnderHeight = Mathf.Max(_layeredUnderHeight, underHeight);
        }

        private void AppendSlicedEnd(in EdgeTile tile, float thickness, float rangeStart, float rangeEnd)
        {
            Sprite sprite = _tileSet.Shape(tile.Role, tile.Variant).Sprite;

            if (sprite == null)
                return;

            float surfaceY = _tileSet.SurfaceY(tile.Role, tile.Variant);
            float below = _tileSet.BelowSurfaceScale(tile.Role, tile.Variant);
            EdgeTileArt full = EdgeTileArt.FromSprite(sprite, surfaceY, _tileSet.Scale(tile.Role), below);
            Rect local = full.LocalRect;
            float depth = (surfaceY - local.yMin) * below;
            float extra = thickness - (surfaceY - _tileSet.Shape(tile.Role, tile.Variant).Bounds.yMin) * below;

            if (extra <= 0f || depth <= 0f)
            {
                AppendArt(sprite, full, tile, 0f, rangeStart, rangeEnd);
                return;
            }

            if (!UseLayeredEnds(thickness))
            {
                float whole = thickness / (thickness - extra);
                AppendArt(sprite, EdgeTileArt.FromSprite(sprite, surfaceY, _tileSet.Scale(tile.Role), below * whole), tile, 0f, rangeStart, rangeEnd);
                return;
            }

            SpriteShape crust = _crustSet.Shape(tile.Role, tile.Variant);
            SpriteShape under = _underSet.Shape(tile.Role, tile.Variant);

            float crustDepth = (_crustSet.SurfaceY(tile.Role, tile.Variant) - crust.Bounds.yMin) * _crustSet.Scale(tile.Role).y;
            float underHeight = under.Bounds.height * _underSet.Scale(tile.Role).y;
            AppendScaledTile(_crustSet, tile, _crustSet.SurfaceY(tile.Role, tile.Variant), _crustSet.Scale(tile.Role).y, 0f, rangeStart, rangeEnd, _sortingOrder);
            AppendScaledTile(_underSet, tile, under.Bounds.yMax, _underSet.Scale(tile.Role).y, underHeight - thickness, rangeStart, rangeEnd, _sortingOrder);
            AppendEndSide(tile, surfaceY - crustDepth * EndCrustOverlap / below, crustDepth * EndCrustOverlap, thickness - underHeight * EndUnderOverlap);
            _layeredCrustDepth = Mathf.Max(_layeredCrustDepth, crustDepth);
            _layeredUnderHeight = Mathf.Max(_layeredUnderHeight, underHeight);
        }

        private bool UseLayeredEnds(float thickness)
        {
            if (_theme == null || _theme.PlatformEndSide == null || !_tileSet.HasEnds || !_crustSet.HasEnds || !_underSet.HasEnds)
                return false;

            float depth = Mathf.Max(VisibleDepth(EdgeTileRole.Left), VisibleDepth(EdgeTileRole.Right));
            return thickness - depth > depth * MaxWholeEndStretch;
        }

        private float VisibleDepth(EdgeTileRole role) =>
            (_tileSet.SurfaceY(role, 0) - _tileSet.Shape(role, 0).Bounds.yMin) * _tileSet.BelowSurfaceScale(role, 0);

        protected EndSlice EndSliceFor(in EdgeTile tile, float thickness)
        {
            SpriteShape shape = _tileSet.Shape(tile.Role, tile.Variant);

            if (shape == null || shape.Sprite == null)
                return default;

            float surfaceY = _tileSet.SurfaceY(tile.Role, tile.Variant);
            float below = _tileSet.BelowSurfaceScale(tile.Role, tile.Variant);
            float visibleDepth = (surfaceY - shape.Bounds.yMin) * below;
            float extra = thickness - visibleDepth;
            SpriteShape crust = _crustSet.Shape(tile.Role, tile.Variant);
            SpriteShape under = _underSet.Shape(tile.Role, tile.Variant);

            if (!UseLayeredEnds(thickness))
                return extra > 0f ? new EndSlice(0f, visibleDepth, extra) : default;

            float crustDepth = (_crustSet.SurfaceY(tile.Role, tile.Variant) - crust.Bounds.yMin) * _crustSet.Scale(tile.Role).y;
            float underHeight = under.Bounds.height * _underSet.Scale(tile.Role).y;
            return new EndSlice(crustDepth * EndCrustOverlap, visibleDepth - underHeight * EndUnderOverlap, extra, true);
        }

        private void AppendEndSide(in EdgeTile tile, float edgeY, float topDepth, float bottomDepth)
        {
            Sprite side = _theme.PlatformEndSide;
            float scale = _theme.PlatformEndSideScale;
            bool left = tile.Role == EdgeTileRole.Left;
            SpriteShape shape = _tileSet.Shape(tile.Role, tile.Variant);
            float horizontal = _tileSet.Scale(tile.Role).x * tile.FitScale;
            float edgeX = shape.EdgeX(edgeY, left);

            if (float.IsNaN(edgeX))
                edgeX = left ? shape.Bounds.xMin : shape.Bounds.xMax;

            Rect local = EdgeTileArt.FromSprite(shape.Sprite, 0f, Vector2.one, 1f).LocalRect;
            float distance = tile.Center + (edgeX - local.center.x) * horizontal;
            _path.Evaluate(distance, out Vector2 point, out Vector2 normal);
            Vector2 inward = left ? new Vector2(normal.y, -normal.x) : new Vector2(-normal.y, normal.x);
            float width = side.rect.width / side.pixelsPerUnit * scale;
            float tileHeight = side.rect.height / side.pixelsPerUnit * scale;
            Rect uv = SpriteUv(side);
            MeshPart part = AcquirePart(side.texture, _sortingOrder);
            var white = new Color32(255, 255, 255, 255);
            float from = topDepth - EndSideOverlap;
            float to = bottomDepth + EndSideOverlap;

            for (float depth = from; depth < to - SliceEpsilon; depth += tileHeight)
            {
                float next = Mathf.Min(depth + tileHeight, to);
                float vBottom = Mathf.Lerp(uv.yMax, uv.yMin, (next - depth) / tileHeight);
                Vector2 outerTop = point - normal * depth;
                Vector2 outerBottom = point - normal * next;
                int first = part.Vertices.Count;
                part.Vertices.Add(outerTop);
                part.Vertices.Add(outerTop + inward * width);
                part.Vertices.Add(outerBottom + inward * width);
                part.Vertices.Add(outerBottom);
                part.Uvs.Add(new Vector2(uv.xMax, uv.yMax));
                part.Uvs.Add(new Vector2(uv.xMin, uv.yMax));
                part.Uvs.Add(new Vector2(uv.xMin, vBottom));
                part.Uvs.Add(new Vector2(uv.xMax, vBottom));

                for (int i = 0; i < 4; i++)
                    part.Colors.Add(white);

                part.Triangles.Add(first);
                part.Triangles.Add(first + 1);
                part.Triangles.Add(first + 2);
                part.Triangles.Add(first);
                part.Triangles.Add(first + 2);
                part.Triangles.Add(first + 3);
            }

            float inner = distance + (left ? width : -width) * EndFillReach;
            _layeredMiddleStart = Mathf.Min(_layeredMiddleStart, inner);
            _layeredMiddleEnd = Mathf.Max(_layeredMiddleEnd, inner);
        }

        private static Rect SpriteUv(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            return Rect.MinMaxRect(rect.xMin / texture.width, rect.yMin / texture.height, rect.xMax / texture.width, rect.yMax / texture.height);
        }

        private void AppendArt(Sprite sprite, in EdgeTileArt art, in EdgeTile tile, float surfaceOffset, float rangeStart, float rangeEnd)
        {
            MeshPart part = AcquirePart(sprite.texture, _sortingOrder);
            int firstVertex = part.Vertices.Count;
            EdgeMeshBuilder.AppendTile(_path, art, tile, surfaceOffset, part.Vertices, part.Uvs, part.Colors, part.Triangles, rangeStart, rangeEnd);
            _tileSpans.Add(new TileSpan(part, firstVertex, part.Vertices.Count));
        }

        private void AppendScaledTile(EdgeTileSet set, in EdgeTile tile, float surfaceY, float belowScale, float surfaceOffset, float rangeStart, float rangeEnd, int sortingOrder, int variant = -1)
        {
            Sprite sprite = set.Shape(tile.Role, variant >= 0 ? variant : tile.Variant).Sprite;

            if (sprite == null)
                return;

            EdgeTileArt art = EdgeTileArt.FromSprite(sprite, surfaceY, set.Scale(tile.Role), belowScale);
            MeshPart part = AcquirePart(sprite.texture, sortingOrder);
            int firstVertex = part.Vertices.Count;
            EdgeMeshBuilder.AppendTile(_path, art, tile, surfaceOffset, part.Vertices, part.Uvs, part.Colors, part.Triangles, rangeStart, rangeEnd);
            _tileSpans.Add(new TileSpan(part, firstVertex, part.Vertices.Count));
        }

        protected bool TryGetLayeredBody(out float start, out float end, out float crustDepth, out float underHeight)
        {
            start = _layeredMiddleStart;
            end = _layeredMiddleEnd;
            crustDepth = _layeredCrustDepth;
            underHeight = _layeredUnderHeight;
            return Layered && end > start;
        }

        private void BuildFill()
        {
            Texture2D texture = _theme != null ? _theme.Fill : null;

            if (texture == null || !_path.IsValid)
                return;

            BuildFillOutline(_outline);

            if (_outline.Count < 3)
                return;

            MeshPart part = AcquirePart(texture, _sortingOrder + _fillSortingOffset);
            int firstVertex = part.Vertices.Count;
            Vector2 tileSize = new Vector2(texture.width, texture.height) / _theme.FillPixelsPerUnit;
            var white = new Color32(255, 255, 255, 255);

            for (int i = 0; i < _outline.Count; i++)
            {
                Vector2 local = _outline[i];
                Vector3 world = transform.TransformPoint(local);
                part.Vertices.Add(local);
                part.Uvs.Add(new Vector2(world.x / tileSize.x, world.y / tileSize.y));
                part.Colors.Add(white);
            }

            PolygonTriangulator.Triangulate(_outline, firstVertex, part.Triangles, _triangulationScratch);
        }

        protected void CollectColliderSections(List<EdgeSection> sections)
        {
            float wallAngle = _theme != null ? _theme.WallAngle : DefaultWallAngle;
            float ceilingAngle = _theme != null ? _theme.CeilingAngle : DefaultCeilingAngle;
            float minLength = _theme != null ? _theme.MinSectionLength : DefaultMinSectionLength;
            EdgeSections.Build(_path, 0f, _path.Length, IsClosed, wallAngle, ceilingAngle, minLength, sections);
        }

        private void UpdateCollider()
        {
            if (!ResolveMainCollider())
                return;

            _colliderChains.Clear();
            BuildColliderChains(_colliderChains);

            int rootChain = _colliderChains.IndexOf(EdgeKind.Top);
            ApplyChain(_collider, rootChain, MaterialFor(EdgeKind.Top), RootUsesEffector);

            if (_secondaryCollider != null)
                _secondaryCollider.enabled = false;

            ApplyHolder(EdgeKind.Top, rootChain);
            ApplyHolder(EdgeKind.Wall, rootChain);
            ApplyHolder(EdgeKind.Ceiling, rootChain);
        }

        private bool ResolveMainCollider()
        {
            if (_collider != null && _collider != _secondaryCollider)
                return true;

            GetComponents(_edgeColliders);

            for (int i = 0; i < _edgeColliders.Count; i++)
            {
                if (_edgeColliders[i] == _secondaryCollider)
                    continue;

                _collider = _edgeColliders[i];
                return true;
            }

            return false;
        }

        private void ApplyHolder(EdgeKind kind, int rootChain)
        {
            int needed = _colliderChains.CountOf(kind) - (rootChain >= 0 && _colliderChains[rootChain].Kind == kind ? 1 : 0);
            GameObject holder = FindColliderHolder(kind);

            if (needed <= 0)
            {
                if (holder != null)
                    DestroyGenerated(holder);

                return;
            }

            if (holder == null)
                holder = CreateColliderHolder(kind);

            holder.layer = ResolveLayer(kind);

            if (kind == EdgeKind.Top)
                SyncSurfaceProfile(holder);
            holder.GetComponents(_holderColliders);

            while (_holderColliders.Count < needed)
                _holderColliders.Add(holder.AddComponent<EdgeCollider2D>());

            for (int i = _holderColliders.Count - 1; i >= needed; i--)
            {
                DestroyGenerated(_holderColliders[i]);
                _holderColliders.RemoveAt(i);
            }

            int slot = 0;

            for (int chain = 0; chain < _colliderChains.Count; chain++)
            {
                if (chain != rootChain && _colliderChains[chain].Kind == kind)
                    ApplyChain(_holderColliders[slot++], chain, MaterialFor(kind), false);
            }
        }

        private GameObject FindColliderHolder(EdgeKind kind)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if ((child.hideFlags & HideFlags.DontSave) != 0 && child.name == HolderName(kind))
                    return child;
            }

            return null;
        }

        private GameObject CreateColliderHolder(EdgeKind kind)
        {
            var holder = new GameObject(HolderName(kind)) { hideFlags = HideFlags.DontSave | HideFlags.NotEditable };
            holder.transform.SetParent(transform, false);
            return holder;
        }

        private static string HolderName(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall:
                    return ColliderHolderPrefix + "Wall";
                case EdgeKind.Ceiling:
                    return ColliderHolderPrefix + "Ceiling";
                default:
                    return ColliderHolderPrefix + "Top";
            }
        }

        private int ResolveLayer(EdgeKind kind)
        {
            int layer = _theme != null ? _theme.LayerFor(kind) : CollisionLayerAttribute.SameAsSurface;
            return layer >= 0 ? layer : gameObject.layer;
        }

        private PhysicsMaterial2D MaterialFor(EdgeKind kind) => _theme != null ? _theme.MaterialFor(kind) : null;

        private void ApplyChain(EdgeCollider2D edgeCollider, int chain, PhysicsMaterial2D material, bool usedByEffector)
        {
            edgeCollider.enabled = chain >= 0;

            if (!edgeCollider.enabled)
                return;

            _colliderChains.CopyPoints(chain, _chainPoints);
            edgeCollider.SetPoints(_chainPoints);
            edgeCollider.edgeRadius = _edgeRadius;
            edgeCollider.sharedMaterial = material;
            edgeCollider.usedByEffector = usedByEffector;
            edgeCollider.useAdjacentStartPoint = _colliderChains.TryGetAdjacent(chain, true, out Vector2 adjacentStart);
            edgeCollider.adjacentStartPoint = adjacentStart;
            edgeCollider.useAdjacentEndPoint = _colliderChains.TryGetAdjacent(chain, false, out Vector2 adjacentEnd);
            edgeCollider.adjacentEndPoint = adjacentEnd;
        }

        private void CollectParts()
        {
            bool alive = _parts.Count > 0;

            for (int i = 0; i < _parts.Count && alive; i++)
                alive = _parts[i].Owner != null;

            if (alive)
                return;

            _parts.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;

                if (!IsGenerated(child) || !child.TryGetComponent(out MeshFilter filter) || !child.TryGetComponent(out MeshRenderer meshRenderer))
                    continue;

                if (filter.sharedMesh != null && !IsOwnMesh(filter.sharedMesh))
                    filter.sharedMesh = null;

                _parts.Add(new MeshPart(child, filter, meshRenderer));
            }
        }

        private string OwnMeshName => _ownMeshName ??= GeneratedMeshName + GetInstanceID();

        private bool IsOwnMesh(Mesh mesh) => (mesh.hideFlags & HideFlags.DontSave) != 0 && mesh.name == OwnMeshName;

        private MeshPart AcquirePart(Texture texture, int sortingOrder)
        {
            for (int i = 0; i < _usedParts; i++)
            {
                if (_parts[i].Texture == texture && _parts[i].SortingOrder == sortingOrder)
                    return _parts[i];
            }

            if (_usedParts == _parts.Count)
                _parts.Add(CreatePart(_parts.Count));

            MeshPart part = _parts[_usedParts++];
            part.Begin(texture, sortingOrder);
            return part;
        }

        private MeshPart CreatePart(int index)
        {
            var child = new GameObject(GeneratedPrefix + index)
            {
                hideFlags = HideFlags.DontSave | HideFlags.NotEditable,
                layer = gameObject.layer
            };
            child.transform.SetParent(transform, false);
            return new MeshPart(child, child.AddComponent<MeshFilter>(), child.AddComponent<MeshRenderer>());
        }

        private void FinishParts()
        {
            Material material = ResolveMaterial();

            for (int i = 0; i < _usedParts; i++)
                UploadPart(_parts[i], material);

            while (_parts.Count > _usedParts)
            {
                int last = _parts.Count - 1;
                DestroyPartMesh(_parts[last]);
                DestroyGenerated(_parts[last].Owner);
                _parts.RemoveAt(last);
            }
        }

        private void UploadPart(MeshPart part, Material material)
        {
            Mesh mesh = part.Filter.sharedMesh;

            if (mesh == null)
            {
                mesh = new Mesh { name = OwnMeshName, hideFlags = HideFlags.DontSave };
                mesh.MarkDynamic();
                part.Filter.sharedMesh = mesh;
            }

            mesh.Clear();
            mesh.indexFormat = part.Vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(part.Vertices);
            mesh.SetUVs(0, part.Uvs);
            mesh.SetColors(part.Colors);
            mesh.SetTriangles(part.Triangles, 0);
            mesh.RecalculateBounds();

            part.Owner.layer = gameObject.layer;

            MeshRenderer meshRenderer = part.Renderer;
            meshRenderer.sharedMaterial = material;
            meshRenderer.sortingLayerID = _sortingLayerId;
            meshRenderer.sortingOrder = part.SortingOrder;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.Clear();
            _propertyBlock.SetTexture(MainTexId, part.Texture);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        private Material ResolveMaterial()
        {
            if (_material != null)
                return _material;

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;

            if (pipeline != null && pipeline.default2DMaterial != null)
                return pipeline.default2DMaterial;

            _fallbackMaterial ??= new Material(Shader.Find(FallbackShaderName)) { hideFlags = HideFlags.DontSave };
            return _fallbackMaterial;
        }

        private void ApplyGroundLayer()
        {
            int groundLayer = LayerMask.NameToLayer(GroundLayerName);

            if (groundLayer >= 0 && gameObject.layer != groundLayer)
                gameObject.layer = groundLayer;
        }

        private void DestroyPartMesh(MeshPart part)
        {
            if (part.Filter != null && part.Filter.sharedMesh != null && IsOwnMesh(part.Filter.sharedMesh))
                DestroyGenerated(part.Filter.sharedMesh);
        }

        private static void DestroyGenerated(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        private static bool IsGenerated(GameObject candidate) =>
            (candidate.hideFlags & HideFlags.DontSave) != 0
            && candidate.name.StartsWith(GeneratedPrefix, StringComparison.Ordinal);

        private enum EdgeArt
        {
            Main,
            Crust,
            Earth,
            WallSprite
        }

        private readonly struct SectionTrim
        {
            public SectionTrim(float start, float end, bool cornerAtStart, bool cornerAtEnd)
            {
                Start = start;
                End = end;
                CornerAtStart = cornerAtStart;
                CornerAtEnd = cornerAtEnd;
            }

            public float Start { get; }

            public float End { get; }

            public bool CornerAtStart { get; }

            public bool CornerAtEnd { get; }

            public SectionTrim WithStart(float amount) => new SectionTrim(amount, End, true, CornerAtEnd);

            public SectionTrim WithEnd(float amount) => new SectionTrim(Start, amount, CornerAtStart, true);
        }

        private readonly struct CornerPlacement
        {
            public CornerPlacement(Sprite sprite, EdgeCornerArt art, Vector2 corner, Vector2 alongTop, Vector2 alongFace, float topSpan, float wallSpan)
            {
                Sprite = sprite;
                Art = art;
                Corner = corner;
                AlongTop = alongTop;
                AlongFace = alongFace;
                TopSpan = topSpan;
                WallSpan = wallSpan;
            }

            public Sprite Sprite { get; }

            public EdgeCornerArt Art { get; }

            public Vector2 Corner { get; }

            public Vector2 AlongTop { get; }

            public Vector2 AlongFace { get; }

            public float TopSpan { get; }

            public float WallSpan { get; }
        }

        private readonly struct TileSpan
        {
            public TileSpan(MeshPart part, int first, int end)
            {
                Part = part;
                First = first;
                End = end;
            }

            public MeshPart Part { get; }

            public int First { get; }

            public int End { get; }
        }

        private sealed class MeshPart
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<Vector2> _uvs = new List<Vector2>();
            private readonly List<Color32> _colors = new List<Color32>();
            private readonly List<int> _triangles = new List<int>();

            public MeshPart(GameObject owner, MeshFilter filter, MeshRenderer meshRenderer)
            {
                Owner = owner;
                Filter = filter;
                Renderer = meshRenderer;
            }

            public GameObject Owner { get; }

            public MeshFilter Filter { get; }

            public MeshRenderer Renderer { get; }

            public Texture Texture { get; private set; }

            public int SortingOrder { get; private set; }

            public List<Vector3> Vertices => _vertices;

            public List<Vector2> Uvs => _uvs;

            public List<Color32> Colors => _colors;

            public List<int> Triangles => _triangles;

            public void Begin(Texture texture, int sortingOrder)
            {
                Texture = texture;
                SortingOrder = sortingOrder;
                _vertices.Clear();
                _uvs.Clear();
                _colors.Clear();
                _triangles.Clear();
            }
        }
    }
}
