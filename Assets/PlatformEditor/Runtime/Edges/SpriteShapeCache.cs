using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public sealed class SpriteShapeCache
    {
        private readonly Dictionary<Sprite, SpriteShape> _shapes = new Dictionary<Sprite, SpriteShape>();
        private readonly Dictionary<Sprite, SpriteShape> _flippedShapes = new Dictionary<Sprite, SpriteShape>();

        public SpriteShape Get(Sprite sprite, bool flipped = false)
        {
            if (sprite == null)
                return null;

            Dictionary<Sprite, SpriteShape> shapes = flipped ? _flippedShapes : _shapes;

            if (!shapes.TryGetValue(sprite, out SpriteShape shape))
            {
                shape = flipped ? SpriteShape.FromSpriteFlipped(sprite) : SpriteShape.FromSprite(sprite);
                shapes.Add(sprite, shape);
            }

            return shape;
        }

        public void Clear()
        {
            _shapes.Clear();
            _flippedShapes.Clear();
        }
    }
}
