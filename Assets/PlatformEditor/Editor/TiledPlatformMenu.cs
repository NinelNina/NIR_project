using UnityEditor;
using UnityEngine;

namespace Level.Platforms.Editor
{
    public static class TiledPlatformMenu
    {
        private const string MenuPath = "GameObject/2D Object/Tiled Ground Platform";
        private const string PlatformName = "Tiled Platform";

        [MenuItem(MenuPath, false, 11)]
        private static void CreateTiledPlatform(MenuCommand command)
        {
            GameObject platformObject = new GameObject(PlatformName);
            TiledPlatform platform = platformObject.AddComponent<TiledPlatform>();

            GameObject parent = command.context as GameObject;
            GameObjectUtility.SetParentAndAlign(platformObject, parent);

            if (parent == null && SceneView.lastActiveSceneView != null)
            {
                Vector3 pivot = SceneView.lastActiveSceneView.pivot;
                platformObject.transform.position = new Vector3(pivot.x, pivot.y, 0f);
            }

            Undo.RegisterCreatedObjectUndo(platformObject, "Create Tiled Platform");
            Selection.activeGameObject = platformObject;
            platform.Rebuild();
        }
    }
}
