using UnityEngine;

namespace Level.Platforms
{
    public enum BlockWallArt
    {
        WallSprite,
        Crust,
        Earth
    }

    public enum BlockBottomArt
    {
        Earth,
        Crust
    }

    [CreateAssetMenu(fileName = "GroundTheme", menuName = "Level/Ground Theme")]
    public sealed class GroundTheme : ScriptableObject
    {
        private const int MaxBaseDepth = 8;

        [Header("Вариант биома")]
        [SerializeField] private GroundTheme _base;
        [SerializeField] private bool _overrideBlockArt;
        [SerializeField] private bool _overrideAngles;
        [SerializeField] private bool _overrideCollision;
        [SerializeField] private bool _overrideFit;

        [Header("Платформа и скруглённый край")]
        [SerializeField] private Sprite _left;
        [SerializeField] private Sprite[] _middle = new Sprite[1];
        [SerializeField] private Sprite _right;

        [Header("Торец толстой платформы")]
        [SerializeField] private Sprite _platformEndSide;
        [SerializeField, Min(0.01f)] private float _platformEndSideScale = 0.5f;

        [Header("Корка блока")]
        [SerializeField] private Sprite _crustLeft;
        [SerializeField] private Sprite[] _crustMiddle = new Sprite[0];
        [SerializeField] private Sprite _crustRight;

        [Header("Низ блока")]
        [SerializeField] private Sprite _underLeft;
        [SerializeField] private Sprite[] _underMiddle = new Sprite[0];
        [SerializeField] private Sprite _underRight;

        [Header("Блок")]
        [SerializeField] private Sprite _sharpCornerLeft;
        [SerializeField] private Sprite _sharpCornerRight;
        [SerializeField] private Sprite _wall;
        [SerializeField] private BlockWallArt _blockWallArt = BlockWallArt.WallSprite;
        [SerializeField] private BlockBottomArt _blockBottomArt = BlockBottomArt.Earth;

        [Header("Заливка")]
        [SerializeField] private Texture2D _fill;
        [SerializeField, Min(1f)] private float _fillPixelsPerUnit = 200f;

        [Header("Кромка по углу")]
        [SerializeField, Range(0f, 180f)] private float _wallAngle = 45f;
        [SerializeField, Range(0f, 180f)] private float _ceilingAngle = 135f;
        [SerializeField, Min(0f)] private float _minSectionLength = 0.5f;

        [Header("Коллизии")]
        [SerializeField] private PhysicsMaterial2D _groundMaterial;
        [SerializeField] private PhysicsMaterial2D _wallMaterial;
        [SerializeField] private PhysicsMaterial2D _ceilingMaterial;
        [SerializeField] private float _groundColliderOffset;
        [SerializeField] private float _wallColliderOffset;
        [SerializeField] private float _ceilingColliderOffset;
        [SerializeField, CollisionLayer] private int _wallLayer = CollisionLayerAttribute.SameAsSurface;
        [SerializeField, CollisionLayer] private int _ceilingLayer = CollisionLayerAttribute.SameAsSurface;

        [Header("Профиль поверхности")]
        [SerializeField] private SurfaceProfile _surfaceProfile;

        [Header("Подгонка арта")]
        [SerializeField, Min(0f)] private float _surfaceBelowImageTop = 0.265f;
        [SerializeField, Min(0f)] private float _roundCornerOuterInset = 0.235f;
        [SerializeField, Min(0f)] private float _roundCornerWallStart = 2.85f;

        public GroundTheme Base => _base;

        public Sprite Left => Pick(_left, b => b.Left);
        public Sprite[] Middle => PickArray(_middle, b => b.Middle);
        public Sprite Right => Pick(_right, b => b.Right);
        public Sprite PlatformEndSide => Pick(_platformEndSide, b => b.PlatformEndSide);
        public float PlatformEndSideScale => _platformEndSide == null && ValidBase != null ? ValidBase.PlatformEndSideScale : _platformEndSideScale;
        public Sprite CrustLeft => Pick(_crustLeft, b => b.CrustLeft);
        public Sprite[] CrustMiddle => PickArray(_crustMiddle, b => b.CrustMiddle);
        public Sprite CrustRight => Pick(_crustRight, b => b.CrustRight);
        public Sprite UnderLeft => Pick(_underLeft, b => b.UnderLeft);
        public Sprite[] UnderMiddle => PickArray(_underMiddle, b => b.UnderMiddle);
        public Sprite UnderRight => Pick(_underRight, b => b.UnderRight);
        public Sprite SharpCornerLeft => Pick(_sharpCornerLeft, b => b.SharpCornerLeft);
        public Sprite SharpCornerRight => Pick(_sharpCornerRight, b => b.SharpCornerRight);
        public Sprite Wall => Pick(_wall, b => b.Wall);
        public BlockWallArt BlockWallArt => Inherits(_overrideBlockArt) ? ValidBase.BlockWallArt : _blockWallArt;
        public BlockBottomArt BlockBottomArt => Inherits(_overrideBlockArt) ? ValidBase.BlockBottomArt : _blockBottomArt;
        public Texture2D Fill => Pick(_fill, b => b.Fill);
        public float FillPixelsPerUnit => _fill == null && ValidBase != null ? ValidBase.FillPixelsPerUnit : _fillPixelsPerUnit;
        public float WallAngle => Inherits(_overrideAngles) ? ValidBase.WallAngle : _wallAngle;
        public float CeilingAngle => Inherits(_overrideAngles) ? ValidBase.CeilingAngle : _ceilingAngle;
        public float MinSectionLength => Inherits(_overrideAngles) ? ValidBase.MinSectionLength : _minSectionLength;
        public ColliderOffsets ColliderOffsets => Inherits(_overrideCollision) ? ValidBase.ColliderOffsets : new ColliderOffsets(_groundColliderOffset, _wallColliderOffset, _ceilingColliderOffset);
        public float SurfaceBelowImageTop => Inherits(_overrideFit) ? ValidBase.SurfaceBelowImageTop : _surfaceBelowImageTop;
        public float RoundCornerOuterInset => Inherits(_overrideFit) ? ValidBase.RoundCornerOuterInset : _roundCornerOuterInset;
        public float RoundCornerWallStart => Inherits(_overrideFit) ? ValidBase.RoundCornerWallStart : _roundCornerWallStart;
        public SurfaceProfile SurfaceProfile => Pick(_surfaceProfile, b => b.SurfaceProfile);

        public bool DependsOn(GroundTheme other) => DependsOnWithin(other, MaxBaseDepth + 1);

        private GroundTheme ValidBase => _base != null && _base != this && !_base.DependsOnWithin(this, MaxBaseDepth) ? _base : null;

        private bool DependsOnWithin(GroundTheme other, int depth)
        {
            GroundTheme current = this;

            for (int i = 0; i < depth && current != null; i++)
            {
                if (current == other)
                    return true;

                current = current._base;
            }

            return false;
        }

        private bool Inherits(bool overridden) => !overridden && ValidBase != null;

        private T Pick<T>(T own, System.Func<GroundTheme, T> fromBase) where T : Object =>
            own != null || ValidBase == null ? own : fromBase(ValidBase);

        private T[] PickArray<T>(T[] own, System.Func<GroundTheme, T[]> fromBase) where T : Object
        {
            if (HasAny(own) || ValidBase == null)
                return own;

            return fromBase(ValidBase);
        }

        private static bool HasAny<T>(T[] items) where T : Object
        {
            if (items == null)
                return false;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                    return true;
            }

            return false;
        }

        public PhysicsMaterial2D MaterialFor(EdgeKind kind)
        {
            switch (kind)
            {
                case EdgeKind.Wall:
                    return Pick(_wallMaterial, b => b.MaterialFor(EdgeKind.Wall));
                case EdgeKind.Ceiling:
                    return Pick(_ceilingMaterial, b => b.MaterialFor(EdgeKind.Ceiling));
                default:
                    return Pick(_groundMaterial, b => b.MaterialFor(EdgeKind.Top));
            }
        }

        public int LayerFor(EdgeKind kind)
        {
            if (Inherits(_overrideCollision))
                return ValidBase.LayerFor(kind);

            switch (kind)
            {
                case EdgeKind.Wall:
                    return _wallLayer;
                case EdgeKind.Ceiling:
                    return _ceilingLayer;
                default:
                    return CollisionLayerAttribute.SameAsSurface;
            }
        }

        public Sprite FirstMiddle
        {
            get
            {
                Sprite[] middle = Middle;

                if (middle == null)
                    return null;

                for (int i = 0; i < middle.Length; i++)
                {
                    if (middle[i] != null)
                        return middle[i];
                }

                return null;
            }
        }
    }
}
