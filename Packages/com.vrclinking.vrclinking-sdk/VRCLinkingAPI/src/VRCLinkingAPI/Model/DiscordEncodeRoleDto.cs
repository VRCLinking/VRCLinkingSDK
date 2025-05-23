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
    /// DiscordEncodeRoleDto
    /// </summary>
    [DataContract(Name = "DiscordEncodeRoleDto")]
    public partial class DiscordEncodeRoleDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordEncodeRoleDto" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected DiscordEncodeRoleDto() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordEncodeRoleDto" /> class.
        /// </summary>
        /// <param name="roleId">roleId (required).</param>
        /// <param name="roleName">roleName (required).</param>
        /// <param name="position">position (required).</param>
        /// <param name="color">color (required).</param>
        public DiscordEncodeRoleDto(string roleId = default(string), string roleName = default(string), int position = default(int), string color = default(string))
        {
            // to ensure "roleId" is required (not null)
            if (roleId == null)
            {
                throw new ArgumentNullException("roleId is a required property for DiscordEncodeRoleDto and cannot be null");
            }
            this.RoleId = roleId;
            // to ensure "roleName" is required (not null)
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName is a required property for DiscordEncodeRoleDto and cannot be null");
            }
            this.RoleName = roleName;
            this.Position = position;
            // to ensure "color" is required (not null)
            if (color == null)
            {
                throw new ArgumentNullException("color is a required property for DiscordEncodeRoleDto and cannot be null");
            }
            this.Color = color;
        }

        /// <summary>
        /// Gets or Sets RoleId
        /// </summary>
        [DataMember(Name = "roleId", IsRequired = true, EmitDefaultValue = true)]
        public string RoleId { get; set; }

        /// <summary>
        /// Gets or Sets RoleName
        /// </summary>
        [DataMember(Name = "roleName", IsRequired = true, EmitDefaultValue = true)]
        public string RoleName { get; set; }

        /// <summary>
        /// Gets or Sets Position
        /// </summary>
        [DataMember(Name = "position", IsRequired = true, EmitDefaultValue = true)]
        public int Position { get; set; }

        /// <summary>
        /// Gets or Sets Color
        /// </summary>
        [DataMember(Name = "color", IsRequired = true, EmitDefaultValue = true)]
        public string Color { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class DiscordEncodeRoleDto {\n");
            sb.Append("  RoleId: ").Append(RoleId).Append("\n");
            sb.Append("  RoleName: ").Append(RoleName).Append("\n");
            sb.Append("  Position: ").Append(Position).Append("\n");
            sb.Append("  Color: ").Append(Color).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

    }

}
