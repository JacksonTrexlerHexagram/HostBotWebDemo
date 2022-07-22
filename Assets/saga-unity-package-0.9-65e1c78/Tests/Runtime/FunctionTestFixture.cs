using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Hexagram.Saga.Client;
using Random = System.Random;

namespace Hexagram.Saga.Tests {
    
    [TestFixture]
    public class FunctionTestFixture {
        
        private static bool _setup;
        private static Random _random = new Random();
        protected string _testId;
        
        [UnitySetUp]
        public IEnumerator Setup() {
            if (!_setup) {
                _setup = true;
                yield return Run().AsCoroutine();
            }
            async Task Run() {
                await SAGA.InitAsync("http://localhost:3000", "admin", "admin1234");
            }

            _testId = _random.Next().ToString();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            GameObject go = GameObject.Find("Saga Client");
            GameObject.Destroy(go);
            _setup = false;
        }
    }
}