using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Level.Platforms.Editor
{
    [CustomEditor(typeof(SurfaceBase), true)]
    [CanEditMultipleObjects]
    public sealed class SurfaceBaseEditor : UnityEditor.Editor
    {
        private readonly List<TightBend> _bends = new List<TightBend>();

        public override void OnInspectorGUI()
        {
            DrawToolToggle();
            DrawTightBendWarning();
            EditorGUILayout.Space();
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Перемешать варианты"))
                Reshuffle();
        }

        private void DrawToolToggle()
        {
            Type toolType = target is GroundBlock ? typeof(GroundBlockNodeTool) : typeof(TiledPlatformNodeTool);
            bool active = ToolManager.activeToolType == toolType;
            bool toggled = GUILayout.Toggle(active, active ? "Закончить правку узлов" : "Править узлы в сцене", GUI.skin.button);

            if (toggled == active)
                return;

            if (toggled)
                ToolManager.SetActiveTool(toolType);
            else
                ToolManager.RestorePreviousPersistentTool();
        }

        private void DrawTightBendWarning()
        {
            if (targets.Length != 1)
                return;

            ((SurfaceBase)target).FindTightBends(_bends);

            if (_bends.Count > 0)
                EditorGUILayout.HelpBox(TightBendText.Summary(_bends), MessageType.Warning);
        }

        private void Reshuffle()
        {
            foreach (UnityEngine.Object surfaceObject in targets)
            {
                Undo.RecordObject(surfaceObject, "Reshuffle Surface");
                ((SurfaceBase)surfaceObject).Reshuffle();
                PrefabUtility.RecordPrefabInstancePropertyModifications(surfaceObject);
            }
        }
    }
}
