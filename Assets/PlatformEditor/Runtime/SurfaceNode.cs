using System;
using UnityEngine;

namespace Level.Platforms
{
    public enum SurfaceNodeType
    {
        Corner,
        Smooth,
        Bezier
    }

    public enum SurfaceTangentMode
    {
        Free,
        Aligned,
        Symmetric
    }

    public enum SurfaceEdgeKind
    {
        Auto,
        Top,
        Wall,
        Ceiling
    }

    [Serializable]
    public sealed class SurfaceNode
    {
        public const float MinEdgeScale = 0.5f;
        public const float MaxEdgeScale = 2f;

        private const float TangentEpsilon = 0.000001f;

        [SerializeField] private Vector2 _position;
        [SerializeField] private SurfaceNodeType _type = SurfaceNodeType.Smooth;
        [SerializeField] private SurfaceTangentMode _tangentMode = SurfaceTangentMode.Symmetric;
        [SerializeField] private Vector2 _inTangent = Vector2.left;
        [SerializeField] private Vector2 _outTangent = Vector2.right;
        [SerializeField, Range(MinEdgeScale, MaxEdgeScale)] private float _edgeScale = 1f;
        [SerializeField] private bool _edgeVisible = true;
        [SerializeField] private SurfaceEdgeKind _edgeKind = SurfaceEdgeKind.Auto;
        [SerializeField] private bool _cornerDecorAllowed = true;

        public SurfaceNode()
        {
        }

        public SurfaceNode(Vector2 position, SurfaceNodeType type)
        {
            _position = position;
            _type = type;
        }

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public SurfaceNodeType Type
        {
            get => _type;
            set => _type = value;
        }

        public SurfaceTangentMode TangentMode => _tangentMode;

        public Vector2 InTangent => _inTangent;

        public Vector2 OutTangent => _outTangent;

        public float EdgeScale
        {
            get => _edgeScale;
            set => _edgeScale = Mathf.Clamp(value, MinEdgeScale, MaxEdgeScale);
        }

        public bool EdgeVisible
        {
            get => _edgeVisible;
            set => _edgeVisible = value;
        }

        public SurfaceEdgeKind EdgeKind
        {
            get => _edgeKind;
            set => _edgeKind = value;
        }

        public bool CornerDecorAllowed
        {
            get => _cornerDecorAllowed;
            set => _cornerDecorAllowed = value;
        }

        public void SetTangentMode(SurfaceTangentMode mode)
        {
            _tangentMode = mode;
            ConstrainOpposite(_outTangent, ref _inTangent);
        }

        public void SetOutTangent(Vector2 tangent)
        {
            _outTangent = tangent;
            ConstrainOpposite(_outTangent, ref _inTangent);
        }

        public void SetInTangent(Vector2 tangent)
        {
            _inTangent = tangent;
            ConstrainOpposite(_inTangent, ref _outTangent);
        }

        public SurfaceNode Clone() => (SurfaceNode)MemberwiseClone();

        private void ConstrainOpposite(Vector2 driver, ref Vector2 opposite)
        {
            if (_tangentMode == SurfaceTangentMode.Free || driver.sqrMagnitude < TangentEpsilon)
                return;

            float length = _tangentMode == SurfaceTangentMode.Symmetric ? driver.magnitude : opposite.magnitude;
            opposite = -driver.normalized * length;
        }
    }
}
