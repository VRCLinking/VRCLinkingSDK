using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Data;
using VRCLinking.Utilitites;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    public class SupporterUtilities
    {
        public static List<SupporterRole> ConvertToSupporterRoles(DataList roles)
        {
            var supporterRoles = new List<SupporterRole>();
            Debug.Log($"Converting roles to supporter roles: {roles.Count}");
            for (var i = 0; i < roles.Count; i++)
            {
                var role = roles[i];
                if (role.TokenType != TokenType.DataDictionary)
                {
                    continue;
                }

                var roleDictionary = role.DataDictionary;

                if (roleDictionary == null)
                {
                    Debug.LogError("Role dictionary is null");
                    continue;
                }
                
                var supporterRole = new SupporterRole();
                supporterRole.roleType = roleDictionary.ContainsKey("roleType") ? (RoleType) roleDictionary["roleType"].Int : RoleType.RoleId;
                supporterRole.roleValue = roleDictionary.ContainsKey("roleValue") ? roleDictionary["roleValue"].String : "";
                supporterRole.roleTitle = roleDictionary.ContainsKey("roleTitle") ? roleDictionary["roleTitle"].String : "";
                supporterRole.roleTitleRelativeSize = roleDictionary.ContainsKey("roleTitleRelativeSize") ? roleDictionary["roleTitleRelativeSize"].Float : 100f;
                supporterRole.roleColor = roleDictionary.ContainsKey("roleColor") ? (Color) roleDictionary["roleColor"].Reference : Color.white;
                supporterRole.roleSeparator = roleDictionary.ContainsKey("roleSeparator") ? roleDictionary["roleSeparator"].String : ", ";
                supporterRole.roleRelativeSize = roleDictionary.ContainsKey("roleRelativeSize") ? roleDictionary["roleRelativeSize"].Float : 100f;

                supporterRoles.Add(supporterRole);
            }

            return supporterRoles;
        }
        
        public static DataList ConvertToDataList(List<SupporterRole> roles)
        {
            Debug.Log($"Converting supporter roles to data list: {roles.Count}");
            var dataList = new DataList();
            foreach (var role in roles)
            {
                var dataDictionary = new DataDictionary();
                dataDictionary["roleType"] = new DataToken((int)role.roleType);
                dataDictionary["roleValue"] = new DataToken(role.roleValue);
                dataDictionary["roleTitle"] = new DataToken(role.roleTitle);
                dataDictionary["roleTitleRelativeSize"] = new DataToken(role.roleTitleRelativeSize);
                dataDictionary["roleColor"] = new DataToken(role.roleColor);
                dataDictionary["roleSeparator"] = new DataToken(role.roleSeparator);
                dataDictionary["roleRelativeSize"] = new DataToken(role.roleRelativeSize);
                dataList.Add(dataDictionary);
            }

            return dataList;
        }
    }
}