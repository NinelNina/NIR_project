using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Level.Platforms.Editor
{
    public static class BiomeFromFolderMenu
    {
        private const string MenuPath = "Assets/Create/Level/Biome From Folder";
        private const string ThemeSuffix = "Theme.asset";

        private static readonly BiomePart[] SingleParts =
        {
            BiomePart.Left, BiomePart.Right, BiomePart.CrustLeft, BiomePart.CrustRight, BiomePart.UnderLeft,
            BiomePart.UnderRight, BiomePart.Wall, BiomePart.CornerLeft, BiomePart.CornerRight, BiomePart.EndSide
        };

        [MenuItem(MenuPath, true)]
        private static bool CanCreate() => AssetDatabase.IsValidFolder(SelectedFolder());

        [MenuItem(MenuPath, false, 120)]
        private static void Create()
        {
            string folder = SelectedFolder();
            GroundTheme theme = FindTheme(folder);
            bool created = theme == null;

            if (created)
            {
                theme = ScriptableObject.CreateInstance<GroundTheme>();
                AssetDatabase.CreateAsset(theme, AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Path.GetFileName(folder)}{ThemeSuffix}"));
            }
            else
            {
                Undo.RecordObject(theme, "Fill Biome From Folder");
            }

            int filled = Fill(theme, folder);
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Selection.activeObject = theme;
            Debug.Log($"{(created ? "Создан" : "Обновлён")} биом {AssetDatabase.GetAssetPath(theme)}: заполнено полей {filled}.", theme);
        }

        public static int Fill(GroundTheme theme, string folder)
        {
            var singles = new Dictionary<BiomePart, Object>();
            var middles = new SortedDictionary<string, Sprite>();
            var crustMiddles = new SortedDictionary<string, Sprite>();
            var underMiddles = new SortedDictionary<string, Sprite>();

            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                BiomePart part = BiomeFileNames.Parse(name);

                switch (part)
                {
                    case BiomePart.None:
                        continue;
                    case BiomePart.Fill:
                        singles[part] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        continue;
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite == null)
                    continue;

                if (part == BiomePart.Middle)
                    middles[name] = sprite;
                else if (part == BiomePart.CrustMiddle)
                    crustMiddles[name] = sprite;
                else if (part == BiomePart.UnderMiddle)
                    underMiddles[name] = sprite;
                else
                    singles[part] = sprite;
            }

            var serialized = new SerializedObject(theme);
            int filled = 0;

            foreach (BiomePart part in SingleParts)
            {
                if (singles.TryGetValue(part, out Object asset))
                    filled += SetReference(serialized, FieldFor(part), asset);
            }

            if (singles.TryGetValue(BiomePart.Fill, out Object fill))
                filled += SetReference(serialized, "_fill", fill);

            filled += SetArray(serialized, "_middle", middles.Values);
            filled += SetArray(serialized, "_crustMiddle", crustMiddles.Values);
            filled += SetArray(serialized, "_underMiddle", underMiddles.Values);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return filled;
        }

        private static int SetReference(SerializedObject serialized, string field, Object asset)
        {
            serialized.FindProperty(field).objectReferenceValue = asset;
            return 1;
        }

        private static int SetArray(SerializedObject serialized, string field, ICollection<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return 0;

            SerializedProperty array = serialized.FindProperty(field);
            array.arraySize = sprites.Count;
            int index = 0;

            foreach (Sprite sprite in sprites)
                array.GetArrayElementAtIndex(index++).objectReferenceValue = sprite;

            return 1;
        }

        private static string FieldFor(BiomePart part)
        {
            switch (part)
            {
                case BiomePart.Left:
                    return "_left";
                case BiomePart.Right:
                    return "_right";
                case BiomePart.CrustLeft:
                    return "_crustLeft";
                case BiomePart.CrustRight:
                    return "_crustRight";
                case BiomePart.UnderLeft:
                    return "_underLeft";
                case BiomePart.UnderRight:
                    return "_underRight";
                case BiomePart.Wall:
                    return "_wall";
                case BiomePart.CornerLeft:
                    return "_sharpCornerLeft";
                case BiomePart.EndSide:
                    return "_platformEndSide";
                default:
                    return "_sharpCornerRight";
            }
        }

        private static GroundTheme FindTheme(string folder)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Level.Platforms.GroundTheme", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetDirectoryName(path)?.Replace('\\', '/') == folder)
                    return AssetDatabase.LoadAssetAtPath<GroundTheme>(path);
            }

            return null;
        }

        private static string SelectedFolder()
        {
            Object selected = Selection.activeObject;
            return selected != null ? AssetDatabase.GetAssetPath(selected) : string.Empty;
        }
    }
}
