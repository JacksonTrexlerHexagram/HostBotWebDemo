using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hexagram.Saga.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using UnityEngine;

namespace Hexagram.Saga {

    public partial class Client {

        public class PropertyReceivedHandlers {
            
            private Dictionary<string, SagaPropertyHandler> _propertyHandlers = new Dictionary<string, SagaPropertyHandler>();

            /// <summary>
            /// Get a property event to bind event handlers to.
            /// </summary>
            /// <example>
            /// <code>
            /// property.On["example_property"] += (property) => {
            ///     string exampleValue = property.Get&lt;string&gt;();
            /// }
            /// </code>
            /// </example>
            /// <param name="propertyName">The property name to bind an event to.</param>
            public SagaPropertyHandler this [string propertyName]
            {
                get
                {
                    if (!_propertyHandlers.ContainsKey(propertyName)) {
                        _propertyHandlers[propertyName] = delegate { };
                    }

                    return _propertyHandlers[propertyName];
                }

                set => _propertyHandlers[propertyName] = value;
                
            }
        }

        public abstract class SagaEntity {
            
            protected SagaEntity() { }
            
            [JsonProperty] private string _id;
            [JsonIgnore] public string Id => _id;
            
            [JsonProperty("properties")] 
            protected Dictionary<string, SagaProperty> properties = new Dictionary<string, SagaProperty>();

            [JsonIgnore] private PropertyReceivedHandlers _propertyReceivedHandlers = new PropertyReceivedHandlers();
            [JsonIgnore] public PropertyReceivedHandlers On => _propertyReceivedHandlers;
            
            private void SetProperty(SagaProperty property) {

                properties[property.Name] = property;
            }

            public static void HandleProperty(SocketIOResponse ev) {
            
                SagaProperty property = ev.GetValue<SagaProperty>();
                
                string name = property.Name;
                string parentId = property.ParentId;
            
                if (parentId == null) {
                    if (SAGA._globals != null) {
                        SAGA._globals.SetProperty(property);
                        SAGA._globals.On[name](property);
                    }
                } else {
                    if (SAGA._users.ContainsKey(parentId)) {
                        SAGA._users[parentId].SetProperty(property);
                        SAGA._users[parentId].On[name](property);
                    }
                    else if (SAGA._bots.ContainsKey(parentId)) {
                        SAGA._bots[parentId].SetProperty(property);
                        SAGA._bots[parentId].On[name](property);
                    }
                    else {
                        Debug.LogWarning("Received property for unknown id '" + parentId + "'.");
                    }
                }
            }
            
            public event MessageHandler OnMessage = delegate { };

            public static void HandleMessage(SocketIOResponse ev) {
                SagaMessage message = ev.GetValue<SagaMessage>();

                if (message.from.Equals("bot") && SAGA._users.ContainsKey(message.userId)) {
                    SAGA._users[message.userId].OnMessage(message);
                } else if (message.from.Equals("user") && SAGA._bots.ContainsKey(message.botId)) {
                    SAGA._bots[message.botId].OnMessage(message);
                }
            }
        }
        
        public sealed class User : SagaEntity {
            
            private User() { }

            [JsonProperty("username")] private string _username;
            [JsonIgnore] public string Username => _username;

            /// <summary>
            /// Sends a message from the user to the specified bot.
            /// </summary>
            /// <param name="botId">The id of the target bot.</param>
            /// <param name="message">The message to send.</param>
            public async Task SendMessageAsync(string botId, string message) {
                
                SagaMessage sagaMessage = new SagaMessage() {
                    userId = Id,
                    botId = botId,
                    from = null,
                    message = message
                };
                
                await SAGA._webSocket.EmitAsync(Routes.WS_MESSAGE_SEND, sagaMessage, SAGA._ct.Token);
            }
            
            /// <summary>
            /// Sends a message from the user to the specified bot.
            /// </summary>
            /// <param name="bot">The target bot.</param>
            /// <param name="message">The message to send.</param>
            public async Task SendMessageAsync(Bot bot, string message) {
                
                SagaMessage sagaMessage = new SagaMessage() {
                    userId = Id,
                    botId = bot.Id,
                    from = null,
                    message = message
                };
                
                await SAGA._webSocket.EmitAsync(Routes.WS_MESSAGE_SEND, sagaMessage, SAGA._ct.Token);
            }

            /// <summary>
            /// Get a <see cref="SagaProperty"/>.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            public UserProperty this[string propertyName] {
                get {
                    if (properties.ContainsKey(propertyName)) {
                        return new UserProperty(properties[propertyName]);
                    }
                    return new UserProperty(propertyName, Id);
                }
            }
            
            /// <summary>
            /// Unsubscribes from the user. After this call has been made, property and message events will no longer
            /// be raised, and properties will no longer be synced with Saga.
            /// </summary>
            public async Task Unsubscribe() {
                await SAGA.Unsubscribe(this);
            }
        }
        
        public sealed class Bot : SagaEntity {
            
            private Bot() { }

            [JsonProperty("name")] private string _name;
            [JsonIgnore] public string Name => _name;
            
            /// <summary>
            /// Get a <see cref="SagaProperty"/>.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            public BotProperty this[string propertyName] {
                get {
                    if (properties.ContainsKey(propertyName)) {
                        return new BotProperty(properties[propertyName]);
                    }
                    return new BotProperty(propertyName, Id);
                }
            }

            /// <summary>
            /// Unsubscribes from the bot. After this call has been made, property and message events will no longer
            /// be raised, and properties will no longer be synced with Saga.
            /// </summary>
            public async Task Unsubscribe() {
                await SAGA.Unsubscribe(this);
            }
        }

        public sealed class Globals : SagaEntity {

            private Globals() { }

            public Globals(Dictionary<string, SagaProperty> properties) {
                this.properties = properties;
            }
            
            /// <summary>
            /// Get a <see cref="SagaProperty"/>.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            public GlobalProperty this[string propertyName] {
                get {
                    if (properties.ContainsKey(propertyName)) {
                        return new GlobalProperty(properties[propertyName]);
                    }
                    return new GlobalProperty(propertyName);
                }
            }

            /// <summary>
            /// Unsubscribes from globals. After this call has been made, property events will no longer
            /// be raised, and properties will no longer be synced with Saga.
            /// </summary>
            public async Task Unsubscribe() {
                await SAGA.Unsubscribe();
            }
        }
        
        /// <summary>
        /// Fetches and subscribes to a Saga user.
        /// </summary>
        /// <param name="username">The username of the user to fetch.</param>
        /// <returns><see cref="User"/></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<User> USER(string username) {
            if (_userNameId.ContainsKey(username)) {
                return _users[_userNameId[username]];
            }
            
            Debug.Log("Getting user " + username + "...");
            
            User[] matches = await _networkBroker.GetAsync<User[]>(Routes.USERS, _ct.Token, "^" + username + "$");
            
            User user;
            if (matches.Length != 1) { // If there is not exactly one user with the username
                throw new NotFoundException("User " + username + " could not be found.");
            }
            
            user = matches[0];
            await Join(user);

            return user;
        }

        private struct CreateBot {
            public string name;
            
            [JsonProperty("test_data")]
            public bool testData;
        }
        
        /// <summary>
        /// Fetches and subscribes to a Saga bot.
        /// </summary>
        /// <param name="name">The name of the bot to fetch.</param>
        /// <param name="createIfNotExists">If true and the bot does not exist, create it.</param>
        /// <param name="testData">If <paramref name="createIfNotExists"/>, sets test_data flag.</param>
        /// <returns><see cref="Bot"/></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Bot> BOT(string name, bool createIfNotExists = false, bool testData = false) {
            if (_botNameId.ContainsKey(name)) {
                return _bots[_botNameId[name]];
            }
            
            Debug.Log("Getting bot " + name + "...");
            
            Bot[] matches = await _networkBroker.GetAsync<Bot[]>(Routes.BOTS, _ct.Token, "^" + name + "$");
            Bot bot;
            if (matches.Length != 1) { // If there is not exactly one bot with the name
                if (createIfNotExists) {
                    CreateBot createBot = new CreateBot() {
                        name = name,
                        testData = testData
                    };
                    bot = await _networkBroker.PostAsync<CreateBot, Bot>(Routes.BOTS, createBot, _ct.Token);
                } else {
                    throw new NotFoundException("Bot " + name + " could not be found.");
                }
            } else {
                bot = matches[0];
            }
            
            await Join(bot);

            return bot;
        }

        /// <summary>
        /// Fetches and subscribes to global properties.
        /// </summary>
        /// <returns><see cref="Globals"/></returns>
        public async Task<Globals> GLOBALS() {
            if (_globals != null) {
                return _globals;
            }
            
            Debug.Log("Getting globals...");
            SagaProperty[] globalProperties = await _networkBroker.GetAsync<SagaProperty[]>(Routes.GLOBAL_PROPERTIES, _ct.Token);

            Dictionary<string, SagaProperty> properties = new Dictionary<string, SagaProperty>();
            
            foreach (SagaProperty property in globalProperties) {
                properties[property.Name] = property;
            }
            
            Globals globals = new Globals(properties);

            await Join(globals);

            return globals;
        }
        
        private async Task Join(User user) {
            Debug.Log("Joining user room " + user.Id + "...");
            await _webSocket.EmitAsync(Routes.WS_USER_JOIN, user.Id, _ct.Token);
            _users[user.Id] = user;
            _userNameId[user.Username] = user.Id;
            Debug.Log("Joined user room " + user.Id + ".");
        }

        private async Task Join(Bot bot) {
            Debug.Log("Joining bot room " + bot.Id + "...");
            await _webSocket.EmitAsync(Routes.WS_BOT_JOIN, bot.Id, _ct.Token);
            _bots[bot.Id] = bot;
            _botNameId[bot.Name] = bot.Id;
            Debug.Log("Joined bot room " + bot.Id + ".");
        }

        private async Task Join(Globals globals) {
            Debug.Log("Joining globals room...");
            await _webSocket.EmitAsync(Routes.WS_GLOBALS_JOIN, "", _ct.Token);
            _globals = globals;
            Debug.Log("Joined globals room.");
        }

        private async Task Unsubscribe(User user) {
            if (!_users.ContainsKey(user.Id)) {
                return;
            }
            
            Debug.Log("Leaving user room " + user.Id + "...");
            await _webSocket.EmitAsync(Routes.WS_USER_LEAVE, user.Id, _ct.Token);
            _users.Remove(user.Id);
            _userNameId.Remove(user.Username);
            Debug.Log("Left user room " + user.Id + ".");
        }
        
        private async Task Unsubscribe(Bot bot) {
            if (!_bots.ContainsKey(bot.Id)) {
                return;
            }
            
            Debug.Log("Leaving bot room " + bot.Id + "...");
            await _webSocket.EmitAsync(Routes.WS_BOT_LEAVE, bot.Id, _ct.Token);
            _bots.Remove(bot.Id);
            _botNameId.Remove(bot.Name);
            Debug.Log("Left bot room " + bot.Id + ".");
        }

        private async Task Unsubscribe() {
            if (_globals == null) {
                return;
            }
            
            Debug.Log("Leaving globals room...");
            await _webSocket.EmitAsync(Routes.WS_GLOBALS_LEAVE, "", _ct.Token);
            _globals = null;
            Debug.Log("Left globals room.");
        }
    }
}