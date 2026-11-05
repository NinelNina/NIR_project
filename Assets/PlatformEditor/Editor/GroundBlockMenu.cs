using UnityEditor;
using UnityEngine;

namespace Level.Platforms.Editor
{
    public static class GroundBlockMenu
    {
        private const string MenuPath = "GameObject/2D Object/Ground Block";
        private const string BlockName = "Ground Block";
        private const string DefaultMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";

        [MenuItem(MenuPath, false, 14)]
        private static void CreateBlock(MenuCommand command)
        {
            GameObject blockObject = new GameObject(BlockName);
            GroundBlock block = blockObject.AddComponent<GroundBlock>();

            SerializedObject serializedBlock = new SerializedObject(block);
            serializedBlock.FindProperty("_material").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialPath);
            serializedBlock.ApplyModifiedPropertiesWithoutUndo();

            GameObject parent = command.context as GameObject;
            GameObjectUtility.SetParentAndAlign(blockObject, parent);

            if (parent == null && SceneView.lastActiveSceneView != null)
            {
                Vector3 pivot = SceneView.lastActiveSceneView.pivot;
                blockObject.transform.position = new Vector3(pivot.x, pivot.y, 0f);
            }

            Undo.RegisterCreatedObjectUndo(blockObject, "Create Ground Block");
            Selection.activeGameObject = blockObject;
            block.Rebuild();
        }
    }
}
