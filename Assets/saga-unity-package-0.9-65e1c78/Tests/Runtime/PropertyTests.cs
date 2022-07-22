using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Hexagram.Saga.Client;

namespace Hexagram.Saga.Tests {
    
    
    [TestFixture]
    public class PropertyTests : FunctionTestFixture {

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