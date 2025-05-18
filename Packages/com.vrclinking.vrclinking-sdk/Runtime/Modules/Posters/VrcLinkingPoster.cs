using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace VRCLinking.Modules.Posters
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(VrcLinkingPoster))]
    public class PosterEditor : Editor
    {
        void OnEnable()
        {
            UpdatePostersModule();
        }

        public override void OnInspectorGUI()
        {
            VrcLinkingPoster script = (VrcLinkingPoster)target;
            var postersModule = FindObjectOfType<VrcLinkingPostersModule>(true);

            if (postersModule == null)
            {
                EditorGUILayout.HelpBox("No Poster Module found in the scene", MessageType.Error);
                return;
            }

            var newPosterID = EditorGUILayout.IntField("Poster Slot ID", script.slotId);
            var newPosterName = EditorGUILayout.TextField("Poster Slot Name", script.slotName);
            var newAutoSize = EditorGUILayout.Toggle("Enable Auto Size", script.enableAutoSize);
            var newWidthSizeControl = (SizeControl)EditorGUILayout.EnumPopup("Width Size Control",
                script.widthSizeControl);
            var newHeightSizeControl = (SizeControl)EditorGUILayout.EnumPopup("Height Size Control",
                script.heightSizeControl);

            if (newPosterID != script.slotId)
            {
                Undo.RecordObject(script, "Poster Change");
                script.slotId = newPosterID;
                script.gameObject.name = "Poster " + script.slotId;
                UpdatePostersModule();
                EditorUtility.SetDirty(script);
            }

            if (newPosterName != script.slotName)
            {
                Undo.RecordObject(script, "Poster Change");
                script.slotName = newPosterName;
                EditorUtility.SetDirty(script);
            }
            
            if (newAutoSize != script.enableAutoSize)
            {
                Undo.RecordObject(script, "Poster Change");
                script.enableAutoSize = newAutoSize;
                EditorUtility.SetDirty(script);
            }
            
            if (newWidthSizeControl != script.widthSizeControl)
            {
                Undo.RecordObject(script, "Poster Change");
                script.widthSizeControl = newWidthSizeControl;
                EditorUtility.SetDirty(script);
            }
            
            if (newHeightSizeControl != script.heightSizeControl)
            {
                Undo.RecordObject(script, "Poster Change");
                script.heightSizeControl = newHeightSizeControl;
                EditorUtility.SetDirty(script);
            }

            var idCount = postersModule.posterSlotIds.Count(x => x == script.slotId);
            if (idCount > 1)
            {
                EditorGUILayout.HelpBox("Poster ID " + script.slotId + " is used " + idCount + " times",
                    MessageType.Error);

                if (GUILayout.Button("Fix Poster IDs"))
                {
                    script.slotId = postersModule.posterSlotIds.Max() + 1;

                    UpdatePostersModule();
                    script.gameObject.name = "Poster " + script.slotId;
                }
            }
        }

        private void UpdatePostersModule()
        {
            var posterManager = FindObjectOfType<VrcLinkingPostersModule>(true);

            List<VrcLinkingPoster> posters = FindObjectsOfType<VrcLinkingPoster>(true).ToList();

            posterManager.posterSlotIds = posters.Select(x => x.slotId).ToArray();
            var meshRenderers = posters.Select(x => x.transform.GetComponentInChildren<MeshRenderer>());
            var renderers = meshRenderers.ToList();
            posterManager.posterSlotsRenderers = renderers.ToArray();
            posterManager.posterSlotsImages =
                posters.Select(x => x.transform.GetComponentInChildren<RawImage>()).ToArray();
            //posterManager.posters = posters.Select(x => x.gameObject).ToArray();
        }
    }


#endif
    public class VrcLinkingPoster : MonoBehaviour
    {
        public int slotId;
        public string slotName;
        public bool enableAutoSize = true;
        public SizeControl widthSizeControl = SizeControl.X;
        public SizeControl heightSizeControl = SizeControl.Z;

        public float GetAspectRatio()
        {
            var meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Figure out the 2 largest dimensions of the mesh
                var bounds = meshRenderer.bounds;
                var size = bounds.size;
                // Try to figure out aspect ration width/height
                var width = Mathf.Max(size.x, size.z);
                var height = Mathf.Max(size.y, size.z);
                var aspectRatio = width / height;
                // If the width is smaller than the height, flip the aspect ratio

                return aspectRatio;
            }

            // Try get aspect from raw image
            var rawImage = GetComponentInChildren<RawImage>();
            if (rawImage != null)
            {
                var texture = rawImage.texture;
                if (texture != null)
                {
                    return (float)texture.width / texture.height;
                }
            }

            // Lastly, just get get aspect from "transform" scale
            if (transform != null)
            {
                var scale = transform.lossyScale;
                var aspectRatio = scale.x / scale.y;
                return aspectRatio;
            }

            // If all else fails, return 1.0f (square)
            return 1.0f;
        }
    }
}