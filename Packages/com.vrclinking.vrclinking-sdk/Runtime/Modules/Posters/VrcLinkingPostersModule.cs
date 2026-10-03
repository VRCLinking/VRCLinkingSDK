using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace VRCLinking.Modules.Posters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VrcLinkingPostersModule : VrcLinkingModuleBase
    {
        public override string ModuleName => "VrcLinkingPostersModule";
        [Min(1)] public int maxAtlasCount = 25;
        public Material posterMaterial;
        public bool disablePostersOnBuild = true;
        [Tooltip("Retries per failed atlas. Playback remains in its local loading state if exhausted.")]
        [Min(0)] public int downloadRetries = 3;

        [UdonSynced] private bool _timelineInitialized;
        [UdonSynced] private int _seed;
        [UdonSynced] private double _startTime;

        [HideInInspector] public VRCUrl[] atlasUrlsVariation0;
        [HideInInspector] public VRCUrl[] atlasUrlsVariation1;
        // Flattened authoring data: one playlist slice per physical display.
        [HideInInspector] public int[] playlistIds;
        [HideInInspector] public int[] playlistOffsets;
        [HideInInspector] public int[] playlistCounts;
        [HideInInspector] public int[] sequenceKeys;
        [HideInInspector] public int[] memberIndices;
        [HideInInspector] public bool[] uniqueGroups;
        [HideInInspector] public int[] playbackModes;
        [HideInInspector] public int[] orders;
        [HideInInspector] public bool[] synchronizedPlayback;
        [HideInInspector] public bool[] randomStarts;
        [HideInInspector] public int[] transitions;
        [HideInInspector] public int[] directions;
        // x = hold, y = image transition, z = start delay (including stagger), w = resize.
        [HideInInspector] public Vector4[] timings;
        [HideInInspector] public float[] groupChangeDurations;
        [HideInInspector] public int[] sizeModes;
        [HideInInspector] public int[] fitModes;
        [HideInInspector] public Color[] backgroundColors;
        [HideInInspector] public Color[] fadeColors;
        [HideInInspector] public GameObject[] posterRoots;
        [HideInInspector] public MeshRenderer[] posterSlotsRenderers;
        [HideInInspector] public RawImage[] posterSlotsImages;
        [HideInInspector] public SizeControl[] posterSlotsWidth;
        [HideInInspector] public SizeControl[] posterSlotsHeight;
        [HideInInspector] public Material[] imageMaterials;

        private VRCImageDownloader _imageDownloader;
        private MaterialPropertyBlock _block;
        private bool _setup;
        private bool _metadataAccepted;
        private bool _ready;
        private bool _requestInFlight;
        private bool _retryScheduled;
        private bool _publishPending;
        private int _variation;
        private int _atlasIndex;
        private int _retryCount;
        private Texture2D[] _atlases;
        private bool[] _neededAtlases;
        private int[] _metadataSlots;
        private int[] _metadataAtlases;
        private Vector4[] _metadataRects;
        private int[] _entryMetadata;
        private int[] _shuffle;
        private int[] _cachedStep;
        private int[] _from;
        private int[] _to;
        private float[] _nextUpdate;
        private float[] _lastBlend;
        private float[] _lastResize;
        private Vector3[] _originalScales;
        private Vector2[] _originalSizes;
        private Vector2[] _originalUiSizes;
        private bool[] _displayShown;
        private int _localSeed;
        private double _localStart;

        private void Start()
        {
            Setup();
            EnsureTimeline();
        }

        private void Setup()
        {
            if (_setup) return;
            _setup = true;
            _imageDownloader = new VRCImageDownloader();
            _block = new MaterialPropertyBlock();
            _localSeed = UnityEngine.Random.Range(1, 2147483646);
            _localStart = Networking.GetServerTimeInSeconds();
            int count = playlistCounts == null ? 0 : playlistCounts.Length;
            _cachedStep = new int[count];
            _from = new int[count];
            _to = new int[count];
            _nextUpdate = new float[count];
            _lastBlend = new float[count];
            _lastResize = new float[count];
            _originalScales = new Vector3[count];
            _originalSizes = new Vector2[count];
            _originalUiSizes = new Vector2[count];
            _displayShown = new bool[count];
            int largest = 1;
            for (int i = 0; i < count; i++)
            {
                largest = Mathf.Max(largest, playlistCounts[i]);
                _cachedStep[i] = -1;
                var renderer = posterSlotsRenderers[i];
                var image = posterSlotsImages[i];
                if (renderer != null)
                {
                    var tf = renderer.transform;
                    _originalScales[i] = tf.localScale;
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    _originalSizes[i] = new Vector2(
                        Mathf.Abs(mesh.bounds.size[(int)posterSlotsWidth[i]] * tf.lossyScale[(int)posterSlotsWidth[i]]),
                        Mathf.Abs(mesh.bounds.size[(int)posterSlotsHeight[i]] * tf.lossyScale[(int)posterSlotsHeight[i]]));
                }
                else if (image != null)
                {
                    _originalUiSizes[i] = image.rectTransform.rect.size;
                    var scale = image.rectTransform.lossyScale;
                    _originalSizes[i] = new Vector2(_originalUiSizes[i].x * Mathf.Abs(scale.x), _originalUiSizes[i].y * Mathf.Abs(scale.y));
                }
            }
            _shuffle = new int[largest];
        }

        public void EnsureTimeline()
        {
            Setup();
            if (_timelineInitialized) return;
            // Never depend on local image loading. Do not overwrite late-join state before networking settles.
            if (Networking.IsOwner(gameObject) && Networking.IsNetworkSettled)
            {
                _seed = _localSeed;
                _startTime = Networking.GetServerTimeInSeconds();
                _timelineInitialized = true;
                _publishPending = true;
                RequestSerialization();
                return;
            }
            SendCustomEventDelayedSeconds(nameof(EnsureTimeline), 1);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (player.isLocal && !_timelineInitialized) EnsureTimeline();
            // An initialized clock continues unchanged when its owner leaves.
        }

        public override void OnDeserialization()
        {
            if (!_setup) return;
            for (int i = 0; i < _cachedStep.Length; i++)
            {
                _cachedStep[i] = -1;
                _nextUpdate[i] = 0;
            }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            _publishPending = !result.success;
            if (_publishPending) SendCustomEventDelayedSeconds(nameof(RetryTimelinePublish), 2);
        }

        public void RetryTimelinePublish()
        {
            if (_publishPending && Networking.IsOwner(gameObject)) RequestSerialization();
        }

        public override void OnDataLoaded()
        {
            Setup();
            if (_metadataAccepted) return;
            if (downloader == null || !downloader.TryGetAtlasDetail(out var details)) return;
            if (!details.TryGetValue("Variation", out var variation) || !variation.IsNumber ||
                (variation.Number != 0 && variation.Number != 1) ||
                !details.TryGetValue("MetaData", TokenType.DataList, out var metadata))
            {
                LogError("Invalid poster atlas metadata; waiting for valid data.");
                return;
            }
            _variation = (int)variation.Number;
            var urls = _variation == 0 ? atlasUrlsVariation0 : atlasUrlsVariation1;
            if (urls == null || urls.Length == 0) return;
            _atlases = new Texture2D[urls.Length];
            _neededAtlases = new bool[urls.Length];
            var list = metadata.DataList;
            _metadataSlots = new int[list.Count];
            _metadataAtlases = new int[list.Count];
            _metadataRects = new Vector4[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TokenType != TokenType.DataDictionary) { LogError("Invalid poster entry."); return; }
                var entry = list[i].DataDictionary;
                if (!ReadInteger(entry, "SlotId") || !ReadInteger(entry, "AtlasIndex") ||
                    !ReadInteger(entry, "X") || !ReadInteger(entry, "Y") ||
                    !ReadInteger(entry, "Width") || !ReadInteger(entry, "Height"))
                { LogError("Invalid poster coordinates."); return; }
                int atlas = (int)entry["AtlasIndex"].Number;
                int slot = (int)entry["SlotId"].Number;
                var rect = new Vector4((float)entry["Width"].Number, (float)entry["Height"].Number,
                    (float)entry["X"].Number, (float)entry["Y"].Number);
                if (atlas < 0 || atlas >= urls.Length || slot < 0 || rect.x <= 0 || rect.y <= 0 || rect.z < 0 || rect.w < 0)
                { LogError("Poster atlas exceeds configured limits or has invalid dimensions."); return; }
                _metadataSlots[i] = slot;
                _metadataAtlases[i] = atlas;
                _metadataRects[i] = rect;
            }
            _entryMetadata = new int[playlistIds.Length];
            for (int i = 0; i < playlistIds.Length; i++)
            {
                _entryMetadata[i] = Array.IndexOf(_metadataSlots, playlistIds[i]);
                // Server metadata may contain slots not used by this world's displays.
                // Missing playlist slots keep their background fallback without a download.
                if (_entryMetadata[i] >= 0)
                    _neededAtlases[_metadataAtlases[_entryMetadata[i]]] = true;
            }
            _metadataAccepted = true;
            _atlasIndex = 0;
            LoadNextAtlas();
        }

        private bool ReadInteger(DataDictionary entry, string key)
        {
            if (!entry.TryGetValue(key, out var value) || !value.IsNumber) return false;
            double number = value.Number;
            return number >= 0 && number <= int.MaxValue && number == Math.Floor(number);
        }

        private void LoadNextAtlas()
        {
            if (_requestInFlight || _ready) return;
            while (_atlasIndex < _atlases.Length && !_neededAtlases[_atlasIndex]) _atlasIndex++;
            if (_atlasIndex == _atlases.Length)
            {
                _ready = true;
                Log("All required poster atlases loaded locally; joining playback timeline.");
                return;
            }
            var urls = _variation == 0 ? atlasUrlsVariation0 : atlasUrlsVariation1;
            _requestInFlight = true;
            _imageDownloader.DownloadImage(urls[_atlasIndex], null, (IUdonEventReceiver)this, null);
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            _requestInFlight = false;
            var texture = result.Result;
            if (texture == null) { ScheduleAtlasRetry(); return; }
            for (int i = 0; i < _entryMetadata.Length; i++)
            {
                int metadata = _entryMetadata[i];
                if (metadata < 0) continue;
                var rect = _metadataRects[metadata];
                if (_metadataAtlases[metadata] == _atlasIndex && (rect.x + rect.z > texture.width || rect.y + rect.w > texture.height))
                {
                    LogError("Poster atlas coordinates do not fit the downloaded image; retaining local loading state.");
                    ScheduleAtlasRetry();
                    return;
                }
            }
            _atlases[_atlasIndex] = texture;
            _atlasIndex++;
            _retryCount = 0;
            LoadNextAtlas();
        }

        public override void OnImageLoadError(IVRCImageDownload result)
        {
            _requestInFlight = false;
            LogError($"Poster atlas {_atlasIndex} failed: {result.ErrorMessage}");
            ScheduleAtlasRetry();
        }

        private void ScheduleAtlasRetry()
        {
            if (_retryCount >= downloadRetries)
            {
                LogError("Poster downloads stopped locally. Enable image URLs if needed, then call RetryAtlasDownloads to retry.");
                return;
            }
            _retryCount++;
            _retryScheduled = true;
            SendCustomEventDelayedSeconds(nameof(RetryAtlasDownload), Mathf.Min(60, 5 * _retryCount));
        }

        public void RetryAtlasDownload()
        {
            _retryScheduled = false;
            LoadNextAtlas();
        }

        public void RetryAtlasDownloads()
        {
            if (!_metadataAccepted) { OnDataLoaded(); return; }
            if (_ready || _requestInFlight || _retryScheduled) return;
            _retryCount = 0;
            LoadNextAtlas();
        }

        private void Update()
        {
            if (!_ready) return;
            double now = Networking.GetServerTimeInSeconds();
            for (int i = 0; i < playlistCounts.Length; i++)
            {
                if (synchronizedPlayback[i] && !_timelineInitialized) continue;
                if (Time.time < _nextUpdate[i]) continue;
                var timing = timings[i];
                double elapsed = Networking.CalculateServerDeltaTime(now, synchronizedPlayback[i] ? _startTime : _localStart);
                // Delay the first display as well as cycling, including Static/ChooseOnce.
                // Late joiners whose shared delay has elapsed still catch up immediately.
                if (elapsed < timing.z)
                {
                    _nextUpdate[i] = Time.time + 0.1f;
                    continue;
                }
                double time = elapsed - timing.z;
                float changeLength = Mathf.Max(groupChangeDurations[i], Mathf.Max(timing.y, sizeModes[i] == 3 ? timing.w : 0));
                double period = timing.x + changeLength;
                bool cycling = playbackModes[i] == 2 && playlistCounts[i] > 1;
                if (!cycling && _displayShown[i]) continue;
                int step = cycling ? (int)Math.Floor(time / period) : 0;
                float phase = cycling ? (float)(time - step * period) : 0;
                bool stepChanged = _cachedStep[i] != step;
                if (stepChanged)
                {
                    _from[i] = SelectEntry(i, step);
                    _to[i] = cycling ? SelectEntry(i, step + 1) : _from[i];
                    _cachedStep[i] = step;
                }
                float changeTime = phase - timing.x;
                float blend = !cycling || changeTime < 0 ? 0 : timing.y <= 0 ? 1 : Mathf.Clamp01(changeTime / timing.y);
                float resize = !cycling || changeTime < 0 ? 0 : sizeModes[i] == 3 && timing.w > 0 ? Mathf.Clamp01(changeTime / timing.w) : 1;
                if (stepChanged || !_displayShown[i] || blend != _lastBlend[i] || resize != _lastResize[i])
                {
                    ApplyDisplay(i, _from[i], _to[i], blend, resize);
                    _lastBlend[i] = blend;
                    _lastResize[i] = resize;
                }
                // Only changing posters need per-frame Udon work; idle posters check at 10 Hz.
                _nextUpdate[i] = cycling && changeTime >= -0.1f && (blend < 1 || resize < 1) ? 0 : Time.time + 0.1f;
            }
        }

        // Park-Miller with Schrage's method: no overflow or unsupported Udon long modulus.
        private int NextRandom(int value)
        {
            int next = 48271 * (value % 44488) - 3399 * (value / 44488);
            return next > 0 ? next : next + 2147483647;
        }

        private int SequenceSeed(int display, int cycle)
        {
            int value = (synchronizedPlayback[display] ? _seed : _localSeed) ^ sequenceKeys[display] ^ NextRandom(cycle + 1);
            return NextRandom(Mathf.Max(1, value));
        }

        private void Shuffle(int count, int seed)
        {
            for (int i = 0; i < count; i++) _shuffle[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                seed = NextRandom(seed);
                int j = seed % (i + 1);
                int temp = _shuffle[i]; _shuffle[i] = _shuffle[j]; _shuffle[j] = temp;
            }
        }

        private int SelectEntry(int display, int step)
        {
            int count = playlistCounts[display];
            int selected = 0;
            int seed = SequenceSeed(display, 0);
            int start = randomStarts[display] ? NextRandom(seed) % count : 0;
            if (uniqueGroups[display])
            {
                selected = (memberIndices[display] + step + start) % count;
                if (orders[display] != 0 || playbackModes[display] == 1)
                {
                    Shuffle(count, SequenceSeed(display, step));
                    selected = _shuffle[memberIndices[display] % count];
                }
            }
            else if (playbackModes[display] == 1) selected = NextRandom(seed) % count;
            else if (playbackModes[display] == 0 || orders[display] == 0) selected = (step + start) % count;
            else if (orders[display] == 1) selected = NextRandom(SequenceSeed(display, step)) % count;
            else
            {
                int position = step % count;
                int bag = step / count;
                if (count <= 2) { Shuffle(count, seed); selected = _shuffle[position]; }
                else
                {
                    // Swapping the first two leaves the previous bag's last entry unchanged.
                    // This permits direct late-join access without replaying historical bags.
                    int previousLast = -1;
                    if (bag > 0) { Shuffle(count, SequenceSeed(display, bag - 1)); previousLast = _shuffle[count - 1]; }
                    Shuffle(count, SequenceSeed(display, bag));
                    if (_shuffle[0] == previousLast)
                    { int temp = _shuffle[0]; _shuffle[0] = _shuffle[1]; _shuffle[1] = temp; }
                    selected = _shuffle[position];
                }
            }
            return _entryMetadata[playlistOffsets[display] + selected];
        }

        private Vector2 FittedSize(int display, int metadata)
        {
            Vector2 bounds = _originalSizes[display];
            if (metadata < 0 || sizeModes[display] == 1) return bounds;
            var rect = _metadataRects[metadata];
            float aspect = rect.x / rect.y;
            return aspect > bounds.x / bounds.y ? new Vector2(bounds.x, bounds.x / aspect) : new Vector2(bounds.y * aspect, bounds.y);
        }

        private Vector4 AtlasRect(int metadata)
        {
            if (metadata < 0) return new Vector4(1, 1, 0, 0);
            var rect = _metadataRects[metadata];
            var texture = _atlases[_metadataAtlases[metadata]];
            return new Vector4(rect.x / texture.width, rect.y / texture.height,
                rect.z / texture.width, 1 - (rect.w + rect.y) / texture.height);
        }

        private float Aspect(int metadata) => metadata < 0 ? 1 : _metadataRects[metadata].x / _metadataRects[metadata].y;

        private void ApplyDisplay(int display, int from, int to, float blend, float resize)
        {
            Vector2 size = Vector2.Lerp(FittedSize(display, from), FittedSize(display, to), Mathf.SmoothStep(0, 1, resize));
            var renderer = posterSlotsRenderers[display];
            var image = posterSlotsImages[display];
            if (sizeModes[display] != 1)
            {
                if (renderer != null)
                {
                    var scale = _originalScales[display];
                    scale[(int)posterSlotsWidth[display]] *= size.x / _originalSizes[display].x;
                    scale[(int)posterSlotsHeight[display]] *= size.y / _originalSizes[display].y;
                    renderer.transform.localScale = scale;
                }
                else if (image != null)
                {
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originalUiSizes[display].x * size.x / _originalSizes[display].x);
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _originalUiSizes[display].y * size.y / _originalSizes[display].y);
                }
            }
            // Missing slots keep their place in the sequence and render the background color.
            Texture textureA = from < 0 ? Texture2D.whiteTexture : _atlases[_metadataAtlases[from]];
            Texture textureB = to < 0 ? Texture2D.whiteTexture : _atlases[_metadataAtlases[to]];
            var texelA = new Vector4(1f / textureA.width, 1f / textureA.height, textureA.width, textureA.height);
            var texelB = new Vector4(1f / textureB.width, 1f / textureB.height, textureB.width, textureB.height);
            var rectA = AtlasRect(from);
            var rectB = AtlasRect(to);
            var aspects = new Vector4(Aspect(from), Aspect(to), size.x / Mathf.Max(0.0001f, size.y), fitModes[display]);
            var state = new Vector4(blend, transitions[display], directions[display], 1);
            var available = new Vector4(from >= 0 ? 1 : 0, to >= 0 ? 1 : 0, 0, 0);
            if (renderer != null)
            {
                if (!_displayShown[display]) renderer.sharedMaterial = posterMaterial;
                renderer.GetPropertyBlock(_block);
                _block.SetTexture("_MainTex", textureA);
                _block.SetTexture("_NextTex", textureB);
                _block.SetVector("_MainTex_TexelSize", texelA);
                _block.SetVector("_NextTex_TexelSize", texelB);
                _block.SetVector("_MainTex_Offset", rectA);
                _block.SetVector("_NextRect", rectB);
                _block.SetVector("_PosterAspects", aspects);
                _block.SetVector("_PosterState", state);
                _block.SetVector("_PosterAvailable", available);
                _block.SetColor("_BoxingColor", backgroundColors[display]);
                _block.SetColor("_FadeColor", fadeColors[display]);
                renderer.SetPropertyBlock(_block);
                _block.Clear();
            }
            else if (image != null)
            {
                if (!_displayShown[display]) { image.material = imageMaterials[display]; image.uvRect = new Rect(0, 0, 1, 1); }
                image.texture = textureA;
                var material = image.materialForRendering;
                material.SetTexture("_NextTex", textureB);
                material.SetVector("_MainTex_TexelSize", texelA);
                material.SetVector("_NextTex_TexelSize", texelB);
                material.SetVector("_MainTex_Offset", rectA);
                material.SetVector("_NextRect", rectB);
                material.SetVector("_PosterAspects", aspects);
                material.SetVector("_PosterState", state);
                material.SetVector("_PosterAvailable", available);
                material.SetColor("_BoxingColor", backgroundColors[display]);
                material.SetColor("_FadeColor", fadeColors[display]);
            }
            if (!_displayShown[display])
            {
                // Reactivate the authored root, not just its child renderer.
                if (disablePostersOnBuild) posterRoots[display].SetActive(true);
                _displayShown[display] = true;
            }
        }

        private void OnDestroy()
        {
            if (_imageDownloader != null) _imageDownloader.Dispose();
        }
    }
}
