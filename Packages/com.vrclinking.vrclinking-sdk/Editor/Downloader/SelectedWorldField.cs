using System;
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
        private const float MinimumReloadInterval = 60f;

        // Makes the LoginField show up as an option within UI Builder.
        public new class UxmlFactory : UxmlFactory<SelectedWorldField, UxmlTraits> {}

        public event Action<float> OnReloadTimerChanged;
        
        private VisualElement ServerIconField;
        private Label ServerNameLabel;
        private Label WorldNameLabel;
        private Toggle AutomaticReloadToggle;
        private FloatField ReloadIntervalField;

        private VrcLinkingApiHelper _apiHelper;
        private float _lastEnabledReloadInterval = MinimumReloadInterval;
        
        public SelectedWorldField()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("SelectedWorldField");
            uxml.CloneTree(this);
            
            ServerIconField = this.Q<VisualElement>(nameof(ServerIconField));
            ServerNameLabel = this.Q<Label>(nameof(ServerNameLabel));
            WorldNameLabel = this.Q<Label>(nameof(WorldNameLabel));
            AutomaticReloadToggle = this.Q<Toggle>(nameof(AutomaticReloadToggle));
            ReloadIntervalField = this.Q<FloatField>(nameof(ReloadIntervalField));

            ReloadIntervalField.isDelayed = true;

            AutomaticReloadToggle.RegisterValueChangedCallback(OnAutomaticReloadToggleChanged);
            ReloadIntervalField.RegisterValueChangedCallback(OnReloadIntervalChanged);

            SetReloadTimer(0f);
        }
        
        /// <summary>
        /// Copy the currently used API Helper.
        /// </summary>
        public void BindApi(VrcLinkingApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public void SetReloadTimer(float reloadTimer)
        {
            float normalizedReloadTimer = NormalizeReloadTimer(reloadTimer);
            bool automaticReloadEnabled = normalizedReloadTimer > 0f;

            if (automaticReloadEnabled)
                _lastEnabledReloadInterval = normalizedReloadTimer;

            AutomaticReloadToggle.SetValueWithoutNotify(automaticReloadEnabled);
            ReloadIntervalField.SetEnabled(automaticReloadEnabled);
            ReloadIntervalField.SetValueWithoutNotify(normalizedReloadTimer);
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

        private void OnAutomaticReloadToggleChanged(ChangeEvent<bool> evt)
        {
            float reloadTimer = evt.newValue
                ? NormalizeReloadTimer(_lastEnabledReloadInterval)
                : 0f;

            ReloadIntervalField.SetEnabled(evt.newValue);
            ReloadIntervalField.SetValueWithoutNotify(reloadTimer);
            OnReloadTimerChanged?.Invoke(reloadTimer);
        }

        private void OnReloadIntervalChanged(ChangeEvent<float> evt)
        {
            float normalizedReloadTimer = NormalizeReloadTimer(evt.newValue);
            bool automaticReloadEnabled = normalizedReloadTimer > 0f;

            if (automaticReloadEnabled)
                _lastEnabledReloadInterval = normalizedReloadTimer;

            AutomaticReloadToggle.SetValueWithoutNotify(automaticReloadEnabled);
            ReloadIntervalField.SetEnabled(automaticReloadEnabled);

            if (!Mathf.Approximately(ReloadIntervalField.value, normalizedReloadTimer))
                ReloadIntervalField.SetValueWithoutNotify(normalizedReloadTimer);

            OnReloadTimerChanged?.Invoke(normalizedReloadTimer);
        }

        private static float NormalizeReloadTimer(float reloadTimer)
        {
            if (reloadTimer <= 0f)
                return 0f;

            return Mathf.Max(MinimumReloadInterval, reloadTimer);
        }
        
        
    }
}