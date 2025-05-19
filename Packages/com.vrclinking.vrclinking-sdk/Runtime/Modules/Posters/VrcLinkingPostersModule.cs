using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

// ReSharper disable once CheckNamespace
namespace VRCLinking.Modules.Posters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VrcLinkingPostersModule : VrcLinkingModuleBase
    {
        private protected override string ModuleName => "VrcLinkingPostersModule";

        public int maxAtlasCount = 25;
        [Header("Material Settings")]
        public Material posterMaterial;
        //public int posterMaterialIndex = 0;
        
        public bool disablePostersOnBuild = true;

        [HideInInspector] public VRCUrl[] atlasUrlsVariation0;
        [HideInInspector] public VRCUrl[] atlasUrlsVariation1;

        [HideInInspector] public int[] posterSlotIds;
        [HideInInspector] public MeshRenderer[] posterSlotsRenderers;
        [HideInInspector] public RawImage[] posterSlotsImages;
        [HideInInspector] public SizeControl[] posterSlotsWidth;
        [HideInInspector] public SizeControl[] posterSlotsHeight;
        [HideInInspector] public bool[] posterAutoSize;

        [HideInInspector] public Material[] atlasMaterials;

        VRCImageDownloader _imageDownloader;
        DataList _posterMetadata;
        DataDictionary _atlasDetails;
        int _variation;
        int _atlasCount;
        int _loadedAtlasCount;
        MaterialPropertyBlock _materialPropertyBlock;

        void Start()
        {
            _imageDownloader = new VRCImageDownloader();
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public override void OnDataLoaded()
        {
            if (downloader.TryGetAtlasDetail(out var detail))
            {
                _atlasDetails = detail;
                LoadAtlasDetails();
            }
            else
            {
                LogError("Atlas Details not found");
            }
        }

        void LoadAtlasDetails()
        {
            if (!_atlasDetails.TryGetValue("MetaData", out var posterMetadata))
            {
                LogError("Atlas metadata not found");
                return;
            }

            if (posterMetadata.TokenType != TokenType.DataList)
            {
                LogError("Atlas metadata is not a list");
                return;
            }

            if (!_atlasDetails.TryGetValue("LastGeneration", out var lastGeneration))
            {
                LogError("Atlas metadata does not contain a last generation");
                return;
            }

            if (!_atlasDetails.TryGetValue("Variation", out var variation))
            {
                LogError("Atlas metadata does not contain a variation");
                return;
            }

            if (!variation.IsNumber)
            {
                LogError("Atlas metadata variation is not a number");
                return;
            }

            if (lastGeneration.TokenType != TokenType.String)
            {
                LogError("Atlas metadata last generation is not a string");
                return;
            }

            _variation = (int)variation.Number;

            _posterMetadata = posterMetadata.DataList;

            _atlasCount = FindAtlasCount();
            if (_atlasCount > 0)
            {
                LoadNextAtlas();
            }
        }

        void LoadNextAtlas()
        {
            if (_loadedAtlasCount >= _atlasCount)
            {
                Log("All atlases loaded");
                return;
            }

            var atlasIndex = _loadedAtlasCount;
            var atlasUrl = _variation == 0 ? atlasUrlsVariation0[atlasIndex] : atlasUrlsVariation1[atlasIndex];
            if (atlasUrl == null)
            {
                LogError($"Atlas URL not found for index {atlasIndex}");
                return;
            }


            _imageDownloader.DownloadImage(atlasUrl, null, (IUdonEventReceiver)this, null);
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            var texture = result.Result;
            var atlasMetadata = GetAtlasMetadata(_loadedAtlasCount);

            if (!texture)
            {
                LogError("Texture is null");
                return;
            }

            ProcessAtlasMetadata(atlasMetadata, texture);

            _loadedAtlasCount++;
            LoadNextAtlas();
        }

        void ProcessAtlasMetadata(DataList atlasMetadata, Texture2D texture)
        {
            var newMaterial = atlasMaterials[_loadedAtlasCount];

            newMaterial.mainTexture = texture;
            for (int i = 0; i < atlasMetadata.Count; i++)
            {
                var metadata = atlasMetadata[i].DataDictionary;
                var offsetX = (int)metadata["X"].Number;
                var offsetY = (int)metadata["Y"].Number;
                var targetWidth = (int)metadata["Width"].Number;
                var targetHeight = (int)metadata["Height"].Number;
                var slotId = (int)metadata["SlotId"].Number;

                var slotIndex = FindIndexBySlotId(slotId);
                var meshRenderer = FindMeshRendererBySlotId(slotId);
                
                Debug.Log($"SlotId: {slotId} - Index: {slotIndex} - OffsetX: {offsetX} - OffsetY: {offsetY} - Width: {targetWidth} - Height: {targetHeight}");
                
                if (meshRenderer)
                {
                    Debug.Log(posterSlotsWidth.Length);
                    Debug.Log(posterSlotsHeight.Length);
                    var width = posterSlotsWidth[slotIndex];
                    var height = posterSlotsHeight[slotIndex];

                    if (posterAutoSize[slotIndex])
                    {
                        SetRendererSize(meshRenderer, targetWidth, targetHeight, width, height);
                    }

                    // if (meshRenderer.materials.Length <= posterMaterialIndex)
                    // {
                    //     LogError($"MeshRenderer {meshRenderer.name} does not have enough materials");
                    //     return;
                    // }
                    
                    meshRenderer.material = newMaterial;
                    _materialPropertyBlock.SetVector("_SurfaceDimensions", new Vector2(
                        GetLength(width, meshRenderer.gameObject),
                        GetLength(height, meshRenderer.gameObject)
                    ));


                    _materialPropertyBlock.SetVector("_MainTex_Offset", new Vector4(
                        targetWidth, targetHeight,
                        offsetX, 2048 - offsetY - targetHeight
                    ) / 2048f);

                    meshRenderer.SetPropertyBlock(_materialPropertyBlock);
                    meshRenderer.gameObject.SetActive(true);
                }


                var image = FindImageBySlotId(slotId);

                if (image)
                {
                    SetImageData(image, texture, targetWidth, targetHeight, offsetX, offsetY);
                    image.gameObject.SetActive(true);
                }
            }
        }

        MeshRenderer FindMeshRendererBySlotId(int slotId)
        {
            var slotIndex = Array.IndexOf(posterSlotIds, slotId);

            if (slotIndex < 0 || slotIndex >= posterSlotsRenderers.Length)
            {
                LogError($"MeshRenderer not found for SlotId {slotId}");
                return null;
            }

            return posterSlotsRenderers[slotIndex];
        }

        int FindIndexBySlotId(int slotId) => Array.IndexOf(posterSlotIds, slotId);

        RawImage FindImageBySlotId(int slotId)
        {
            var slotIndex = Array.IndexOf(posterSlotIds, slotId);

            if (slotIndex < 0 || slotIndex >= posterSlotsImages.Length)
            {
                LogError($"Image not found for SlotId {slotId}");
                return null;
            }

            return posterSlotsImages[slotIndex];
        }

        int FindAtlasCount()
        {
            double highestAtlas = -1;
            for (int i = 0; i < _posterMetadata.Count; i++)
            {
                var atlasIndex = _posterMetadata[i].DataDictionary["AtlasIndex"].Number;
                if (atlasIndex > highestAtlas)
                {
                    highestAtlas = atlasIndex;
                }
            }

            return (int)highestAtlas + 1;
        }

        DataList GetAtlasMetadata(int atlas)
        {
            var atlasMetadata = new DataList();
            for (int i = 0; i < _posterMetadata.Count; i++)
            {
                var atlasIndex = (int)_posterMetadata[i].DataDictionary["AtlasIndex"].Number;
                if (atlasIndex == atlas)
                {
                    atlasMetadata.Add(_posterMetadata[i]);
                } 
            }

            return atlasMetadata;
        }

        void SetRendererSize(MeshRenderer meshRenderer, float targetWidth, float targetHeight, SizeControl width,
            SizeControl height)
        {
            var tf = meshRenderer.transform;
            var mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
            if (!mesh)
            {
                LogError($"Mesh not found on {meshRenderer.name}");
                return;
            }

            // Get mesh size in local space
            var meshSize = mesh.bounds.size;

            // Get world size per axis
            var lossyScale = tf.lossyScale;
            var worldWidth = meshSize[(int)width] * lossyScale[(int)width];
            var worldHeight = meshSize[(int)height] * lossyScale[(int)height];

            var aspectRatio = targetWidth / targetHeight;
            var originalAspect = worldWidth / worldHeight;

            float finalWidth;
            float finalHeight;

            // Fit while preserving aspect and clamping to original size
            if (aspectRatio > originalAspect)
            {
                finalHeight = Mathf.Min(worldHeight, targetHeight);
                finalWidth = finalHeight * aspectRatio;
                if (finalWidth > worldWidth)
                {
                    finalWidth = worldWidth;
                    finalHeight = finalWidth / aspectRatio;
                }
            }
            else
            {
                finalWidth = Mathf.Min(worldWidth, targetWidth);
                finalHeight = finalWidth / aspectRatio;
                if (finalHeight > worldHeight)
                {
                    finalHeight = worldHeight;
                    finalWidth = finalHeight * aspectRatio;
                }
            }

            // Calculate new localScale based on original world size
            var newLocalScale = tf.localScale;
            newLocalScale[(int)width] *= (finalWidth / worldWidth);
            newLocalScale[(int)height] *= (finalHeight / worldHeight);

            tf.localScale = newLocalScale;
        }

        float GetLength(SizeControl size, GameObject sizeTransform)
        {
            var meshFilter = sizeTransform.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                LogError($"MeshFilter not found on {sizeTransform.name}");
                return 0;
            }

            var mesh = meshFilter.sharedMesh;
            if (!mesh)
            {
                LogError($"Mesh not found on {sizeTransform.name}");
                return 0;
            }

            var bounds = mesh.bounds;
            var sizeValue = bounds.size[(int)size];
            var scale = sizeTransform.transform.lossyScale;
            var length = sizeValue * scale[(int)size];
            return length;
        }

        void SetMaterialData(Material material, Texture2D texture)
        {
            material.mainTexture = texture;
        }

        void SetImageData(RawImage image, Texture2D texture, int imageWidth, int imageHeight, int offsetX, int offsetY)
        {
            image.texture = texture;

            var textureWidth = texture.width;
            var textureHeight = texture.height;

            var tiling = new Vector2((float)imageWidth / textureWidth, (float)imageHeight / textureHeight);
            var offset = new Vector2((float)offsetX / textureWidth,
                -(imageHeight / (float)textureHeight) - (float)offsetY / textureHeight);

            image.uvRect = new Rect(offset.x, offset.y, tiling.x, tiling.y);
        }
    }
}