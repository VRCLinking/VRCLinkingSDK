using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    public class ServerWorldSelection : VisualElement
    {
        // Makes the LoginField show up as an option within UI Builder.
        public new class UxmlFactory : UxmlFactory<ServerWorldSelection, UxmlTraits>
        {
        }

        public event Action OnValuesChanged;

        private DropdownField _serversDropdown;
        private DropdownField _worldsDropdown;

        private VrcLinkingApiHelper _apiHelper;

        private List<ServerWorldData> _worldData;

        private List<string> _serverNames = new List<string>();
        private List<string> _serverIds = new List<string>();
        private List<string> _worldNames = new List<string>();
        private List<Guid> _worldIds = new List<Guid>();


        public ServerWorldData GetWorldData()
        {
            return new ServerWorldData
            {
                ServerName = ServerName,
                ServerId = ServerId,
                WorldName = WorldName,
                WorldId = WorldId
            };
        }


        private string ServerName => _serverName;

        /// <summary>
        /// The Server ID correlating to the current ServerName.
        /// </summary>
        private string ServerId
        {
            get
            {
                int index = _serverNames.IndexOf(_serverName);
                if (index == -1)
                    return string.Empty;
                return _serverIds[index];
            }
        }

        private string WorldName => _worldName;

        /// <summary>
        /// The World ID correlating to the current WorldName.
        /// </summary>
        private Guid WorldId
        {
            get
            {
                int index = _worldNames.IndexOf(_worldName);
                if (index == -1)
                    return Guid.Empty;
                return _worldIds[index];
            }
        }

        private string _serverName;
        private string _worldName;


        public ServerWorldSelection()
        {
            this.AddToClassList("unity-base-field__inspector-field");

            _serversDropdown = new DropdownField();
            _worldsDropdown = new DropdownField();

            _serversDropdown.style.height = 18;
            _worldsDropdown.style.height = 18;

            _serversDropdown.choices = _serverNames;
            _worldsDropdown.choices = _worldNames;

            _serversDropdown.label = "Servers";
            _worldsDropdown.label = "Worlds";

            _serversDropdown.RegisterValueChangedCallback(ServerChanged);
            _worldsDropdown.RegisterValueChangedCallback(WorldChanged);

            this.Add(_serversDropdown);
            this.Add(_worldsDropdown);
        }

        private void ServerChanged(ChangeEvent<string> evt)
        {
            _serverName = evt.newValue;

            _worldsDropdown.SetEnabled(false);
            _worldsDropdown.value = null;

            if (!string.IsNullOrEmpty(evt.newValue))
                _ = PopulateWorlds();
        }

        private void WorldChanged(ChangeEvent<string> evt)
        {
            _worldName = evt.newValue;
            OnValuesChanged?.Invoke();
        }

        /// <summary>
        /// Copy the currently used API Helper.
        /// </summary>
        public void BindApi(VrcLinkingApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        // Write the initial values to the dropdowns.
        public void SetSelectedValues(
            string serverName,
            string worldName)
        {
            _serverName = serverName;
            _worldName = worldName;

            _serversDropdown.value = _serverName;
            _worldsDropdown.value = _worldName;
        }

        /// <summary>
        /// Fill out the list of servers.
        /// If the current one is found, populate the list of worlds as well.
        /// </summary>
        public async Task PopulateServers()
        {
            List<LimitedGuild> guilds = await _apiHelper.GetAvailableGuilds();
            _serverNames.Clear();
            _serverIds.Clear();
            foreach (LimitedGuild guild in guilds)
            {
                _serverNames.Add(guild.Name);
                _serverIds.Add(guild.Id);
            }

            if (_serverNames.Contains(_serversDropdown.value))
                await PopulateWorlds();
        }

        private async Task PopulateWorlds()
        {
            _worldNames.Clear();
            _worldIds.Clear();

            if (string.IsNullOrEmpty(ServerId))
            {
                Debug.Log("Id is invalid.");
                return;
            }

            List<WorldSettingsDto> worlds = await _apiHelper.GetWorldSettingsList(ServerId);

            foreach (WorldSettingsDto world in worlds)
            {
                _worldNames.Add(world.Name);
                _worldIds.Add(world.Id);
            }

            if (_worldNames.Count > 0)
            {
                _worldsDropdown.SetEnabled(true);
                if (string.IsNullOrEmpty(_worldsDropdown.value))
                    _worldsDropdown.SetValueWithoutNotify("Select a world...");
            }
            else
            {
                _worldsDropdown.SetEnabled(false);
                _worldsDropdown.SetValueWithoutNotify("- No worlds available. -");
            }
        }
    }
}