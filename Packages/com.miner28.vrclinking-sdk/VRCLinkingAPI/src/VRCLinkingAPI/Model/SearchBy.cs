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
    /// Defines SearchBy
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchBy
    {
        /// <summary>
        /// Enum DiscordId for value: DiscordId
        /// </summary>
        [EnumMember(Value = "DiscordId")]
        DiscordId = 1,

        /// <summary>
        /// Enum DiscordName for value: DiscordName
        /// </summary>
        [EnumMember(Value = "DiscordName")]
        DiscordName = 2,

        /// <summary>
        /// Enum VrcId for value: VrcId
        /// </summary>
        [EnumMember(Value = "VrcId")]
        VrcId = 3,

        /// <summary>
        /// Enum VrcName for value: VrcName
        /// </summary>
        [EnumMember(Value = "VrcName")]
        VrcName = 4
    }

}
