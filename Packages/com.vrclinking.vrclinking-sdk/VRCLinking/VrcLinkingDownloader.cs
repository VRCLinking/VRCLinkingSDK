using System;
using System.Diagnostics;
using System.Text;
using TMPro;
using UdonSharp;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRCLinking.Utilities;
using Debug = UnityEngine.Debug;

namespace VRCLinking
{   
    public partial class VrcLinkingDownloader : UdonSharpBehaviour
    {
        public VRCUrl mainUrl;
        public VRCUrl fallbackUrl;

        public LzwCompressor compressor;
        public TextMeshProUGUI text;


        void Start()
        {
            Debug.Log("Start");
            VRCStringDownloader.LoadUrl(mainUrl, (IUdonEventReceiver)this);
        }

        Stopwatch sw = new Stopwatch();

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            sw = System.Diagnostics.Stopwatch.StartNew();
            if (result.Error != null)
            {
                Debug.LogError("Failed to download string: " + result.Error);
                return;
            }

            Debug.Log("Downloaded string: " + result.Result.Length + " bytes");

            var unicode = Encoding.Unicode.GetBytes(result.Result);
            Debug.Log("ToArray time: " + sw.ElapsedMilliseconds + "ms");


            compressor.StartDecompression(unicode, (IUdonEventReceiver)this, nameof(OnDecompressionSuccess));
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError("Failed to download string: " + result.Error);
        }

        public void OnDecompressionSuccess()
        {
            Debug.Log("Decompression success");
            var bytesLength = compressor.GetDecompressedData().Length;
            var textData = Encoding.UTF8.GetString(compressor.GetDecompressedData());
            text.text =
                $"Decompression success\nDecompression time: {sw.ElapsedMilliseconds}ms\nResult bytes: {bytesLength}, KB: {bytesLength / 1024}, MB: {bytesLength / 1024 / 1024}" +
                $"\n{textData.Substring(0, Math.Min(1000, textData.Length))}";

            Debug.Log($"Result bytes: {compressor.GetDecompressedData().Length}");
            Debug.Log($"Decompression time: {sw.ElapsedMilliseconds}ms");
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