using System;
using UnityEngine;

namespace VRCLinking.Modules.Posters
{
    public enum PosterPlaybackMode { Static, ChooseOnce, Cycle }
    public enum PosterOrder { Sequential, Random, ShuffleBag }
    public enum PosterTransition { Instant, Crossfade, FadeThroughColor, Slide, Wipe }
    public enum PosterDirection { Left, Right, Up, Down }
    public enum PosterFitMode { Fit, Fill, Stretch }
    public enum PosterSizeMode { Automatic, Fixed, Instant, Animated }
    public enum PosterGroupMode { SharedTiming, UniqueDistribution }

    [Serializable]
    public class PosterPlaybackSettings
    {
        public PosterPlaybackMode mode = PosterPlaybackMode.Cycle;
        public PosterOrder order = PosterOrder.ShuffleBag;
        public bool synchronize = true;
        [Tooltip("Offsets sequential/static playback. Random and shuffled orders choose their own starting entry.")]
        public bool randomStart = true;
        public PosterTransition transition = PosterTransition.Crossfade;
        public PosterDirection direction = PosterDirection.Left;
        [Min(0.1f)] public float displayDuration = 20;
        [Min(0)] public float transitionDuration = 1;
        [Min(0)] public float startDelay;
        [Min(0)] public float staggerSeconds;
        public Color fadeColor = Color.black;
    }

    [AddComponentMenu("VRCLinking/Poster Group")]
    public class VrcLinkingPosterGroup : MonoBehaviour
    {
        public string slotName;
        public int[] slotIds = new int[0];
        public PosterGroupMode distribution = PosterGroupMode.UniqueDistribution;
        public PosterPlaybackSettings playback = new PosterPlaybackSettings();
    }
}
