using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Hexagram.Saga.Networking;
using UnityEngine;

namespace Hexagram.Saga
{
    public partial class Client : MonoBehaviour {

        private static Client _saga;
        private static Mutex _mut = new Mutex();

        public static Client SAGA
        {
            get
            {
                if (_saga == null){
                    GameObject go = new GameObject("Saga Client");
                    _saga = go.AddComponent<Client>();
                    DontDestroyOnLoad(go);
                    return _saga;
                }

                // This means that the first call to SAGA was not a call to Init, so we let the user know they messed up
                // There is an edge case. If the user makes only one call to Saga, they will receive no error for their mistake,
                // Since the above logic will just return the newly created Client.
                if (!_saga.initialized) {
                    throw new UninitializedException("Please initialize Saga before using functions.");
                }
                
                return _saga;
            }
        } 
        
        private CancellationTokenSource _ct;

        private NetworkBroker _networkBroker;
        private WebSocket _webSocket;
        private LoginResponse _cachedLoginResponse;
        
        private Dictionary<string, User> _users;
        private Dictionary<string, Bot> _bots;
        private Globals _globals;
        
        private Dictionary<string, string> _userNameId;
        private Dictionary<string, string> _botNameId;
        
        public bool initialized;
        
        public void Awake() {
            initialized = false;

            _cachedLoginResponse = new LoginResponse() { };
            _ct = new CancellationTokenSource();

            var assembly = Assembly.Load("Assembly-CSharp");

            _globals = null;
            _users = new Dictionary<string, User>();
            _bots = new Dictionary<string, Bot>();
            
            _userNameId = new Dictionary<string, string>();
            _botNameId = new Dictionary<string, string>();

        }
        
        /// <summary>
        /// Initializes the Saga client. <br/>
        /// This should be the first call made to Saga. If not, you will get errors and the client
        /// may not work properly.
        /// </summary>
        /// <param name="host">The url of the Saga host.</param>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <example>
        /// <code>
        /// await SAGA.InitAsync("https://test.saga.hexagram.io", "my_user", "my_password");
        /// </code>
        /// </example>
        public async Task InitAsync(string host, string username, string password) {

            if (initialized) {
                Debug.Log("Tried to initialize when Saga is already initialized.");
                return;
            }

            _mut.WaitOne();
            
            Debug.Log("Initializing...");

            if (_webSocket != null) {
                await _webSocket.DisconnectAsync();
            }
            
            _networkBroker = new NetworkBroker(host, RefreshAccessToken());
            _webSocket = new WebSocket(host);
            
            _webSocket.On("authenticated", (res) => {
                initialized = true;
                OnInitialized();
            });
            
            _webSocket.On(Routes.WS_PROPERTY_CHANGE, SagaEntity.HandleProperty);

            _webSocket.On(Routes.WS_MESSAGE, SagaEntity.HandleMessage);
            
            await Login(username, password, _ct.Token);
            await Util.WaitUntil(() => initialized, _ct.Token);
            Debug.Log("Initialized.");
            _mut.ReleaseMutex();
        }

        private void Deinit() {

            _mut.WaitOne();
            // Cancel outstanding async functions
            Debug.Log("Deinitializing...");
            
            _ct.Cancel();
            Logout();
            initialized = false;
            _saga = null;
            
            Debug.Log("Deinitialized.");
            _mut.ReleaseMutex();
        }

        private void OnDisable() {
            Deinit();
        }
    }
}