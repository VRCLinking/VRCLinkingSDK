using VRCLinking;
using UnityEditor;

namespace VRCLinking.Editor
{
    [CustomEditor(typeof(VrcLinkingDownloader))]
    public class VRCLinkingDownloaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            VrcLinkingDownloader downloader = (VrcLinkingDownloader)target;
            
            
        }
        
        
    }
}