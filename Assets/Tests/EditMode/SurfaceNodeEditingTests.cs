using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SurfaceNodeEditingTests
    {
        private const float Tolerance = 0.0001f;

        private GameObject _owner;

        [TearDown]
        public void TearDown()
        {
            if (_owner != null)
                Object.DestroyImmediate(_owner);
        }

        [Test]
        public void InsertNode_AddsNodeAtIndex()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            platform.InsertNode(1, new Vector2(0f, 2f), SurfaceNodeType.Smooth);

            Assert.AreEqual(3, platform.NodeCount);
            AssertVector(new Vector2(0f, 2f), platform.GetNode(1).Position);
        }

        [Test]
        public void RemoveNode_AtMinimum_IsRefused()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            bool removed = platform.RemoveNode(0);

            Assert.IsFalse(removed);
            Assert.AreEqual(platform.MinNodeCount, platform.NodeCount);
        }

        [Test]
        public void RemoveNode_AboveMinimum_Removes()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            platform.InsertNode(1, new Vector2(0f, 2f), SurfaceNodeType.Smooth);

            bool removed = platform.RemoveNode(1);

            Assert.IsTrue(removed);
            Assert.AreEqual(2, platform.NodeCount);
        }

        [Test]
        public void SetNodeType_ToBezier_CreatesSymmetricTangentsAlongNeighbours()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            platform.SetNodeType(0, SurfaceNodeType.Bezier);

            SurfaceNode node = platform.GetNode(0);
            Assert.AreEqual(SurfaceTangentMode.Symmetric, node.TangentMode);
            AssertVector(new Vector2(4f, 0f), node.OutTangent);
            AssertVector(new Vector2(-4f, 0f), node.InTangent);
        }

        [Test]
        public void InsertNode_AfterHiddenSegment_StaysHidden()
        {
            GroundBlock block = Create<GroundBlock>();

            block.InsertNode(1, new Vector2(0f, -4f), SurfaceNodeType.Corner);

            Assert.IsFalse(block.GetNode(1).EdgeVisible);
        }

        [Test]
        public void FindSegmentAt_MiddleOfEachSegment_ReturnsThatSegment()
        {
            GroundBlock block = Create<GroundBlock>();
            SurfacePath path = block.Path;

            for (int segment = 0; segment < path.SegmentCount; segment++)
            {
                path.SegmentRange(segment, out float start, out float end);
                float middle = Mathf.Repeat((start + end) * 0.5f, path.Length);

                Assert.AreEqual(segment, block.FindSegmentAt(middle));
            }
        }

        [Test]
        public void TiledPlatform_DrawnRightToLeft_StillFacesUp()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            platform.SetNodePosition(0, new Vector2(3f, 0f));
            platform.SetNodePosition(1, new Vector2(-3f, 0f));

            platform.Path.Evaluate(platform.Path.Length * 0.5f, out _, out Vector2 normal);

            Assert.Greater(normal.y, 0.99f);
        }

        [Test]
        public void NewTiledPlatform_EdgeByAngle_IsOffByDefault()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            Assert.IsFalse(EdgeByAngle(platform));
        }

        [Test]
        public void NewGroundBlock_EdgeByAngle_IsOnByDefault()
        {
            GroundBlock block = Create<GroundBlock>();

            Assert.IsTrue(EdgeByAngle(block));
        }

        [Test]
        public void TiledPlatform_NegativeColliderOffset_LowersTopAndKeepsBottom()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            float bottomBefore = ChainY(platform, "[Generated] Collider Ceiling");

            typeof(SurfaceBase)
                .GetField("_colliderOffset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(platform, -0.2f);
            platform.Rebuild();

            Assert.AreEqual(-0.2f, platform.GetComponent<EdgeCollider2D>().points[1].y, Tolerance);
            Assert.AreEqual(bottomBefore, ChainY(platform, "[Generated] Collider Ceiling"), Tolerance);
        }

        [Test]
        public void ThickPlatform_Collider_IsPlatformShapedWithBottomAtThickness()
        {
            TiledPlatform platform = CreateThick(2f);

            Assert.AreEqual(0f, platform.GetComponent<EdgeCollider2D>().points[1].y, Tolerance);
            Assert.AreEqual(-2f, ChainY(platform, "[Generated] Collider Ceiling"), Tolerance);
            Assert.AreEqual(2, platform.transform.Find("[Generated] Collider Wall").GetComponents<EdgeCollider2D>().Length);
        }

        [Test]
        public void ThickPlatform_FindSegmentAt_UsesNodeSegments()
        {
            TiledPlatform platform = CreateThick(2f);
            platform.InsertNode(1, new Vector2(0f, 0f), SurfaceNodeType.Corner);

            Assert.AreEqual(0, platform.FindSegmentAt(4f));
            Assert.AreEqual(1, platform.FindSegmentAt(12f));
        }

        [Test]
        public void ThickPlatform_SetBodyThickness_ClampsToMinimum()
        {
            TiledPlatform platform = CreateThick(2f);

            platform.SetBodyThickness(0f);

            Assert.Greater(platform.BodyThickness, 0.5f);
        }

        [Test]
        public void PassablePlatform_Flag_AddsOneWayEffectorAndKeepsOnlyTopCollider()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            SetPassable(platform, true);

            Assert.IsTrue(platform.TryGetComponent(out PlatformEffector2D effector));
            Assert.IsTrue(effector.useOneWay);
            Assert.IsTrue(platform.TryGetComponent(out PassablePlatform _));
            Assert.IsTrue(platform.GetComponent<EdgeCollider2D>().usedByEffector);
            Assert.IsNull(platform.transform.Find("[Generated] Collider Wall"));
            Assert.IsNull(platform.transform.Find("[Generated] Collider Ceiling"));
        }

        [Test]
        public void PassablePlatform_FlagOff_RemovesEffectorAndRestoresSolidCollider()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            SetPassable(platform, true);

            SetPassable(platform, false);

            Assert.IsFalse(platform.TryGetComponent(out PlatformEffector2D _));
            Assert.IsFalse(platform.TryGetComponent(out PassablePlatform _));
            Assert.IsFalse(platform.GetComponent<EdgeCollider2D>().usedByEffector);
            Assert.IsNotNull(platform.transform.Find("[Generated] Collider Ceiling"));
        }

        [Test]
        public void PassablePlatform_Activate_LetsPlayerThrough()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            SetPassable(platform, true);
            int player = LayerMask.NameToLayer("Player");

            platform.GetComponent<PassablePlatform>().ActivateTrap();

            Assert.AreEqual(0, platform.GetComponent<PlatformEffector2D>().colliderMask & (1 << player));
            Assert.IsTrue(platform.GetComponent<PassablePlatform>().IsPassing);
        }

        [Test]
        public void SurfaceProfile_FromTheme_IsPutOnTopCollider()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            SurfaceProfile ice = ScriptableObject.CreateInstance<SurfaceProfile>();
            GroundTheme theme = ScriptableObject.CreateInstance<GroundTheme>();
            SetPrivate(theme, typeof(GroundTheme), "_surfaceProfile", ice);
            SetPrivate(platform, typeof(SurfaceBase), "_theme", theme);

            platform.Rebuild();

            Assert.AreSame(ice, platform.GetComponent<GroundSurface>().Profile);
            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(ice);
        }

        [Test]
        public void SurfaceProfile_OnSurface_OverridesTheme()
        {
            TiledPlatform platform = Create<TiledPlatform>();
            SurfaceProfile ice = ScriptableObject.CreateInstance<SurfaceProfile>();
            SurfaceProfile swamp = ScriptableObject.CreateInstance<SurfaceProfile>();
            GroundTheme theme = ScriptableObject.CreateInstance<GroundTheme>();
            SetPrivate(theme, typeof(GroundTheme), "_surfaceProfile", ice);
            SetPrivate(platform, typeof(SurfaceBase), "_theme", theme);
            SetPrivate(platform, typeof(SurfaceBase), "_surfaceProfile", swamp);

            platform.Rebuild();

            Assert.AreSame(swamp, platform.GetComponent<GroundSurface>().Profile);
            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(ice);
            Object.DestroyImmediate(swamp);
        }

        [Test]
        public void SurfaceProfile_None_LeavesNoGroundSurface()
        {
            TiledPlatform platform = Create<TiledPlatform>();

            Assert.IsFalse(platform.TryGetComponent(out GroundSurface _));
        }

        private static void SetPrivate(object target, System.Type type, string field, object value) =>
            type.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(target, value);

        private static void SetPassable(TiledPlatform platform, bool passable)
        {
            typeof(TiledPlatform)
                .GetField("_passable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(platform, passable);
            platform.Rebuild();
        }

        private TiledPlatform CreateThick(float thickness)
        {
            TiledPlatform platform = Create<TiledPlatform>();
            typeof(TiledPlatform)
                .GetField("_thick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(platform, true);
            platform.SetBodyThickness(thickness);
            return platform;
        }

        private static float ChainY(SurfaceBase surface, string holder) =>
            surface.transform.Find(holder).GetComponent<EdgeCollider2D>().points[0].y;

        private static bool EdgeByAngle(SurfaceBase surface) =>
            (bool)typeof(SurfaceBase)
                .GetField("_edgeByAngle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(surface);

        private T Create<T>() where T : SurfaceBase
        {
            _owner = new GameObject(typeof(T).Name);
            T surface = _owner.AddComponent<T>();
            surface.Rebuild();
            return surface;
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
