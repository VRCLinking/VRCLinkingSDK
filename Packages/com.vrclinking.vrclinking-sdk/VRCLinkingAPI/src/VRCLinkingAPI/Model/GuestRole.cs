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
    /// GuestRole
    /// </summary>
    [DataContract(Name = "GuestRole")]
    public partial class GuestRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuestRole" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected GuestRole() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GuestRole" /> class.
        /// </summary>
        /// <param name="id">id (required).</param>
        /// <param name="primaryRoleId">primaryRoleId.</param>
        /// <param name="primaryRoleName">primaryRoleName.</param>
        /// <param name="guestRoleId">guestRoleId.</param>
        /// <param name="guestRoleName">guestRoleName.</param>
        /// <param name="count">count (required).</param>
        public GuestRole(string id = default(string), string primaryRoleId = default(string), string primaryRoleName = default(string), string guestRoleId = default(string), string guestRoleName = default(string), int count = default(int))
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new ArgumentNullException("id is a required property for GuestRole and cannot be null");
            }
            this.Id = id;
            this.Count = count;
            this.PrimaryRoleId = primaryRoleId;
            this.PrimaryRoleName = primaryRoleName;
            this.GuestRoleId = guestRoleId;
            this.GuestRoleName = guestRoleName;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets PrimaryRoleId
        /// </summary>
        [DataMember(Name = "primaryRoleId", EmitDefaultValue = true)]
        public string PrimaryRoleId { get; set; }

        /// <summary>
        /// Gets or Sets PrimaryRoleName
        /// </summary>
        [DataMember(Name = "primaryRoleName", EmitDefaultValue = true)]
        public string PrimaryRoleName { get; set; }

        /// <summary>
        /// Gets or Sets GuestRoleId
        /// </summary>
        [DataMember(Name = "guestRoleId", EmitDefaultValue = true)]
        public string GuestRoleId { get; set; }

        /// <summary>
        /// Gets or Sets GuestRoleName
        /// </summary>
        [DataMember(Name = "guestRoleName", EmitDefaultValue = true)]
        public string GuestRoleName { get; set; }

        /// <summary>
        /// Gets or Sets Count
        /// </summary>
        [DataMember(Name = "count", IsRequired = true, EmitDefaultValue = true)]
        public int Count { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class GuestRole {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  PrimaryRoleId: ").Append(PrimaryRoleId).Append("\n");
            sb.Append("  PrimaryRoleName: ").Append(PrimaryRoleName).Append("\n");
            sb.Append("  GuestRoleId: ").Append(GuestRoleId).Append("\n");
            sb.Append("  GuestRoleName: ").Append(GuestRoleName).Append("\n");
            sb.Append("  Count: ").Append(Count).Append("\n");
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
