using Level.Platforms;
using NUnit.Framework;
using UnityEngine;

namespace Level.Platforms.Tests
{
    public class SurfaceNodeTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void SymmetricMode_SetOutTangent_MirrorsInTangent()
        {
            var node = new SurfaceNode(Vector2.zero, SurfaceNodeType.Bezier);
            node.SetTangentMode(SurfaceTangentMode.Symmetric);

            node.SetOutTangent(new Vector2(2f, 1f));

            AssertVector(new Vector2(-2f, -1f), node.InTangent);
        }

        [Test]
        public void AlignedMode_SetOutTangent_KeepsInTangentLength()
        {
            var node = new SurfaceNode(Vector2.zero, SurfaceNodeType.Bezier);
            node.SetTangentMode(SurfaceTangentMode.Free);
            node.SetInTangent(new Vector2(-3f, 0f));
            node.SetTangentMode(SurfaceTangentMode.Aligned);

            node.SetOutTangent(new Vector2(0f, 1f));

            AssertVector(new Vector2(0f, -3f), node.InTangent);
        }

        [Test]
        public void FreeMode_SetOutTangent_LeavesInTangent()
        {
            var node = new SurfaceNode(Vector2.zero, SurfaceNodeType.Bezier);
            node.SetTangentMode(SurfaceTangentMode.Free);
            node.SetInTangent(new Vector2(-3f, 0f));

            node.SetOutTangent(new Vector2(0f, 1f));

            AssertVector(new Vector2(-3f, 0f), node.InTangent);
        }

        [Test]
        public void EdgeScale_OutOfRange_IsClamped()
        {
            var node = new SurfaceNode();

            node.EdgeScale = 5f;

            Assert.AreEqual(SurfaceNode.MaxEdgeScale, node.EdgeScale, Tolerance);
        }

        private static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
        }
    }
}
