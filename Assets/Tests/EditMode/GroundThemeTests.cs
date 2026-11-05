using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class GroundThemeTests
    {
        private GroundTheme _base;
        private GroundTheme _variant;
        private Texture2D _texture;

        [SetUp]
        public void SetUp()
        {
            _base = ScriptableObject.CreateInstance<GroundTheme>();
            _variant = ScriptableObject.CreateInstance<GroundTheme>();
            _texture = new Texture2D(4, 4);
            Set(_variant, "_base", _base);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_base);
            Object.DestroyImmediate(_variant);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void Variant_EmptyReference_TakesBase()
        {
            Set(_base, "_fill", _texture);

            Assert.AreSame(_texture, _variant.Fill);
        }

        [Test]
        public void Variant_OwnReference_WinsOverBase()
        {
            var own = new Texture2D(2, 2);
            Set(_base, "_fill", _texture);
            Set(_variant, "_fill", own);

            Assert.AreSame(own, _variant.Fill);
            Object.DestroyImmediate(own);
        }

        [Test]
        public void Variant_AnglesWithoutOverride_TakeBase()
        {
            Set(_base, "_wallAngle", 30f);
            Set(_variant, "_wallAngle", 60f);

            Assert.AreEqual(30f, _variant.WallAngle);
        }

        [Test]
        public void Variant_AnglesWithOverride_UseOwn()
        {
            Set(_base, "_wallAngle", 30f);
            Set(_variant, "_wallAngle", 60f);
            Set(_variant, "_overrideAngles", true);

            Assert.AreEqual(60f, _variant.WallAngle);
        }

        [Test]
        public void Variant_CyclicBase_DoesNotLoop()
        {
            Set(_base, "_base", _variant);
            Set(_base, "_fill", _texture);

            Assert.IsNull(_variant.Fill);
            Assert.IsTrue(_variant.DependsOn(_base));
        }

        private static void Set(GroundTheme theme, string field, object value) =>
            typeof(GroundTheme).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(theme, value);
    }
}
