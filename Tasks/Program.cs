using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctr = new CancellationTokenSource(2);
            var result = ExecuteOnTask(x => x * 2, 2, 1000000, ctr.Token);
            Console.WriteLine(result);

            Console.ReadKey();
        }

        private static int ExecuteOnTask(Func<int, int> func, int arg, int times, CancellationToken token)
        {
            var result = 0;
            var task = new Task(() =>
            {
                for (var i = 0; i < times; i++)
                {
                    //better for tasks, because can be easy handled 
                    token.ThrowIfCancellationRequested();

                    result += func(arg);
                }
            }, token);

            task.Start();
            task.Wait(2000);

            return result;
        }
    }
}
