using System.Collections.Generic;
using UnityEngine;

namespace Level.Platforms.Editor
{
    public static class TightBendText
    {
        private const float MinRequired = 0.0001f;

        public static string Describe(in TightBend bend) =>
            $"{(bend.Valley ? "Впадина" : "Горб")}: радиус {bend.Radius:0.0} < {bend.RequiredRadius:0.0}";

        public static string Summary(IReadOnlyList<TightBend> bends)
        {
            float worstRatio = float.MaxValue;
            float worst = 0f;
            float required = 0f;

            for (int i = 0; i < bends.Count; i++)
            {
                float ratio = bends[i].Radius / Mathf.Max(bends[i].RequiredRadius, MinRequired);

                if (ratio >= worstRatio)
                    continue;

                worstRatio = ratio;
                worst = bends[i].Radius;
                required = bends[i].RequiredRadius;
            }

            return $"Слишком крутые изгибы: {bends.Count}. Самый крутой — радиус {worst:0.0} при минимуме {required:0.0}. Арт и коллайдер на них могут сминаться. Участки подсвечены красным в режиме правки узлов.";
        }
    }
}
