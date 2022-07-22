using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Hexagram.Saga.Client;

namespace Hexagram.Saga.Tests {
    
    
    [TestFixture]
    public class UserTests : FunctionTestFixture {

        [Test]
        public async Task USER() {
            Exception exception = null;
            User user = null;
            try {
                user = await SAGA.USER(SAGA.GetUsername());
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.Null(exception);
            Assert.NotNull(user);
            Assert.AreEqual(SAGA.GetUsername(), user.Username);
            Assert.AreEqual(SAGA.GetId(), user.Id);
        }

        [Test]
        public async Task USERThrowsNotFound() {
            Exception exception = null;
            try {
                await SAGA.USER("Gibberish");
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.NotNull(exception);
            Assert.IsInstanceOf<NotFoundException>(exception);
        }

        [Test]
        public async Task OnPropertyReceived() {
            User user = await SAGA.USER(SAGA.GetUsername());
            SagaProperty property = null;
            user.On[_testId] += (prop) => {
                property = prop;
            };
            
            await user[_testId].AddAsync("test value");
            
            Assert.NotNull(property);
            Assert.AreEqual(SAGA.GetId(), property.ParentId);
            Assert.AreEqual("test value", property.Get<string>());
        }

        // TODO test user messages received properly
        // [Test]
        // public async Task OnMessageReceived() {
        //     User user = await SAGA.USER(SAGA.GetUsername());
        //     Bot bot = await SAGA.BOT(_testId, true);
        //     SagaMessage message = null;
        //     
        //     user.OnMessage += (msg) => {
        //         message = msg;
        //     };
        // }

        [Test]
        public async Task SendMessageDoesNotThrow() {
            User user = await SAGA.USER(SAGA.GetUsername());
            Bot bot = await SAGA.BOT(_testId, true);
            
            Exception exception = null;
            try {
                await user.SendMessageAsync(bot, "Hey what's up?");
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.Null(exception);
        }
        

        [Test]
        public async Task GetNonexistentProperty() {
            User user = await SAGA.USER(SAGA.GetUsername());

            string property = user[_testId].Get<string>();
            Assert.Null(property);
        }
        
        [Test]
        public async Task GetProperty() {
            User user = await SAGA.USER(SAGA.GetUsername());

            await user[_testId].AddAsync("test value");
            string value = user[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }

        [Test]
        public async Task AddProperty() {
            User user = await SAGA.USER(SAGA.GetUsername());

            await user[_testId].AddAsync("test value");
            string value = user[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }
        
        [Test]
        public async Task AddPropertyWithBadName() {
            User user = await SAGA.USER(SAGA.GetUsername());

            Exception exception = null;
            try {
                await user[_testId + " illegal stuff"].AddAsync("test value");
            }
            catch (Exception e) {
                exception = e;
            }
            
            string value = user[_testId + " illegal stuff"].Get<string>();
            Assert.NotNull(exception);
            Assert.IsInstanceOf<UnprocessableException>(exception);
            Assert.Null(value);
        }
    }
}