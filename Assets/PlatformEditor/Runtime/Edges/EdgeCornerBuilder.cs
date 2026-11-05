using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public readonly struct EdgeCornerArt
    {
        public EdgeCornerArt(Rect localRect, Rect uvRect, float surfaceY, float faceX, bool faceLeft)
        {
            LocalRect = localRect;
            UvRect = uvRect;
            SurfaceY = surfaceY;
            FaceX = faceX;
            FaceLeft = faceLeft;
        }

        public Rect LocalRect { get; }

        public Rect UvRect { get; }

        public float SurfaceY { get; }

        public float FaceX { get; }

        public bool FaceLeft { get; }

        public float MaxTopSpan => FaceLeft ? LocalRect.xMax - FaceX : FaceX - LocalRect.xMin;

        public float MaxWallSpan => SurfaceY - LocalRect.yMin;
    }

    public static class EdgeCornerBuilder
    {
        private const int GridSize = 3;

        public static void Append(
            in EdgeCornerArt art,
            Vector2 corner,
            Vector2 alongTop,
            Vector2 alongFace,
            float topSpan,
            float wallSpan,
            float fade,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<Color32> colors,
            List<int> triangles)
        {
            Rect local = art.LocalRect;
            Vector2 right = art.FaceLeft ? alongTop : -alongTop;
            float top = Mathf.Min(topSpan, art.MaxTopSpan);
            float wall = Mathf.Min(wallSpan, art.MaxWallSpan);
            float xLow = art.FaceLeft ? local.xMin : art.FaceX - top;
            float xHigh = art.FaceLeft ? art.FaceX + top : local.xMax;
            float yLow = art.SurfaceY - wall;
            float yHigh = local.yMax;
            float horizontalFade = Mathf.Clamp(fade, 0f, (xHigh - xLow) * 0.5f);
            float verticalFade = Mathf.Clamp(fade, 0f, (yHigh - yLow) * 0.5f);
            int firstVertex = vertices.Count;

            for (int row = 0; row < GridSize; row++)
            {
                float y = row == 0 ? yLow : row == 1 ? yLow + verticalFade : yHigh;

                for (int column = 0; column < GridSize; column++)
                {
                    float x = column == 0 ? xLow : column == 1 ? (art.FaceLeft ? xHigh - horizontalFade : xLow + horizontalFade) : xHigh;
                    bool innerColumn = art.FaceLeft ? column == GridSize - 1 : column == 0;
                    bool innerRow = row == 0;
                    byte alpha = innerColumn || innerRow ? (byte)0 : (byte)255;

                    vertices.Add(corner + right * (x - art.FaceX) + alongFace * (art.SurfaceY - y));
                    uvs.Add(new Vector2(
                        Mathf.Lerp(art.UvRect.xMin, art.UvRect.xMax, Mathf.InverseLerp(local.xMin, local.xMax, x)),
                        Mathf.Lerp(art.UvRect.yMin, art.UvRect.yMax, Mathf.InverseLerp(local.yMin, local.yMax, y))));
                    colors.Add(new Color32(255, 255, 255, alpha));
                }
            }

            for (int row = 0; row < GridSize - 1; row++)
            {
                for (int column = 0; column < GridSize - 1; column++)
                {
                    int bottomLeft = firstVertex + row * GridSize + column;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + GridSize;
                    int topRight = topLeft + 1;
                    triangles.Add(bottomLeft);
                    triangles.Add(topLeft);
                    triangles.Add(topRight);
                    triangles.Add(bottomLeft);
                    triangles.Add(topRight);
                    triangles.Add(bottomRight);
                }
            }
        }
    }
}
