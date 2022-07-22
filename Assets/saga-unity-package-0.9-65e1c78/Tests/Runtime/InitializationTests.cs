using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hexagram.Saga;
using static Hexagram.Saga.Client;

namespace Hexagram.Saga.Tests {
    
    [TestFixture]
    public class InitializationTests {

        [TearDown]
        public void TearDown() {
            GameObject go = GameObject.Find("Saga Client");
            GameObject.Destroy(go);
        }

        [Test]
        public async Task SuccessWithCorrectCredentials() {
             Exception exception = null;
             try {
                 await SAGA.InitAsync("http://localhost:3000", "admin", "admin1234");
             } catch (Exception e) {
                 exception = e;
             }
             
             Assert.Null(exception);
        }

        [Test]
        public async Task FailureWithIncorrectCredentials() {
            Exception exception = null;
            try {
                await SAGA.InitAsync("http://localhost:3000", "admin", "bogus");
            } catch (Exception e) {
                exception = e;
            }
        
            Assert.NotNull(exception);
            Assert.IsInstanceOf<UnauthorizedException>(exception);
        }
    }
}
