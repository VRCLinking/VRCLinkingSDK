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
    /// MemberSearchResponse
    /// </summary>
    [DataContract(Name = "MemberSearchResponse")]
    public partial class MemberSearchResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSearchResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected MemberSearchResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSearchResponse" /> class.
        /// </summary>
        /// <param name="count">count (required).</param>
        /// <param name="page">page (required).</param>
        /// <param name="totalCount">totalCount (required).</param>
        /// <param name="totalPages">totalPages (required).</param>
        /// <param name="results">results (required).</param>
        public MemberSearchResponse(int count = default(int), int page = default(int), int totalCount = default(int), int totalPages = default(int), List<SearchMember> results = default(List<SearchMember>))
        {
            this.Count = count;
            this.Page = page;
            this.TotalCount = totalCount;
            this.TotalPages = totalPages;
            // to ensure "results" is required (not null)
            if (results == null)
            {
                throw new ArgumentNullException("results is a required property for MemberSearchResponse and cannot be null");
            }
            this.Results = results;
        }

        /// <summary>
        /// Gets or Sets Count
        /// </summary>
        [DataMember(Name = "count", IsRequired = true, EmitDefaultValue = true)]
        public int Count { get; set; }

        /// <summary>
        /// Gets or Sets Page
        /// </summary>
        [DataMember(Name = "page", IsRequired = true, EmitDefaultValue = true)]
        public int Page { get; set; }

        /// <summary>
        /// Gets or Sets TotalCount
        /// </summary>
        [DataMember(Name = "totalCount", IsRequired = true, EmitDefaultValue = true)]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or Sets TotalPages
        /// </summary>
        [DataMember(Name = "totalPages", IsRequired = true, EmitDefaultValue = true)]
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or Sets Results
        /// </summary>
        [DataMember(Name = "results", IsRequired = true, EmitDefaultValue = true)]
        public List<SearchMember> Results { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class MemberSearchResponse {\n");
            sb.Append("  Count: ").Append(Count).Append("\n");
            sb.Append("  Page: ").Append(Page).Append("\n");
            sb.Append("  TotalCount: ").Append(TotalCount).Append("\n");
            sb.Append("  TotalPages: ").Append(TotalPages).Append("\n");
            sb.Append("  Results: ").Append(Results).Append("\n");
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
