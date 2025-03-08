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
    /// ScheduledTaskInfo
    /// </summary>
    [DataContract(Name = "ScheduledTaskInfo")]
    public partial class ScheduledTaskInfo
    {

        /// <summary>
        /// Gets or Sets Api
        /// </summary>
        [DataMember(Name = "api", IsRequired = true, EmitDefaultValue = true)]
        public ApiName Api { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTaskInfo" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ScheduledTaskInfo() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTaskInfo" /> class.
        /// </summary>
        /// <param name="api">api (required).</param>
        /// <param name="container">container (required).</param>
        /// <param name="method">method (required).</param>
        /// <param name="nextRun">nextRun (required).</param>
        /// <param name="lastRun">lastRun.</param>
        /// <param name="timer">timer (required).</param>
        /// <param name="enabled">enabled (required).</param>
        public ScheduledTaskInfo(ApiName api = default(ApiName), string container = default(string), string method = default(string), DateTime nextRun = default(DateTime), DateTime? lastRun = default(DateTime?), float timer = default(float), bool enabled = default(bool))
        {
            this.Api = api;
            // to ensure "container" is required (not null)
            if (container == null)
            {
                throw new ArgumentNullException("container is a required property for ScheduledTaskInfo and cannot be null");
            }
            this.Container = container;
            // to ensure "method" is required (not null)
            if (method == null)
            {
                throw new ArgumentNullException("method is a required property for ScheduledTaskInfo and cannot be null");
            }
            this.Method = method;
            this.NextRun = nextRun;
            this.Timer = timer;
            this.Enabled = enabled;
            this.LastRun = lastRun;
        }

        /// <summary>
        /// Gets or Sets Container
        /// </summary>
        [DataMember(Name = "container", IsRequired = true, EmitDefaultValue = true)]
        public string Container { get; set; }

        /// <summary>
        /// Gets or Sets Method
        /// </summary>
        [DataMember(Name = "method", IsRequired = true, EmitDefaultValue = true)]
        public string Method { get; set; }

        /// <summary>
        /// Gets or Sets NextRun
        /// </summary>
        [DataMember(Name = "nextRun", IsRequired = true, EmitDefaultValue = true)]
        public DateTime NextRun { get; set; }

        /// <summary>
        /// Gets or Sets LastRun
        /// </summary>
        [DataMember(Name = "lastRun", EmitDefaultValue = true)]
        public DateTime? LastRun { get; set; }

        /// <summary>
        /// Gets or Sets Timer
        /// </summary>
        [DataMember(Name = "timer", IsRequired = true, EmitDefaultValue = true)]
        public float Timer { get; set; }

        /// <summary>
        /// Gets or Sets Enabled
        /// </summary>
        [DataMember(Name = "enabled", IsRequired = true, EmitDefaultValue = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ScheduledTaskInfo {\n");
            sb.Append("  Api: ").Append(Api).Append("\n");
            sb.Append("  Container: ").Append(Container).Append("\n");
            sb.Append("  Method: ").Append(Method).Append("\n");
            sb.Append("  NextRun: ").Append(NextRun).Append("\n");
            sb.Append("  LastRun: ").Append(LastRun).Append("\n");
            sb.Append("  Timer: ").Append(Timer).Append("\n");
            sb.Append("  Enabled: ").Append(Enabled).Append("\n");
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
