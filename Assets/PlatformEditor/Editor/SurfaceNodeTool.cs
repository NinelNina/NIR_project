using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Level.Platforms.Editor
{
    public abstract class SurfaceNodeTool : EditorTool
    {
        private const string DeleteCommand = "Delete";
        private const string SoftDeleteCommand = "SoftDelete";
        private const float NodeHandleSize = 0.09f;
        private const float TangentHandleSize = 0.06f;
        private const float InsertPickPixels = 12f;
        private const float NormalTickLength = 0.35f;
        private const int NormalTickStep = 4;
        private const float PathThickness = 2f;
        private const float TightBendThickness = 5f;
        private const float DottedSpacing = 4f;
        private const float IntersectionDiscSize = 0.15f;
        private const float PanelWidth = 280f;
        private const float PanelHeight = 170f;
        private const float PanelMargin = 10f;
        private const float PanelLabelWidth = 120f;

        private static readonly Color PathColor = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color HiddenPathColor = new Color(1f, 0.85f, 0.2f, 0.45f);
        private static readonly Color NormalColor = new Color(0.4f, 0.9f, 1f, 0.8f);
        private static readonly Color NodeColor = Color.white;
        private static readonly Color SelectedNodeColor = new Color(1f, 0.45f, 0.1f, 1f);
        private static readonly Color TangentColor = new Color(0.6f, 1f, 0.6f, 1f);
        private static readonly Color IntersectionColor = Color.red;
        private static readonly Color TightBendColor = new Color(1f, 0.2f, 0.2f, 1f);

        private readonly List<int> _nodeControls = new List<int>();
        private readonly List<TightBend> _bends = new List<TightBend>();

        private int _selectedNode = -1;
        private int _dragUndoGroup = -1;

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView sceneView) || !(target is SurfaceBase surface))
                return;

            if (!surface.Path.IsValid)
                surface.Rebuild();

            if (!surface.Path.IsValid)
                return;

            _selectedNode = Mathf.Clamp(_selectedNode, -1, surface.NodeCount - 1);

            Event current = Event.current;
            EventType eventType = current.type;
            Rect panel = PanelRect(sceneView);

            AllocateControls(surface.NodeCount);
            HandleCommands(surface, current);
            HandleKeys(surface, current, window);

            if (HandleClicks(surface, current, panel))
                return;

            using (new Handles.DrawingScope(surface.transform.localToWorldMatrix))
            {
                DrawPath(surface);
                DrawTightBends(surface);
                DrawNormals(surface.Path);
                DrawSelfIntersection(surface.Path);
                DrawTangentHandles(surface);
                DrawNodeHandles(surface);
                DrawExtraHandles(surface);
            }

            CollapseDragUndo(eventType);
            DrawPanel(surface, panel, current);
        }

        private void AllocateControls(int count)
        {
            _nodeControls.Clear();

            for (int i = 0; i < count; i++)
                _nodeControls.Add(GUIUtility.GetControlID(FocusType.Passive));
        }

        private void HandleCommands(SurfaceBase surface, Event current)
        {
            if (_selectedNode < 0)
                return;

            if (current.type != EventType.ValidateCommand && current.type != EventType.ExecuteCommand)
                return;

            if (current.commandName != DeleteCommand && current.commandName != SoftDeleteCommand)
                return;

            if (current.type == EventType.ExecuteCommand)
                RemoveSelected(surface);

            current.Use();
        }

        private void HandleKeys(SurfaceBase surface, Event current, EditorWindow window)
        {
            if (current.type != EventType.KeyDown || _selectedNode < 0)
                return;

            switch (current.keyCode)
            {
                case KeyCode.K:
                    Undo.RecordObject(surface, "Change Surface Node Type");
                    surface.SetNodeType(_selectedNode, NextType(surface.GetNode(_selectedNode).Type));
                    RecordPrefab(surface);
                    break;
                case KeyCode.Tab:
                    int step = current.shift ? -1 : 1;
                    _selectedNode = (_selectedNode + step + surface.NodeCount) % surface.NodeCount;
                    break;
                case KeyCode.Backspace:
                    RemoveSelected(surface);
                    break;
                default:
                    return;
            }

            current.Use();
            window.Repaint();
        }

        private bool HandleClicks(SurfaceBase surface, Event current, Rect panel)
        {
            if (current.type != EventType.MouseDown || current.button != 0 || current.alt || panel.Contains(current.mousePosition))
                return false;

            int clickedNode = _nodeControls.IndexOf(HandleUtility.nearestControl);

            if (EditorGUI.actionKey && clickedNode >= 0)
            {
                _selectedNode = clickedNode;
                RemoveSelected(surface);
                current.Use();
                return true;
            }

            if (current.shift && clickedNode < 0 && TryFindInsertPoint(surface, current.mousePosition, out Vector2 local, out float distance))
            {
                int segment = surface.FindSegmentAt(distance);

                if (segment < 0)
                    return false;

                SurfaceNodeType type = surface.GetNode(segment).Type == SurfaceNodeType.Corner ? SurfaceNodeType.Corner : SurfaceNodeType.Smooth;
                Undo.RecordObject(surface, "Insert Surface Node");
                surface.InsertNode(segment + 1, local, type);
                RecordPrefab(surface);
                _selectedNode = segment + 1;
                current.Use();
                return true;
            }

            if (clickedNode >= 0)
                _selectedNode = clickedNode;

            return false;
        }

        private void RemoveSelected(SurfaceBase surface)
        {
            if (_selectedNode < 0)
                return;

            Undo.RecordObject(surface, "Remove Surface Node");

            if (!surface.RemoveNode(_selectedNode))
                return;

            RecordPrefab(surface);
            _selectedNode = Mathf.Min(_selectedNode, surface.NodeCount - 1);
        }

        private static bool TryFindInsertPoint(SurfaceBase surface, Vector2 mouse, out Vector2 local, out float distance)
        {
            Transform surfaceTransform = surface.transform;
            Vector2 pointer = surfaceTransform.InverseTransformPoint(HandleUtility.GUIPointToWorldRay(mouse).origin);
            SurfacePath path = surface.Path;
            int count = path.SampleCount;
            int chords = path.IsClosed ? count : count - 1;
            float best = float.MaxValue;

            local = default;
            distance = 0f;

            for (int i = 0; i < chords; i++)
            {
                Vector2 a = path.Points[i];
                Vector2 chord = path.Points[(i + 1) % count] - a;
                float lengthSquared = chord.sqrMagnitude;

                if (lengthSquared <= 0f)
                    continue;

                float t = Mathf.Clamp01(Vector2.Dot(pointer - a, chord) / lengthSquared);
                Vector2 candidate = a + chord * t;
                float squared = (pointer - candidate).sqrMagnitude;

                if (squared >= best)
                    continue;

                best = squared;
                local = candidate;
                distance = path.Distances[i] + Mathf.Sqrt(lengthSquared) * t;
            }

            if (best == float.MaxValue)
                return false;

            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(surfaceTransform.TransformPoint(local));
            return (guiPoint - mouse).magnitude <= InsertPickPixels;
        }

        private static void DrawPath(SurfaceBase surface)
        {
            SurfacePath path = surface.Path;
            int count = path.SampleCount;
            int chords = path.IsClosed ? count : count - 1;

            for (int i = 0; i < chords; i++)
            {
                Vector2 a = path.Points[i];
                Vector2 b = path.Points[(i + 1) % count];
                float end = i + 1 < count ? path.Distances[i + 1] : path.Length;
                int segment = surface.FindSegmentAt((path.Distances[i] + end) * 0.5f);

                if (segment >= 0 && surface.GetNode(segment).EdgeVisible)
                {
                    Handles.color = PathColor;
                    Handles.DrawLine(a, b, PathThickness);
                }
                else
                {
                    Handles.color = HiddenPathColor;
                    Handles.DrawDottedLine(a, b, DottedSpacing);
                }
            }
        }

        private void DrawTightBends(SurfaceBase surface)
        {
            surface.FindTightBends(_bends);

            if (_bends.Count == 0)
                return;

            SurfacePath path = surface.Path;
            Handles.color = TightBendColor;

            for (int i = 0; i < _bends.Count; i++)
            {
                TightBend bend = _bends[i];

                if (bend.End >= bend.Start)
                {
                    DrawPathRange(path, bend.Start, bend.End);
                }
                else
                {
                    DrawPathRange(path, bend.Start, path.Length);
                    DrawPathRange(path, 0f, bend.End);
                }

                Handles.Label(bend.Point, TightBendText.Describe(bend));
            }
        }

        private static void DrawPathRange(SurfacePath path, float start, float end)
        {
            int count = path.SampleCount;
            path.Evaluate(start, out Vector2 first, out _);
            Vector3 previous = first;

            for (int i = 0; i < count; i++)
            {
                float distance = path.Distances[i];

                if (distance <= start || distance >= end)
                    continue;

                Handles.DrawLine(previous, path.Points[i], TightBendThickness);
                previous = path.Points[i];
            }

            path.Evaluate(end, out Vector2 last, out _);
            Handles.DrawLine(previous, last, TightBendThickness);
        }

        private static void DrawNormals(SurfacePath path)
        {
            Handles.color = NormalColor;

            for (int i = 0; i < path.SampleCount; i += NormalTickStep)
            {
                Vector3 point = path.Points[i];
                float length = HandleUtility.GetHandleSize(point) * NormalTickLength;
                Handles.DrawLine(point, point + (Vector3)(path.Normals[i] * length));
            }
        }

        private static void DrawSelfIntersection(SurfacePath path)
        {
            if (!path.TryFindSelfIntersection(out Vector2 point))
                return;

            Handles.color = IntersectionColor;
            Handles.DrawSolidDisc(point, Vector3.forward, HandleUtility.GetHandleSize(point) * IntersectionDiscSize);
            Handles.Label(point, "Самопересечение");
        }

        private void DrawTangentHandles(SurfaceBase surface)
        {
            for (int i = 0; i < surface.NodeCount; i++)
            {
                SurfaceNode node = surface.GetNode(i);

                if (node.Type != SurfaceNodeType.Bezier)
                    continue;

                Vector3 position = node.Position;
                Vector3 inPoint = position + (Vector3)node.InTangent;
                Vector3 outPoint = position + (Vector3)node.OutTangent;

                Handles.color = TangentColor;
                Handles.DrawLine(position, inPoint);
                Handles.DrawLine(position, outPoint);

                EditorGUI.BeginChangeCheck();
                Vector3 movedIn = Handles.FreeMoveHandle(inPoint, HandleUtility.GetHandleSize(inPoint) * TangentHandleSize, Vector3.zero, Handles.CircleHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    BeginDragUndo(surface, "Move Surface Tangent");
                    surface.SetNodeInTangent(i, movedIn - position);
                    RecordPrefab(surface);
                    _selectedNode = i;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 movedOut = Handles.FreeMoveHandle(outPoint, HandleUtility.GetHandleSize(outPoint) * TangentHandleSize, Vector3.zero, Handles.CircleHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    BeginDragUndo(surface, "Move Surface Tangent");
                    surface.SetNodeOutTangent(i, movedOut - position);
                    RecordPrefab(surface);
                    _selectedNode = i;
                }
            }
        }

        private void DrawNodeHandles(SurfaceBase surface)
        {
            for (int i = 0; i < surface.NodeCount; i++)
            {
                SurfaceNode node = surface.GetNode(i);
                Vector3 position = node.Position;
                float size = HandleUtility.GetHandleSize(position) * NodeHandleSize;

                Handles.color = i == _selectedNode ? SelectedNodeColor : NodeColor;
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.FreeMoveHandle(_nodeControls[i], position, size, Vector3.zero, CapFor(node.Type));

                if (GUIUtility.hotControl == _nodeControls[i])
                    _selectedNode = i;

                if (!EditorGUI.EndChangeCheck())
                    continue;

                BeginDragUndo(surface, "Move Surface Node");
                surface.SetNodePosition(i, Snap(surface.transform, moved));
                RecordPrefab(surface);
            }
        }

        private void BeginDragUndo(SurfaceBase surface, string name)
        {
            if (_dragUndoGroup < 0)
                _dragUndoGroup = Undo.GetCurrentGroup();

            Undo.RecordObject(surface, name);
        }

        private void CollapseDragUndo(EventType eventType)
        {
            if (_dragUndoGroup < 0 || eventType != EventType.MouseUp)
                return;

            Undo.CollapseUndoOperations(_dragUndoGroup);
            _dragUndoGroup = -1;
        }

        private void DrawPanel(SurfaceBase surface, Rect panel, Event current)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(panel, EditorStyles.helpBox);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = PanelLabelWidth;

            GUILayout.Label(surface.name, EditorStyles.boldLabel);

            if (_selectedNode < 0)
                GUILayout.Label("Клик по узлу — выбрать");
            else
                DrawSelectedNode(surface);

            GUILayout.FlexibleSpace();
            GUILayout.Label("Shift+клик по линии — вставить узел\nCtrl+клик или Del — удалить\nK — тип узла, Tab — следующий", EditorStyles.miniLabel);

            EditorGUIUtility.labelWidth = labelWidth;
            GUILayout.EndArea();
            Handles.EndGUI();

            if (current.type == EventType.MouseDown && panel.Contains(current.mousePosition))
                current.Use();
        }

        private void DrawSelectedNode(SurfaceBase surface)
        {
            SurfaceNode node = surface.GetNode(_selectedNode);

            GUILayout.Label($"Узел {_selectedNode + 1} из {surface.NodeCount}");

            EditorGUI.BeginChangeCheck();
            var type = (SurfaceNodeType)EditorGUILayout.EnumPopup("Тип", node.Type);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(surface, "Change Surface Node Type");
                surface.SetNodeType(_selectedNode, type);
                RecordPrefab(surface);
            }

            if (node.Type == SurfaceNodeType.Bezier)
            {
                EditorGUI.BeginChangeCheck();
                var mode = (SurfaceTangentMode)EditorGUILayout.EnumPopup("Рычаги", node.TangentMode);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(surface, "Change Surface Tangent Mode");
                    surface.SetNodeTangentMode(_selectedNode, mode);
                    RecordPrefab(surface);
                }
            }

            EditorGUI.BeginChangeCheck();
            bool visible = EditorGUILayout.Toggle("Кромка после узла", node.EdgeVisible);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(surface, "Toggle Surface Edge");
                surface.SetNodeEdgeVisible(_selectedNode, visible);
                RecordPrefab(surface);
            }

            EditorGUI.BeginChangeCheck();
            Vector2 position = EditorGUILayout.Vector2Field("Позиция", node.Position);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(surface, "Move Surface Node");
                surface.SetNodePosition(_selectedNode, position);
                RecordPrefab(surface);
            }
        }

        private static Rect PanelRect(SceneView sceneView)
        {
            float viewHeight = sceneView.camera.pixelRect.height / EditorGUIUtility.pixelsPerPoint;
            return new Rect(PanelMargin, viewHeight - PanelHeight - PanelMargin, PanelWidth, PanelHeight);
        }

        private static Vector2 Snap(Transform surfaceTransform, Vector3 local)
        {
            if (!EditorSnapSettings.gridSnapEnabled)
                return local;

            Vector3 world = surfaceTransform.TransformPoint(local);
            Vector3 step = EditorSnapSettings.move;
            world.x = SnapValue(world.x, step.x);
            world.y = SnapValue(world.y, step.y);
            return surfaceTransform.InverseTransformPoint(world);
        }

        private static float SnapValue(float value, float step) => step > 0f ? Mathf.Round(value / step) * step : value;

        private static Handles.CapFunction CapFor(SurfaceNodeType type)
        {
            switch (type)
            {
                case SurfaceNodeType.Corner:
                    return Handles.DotHandleCap;
                case SurfaceNodeType.Bezier:
                    return Handles.CircleHandleCap;
                default:
                    return Handles.SphereHandleCap;
            }
        }

        private static SurfaceNodeType NextType(SurfaceNodeType type)
        {
            switch (type)
            {
                case SurfaceNodeType.Corner:
                    return SurfaceNodeType.Smooth;
                case SurfaceNodeType.Smooth:
                    return SurfaceNodeType.Bezier;
                default:
                    return SurfaceNodeType.Corner;
            }
        }

        protected virtual void DrawExtraHandles(SurfaceBase surface)
        {
        }

        protected static void RecordPrefab(SurfaceBase surface) => PrefabUtility.RecordPrefabInstancePropertyModifications(surface);
    }
}
