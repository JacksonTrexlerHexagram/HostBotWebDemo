using System;
using System.Threading;
using System.Threading.Tasks;
using Hexagram.Saga.Networking;
using Newtonsoft.Json;
using UnityEngine;

namespace Hexagram.Saga {
    struct LoginPost {
        public string username;
        public string password;
    }

    struct LoginResponse {
        [JsonProperty("_id")] 
        public string id;
        public string username;
        public string accessToken;
        public string refreshToken;
    }

    struct Authentication {
        public string accessToken;
    }
    
    struct RefreshTokenSend {
        public string refreshToken;
    }

    struct RefreshTokenReceive {
        public string accessToken;
        public string refreshToken;
    }

    public partial class Client {

        /// <summary>
        /// If logged in, returns the access token of the logged-in user.<br/>
        /// Otherwise, returns an empty string.
        /// </summary>
        /// <returns>string</returns>
        public string GetAccessToken() {
            return _cachedLoginResponse.accessToken;
        }

        /// <summary>
        /// If logged in, returns the id of the logged-in user.<br/>
        /// Otherwise, returns an empty string.
        /// </summary>
        /// <returns>string</returns>
        public string GetId() {
            return _cachedLoginResponse.id;
        }
        
        /// <summary>
        /// If logged in, returns the username of the logged-in user.<br/>
        /// Otherwise, returns an empty string.
        /// </summary>
        /// <returns>string</returns>
        public string GetUsername() {
            return _cachedLoginResponse.username;
        }

        private async Task Login(string username, string password, CancellationToken ct) {

            LoginPost loginPost = new LoginPost() {
                username = username,
                password = password
            };

            LoginResponse loginResponse = await _networkBroker.PostAsync<LoginPost, LoginResponse>(Routes.USER_LOGIN, loginPost, ct);

            _cachedLoginResponse = loginResponse;
            _networkBroker.SetAccessToken(_cachedLoginResponse.accessToken);
            
            Authentication auth = new Authentication() {
                accessToken = _cachedLoginResponse.accessToken
            };

            _webSocket.OnConnected(async (sender, eventArgs) => {
                await _webSocket.EmitAsync(Routes.WS_AUTHENTICATE, auth, ct);
            });

            await _webSocket.ConnectAsync();
            
        }

        private async Task RefreshAccessToken() {
            RefreshTokenSend refreshTokenSend = new RefreshTokenSend() {
                refreshToken = _cachedLoginResponse.refreshToken
            };
                    
            RefreshTokenReceive refreshTokenReceive = await _networkBroker.PostAsync<RefreshTokenSend, RefreshTokenReceive>(
                Routes.USER_REFRESH_TOKEN(_cachedLoginResponse.id),
                refreshTokenSend, 
                _ct.Token);

            _cachedLoginResponse.accessToken = refreshTokenReceive.accessToken;
            _cachedLoginResponse.refreshToken = refreshTokenReceive.refreshToken;
            _networkBroker.SetAccessToken(_cachedLoginResponse.accessToken);

        }

        private void Logout() {
            _cachedLoginResponse = new LoginResponse() { };
            _users.Clear();
            _bots.Clear();
            _globals = null;
            _userNameId.Clear();
            _botNameId.Clear();
        }
    }
}
