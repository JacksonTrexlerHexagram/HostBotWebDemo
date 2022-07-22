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
    public class GlobalsTests : FunctionTestFixture {

        [Test]
        public async Task GLOBALS() {
            Exception exception = null;
            Globals globals = null;
            try {
                globals = await SAGA.GLOBALS();
            }
            catch (Exception e) {
                exception = e;
            }

            Assert.Null(exception);
            Assert.NotNull(globals);
        }
        
        [Test]
        public async Task GetNonexistentProperty() {
            Globals globals = await SAGA.GLOBALS();

            string property = globals[_testId].Get<string>();
            Assert.Null(property);
        }
        
        [Test]
        public async Task GetProperty() {
            Globals globals = await SAGA.GLOBALS();

            await globals[_testId].AddAsync("test value");
            string value = globals[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }

        [Test]
        public async Task AddProperty() {
            Globals globals = await SAGA.GLOBALS();

            await globals[_testId].AddAsync("test value");
            string value = globals[_testId].Get<string>();
            Assert.NotNull(value);
            Assert.AreEqual("test value", value);
        }
        
        [Test]
        public async Task AddPropertyWithBadName() {
            Globals globals = await SAGA.GLOBALS();

            Exception exception = null;
            try {
                await globals[_testId + " illegal stuff"].AddAsync("test value");
            }
            catch (Exception e) {
                exception = e;
            }
            
            string value = globals[_testId + " illegal stuff"].Get<string>();
            Assert.NotNull(exception);
            Assert.IsInstanceOf<UnprocessableException>(exception);
            Assert.Null(value);
        }
    }
}