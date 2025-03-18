using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    public class SelectedWorldField : VisualElement
    {
        // Makes the LoginField show up as an option within UI Builder.
        public new class UxmlFactory : UxmlFactory<SelectedWorldField, UxmlTraits> {}
        
        private VisualElement ServerIconField;
        private Label ServerNameLabel;
        private Label WorldNameLabel;

        private VrcLinkingApiHelper _apiHelper;
        
        public SelectedWorldField()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("SelectedWorldField");
            uxml.CloneTree(this);
            
            ServerIconField = this.Q<VisualElement>(nameof(ServerIconField));
            ServerNameLabel = this.Q<Label>(nameof(ServerNameLabel));
            WorldNameLabel = this.Q<Label>(nameof(WorldNameLabel));
        }
        
        /// <summary>
        /// Copy the currently used API Helper.
        /// </summary>
        public void BindApi(VrcLinkingApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        /// <summary>
        /// Pass in the information needed to display the current server and world info.
        /// </summary>
        public async Task FillData(ServerWorldData worldData)
        {
            ServerNameLabel.text = worldData.ServerName;
            WorldNameLabel.text = worldData.WorldName;

            // If any of the values are empty, quit out.
            if (worldData.AnyEmpty())
            {
                ServerIconField.style.backgroundImage = null;
                return;
            }

            Guild guild = await _apiHelper.GetGuild(worldData.ServerId);

            // If the guild Icon url isn't valid...
            if (string.IsNullOrEmpty(guild.Icon))
            {
                ServerIconField.style.backgroundImage = null;
                return;
            }
            
            using (HttpClient client = new HttpClient())
            {
                byte[] imageByteData = await client.GetByteArrayAsync(guild.Icon);
                Texture2D icon = new Texture2D(0, 0);
                icon.LoadImage(imageByteData);

                ServerIconField.style.backgroundImage = icon;
            }
        }
        
        
    }
}