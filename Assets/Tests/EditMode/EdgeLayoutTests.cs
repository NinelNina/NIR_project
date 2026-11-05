using NUnit.Framework;

namespace Level.Platforms.Tests
{
    public class EdgeLayoutTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void WholeTiles_TieBetweenCounts_PicksFewerTiles()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0f), 0f, 14f, false, EdgeFitMode.WholeTiles, 0);

            Assert.AreEqual(4, layout.Tiles.Count);
            Assert.AreEqual(12f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void FitLength_ScalesMiddlesToReachLastNode()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0f), 0f, 15f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(5, layout.Tiles.Count);
            Assert.AreEqual(15f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void FitLength_StretchBeyondLimit_ScalesEndsToReachLastNode()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0f), 0f, 9.9f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(9.9f, layout.CoveredLength, Tolerance);
            Assert.AreEqual(EdgeTileRole.Left, layout.Tiles[0].Role);
            Assert.AreEqual(EdgeTileRole.Right, layout.Tiles[layout.Tiles.Count - 1].Role);
        }

        [Test]
        public void FitLength_RunCloseToOneMiddle_KeepsBothEnds()
        {
            var layout = new EdgeLayout();

            layout.Build(new VariantMetrics(3.6f, 5.21f, 5.21f, 11.35f, 9.05f), 0f, 9.23f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(EdgeTileRole.Left, layout.Tiles[0].Role);
            Assert.AreEqual(EdgeTileRole.Right, layout.Tiles[layout.Tiles.Count - 1].Role);
            Assert.AreEqual(9.23f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void FitLength_RunShorterThanEnds_SqueezesEndsInsideRun()
        {
            var layout = new EdgeLayout();

            layout.Build(new VariantMetrics(3.6f, 5.21f, 5.21f, 11.35f, 9.05f), 0f, 1.5f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(2, layout.Tiles.Count);
            Assert.AreEqual(EdgeTileRole.Left, layout.Tiles[0].Role);
            Assert.AreEqual(EdgeTileRole.Right, layout.Tiles[1].Role);
            Assert.AreEqual(1.5f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void FitLength_WithOverlap_ReachesLastNodeExactly()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0.5f), 0f, 9f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(9f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void RunShorterThanEnds_UsesSingleMiddle()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(3f, 4f, 0f), 0f, 4f, false, EdgeFitMode.WholeTiles, 0);

            Assert.AreEqual(1, layout.Tiles.Count);
            Assert.AreEqual(EdgeTileRole.Middle, layout.Tiles[0].Role);
        }

        [Test]
        public void Loop_HasNoEndsAndClosesExactly()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0f), 0f, 18f, true, EdgeFitMode.WholeTiles, 0);

            Assert.AreEqual(4, layout.Tiles.Count);
            Assert.AreEqual(18f, layout.CoveredLength, Tolerance);

            foreach (EdgeTile tile in layout.Tiles)
                Assert.AreEqual(EdgeTileRole.Middle, tile.Role);
        }

        [Test]
        public void Overlap_ShiftsEachTileBack()
        {
            var layout = new EdgeLayout();

            layout.Build(new FakeMetrics(2f, 4f, 0.5f), 0f, 7f, false, EdgeFitMode.WholeTiles, 0);

            Assert.AreEqual(3, layout.Tiles.Count);
            Assert.AreEqual(0f, layout.Tiles[0].Start, Tolerance);
            Assert.AreEqual(1.5f, layout.Tiles[1].Start, Tolerance);
            Assert.AreEqual(5f, layout.Tiles[2].Start, Tolerance);
            Assert.AreEqual(7f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void SameSeed_GivesSameVariants()
        {
            var metrics = new FakeMetrics(2f, 4f, 0f, 3);
            var first = new EdgeLayout();
            var second = new EdgeLayout();

            first.Build(metrics, 0f, 40f, false, EdgeFitMode.WholeTiles, 42);
            second.Build(metrics, 0f, 40f, false, EdgeFitMode.WholeTiles, 42);

            for (int i = 0; i < first.Tiles.Count; i++)
                Assert.AreEqual(first.Tiles[i].Variant, second.Tiles[i].Variant);
        }

        [Test]
        public void FitLength_PicksVariantsToReachLastNode()
        {
            var layout = new EdgeLayout();

            layout.Build(new VariantMetrics(3.6f, 5f, 9f), 0f, 20f, false, EdgeFitMode.FitLength, 0);

            Assert.AreEqual(20f, layout.CoveredLength, Tolerance);
        }

        [Test]
        public void Variants_DoNotRepeatNeighbour()
        {
            var layout = new EdgeLayout();

            layout.Build(new VariantMetrics(2f, 4f, 4f), 0f, 40f, false, EdgeFitMode.WholeTiles, 0);

            for (int i = 1; i < layout.Tiles.Count; i++)
            {
                if (layout.Tiles[i].Role == EdgeTileRole.Middle && layout.Tiles[i - 1].Role == EdgeTileRole.Middle)
                    Assert.AreNotEqual(layout.Tiles[i - 1].Variant, layout.Tiles[i].Variant);
            }
        }

        private sealed class VariantMetrics : IEdgeTileMetrics
        {
            private readonly float _endWidth;
            private readonly float[] _middleWidths;

            public VariantMetrics(float endWidth, params float[] middleWidths)
            {
                _endWidth = endWidth;
                _middleWidths = middleWidths;
            }

            public bool HasEnds => true;

            public int MiddleVariantCount => _middleWidths.Length;

            public float Width(EdgeTileRole role, int variant) => role == EdgeTileRole.Middle ? _middleWidths[variant] : _endWidth;

            public float Overlap(EdgeTileRole leftRole, int leftVariant, EdgeTileRole rightRole, int rightVariant) => 0f;
        }

        private sealed class FakeMetrics : IEdgeTileMetrics
        {
            private readonly float _endWidth;
            private readonly float _middleWidth;
            private readonly float _overlap;

            public FakeMetrics(float endWidth, float middleWidth, float overlap, int variants = 1)
            {
                _endWidth = endWidth;
                _middleWidth = middleWidth;
                _overlap = overlap;
                MiddleVariantCount = variants;
            }

            public bool HasEnds => true;

            public int MiddleVariantCount { get; }

            public float Width(EdgeTileRole role, int variant) => role == EdgeTileRole.Middle ? _middleWidth : _endWidth;

            public float Overlap(EdgeTileRole leftRole, int leftVariant, EdgeTileRole rightRole, int rightVariant) => _overlap;
        }
    }
}
