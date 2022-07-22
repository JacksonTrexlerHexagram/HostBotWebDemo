using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Extensions;
using Hexagram.Saga.Networking;
using SocketIOClient;
using UnityEngine;

namespace Hexagram.Saga {

    public partial class Client {

        protected class Operations {
            public const string INCR = "incr";
            public const string MUL = "mul";
            public const string SADD = "sadd";
            public const string REM = "rem";
            public const string LPUSH = "lpush";
            public const string RPUSH = "rpush";
            public const string LPOP = "lpop";
            public const string RPOP = "rpop";
            public const string SET = "set";
            public const string UNSET = "unset";
            public const string HINCR = "hincr";
            public const string HMUL = "hmul";
            public const string HLPUSH = "hlpush";
            public const string HRPUSH = "hrpush";
            public const string HLPOP = "hlpop";
            public const string HRPOP = "hrpop";
            public const string HREM = "hrem";
            public const string HSADD = "hsadd";
        }

        /// <summary>
        /// Represents a Saga Property.
        /// </summary>
        public class SagaProperty {

            public SagaProperty(string name, string parentId) {
                _name = name;
                _parentId = parentId;
            }

            protected Func<string, string> route;
            
            [JsonProperty("name", Required = Required.Always)]
            private string _name;
            [JsonIgnore] public string Name => _name;
            
            [JsonProperty("parent_id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            private string _parentId;

            /// <summary>
            /// The id of the parent object;
            /// </summary>
            [JsonIgnore] public string ParentId => _parentId;

            [JsonProperty("key", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            protected string key;
            
            [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            protected object value;
            
            /// <summary>
            /// The value of the property.<br/>
            /// To cast to a specific type, use <see cref="Get{TValue}"/>
            /// </summary>
            [JsonIgnore] public object Value => value;
            
            /// <summary>
            /// Add or update a property value.
            /// </summary>
            /// <param name="value">The value to add.</param>
            public async Task AddAsync(object value) {
                await SendAsync(null, null, value);
            }
            
            /// <summary>
            /// Increments the value of a numeric property. If property doesn't exist it will create it with the given value.
            /// </summary>
            /// <param name="value">The amount to increment by.</param>
            public async Task IncrAsync(double value) {
                await SendAsync(Operations.INCR, null, value);
            }
            
            /// <summary>
            /// Increments the value of a numeric property. If property doesn't exist it will create it with the given value.
            /// </summary>
            /// <param name="value">The amount to increment by.</param>
            public async Task IncrAsync(int value) {
                await SendAsync(Operations.INCR, null, value);
            }
            
            /// <summary>
            /// Multiplies the value of a numeric property. If property doesn't exist it will be set to 0.
            /// </summary>
            /// <param name="value">The amount to multiply by.</param>
            public async Task MulAsync(double value) {
                await SendAsync(Operations.MUL, null, value);
            }
            
            /// <summary>
            /// Multiplies the value of a numeric property. If property doesn't exist it will be set to 0.
            /// </summary>
            /// <param name="value">The amount to multiply by.</param>
            public async Task MulAsync(int value) {
                await SendAsync(Operations.MUL, null, value);
            }
            
            /// <summary>
            /// Adds a value to an array property that is treated as a set.
            /// If the same value already exists it won't create a duplicate value.
            /// </summary>
            /// <param name="value">The value to add.</param>
            public async Task SAddAsync(object value) {
                await SendAsync(Operations.SADD, null, value);
            }

            /// <summary>
            /// Removes all instances of the given value from an existing array property.
            /// </summary>
            /// <param name="value">The value to remove.</param>
            public async Task RemAsync(object value) {
                await SendAsync(Operations.REM, null, value);
            }
            
            /// <summary>
            /// Left push to an array property. If property doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="value">The value to push.</param>
            public async Task LPushAsync(object value) {
                await SendAsync(Operations.LPUSH, null, value);
            }
            
            /// <summary>
            /// Right push to an array property. If property doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="value">The value to push.</param>
            public async Task RPushAsync(object value) {
                await SendAsync(Operations.RPUSH, null, value);
            }
            
            /// <summary>
            /// Left pop from an array property. If property doesn't exist it will be set to undefined.
            /// </summary>
            public async Task LPopAsync() {
                await SendAsync(Operations.LPOP);
            }
            
            /// <summary>
            /// Right pop from an array property. If property doesn't exist it will be set to undefined.
            /// </summary>
            public async Task RPopAsync() {
                await SendAsync(Operations.RPOP);
            }
            
            /// <summary>
            /// Set the key value pair in the given property. If property or key doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public async Task SetAsync(string key, object value) {
                await SendAsync(Operations.SET, key, value);
            }
            
            /// <summary>
            /// Unset the key value pair in the given property. 
            /// </summary>
            /// <param name="key"></param>
            public async Task UnsetAsync(string key) {
                await SendAsync(Operations.UNSET, key, null);
            }
            
            /// <summary>
            /// Increment the key value pair in the given property. If property or key doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The amount to increment by.</param>
            public async Task HIncrAsync(string key, double value) {
                await SendAsync(Operations.HINCR, key, value);
            }
            
            /// <summary>
            /// Increment the key value pair in the given property. If property or key doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The amount to increment by.</param>
            public async Task HIncrAsync(string key, int value) {
                await SendAsync(Operations.HINCR, key, value);
            }
            
            /// <summary>
            /// Multiply the key value pair in the given property. If property or key doesn't exist it will be set to 0.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The amount to multiply by.</param>
            public async Task HMulAsync(string key, double value) {
                await SendAsync(Operations.HMUL, key, value);
            }
            
            /// <summary>
            /// Multiply the key value pair in the given property. If property or key doesn't exist it will be set to 0.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The amount to multiply by.</param>
            public async Task HMulAsync(string key, int value) {
                await SendAsync(Operations.HMUL, key, value);
            }
                
            /// <summary>
            /// Left push to a property array entry. If property or key doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The object to push to the array at key.</param>
            public async Task HLPushAsync(string key, object value) {
                await SendAsync(Operations.HLPUSH, key, value);
            }
            
            /// <summary>
            /// Right push to a property array entry. If property or key doesn't exist it will be set to [value].
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The object to push to the array at key.</param>
            public async Task HRPushAsync(string key, object value) {
                await SendAsync(Operations.HRPUSH, key, value);
            }
            
            /// <summary>
            /// Left pop from property array entry. If property doesn't exist it will be set to undefined.
            /// </summary>
            /// <param name="key"></param>
            public async Task HLPopAsync(string key) {
                await SendAsync(Operations.HLPOP, key, null);
            }

            /// <summary>
            /// Right pop from property array entry. If property doesn't exist it will be set to undefined.
            /// </summary>
            /// <param name="key"></param>
            public async Task HRPopAsync(string key) {
                await SendAsync(Operations.HRPOP, key, null);
            }
            
            /// <summary>
            /// Removes all entries from the property array entry that match the value.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value">The value to remove all entries in key from.</param>
            public async Task HRemAsync(string key, object value) {
                await SendAsync(Operations.HREM, key, value);
            }
            
            /// <summary>
            /// Adds a value to a property array entry that is treated as a set.
            /// If the same value  already exists it won't create a duplicate value.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public async Task HSAddAsync(string key, object value) {
                await SendAsync(Operations.HSADD, key, value);
            }

            private async Task SendAsync(string op = null, string k = null, object val = null) {
                SagaProperty property = new SagaProperty(Name, ParentId) {
                    value = val,
                    key = k
                };
                await SAGA._webSocket.EmitAsync(route(op), property, SAGA._ct.Token);
            }

            public TValue Get<TValue>() {
                if (Value == null) {
                    return default;
                }
                
                JToken token = JToken.FromObject(Value);
                return token.ToObject<TValue>();
            }
        }

        public class UserProperty : SagaProperty {
            
            public UserProperty(SagaProperty property) : base(property.Name, property.ParentId) {
                value = property.Value;
                route = Routes.WS_PROPERTY_USER_SEND;
            }
            
            public UserProperty(string name, string parentId) : base(name, parentId) {
                route = Routes.WS_PROPERTY_USER_SEND;
            }
            
        }
        
        public class BotProperty : SagaProperty {

            public BotProperty(SagaProperty property) : base(property.Name, property.ParentId) {
                value = property.Value;
                route = Routes.WS_PROPERTY_BOT_SEND;
            }
            
            public BotProperty(string name, string parentId) : base(name, parentId) {
                route = Routes.WS_PROPERTY_BOT_SEND;
            }
        }
        
        public class GlobalProperty : SagaProperty {
            
            public GlobalProperty(SagaProperty property) : base(property.Name, null) {
                value = property.Value;
                route = Routes.WS_PROPERTY_GLOBAL_SEND;
            }
            public GlobalProperty(string name) : base(name, null) {
                route = Routes.WS_PROPERTY_GLOBAL_SEND;
            }
        }

    }
}