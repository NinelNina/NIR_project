using NUnit.Framework;

namespace Level.Platforms.Tests
{
    public class EndSliceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ArtDepth_AboveBand_IsUnchanged()
        {
            var slice = new EndSlice(1f, 1.2f, 3f);

            Assert.AreEqual(0.5f, slice.ArtDepth(0.5f), Tolerance);
        }

        [Test]
        public void ArtDepth_BelowStretchedBand_IsShiftedBack()
        {
            var slice = new EndSlice(1f, 1.2f, 3f);

            Assert.AreEqual(1.5f, slice.ArtDepth(4.5f), Tolerance);
        }

        [Test]
        public void ArtDepth_InsideBand_IsCompressedIntoBand()
        {
            var slice = new EndSlice(1f, 1.2f, 3f);

            Assert.AreEqual(1.1f, slice.ArtDepth(2.6f), Tolerance);
        }

        [Test]
        public void ArtDepth_InsideHeldBand_StaysAtBandTop()
        {
            var slice = new EndSlice(1f, 1.2f, 3f, true);

            Assert.AreEqual(1f, slice.ArtDepth(2.6f), Tolerance);
        }

        [Test]
        public void ArtDepth_NoExtra_IsUnchanged()
        {
            var slice = new EndSlice(1f, 1.2f, 0f);

            Assert.AreEqual(3f, slice.ArtDepth(3f), Tolerance);
        }
    }
}
