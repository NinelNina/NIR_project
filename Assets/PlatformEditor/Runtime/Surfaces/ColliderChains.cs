using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms
{
    public static class ColliderChains
    {
        public static void Assemble(
            List<Vector2> left,
            List<Vector2> top,
            List<Vector2> right,
            List<Vector2> bottom,
            bool leftEnabled,
            bool rightEnabled,
            bool bottomEnabled,
            ColliderChainSet chains)
        {
            chains.Clear();

            if (top.Count < 2)
                return;

            if (leftEnabled)
                chains.Add(EdgeKind.Wall, left);

            chains.Add(EdgeKind.Top, top);

            if (rightEnabled)
                chains.Add(EdgeKind.Wall, right);

            if (bottomEnabled)
                chains.Add(EdgeKind.Ceiling, bottom);
        }
    }
}
