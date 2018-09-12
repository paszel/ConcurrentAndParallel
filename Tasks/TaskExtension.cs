using System;
using System.Threading.Tasks;

namespace Tasks
{
    public static class TaskExtension
    {
        /// <summary>
        /// If no cancellation token task can't be terminated :(
        /// Even if exception is thrown - task will complete like nothing happens 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsTimeout)) == task)
                await task;
            else
                throw new TimeoutException($"Timeout after {millisecondsTimeout} ms");
        }
    }
}