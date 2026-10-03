using UnityEngine;
using UnityEngine.UI;

namespace VRCLinking.Modules.Posters
{
    // Authoring only. PostersBuildHandler bakes this into the module and strips it.
    public class VrcLinkingPoster : MonoBehaviour
    {
        public int slotId;
        public string slotName;
        [Tooltip("Empty uses the original Slot ID. Otherwise this is the complete playlist.")]
        public int[] slotIds = new int[0];
        [Tooltip("Optional shared playlist and playback settings.")]
        public VrcLinkingPosterGroup group;
        public PosterPlaybackSettings playback = new PosterPlaybackSettings();

        public PosterSizeMode sizeMode = PosterSizeMode.Automatic;
        [Tooltip("Used by Automatic sizing for existing single-slot posters only.")]
        public bool enableAutoSize = true;
        public PosterFitMode fitMode = PosterFitMode.Fit;
        [Min(0)] public float resizeDuration = 4;
        public Color backgroundColor = Color.black;
        public SizeControl widthSizeControl = SizeControl.X;
        public SizeControl heightSizeControl = SizeControl.Z;
        [Tooltip("Optional explicit target. Otherwise the first child mesh/UI image is used.")]
        public MeshRenderer targetRenderer;
        public RawImage targetImage;

        public int[] GetSlotIds()
        {
            if (group != null) return group.slotIds ?? new int[0];
            return slotIds != null && slotIds.Length > 0 ? slotIds : new[] { slotId };
        }

        public float GetAspectRatio()
        {
            var meshRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<MeshRenderer>(true);
            var filter = meshRenderer != null ? meshRenderer.GetComponent<MeshFilter>() : null;
            if (filter != null && filter.sharedMesh != null)
            {
                var size = filter.sharedMesh.bounds.size;
                var scale = meshRenderer.transform.lossyScale;
                return Mathf.Abs(size[(int)widthSizeControl] * scale[(int)widthSizeControl]) /
                       Mathf.Max(0.0001f, Mathf.Abs(size[(int)heightSizeControl] * scale[(int)heightSizeControl]));
            }
            var image = targetImage != null ? targetImage : GetComponentInChildren<RawImage>(true);
            return image != null ? Mathf.Abs(image.rectTransform.rect.width) /
                Mathf.Max(0.0001f, Mathf.Abs(image.rectTransform.rect.height)) : 1;
        }
    }
}
