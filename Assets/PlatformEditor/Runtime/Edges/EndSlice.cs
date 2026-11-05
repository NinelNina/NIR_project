namespace Level.Platforms
{
    public readonly struct EndSlice
    {
        public EndSlice(float bandTop, float bandBottom, float extra, bool holdBand = false)
        {
            BandTop = bandTop;
            BandBottom = bandBottom;
            Extra = extra;
            HoldBand = holdBand;
        }

        public float BandTop { get; }

        public float BandBottom { get; }

        public float Extra { get; }

        public bool HoldBand { get; }

        public float ArtDepth(float depth)
        {
            if (Extra <= 0f || depth <= BandTop)
                return depth;

            float stretchedBottom = BandBottom + Extra;

            if (depth >= stretchedBottom)
                return depth - Extra;

            if (HoldBand)
                return BandTop;

            float band = BandBottom - BandTop;
            return BandTop + (depth - BandTop) * band / (stretchedBottom - BandTop);
        }
    }
}
