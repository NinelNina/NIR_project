using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class GroundBlock : SurfaceBase
    {
        private const float DefaultHalfWidth = 4f;
        private const float DefaultDepth = 4f;

        [Header("Блок")]
        [SerializeField] private bool _fill = true;
        [SerializeField, Min(0f)] private float _fillCornerChamfer = 0.4f;

        private readonly List<bool> _chamferSamples = new List<bool>();
        private readonly List<EdgeSection> _colliderSections = new List<EdgeSection>();
        private readonly List<Vector2> _colliderScratch = new List<Vector2>();

        public override bool IsClosed => true;

        protected override bool FillEnabled => _fill;

        protected override bool UsesBlockEdges => true;

        protected override void CreateDefaultNodes(List<SurfaceNode> nodes)
        {
            nodes.Add(HiddenCorner(new Vector2(-DefaultHalfWidth, -DefaultDepth)));
            nodes.Add(HiddenCorner(new Vector2(DefaultHalfWidth, -DefaultDepth)));
            nodes.Add(new SurfaceNode(new Vector2(DefaultHalfWidth, 0f), SurfaceNodeType.Corner));
            nodes.Add(HiddenCorner(new Vector2(-DefaultHalfWidth, 0f)));
        }

        protected override void BuildFillOutline(List<Vector2> outline)
        {
            _chamferSamples.Clear();

            for (int i = 0; i < Path.SampleCount; i++)
                _chamferSamples.Add(false);

            int count = NodeCount;

            for (int node = 0; node < count; node++)
            {
                int sample = Path.NodeSampleIndex(node);
                bool bothVisible = GetNode(node).EdgeVisible && GetNode((node - 1 + count) % count).EdgeVisible;

                if (bothVisible && Path.IsSharpSample(sample) && SurfaceOutline.IsConvex(Path, sample))
                    _chamferSamples[sample] = true;
            }

            SurfaceOutline.ClosedFill(Path, FillInset, _chamferSamples, _fillCornerChamfer, outline);
        }

        protected override void BuildColliderChains(ColliderChainSet chains)
        {
            CollectColliderSections(_colliderSections);
            SurfaceColliderBuilder.ClosedSections(Path, _colliderSections, ColliderOffset, KindOffsets, ColliderShape, ColliderTolerance, Simplifier, _colliderScratch, chains);
        }

        private static SurfaceNode HiddenCorner(Vector2 position) =>
            new SurfaceNode(position, SurfaceNodeType.Corner) { EdgeVisible = false };
    }
}
