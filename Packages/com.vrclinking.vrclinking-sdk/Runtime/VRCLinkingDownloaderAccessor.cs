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

            formattedMembers = FormatMembers(members, separator);
            return true;
        }
        
        public bool TryGetFormattedGuildMembersByRoleId(string roleId, out string formattedMembers, string separator = ", ")
        {
            formattedMembers = string.Empty;
            if (!TryGetGuildMembersByRoleId(roleId, out DataList members))
            {
                return false;
            }

            formattedMembers = FormatMembers(members, separator);
            return true;
        }

        public bool TryGetGroupMembersByRoleName(string roleName, out DataList members)
        {
            members = new DataList();
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GroupUsers") || parsedData["GroupUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var groupUsers = parsedData["GroupUsers"].DataDictionary;
            if (!groupUsers.ContainsKey(roleName) || groupUsers[roleName].TokenType != TokenType.DataList)
            {
                return false;
            }

            members = groupUsers[roleName].DataList;

            return true;
        }

        public bool TryGetGroupMembersByRoleName(string roleName, out string[] members)
        {
            members = new string[0];
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GroupUsers") || parsedData["GroupUsers"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var groupUsers = parsedData["GroupUsers"].DataDictionary;
            if (!groupUsers.ContainsKey(roleName) || groupUsers[roleName].TokenType != TokenType.DataList)
            {
                return false;
            }

            var memberList = groupUsers[roleName].DataList;
            members = new string[memberList.Count];
            for (var i = 0; i < memberList.Count; i++)
            {
                members[i] = memberList[i].String;
            }

            return true;
        }

        public bool TryGetGroupMembersByRoleId(string roleId, out DataList members)
        {
            members = new DataList();
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GroupRoleMap") || parsedData["GroupRoleMap"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var groupRoleMap = parsedData["GroupRoleMap"].DataDictionary;

            if (!groupRoleMap.ContainsKey(roleId))
            {
                return false;
            }

            var roleName = groupRoleMap[roleId].String;

            return TryGetGroupMembersByRoleName(roleName, out members);
        }

        public bool TryGetGroupMembersByRoleId(string roleId, out string[] members)
        {
            members = new string[0];
            if (!_isDataValid)
            {
                return false;
            }

            if (!parsedData.ContainsKey("GroupRoleMap") || parsedData["GroupRoleMap"].TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            var groupRoleMap = parsedData["GroupRoleMap"].DataDictionary;

            if (!groupRoleMap.ContainsKey(roleId))
            {
                return false;
            }

            var roleName = groupRoleMap[roleId].String;

            return TryGetGroupMembersByRoleName(roleName, out members);
        }

        public bool TryGetFormattedGroupMembersByRoleName(string roleName, out string formattedMembers, string separator = ", ")
        {
            formattedMembers = string.Empty;
            if (!TryGetGroupMembersByRoleName(roleName, out DataList members))
            {
                return false;
            }

            formattedMembers = FormatMembers(members, separator);
            return true;
        }

        public bool TryGetFormattedGroupMembersByRoleId(string roleId, out string formattedMembers, string separator = ", ")
        {
            formattedMembers = string.Empty;
            if (!TryGetGroupMembersByRoleId(roleId, out DataList members))
            {
                return false;
            }

            formattedMembers = FormatMembers(members, separator);
            return true;
        }

        public bool TryGetAgeVerifiedUsers(out DataList users)
        {
            users = new DataList();
            if (!_isDataValid || !parsedData.ContainsKey("AgeVerifiedUsers") ||
                parsedData["AgeVerifiedUsers"].TokenType != TokenType.DataList)
            {
                return false;
            }

            users = parsedData["AgeVerifiedUsers"].DataList;
            return true;
        }

        public bool TryGetAgeVerifiedUsers(out string[] users)
        {
            users = new string[0];
            if (!TryGetAgeVerifiedUsers(out DataList userList))
            {
                return false;
            }

            users = new string[userList.Count];
            for (var i = 0; i < userList.Count; i++)
            {
                users[i] = userList[i].String;
            }

            return true;
        }

        public bool TryGetFormattedAgeVerifiedUsers(out string formattedUsers, string separator = ", ")
        {
            formattedUsers = string.Empty;
            if (!TryGetAgeVerifiedUsers(out DataList users))
            {
                return false;
            }

            formattedUsers = FormatMembers(users, separator);
            return true;
        }

        private string FormatMembers(DataList members, string separator)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < members.Count; i++)
            {
                sb.Append(members[i].String);
                if (i < members.Count - 1)
                {
                    sb.Append(separator);
                }
            }

            return sb.ToString();
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
