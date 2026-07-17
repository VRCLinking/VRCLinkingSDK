using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRCLinking.Modules;

namespace VRCLinking
{
    public partial class VrcLinkingDownloader
    {
        [NonSerialized] public DataDictionary parsedData;
        bool _isDataValid;
        string _checksum;
        
        readonly string[] _requiredKeys = new string[]
        {
            "Version",
            "LastUpdated",
            "MemberCount"
        }; 

        public void ParseData(string input)
        {
            _checksum = ParseChecksum(input);
            
            VRCJson.TryDeserializeFromJson(input, out var dataOut);

            if (dataOut.Error != DataError.None)
            {
                _isDataValid = false;
                return;
            }

            if (dataOut.TokenType != TokenType.DataDictionary)
            {
                _isDataValid = false;
                return;
            }

            parsedData = dataOut.DataDictionary;
            foreach (var key in _requiredKeys)
            {
                if (!parsedData.ContainsKey(key))
                {
                    _isDataValid = false;
                    LogError($"Missing required key in JSON: {key}");
                    return;
                }
            }

            if (parsedData.ContainsKey("AgeVerifiedUsers"))
            {
                if (parsedData["AgeVerifiedUsers"].TokenType != TokenType.DataList)
                {
                    _isDataValid = false;
                    LogError("Invalid AgeVerifiedUsers data: expected a list.");
                    return;
                }

                var ageVerifiedUsers = parsedData["AgeVerifiedUsers"].DataList;
                for (var i = 0; i < ageVerifiedUsers.Count; i++)
                {
                    if (ageVerifiedUsers[i].TokenType == TokenType.String) continue;

                    _isDataValid = false;
                    LogError("Invalid AgeVerifiedUsers data: every entry must be a string.");
                    return;
                }
            }
            
            _isDataValid = true;
            
            SendEventToModules(nameof(VrcLinkingModuleBase.OnDataLoaded));
        }
        
        string ParseChecksum(string input)
        {
            return string.Empty;
        }
    }
}
