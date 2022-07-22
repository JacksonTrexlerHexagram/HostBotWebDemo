using System.Collections;
using System.Threading.Tasks;

namespace Hexagram.Saga.Tests {
    public static class Util {
        public static IEnumerator AsCoroutine (this Task task)
        {
            while (!task.IsCompleted) yield return null;
            // if task is faulted, throws the exception
            task.GetAwaiter ().GetResult ();
        }
    }
}