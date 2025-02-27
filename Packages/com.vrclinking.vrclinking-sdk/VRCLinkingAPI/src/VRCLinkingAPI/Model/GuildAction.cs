/*
 * VRCLinking
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpenAPIDateConverter = VRCLinkingAPI.Client.OpenAPIDateConverter;

namespace VRCLinkingAPI.Model
{
    /// <summary>
    /// Defines GuildAction
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GuildAction
    {
        /// <summary>
        /// Enum None for value: None
        /// </summary>
        [EnumMember(Value = "None")]
        None = 1,

        /// <summary>
        /// Enum GrantRolePermanent for value: GrantRolePermanent
        /// </summary>
        [EnumMember(Value = "GrantRolePermanent")]
        GrantRolePermanent = 2,

        /// <summary>
        /// Enum GrantRoleTemporary for value: GrantRoleTemporary
        /// </summary>
        [EnumMember(Value = "GrantRoleTemporary")]
        GrantRoleTemporary = 3
    }

}
