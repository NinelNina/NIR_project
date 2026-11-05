using UnityEditor;
using UnityEngine;

namespace Level.Platforms.Editor
{
    [InitializeOnLoad]
    public static class GroundThemeWatcher
    {
        static GroundThemeWatcher()
        {
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeAssetObjectProperties)
                    continue;

                stream.GetChangeAssetObjectPropertiesEvent(i, out ChangeAssetObjectPropertiesEventArgs change);

                if (EditorUtility.InstanceIDToObject(change.instanceId) is GroundTheme theme)
                    RebuildUsers(theme);
            }
        }

        public static void RebuildUsers(GroundTheme theme)
        {
            foreach (SurfaceBase surface in Object.FindObjectsByType<SurfaceBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (surface.Theme != null && surface.Theme.DependsOn(theme))
                    surface.Rebuild();
            }
        }
    }
}
