using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Level.Platforms.Editor
{
    [CustomPropertyDrawer(typeof(CollisionLayerAttribute))]
    public sealed class CollisionLayerDrawer : PropertyDrawer
    {
        private const string SameAsSurfaceLabel = "Как у поверхности";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] layers = InternalEditorUtility.layers;
            var options = new string[layers.Length + 1];
            options[0] = SameAsSurfaceLabel;
            int current = 0;

            for (int i = 0; i < layers.Length; i++)
            {
                options[i + 1] = layers[i];

                if (property.intValue >= 0 && LayerMask.LayerToName(property.intValue) == layers[i])
                    current = i + 1;
            }

            EditorGUI.BeginProperty(position, label, property);
            int selected = EditorGUI.Popup(position, label.text, current, options);
            property.intValue = selected == 0 ? CollisionLayerAttribute.SameAsSurface : LayerMask.NameToLayer(layers[selected - 1]);
            EditorGUI.EndProperty();
        }
    }
}
