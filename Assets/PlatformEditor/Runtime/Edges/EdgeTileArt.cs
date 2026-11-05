using UnityEngine;

namespace Level.Platforms
{
    public readonly struct EdgeTileArt
    {
        public EdgeTileArt(Rect localRect, Rect uvRect, float surfaceY, Vector2 scale, float belowSurfaceScale, bool rotated = false, bool flipped = false, bool fadeTop = false)
        {
            LocalRect = localRect;
            UvRect = uvRect;
            SurfaceY = surfaceY;
            Scale = scale;
            BelowSurfaceScale = belowSurfaceScale;
            Rotated = rotated;
            Flipped = flipped;
            FadeTop = fadeTop;
        }

        public Rect LocalRect { get; }

        public Rect UvRect { get; }

        public float SurfaceY { get; }

        public Vector2 Scale { get; }

        public float BelowSurfaceScale { get; }

        public bool Rotated { get; }

        public bool Flipped { get; }

        public bool FadeTop { get; }

        public EdgeTileArt CropAboveSurface(float maxAbove)
        {
            float top = Mathf.Min(LocalRect.yMax, SurfaceY + maxAbove);

            if (top >= LocalRect.yMax)
                return new EdgeTileArt(LocalRect, UvRect, SurfaceY, Scale, BelowSurfaceScale, Rotated, Flipped, true);

            float kept = Mathf.InverseLerp(LocalRect.yMin, LocalRect.yMax, top);
            Rect local = Rect.MinMaxRect(LocalRect.xMin, LocalRect.yMin, LocalRect.xMax, top);
            Rect uv = Flipped
                ? Rect.MinMaxRect(UvRect.xMin, Mathf.Lerp(UvRect.yMax, UvRect.yMin, kept), UvRect.xMax, UvRect.yMax)
                : Rect.MinMaxRect(UvRect.xMin, UvRect.yMin, UvRect.xMax, Mathf.Lerp(UvRect.yMin, UvRect.yMax, kept));
            return new EdgeTileArt(local, uv, SurfaceY, Scale, BelowSurfaceScale, Rotated, Flipped, true);
        }

        public static EdgeTileArt FromSprite(Sprite sprite, float surfaceY, Vector2 scale, float belowSurfaceScale, bool flipped = false)
        {
            Rect rect = sprite.rect;
            Rect textureRect = sprite.textureRect;
            Vector2 pivot = sprite.pivot;
            float pixelsPerUnit = sprite.pixelsPerUnit;
            Texture2D texture = sprite.texture;
            Rect local = Rect.MinMaxRect(
                -pivot.x / pixelsPerUnit,
                -pivot.y / pixelsPerUnit,
                (rect.width - pivot.x) / pixelsPerUnit,
                (rect.height - pivot.y) / pixelsPerUnit);
            Rect uv = Rect.MinMaxRect(
                textureRect.xMin / texture.width,
                textureRect.yMin / texture.height,
                textureRect.xMax / texture.width,
                textureRect.yMax / texture.height);
            if (flipped)
                local = Rect.MinMaxRect(local.xMin, -local.yMax, local.xMax, -local.yMin);

            return new EdgeTileArt(local, uv, surfaceY, scale, belowSurfaceScale, false, flipped);
        }

        public static EdgeTileArt FromSpriteRotated(Sprite sprite, float surfaceY, float alongFraction, float inwardDepth)
        {
            EdgeTileArt upright = FromSprite(sprite, 0f, Vector2.one, 1f);
            Rect local = upright.LocalRect;
            Rect uv = upright.UvRect;
            float along = Mathf.Clamp01(alongFraction);
            float inner = Mathf.Clamp(surfaceY - inwardDepth, -local.xMax, -local.xMin);
            float innerU = Mathf.Lerp(uv.xMin, uv.xMax, Mathf.InverseLerp(local.xMin, local.xMax, -inner));
            Rect rotatedLocal = Rect.MinMaxRect(local.yMin, inner, local.yMin + local.height * along, -local.xMin);
            Rect rotatedUv = Rect.MinMaxRect(uv.xMin, uv.yMin, innerU, uv.yMin + uv.height * along);
            return new EdgeTileArt(rotatedLocal, rotatedUv, surfaceY, Vector2.one, 1f, true);
        }
    }
}
