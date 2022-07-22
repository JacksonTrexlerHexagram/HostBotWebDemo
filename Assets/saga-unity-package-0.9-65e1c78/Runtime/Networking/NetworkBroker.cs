using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using UnityEngine;
using static Hexagram.Saga.Client;

namespace Hexagram.Saga.Networking {

    public class NetworkBroker {

        private readonly RestClient _client;
        private Task _refreshAccessTokenFunction;
        private bool _tryingToGetRefreshToken = false;

        public NetworkBroker(string host, Task refreshAccessTokenFunction) {
            _client = new RestClient(host);
            _refreshAccessTokenFunction = refreshAccessTokenFunction;
        }

        public void SetAccessToken(string token) {
            _client.AddDefaultHeader("access-token", token);
        }

        public async Task<TResponse> GetAsync<TResponse>(string route, CancellationToken ct, string searchFor = null) {
            TResponse response = await DispatchAsync<TResponse>(route, null, Method.Get, ct, searchFor);
            return response;
        }
        
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest body, CancellationToken ct) {
            TResponse response = await DispatchAsync<TResponse>(route, JsonConvert.SerializeObject(body), Method.Post, ct);
            return response;
        }
        
        public async Task<TResponse> PutAsync<TRequest, TResponse>(string route, TRequest body, CancellationToken ct) {
            TResponse response = await DispatchAsync<TResponse>(route, JsonConvert.SerializeObject(body), Method.Put, ct);
            return response;
        }

        private async Task<TResponse> DispatchAsync<TResponse>(string route, string body, Method method, CancellationToken ct, string searchFor = null) {
            
            Debug.Log("Dispatching " + body + " to " + route);

            RestRequest request = new RestRequest(route, method);

            if (body != null) {
                request.AddStringBody(body, DataFormat.Json);
            }

            if (searchFor != null) {
                request.AddQueryParameter("search", searchFor);
            }
            
            RestResponse response = await _client.ExecuteAsync(request, ct);
            
            try {
                Util.ThrowException((int) response.StatusCode, response.Content);
            }
            catch (UnauthorizedException e) {
                // If a request is made, there is an access token, and
                // the request is not authorized, try to get a new access token
                if (e.Message.Equals("Token Expired") && !_tryingToGetRefreshToken) {
                    _tryingToGetRefreshToken = true;
                    await _refreshAccessTokenFunction;
                }
                else {
                    throw;
                }
            } finally {
                _tryingToGetRefreshToken = false;
            }


            TResponse sagaResponse = JsonConvert.DeserializeObject<TResponse>(response.Content!);
            
            return sagaResponse;
        }
    }
}