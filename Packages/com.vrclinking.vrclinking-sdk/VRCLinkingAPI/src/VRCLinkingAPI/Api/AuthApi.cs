/*
 * VRCLinking
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using VRCLinkingAPI.Client;
using VRCLinkingAPI.Model;

namespace VRCLinkingAPI.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IAuthApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <returns>LoginCallbackResponse</returns>
        LoginCallbackResponse DiscordAuth(string token = default(string));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <returns>ApiResponse of LoginCallbackResponse</returns>
        ApiResponse<LoginCallbackResponse> DiscordAuthWithHttpInfo(string token = default(string));
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>LoginResponse</returns>
        LoginResponse Login();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of LoginResponse</returns>
        ApiResponse<LoginResponse> LoginWithHttpInfo();
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <returns>LoginCallbackResponse</returns>
        LoginCallbackResponse LoginCallback(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <returns>ApiResponse of LoginCallbackResponse</returns>
        ApiResponse<LoginCallbackResponse> LoginCallbackWithHttpInfo(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest));
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        Object Logout();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Object</returns>
        ApiResponse<Object> LogoutWithHttpInfo();
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IAuthApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginCallbackResponse</returns>
        System.Threading.Tasks.Task<LoginCallbackResponse> DiscordAuthAsync(string token = default(string), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginCallbackResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LoginCallbackResponse>> DiscordAuthWithHttpInfoAsync(string token = default(string), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginResponse</returns>
        System.Threading.Tasks.Task<LoginResponse> LoginAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LoginResponse>> LoginWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginCallbackResponse</returns>
        System.Threading.Tasks.Task<LoginCallbackResponse> LoginCallbackAsync(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginCallbackResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<LoginCallbackResponse>> LoginCallbackWithHttpInfoAsync(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Object</returns>
        System.Threading.Tasks.Task<Object> LogoutAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> LogoutWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IAuthApi : IAuthApiSync, IAuthApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class AuthApi : IDisposable, IAuthApi
    {
        private VRCLinkingAPI.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public AuthApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public AuthApi(string basePath)
        {
            this.Configuration = VRCLinkingAPI.Client.Configuration.MergeConfigurations(
                VRCLinkingAPI.Client.GlobalConfiguration.Instance,
                new VRCLinkingAPI.Client.Configuration { BasePath = basePath }
            );
            this.ApiClient = new VRCLinkingAPI.Client.ApiClient(this.Configuration.BasePath);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = VRCLinkingAPI.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public AuthApi(VRCLinkingAPI.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = VRCLinkingAPI.Client.Configuration.MergeConfigurations(
                VRCLinkingAPI.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new VRCLinkingAPI.Client.ApiClient(this.Configuration.BasePath);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = VRCLinkingAPI.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AuthApi(VRCLinkingAPI.Client.ISynchronousClient client, VRCLinkingAPI.Client.IAsynchronousClient asyncClient, VRCLinkingAPI.Client.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = VRCLinkingAPI.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Disposes resources if they were created by us
        /// </summary>
        public void Dispose()
        {
            this.ApiClient?.Dispose();
        }

        /// <summary>
        /// Holds the ApiClient if created
        /// </summary>
        public VRCLinkingAPI.Client.ApiClient ApiClient { get; set; } = null;

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public VRCLinkingAPI.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public VRCLinkingAPI.Client.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public VRCLinkingAPI.Client.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public VRCLinkingAPI.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <returns>LoginCallbackResponse</returns>
        public LoginCallbackResponse DiscordAuth(string token = default(string))
        {
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = DiscordAuthWithHttpInfo(token);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <returns>ApiResponse of LoginCallbackResponse</returns>
        public VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> DiscordAuthWithHttpInfo(string token = default(string))
        {
            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };

            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (token != null)
            {
                localVarRequestOptions.QueryParameters.Add(VRCLinkingAPI.Client.ClientUtils.ParameterToMultiMap("", "token", token));
            }

            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<LoginCallbackResponse>("/discord_auth", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DiscordAuth", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginCallbackResponse</returns>
        public async System.Threading.Tasks.Task<LoginCallbackResponse> DiscordAuthAsync(string token = default(string), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            var task = DiscordAuthWithHttpInfoAsync(token, cancellationToken);
#if UNITY_EDITOR || !UNITY_WEBGL
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = await task.ConfigureAwait(false);
#else
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = await task;
#endif
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="token"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginCallbackResponse)</returns>
        public async System.Threading.Tasks.Task<VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse>> DiscordAuthWithHttpInfoAsync(string token = default(string), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {

            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };


            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (token != null)
            {
                localVarRequestOptions.QueryParameters.Add(VRCLinkingAPI.Client.ClientUtils.ParameterToMultiMap("", "token", token));
            }

            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var task = this.AsynchronousClient.GetAsync<LoginCallbackResponse>("/discord_auth", localVarRequestOptions, this.Configuration, cancellationToken);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DiscordAuth", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>LoginResponse</returns>
        public LoginResponse Login()
        {
            VRCLinkingAPI.Client.ApiResponse<LoginResponse> localVarResponse = LoginWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of LoginResponse</returns>
        public VRCLinkingAPI.Client.ApiResponse<LoginResponse> LoginWithHttpInfo()
        {
            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };

            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<LoginResponse>("/login", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Login", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginResponse</returns>
        public async System.Threading.Tasks.Task<LoginResponse> LoginAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            var task = LoginWithHttpInfoAsync(cancellationToken);
#if UNITY_EDITOR || !UNITY_WEBGL
            VRCLinkingAPI.Client.ApiResponse<LoginResponse> localVarResponse = await task.ConfigureAwait(false);
#else
            VRCLinkingAPI.Client.ApiResponse<LoginResponse> localVarResponse = await task;
#endif
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginResponse)</returns>
        public async System.Threading.Tasks.Task<VRCLinkingAPI.Client.ApiResponse<LoginResponse>> LoginWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {

            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };


            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var task = this.AsynchronousClient.GetAsync<LoginResponse>("/login", localVarRequestOptions, this.Configuration, cancellationToken);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Login", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <returns>LoginCallbackResponse</returns>
        public LoginCallbackResponse LoginCallback(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest))
        {
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = LoginCallbackWithHttpInfo(loginCallbackRequest);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <returns>ApiResponse of LoginCallbackResponse</returns>
        public VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> LoginCallbackWithHttpInfo(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest))
        {
            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json-patch+json",
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };

            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = loginCallbackRequest;

            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<LoginCallbackResponse>("/login/callback", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LoginCallback", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of LoginCallbackResponse</returns>
        public async System.Threading.Tasks.Task<LoginCallbackResponse> LoginCallbackAsync(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            var task = LoginCallbackWithHttpInfoAsync(loginCallbackRequest, cancellationToken);
#if UNITY_EDITOR || !UNITY_WEBGL
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = await task.ConfigureAwait(false);
#else
            VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse> localVarResponse = await task;
#endif
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="loginCallbackRequest"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (LoginCallbackResponse)</returns>
        public async System.Threading.Tasks.Task<VRCLinkingAPI.Client.ApiResponse<LoginCallbackResponse>> LoginCallbackWithHttpInfoAsync(LoginCallbackRequest loginCallbackRequest = default(LoginCallbackRequest), System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {

            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json-patch+json", 
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };


            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = loginCallbackRequest;

            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var task = this.AsynchronousClient.PostAsync<LoginCallbackResponse>("/login/callback", localVarRequestOptions, this.Configuration, cancellationToken);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("LoginCallback", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Object</returns>
        public Object Logout()
        {
            VRCLinkingAPI.Client.ApiResponse<Object> localVarResponse = LogoutWithHttpInfo();
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of Object</returns>
        public VRCLinkingAPI.Client.ApiResponse<Object> LogoutWithHttpInfo()
        {
            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };

            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<Object>("/logout", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Logout", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Object</returns>
        public async System.Threading.Tasks.Task<Object> LogoutAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            var task = LogoutWithHttpInfoAsync(cancellationToken);
#if UNITY_EDITOR || !UNITY_WEBGL
            VRCLinkingAPI.Client.ApiResponse<Object> localVarResponse = await task.ConfigureAwait(false);
#else
            VRCLinkingAPI.Client.ApiResponse<Object> localVarResponse = await task;
#endif
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="VRCLinkingAPI.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        public async System.Threading.Tasks.Task<VRCLinkingAPI.Client.ApiResponse<Object>> LogoutWithHttpInfoAsync(System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {

            VRCLinkingAPI.Client.RequestOptions localVarRequestOptions = new VRCLinkingAPI.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json"
            };


            var localVarContentType = VRCLinkingAPI.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = VRCLinkingAPI.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (Bearer) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var task = this.AsynchronousClient.GetAsync<Object>("/logout", localVarRequestOptions, this.Configuration, cancellationToken);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Logout", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
