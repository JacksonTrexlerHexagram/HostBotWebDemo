using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hexagram.Saga.Networking;
using UnityEngine;

namespace Hexagram.Saga {
    public class Util {
        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="frequency">The frequency at which the condition will be polled.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, CancellationToken ct, int frequency = 5,
            int timeout = -1) {
            var waitTask = Task.Run(async () => {
                while (!condition()) {
                    await Task.Delay(frequency, ct);
                }
            }, ct);

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout, ct)))
                throw new TimeoutException();
        }

        private struct SagaError {
            public int statusCode;
            public string name;
            public string message;
            public Dictionary<string, string> errors;
        }
        
        public static void ThrowException(int statusCode, string message) {
            SagaError error = new SagaError();

            if (statusCode != (int) SagaResponseCode.Ok) {
                error = JsonConvert.DeserializeObject<SagaError>(message);
            }

            var errorMessage = error.message + "\n" + JsonConvert.SerializeObject(error.errors);
            
            switch (statusCode) {
                case (int) SagaResponseCode.Ok:
                    break;
                case (int) SagaResponseCode.BadRequest:
                    throw new BadRequestException(errorMessage);
                case (int) SagaResponseCode.Unauthorized:
                    throw new UnauthorizedException(errorMessage);
                case (int) SagaResponseCode.Unprocessable:
                    throw new UnprocessableException(errorMessage);
                case (int) SagaResponseCode.NotFound:
                    throw new NotFoundException(errorMessage);
                case (int) SagaResponseCode.ServerError:
                    throw new ServerException(errorMessage);
                default:
                    throw new Exception(message);
            }
        }
    }
}