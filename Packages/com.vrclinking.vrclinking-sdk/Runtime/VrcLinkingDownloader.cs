using System;
using System.Diagnostics;
using System.Text;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRCLinking.Utilities;
using Debug = UnityEngine.Debug;

namespace VRCLinking
{   
    [RequireComponent(typeof(Compressor))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class VrcLinkingDownloader : UdonSharpBehaviour
    {
        public VRCUrl mainUrl;
        public VRCUrl fallbackUrl;
        bool _isLoadingFallback = false;

        public Compressor compressor;

        public string serverName;
        public string serverId;
        public string worldName;
        public Guid worldId;
        public float reloadTimer = 0f;


        void Start()
        {
            Log($"Downloading From: {mainUrl}");
            VRCStringDownloader.LoadUrl(mainUrl, (IUdonEventReceiver)this);

            if (compressor == null)
            {
                compressor = GetComponent<Compressor>();
                
                if (compressor == null)
                {
                    LogError("Compressor is not set.");
                    return;
                }
            }
        }

        public void ReloadData()
        {
            VRCStringDownloader.LoadUrl(mainUrl, (IUdonEventReceiver)this);
        }


        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            if (result.Error != null)
            {
                Debug.LogError("Failed to download string: " + result.Error);
                return;
            }

            if (result.Result.StartsWith("{"))
            {
                ParseData(result.Result);
            }
            else
            {
                var unicode = Encoding.Unicode.GetBytes(result.Result);

                compressor.StartDecompression(unicode, (IUdonEventReceiver)this, nameof(OnDecompressionSuccess));
            }

            if (reloadTimer > 0f)
            { 
                SendCustomEventDelayedSeconds(nameof(ReloadData), Mathf.Max(60f, reloadTimer));
            }
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError("Failed to download string: " + result.Error);

            if (_isLoadingFallback)
            {
                LogError("Failed to download from fallback url.");
                return;
            }
            
            VRCStringDownloader.LoadUrl(fallbackUrl, (IUdonEventReceiver)this);
            _isLoadingFallback = true;
        }

        public void OnDecompressionSuccess()
        {
            var textData = Encoding.UTF8.GetString(compressor.GetDecompressedData());

            ParseData(textData);
        }
        
        void Log(string message)
        {
            Debug.Log($"<color=gray>[{nameof(VrcLinkingDownloader)}]</color> {message}");
        }
        
        void LogError(string message)
        {
            Debug.LogError($"<color=gray>[{nameof(VrcLinkingDownloader)}]</color> {message}");
        }
    }
}