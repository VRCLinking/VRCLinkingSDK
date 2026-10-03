using UnityEditor;
using UnityEngine;
using VRCLinking.Modules.Posters;

namespace VRCLinking.Editor.Modules.Posters
{
    [CustomEditor(typeof(VrcLinkingPoster)), CanEditMultipleObjects]
    public class VrcLinkingPosterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("group"));
            if (serializedObject.FindProperty("group").objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slotIds"), true);
                if (serializedObject.FindProperty("slotIds").arraySize == 0)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("slotId"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slotName"));
                if (serializedObject.FindProperty("slotIds").arraySize > 1)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("playback"), true);
            }
            else EditorGUILayout.HelpBox("The group supplies the playlist, order, timing and synchronization. Duplicate slot IDs across displays are supported.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sizeMode"));
            EditorGUILayout.HelpBox("Automatic preserves legacy single-slot sizing and uses a fixed frame for playlists. Animated resizing fits inside the original authored bounds. A cycle holds for Display Duration, then allows the longer of transition/resize duration to finish. Groups share the longest member change duration.", MessageType.Info);
            DrawPropertiesExcluding(serializedObject, "m_Script", "group", "slotIds", "slotId", "slotName", "playback", "sizeMode");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(VrcLinkingPosterGroup))]
    public class VrcLinkingPosterGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var group = (VrcLinkingPosterGroup)target;
            if (group.distribution == PosterGroupMode.UniqueDistribution)
                EditorGUILayout.HelpBox("Each display receives a different entry. The pool must contain at least as many IDs as displays. Staggered changes may temporarily duplicate images during a group change; uniqueness applies once the change finishes. Random and Shuffle Bag both shuffle each group round.", MessageType.Info);
        }
    }
}
