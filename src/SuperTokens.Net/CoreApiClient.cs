using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SuperTokens.Net.Core;
using SuperTokens.Net.SessionRecipe;

namespace SuperTokens.Net
{
    public sealed class CoreApiClient : ICoreApiClient
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly HttpClient _httpClient;

        public CoreApiClient(HttpClient httpClient) =>
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public Task<CreateSessionResponse> CreateSessionAsync(string? apiKey, string? cdiVersion, CreateSessionRequest body) =>
            this.CreateSessionAsync(apiKey, cdiVersion, body, CancellationToken.None);

        public async Task<CreateSessionResponse> CreateSessionAsync(string? apiKey, string? cdiVersion, CreateSessionRequest body, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/session", UriKind.Relative), apiKey, cdiVersion, body);
            return await SendRequestAsync<CreateSessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<DeleteSessionResponse> DeleteSessionAsync(string? apiKey, string? cdiVersion, DeleteSessionRequest body) =>
            this.DeleteSessionAsync(apiKey, cdiVersion, body, CancellationToken.None);

        public async Task<DeleteSessionResponse> DeleteSessionAsync(string? apiKey, string? cdiVersion, DeleteSessionRequest body, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/session/remove", UriKind.Relative), apiKey, cdiVersion, body);
            return await SendRequestAsync<DeleteSessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<ApiVersionResponse> GetApiVersionAsync(string? apiKey) =>
            this.GetApiVersionAsync(apiKey, CancellationToken.None);

        public async Task<ApiVersionResponse> GetApiVersionAsync(string? apiKey, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, new Uri("/apiversion", UriKind.Relative), apiKey, null);
            return await SendRequestAsync<ApiVersionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion) =>
            this.GetHandshakeAsync(apiKey, cdiVersion, CancellationToken.None);

        public async Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/handshake", UriKind.Relative), apiKey, cdiVersion);
            request.Content = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
            return await SendRequestAsync<Handshake>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<SessionResponse> GetSessionAsync(string? apiKey, string? cdiVersion, string sessionHandle) =>
            this.GetSessionAsync(apiKey, cdiVersion, sessionHandle, CancellationToken.None);

        public async Task<SessionResponse> GetSessionAsync(string? apiKey, string? cdiVersion, string sessionHandle, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, new Uri($"/recipe/session?sessionHandle={sessionHandle}", UriKind.Relative), apiKey, cdiVersion);
            return await SendRequestAsync<SessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<SessionHandlesResponse> GetSessionHandlesAsync(string? apiKey, string? cdiVersion, string userId) =>
            this.GetSessionHandlesAsync(apiKey, cdiVersion, userId, CancellationToken.None);

        public async Task<SessionHandlesResponse> GetSessionHandlesAsync(string? apiKey, string? cdiVersion, string userId, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, new Uri($"/recipe/session/user?userId={userId}", UriKind.Relative), apiKey, cdiVersion);
            return await SendRequestAsync<SessionHandlesResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<RefreshSessionResponse> RefreshSessionAsync(string? apiKey, string? cdiVersion, RefreshSessionRequest body) =>
            this.RefreshSessionAsync(apiKey, cdiVersion, body, CancellationToken.None);

        public async Task<RefreshSessionResponse> RefreshSessionAsync(string? apiKey, string? cdiVersion, RefreshSessionRequest body, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/session/refresh", UriKind.Relative), apiKey, cdiVersion, body);
            return await SendRequestAsync<RefreshSessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<RegenerateSessionResponse> RegenerateSessionAsync(string? apiKey, string? cdiVersion, RegenerateSessionRequest body) =>
            this.RegenerateSessionAsync(apiKey, cdiVersion, body, CancellationToken.None);

        public async Task<RegenerateSessionResponse> RegenerateSessionAsync(string? apiKey, string? cdiVersion, RegenerateSessionRequest body, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/session/regenerate", UriKind.Relative), apiKey, cdiVersion, body);
            return await SendRequestAsync<RegenerateSessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        public Task<VerifySessionResponse> VerifySessionAsync(string? apiKey, string? cdiVersion, VerifySessionRequest body) =>
            this.VerifySessionAsync(apiKey, cdiVersion, body, CancellationToken.None);

        public async Task<VerifySessionResponse> VerifySessionAsync(string? apiKey, string? cdiVersion, VerifySessionRequest body, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Post, new Uri("/recipe/session/verify", UriKind.Relative), apiKey, cdiVersion, body);
            return await SendRequestAsync<VerifySessionResponse>(_httpClient, request, cancellationToken).ConfigureAwait(false);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, Uri uri, string? apiKey, string? cdiVersion) =>
            CreateRequest<object>(method, uri, apiKey, cdiVersion, null);

        private static HttpRequestMessage CreateRequest<T>(HttpMethod method, Uri uri, string? apiKey, string? cdiVersion, T? body)
            where T : class
        {
            var request = new HttpRequestMessage(method, uri);
            if (apiKey != null)
            {
                request.Headers.TryAddWithoutValidation("api-key", apiKey);
            }

            if (cdiVersion != null)
            {
                request.Headers.TryAddWithoutValidation("cdi-version", cdiVersion);
            }

            if (body != null)
            {
                string requestBody;
                try
                {
                    requestBody = JsonSerializer.Serialize(body, _jsonSerializerOptions);
                }
                catch
                {
                    request.Dispose();
                    throw;
                }

                request.Content = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json);
            }

            return request;
        }

        private static async Task<T> SendRequestAsync<T>(HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
            where T : class
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var responseBody = JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions);
                if (responseBody == null)
                {
                    throw new CoreApiResponseException(request.Method.Method, request.RequestUri?.ToString() ?? "", (int)response.StatusCode, responseContent);
                }

                return responseBody;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new CoreApiResponseException(request.Method.Method, request.RequestUri?.ToString() ?? "", (int)response.StatusCode, responseContent);
            }
            else
            {
                throw new CoreApiResponseException(request.Method.Method, request.RequestUri?.ToString() ?? "", (int)response.StatusCode, null);
            }
        }
    }
}
