using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Level.Platforms.Editor
{
    [EditorTool("Узлы платформы", typeof(TiledPlatform))]
    public sealed class TiledPlatformNodeTool : SurfaceNodeTool
    {
        private const float HandleSize = 0.1f;

        private static readonly Color ThicknessColor = new Color(0.35f, 0.75f, 1f, 1f);

        protected override void DrawExtraHandles(SurfaceBase surface)
        {
            if (!(surface is TiledPlatform platform) || !platform.TryGetThicknessHandle(out Vector2 point, out Vector2 normal))
                return;

            Vector2 bottom = point - normal * platform.BodyThickness;
            Handles.color = ThicknessColor;
            Handles.DrawDottedLine(point, bottom, 4f);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.Slider(bottom, -normal, HandleUtility.GetHandleSize(bottom) * HandleSize, Handles.DotHandleCap, 0f);

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(platform, "Change Platform Thickness");
            platform.SetBodyThickness(Vector2.Dot(point - (Vector2)moved, normal));
            RecordPrefab(platform);
        }
    }
}
