using System;
using System.Text;
using VRC.SDK3.Data;

namespace VRCLinking
{
    public partial class VrcLinkingDownloader
    {
        public bool TryGetGuildMembersByRoleName(string roleName, out DataList members)
        {
            members = new DataList();
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GuildUsers") || parsedData["GuildUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var guildUsers = parsedData["GuildUsers"].DataDictionary;
            if (!guildUsers.ContainsKey(roleName) || guildUsers[roleName].TokenType != TokenType.DataList)
            {
                return false;
            }
            
            members = guildUsers[roleName].DataList;
            
            return true;
        } 
        
        public bool TryGetGuildMembersByRoleName(string roleName, out string[] members)
        {
            members = new string[0];
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GuildUsers") || parsedData["GuildUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var guildUsers = parsedData["GuildUsers"].DataDictionary;
            if (!guildUsers.ContainsKey(roleName) || guildUsers[roleName].TokenType != TokenType.DataList)
            {
                return false;
            }
            
            members = new string[guildUsers[roleName].DataList.Count];
            for (var i = 0; i < guildUsers[roleName].DataList.Count; i++)
            {
                members[i] = guildUsers[roleName].DataList[i].String;
            }
            
            return true;
        }
        
        public bool TryGetGuildMembersByRoleId(string roleId, out DataList members)
        {
            members = new DataList();
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GuildRoleMap") || parsedData["GuildRoleMap"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }
            
            var guildRoleMap = parsedData["GuildRoleMap"].DataDictionary;
            
            if (!guildRoleMap.ContainsKey(roleId) || !parsedData.ContainsKey("GuildUsers") || parsedData["GuildUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }
            
            var roleName = guildRoleMap[roleId].String;
            
            var tryGetGuildMembersByRoleName = TryGetGuildMembersByRoleName(roleName, out members);
            return tryGetGuildMembersByRoleName;
        }
        
        public bool TryGetGuildMembersByRoleId(string roleId, out string[] members)
        {
            members = new string[0];
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GuildRoleMap") || parsedData["GuildRoleMap"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }
            
            var guildRoleMap = parsedData["GuildRoleMap"].DataDictionary;
            
            if (!guildRoleMap.ContainsKey(roleId) || !parsedData.ContainsKey("GuildUsers") || parsedData["GuildUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }
            
            var roleName = guildRoleMap[roleId].String;
            
            var tryGetGuildMembersByRoleName = TryGetGuildMembersByRoleName(roleName, out members);
            return tryGetGuildMembersByRoleName;
        }
        
        public bool TryGetFormattedGuildMembersByRoleName(string roleName, out string formattedMembers, string separator = ", ")
        {
            formattedMembers = string.Empty;
            if (!TryGetGuildMembersByRoleName(roleName, out DataList members))
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < members.Count; i++)
            {
                sb.Append(members[i].String);
                if (i < members.Count - 1)
                {
                    sb.Append(separator);
                }
            }
            
            formattedMembers = sb.ToString();
            return true;
        }
        
        public bool TryGetFormattedGuildMembersByRoleId(string roleId, out string formattedMembers, string separator = ", ")
        {
            formattedMembers = string.Empty;
            if (!TryGetGuildMembersByRoleId(roleId, out DataList members))
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < members.Count; i++)
            {
                sb.Append(members[i].String);
                if (i < members.Count - 1)
                {
                    sb.Append(separator);
                }
            }
            
            formattedMembers = sb.ToString();
            return true;
        }

        public bool TryGetAtlasDetail(out DataDictionary atlasMetadata)
        {
            atlasMetadata = null;
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("AtlasDetail") || parsedData["AtlasDetail"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            atlasMetadata = parsedData["AtlasDetail"].DataDictionary;

            return true;
        }
    }
}