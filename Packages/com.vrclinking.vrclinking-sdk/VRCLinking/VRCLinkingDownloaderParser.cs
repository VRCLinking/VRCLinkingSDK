using VRC.SDK3.Data;
using VRCLinking.Modules;

namespace VRCLinking
{
    public partial class VrcLinkingDownloader
    {
        DataDictionary _parsedData;
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
            input = CleanData(input);
            
            
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

            _parsedData = dataOut.DataDictionary;
            foreach (var key in _requiredKeys)
            {
                if (!_parsedData.ContainsKey(key))
                {
                    _isDataValid = false;
                    LogError($"Missing required key in JSON: {key}");
                    return;
                }
            }
            
            _isDataValid = true;
            
            SendEventToModules(nameof(VrcLinkingModuleBase.OnDataLoaded));
        }
        
        string ParseChecksum(string input)
        {
            var firstBracket = input.IndexOf('{');
            var firstSquareBracket = input.IndexOf('[');
            var firstBracketIndex = firstBracket == -1 ? firstSquareBracket : firstSquareBracket == -1 ? firstBracket : firstBracket < firstSquareBracket ? firstBracket : firstSquareBracket;
            
            if (firstBracketIndex == -1)
            {
                return string.Empty;
            }
            
            if (firstBracketIndex > 32)
            {
                return string.Empty;
            }
            
            return input.Substring(0, firstBracketIndex);
        }
        
        string CleanData(string input)
        {
            var firstBracket = input.IndexOf('{');
            var firstSquareBracket = input.IndexOf('[');
            var firstBracketIndex = firstBracket == -1 ? firstSquareBracket : firstSquareBracket == -1 ? firstBracket : firstBracket < firstSquareBracket ? firstBracket : firstSquareBracket;
            
            if (firstBracketIndex == -1)
            {
                return input;
            }
            
            return input.Substring(firstBracketIndex);
        }
    }
}