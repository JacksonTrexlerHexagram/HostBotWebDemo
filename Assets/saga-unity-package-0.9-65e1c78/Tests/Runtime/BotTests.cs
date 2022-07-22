using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Hexagram.Saga.Client;
using Random = System.Random;

namespace Hexagram.Saga.Tests {
    
    
    [TestFixture]
    public class BotTests : FunctionTestFixture {

        [Test]
        public async Task BOT() {
            Exception exception = null;
            Bot bot = null;
            try {
                bot = await SAGA.BOT(_testId, true);
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.Null(exception);
            Assert.NotNull(bot);
            Assert.AreEqual(_testId, bot.Name);
            Assert.AreEqual(24, bot.Id.Length);
        }

        [Test]
        public async Task BOTThrowsNotFound() {
            Exception exception = null;
            try {
                await SAGA.BOT(_testId);
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.NotNull(exception);
            Assert.IsInstanceOf<NotFoundException>(exception);
        }
        
        [Test]
        public async Task OnMessageReceived() {
            User user = await SAGA.USER(SAGA.GetUsername());
            Bot bot = await SAGA.BOT(_testId, true);
            SagaMessage message = null;
            
            bot.OnMessage += (msg) => {
                message = msg;
            };

            await user.SendMessageAsync(bot, "Hey what's up?");
            
            Assert.NotNull(message);
            Assert.AreEqual(bot.Id, message.botId);
            Assert.AreEqual(user.Id, message.userId);
            Assert.AreEqual("user", message.from);
            Assert.AreEqual("Hey what's up?", message.message);
        }
        
        [Test]
        public async Task GetNonexistentProperty() {
            Bot bot = await SAGA.BOT(_testId, true);

            string property = bot[_testId].Get<string>();
            Assert.Null(property);
        }
        
        [Test]
        public async Task GetProperty() {
            Bot bot = await SAGA.BOT(_testId, true);

            await bot[_testId].AddAsync("test value");
            string value = bot[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }

        [Test]
        public async Task AddProperty() {
            Bot bot = await SAGA.BOT(_testId, true);

            await bot[_testId].AddAsync("test value");
            string value = bot[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }
        
        [Test]
        public async Task AddPropertyWithBadName() {
            Bot bot = await SAGA.BOT(_testId, true);

            Exception exception = null;
            try {
                await bot[_testId + " illegal stuff"].AddAsync("test value");
            }
            catch (Exception e) {
                exception = e;
            }
            
            string value = bot[_testId + " illegal stuff"].Get<string>();
            Assert.NotNull(exception);
            Assert.IsInstanceOf<UnprocessableException>(exception);
            Assert.Null(value);
        }
    }
}