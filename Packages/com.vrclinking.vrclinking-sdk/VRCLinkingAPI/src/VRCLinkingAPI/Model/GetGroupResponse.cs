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
    /// GetGroupResponse
    /// </summary>
    [DataContract(Name = "GetGroupResponse")]
    public partial class GetGroupResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetGroupResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected GetGroupResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="GetGroupResponse" /> class.
        /// </summary>
        /// <param name="id">id (required).</param>
        /// <param name="name">name.</param>
        /// <param name="shortCode">shortCode.</param>
        /// <param name="discriminator">discriminator.</param>
        /// <param name="iconUrl">iconUrl.</param>
        /// <param name="bannerUrl">bannerUrl.</param>
        /// <param name="ownerId">ownerId.</param>
        /// <param name="description">description.</param>
        /// <param name="memberCount">memberCount.</param>
        /// <param name="onlineMemberCount">onlineMemberCount.</param>
        /// <param name="fullySynced">fullySynced.</param>
        /// <param name="enabled">enabled (required).</param>
        /// <param name="autoKick">autoKick (required).</param>
        public GetGroupResponse(string id = default(string), string name = default(string), string shortCode = default(string), string discriminator = default(string), string iconUrl = default(string), string bannerUrl = default(string), string ownerId = default(string), string description = default(string), int? memberCount = default(int?), int? onlineMemberCount = default(int?), bool? fullySynced = default(bool?), bool enabled = default(bool), bool autoKick = default(bool))
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new ArgumentNullException("id is a required property for GetGroupResponse and cannot be null");
            }
            this.Id = id;
            this.Enabled = enabled;
            this.AutoKick = autoKick;
            this.Name = name;
            this.ShortCode = shortCode;
            this.Discriminator = discriminator;
            this.IconUrl = iconUrl;
            this.BannerUrl = bannerUrl;
            this.OwnerId = ownerId;
            this.Description = description;
            this.MemberCount = memberCount;
            this.OnlineMemberCount = onlineMemberCount;
            this.FullySynced = fullySynced;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets ShortCode
        /// </summary>
        [DataMember(Name = "shortCode", EmitDefaultValue = true)]
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or Sets Discriminator
        /// </summary>
        [DataMember(Name = "discriminator", EmitDefaultValue = true)]
        public string Discriminator { get; set; }

        /// <summary>
        /// Gets or Sets IconUrl
        /// </summary>
        [DataMember(Name = "iconUrl", EmitDefaultValue = true)]
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or Sets BannerUrl
        /// </summary>
        [DataMember(Name = "bannerUrl", EmitDefaultValue = true)]
        public string BannerUrl { get; set; }

        /// <summary>
        /// Gets or Sets OwnerId
        /// </summary>
        [DataMember(Name = "ownerId", EmitDefaultValue = true)]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = true)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets MemberCount
        /// </summary>
        [DataMember(Name = "memberCount", EmitDefaultValue = true)]
        public int? MemberCount { get; set; }

        /// <summary>
        /// Gets or Sets OnlineMemberCount
        /// </summary>
        [DataMember(Name = "onlineMemberCount", EmitDefaultValue = true)]
        public int? OnlineMemberCount { get; set; }

        /// <summary>
        /// Gets or Sets FullySynced
        /// </summary>
        [DataMember(Name = "fullySynced", EmitDefaultValue = true)]
        public bool? FullySynced { get; set; }

        /// <summary>
        /// Gets or Sets Enabled
        /// </summary>
        [DataMember(Name = "enabled", IsRequired = true, EmitDefaultValue = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or Sets AutoKick
        /// </summary>
        [DataMember(Name = "autoKick", IsRequired = true, EmitDefaultValue = true)]
        public bool AutoKick { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class GetGroupResponse {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  ShortCode: ").Append(ShortCode).Append("\n");
            sb.Append("  Discriminator: ").Append(Discriminator).Append("\n");
            sb.Append("  IconUrl: ").Append(IconUrl).Append("\n");
            sb.Append("  BannerUrl: ").Append(BannerUrl).Append("\n");
            sb.Append("  OwnerId: ").Append(OwnerId).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  MemberCount: ").Append(MemberCount).Append("\n");
            sb.Append("  OnlineMemberCount: ").Append(OnlineMemberCount).Append("\n");
            sb.Append("  FullySynced: ").Append(FullySynced).Append("\n");
            sb.Append("  Enabled: ").Append(Enabled).Append("\n");
            sb.Append("  AutoKick: ").Append(AutoKick).Append("\n");
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
