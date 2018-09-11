using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks
{
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    class Program
    {
        static void Main(string[] args)
        {

            //FactoryForLoop();
            //FactoryForeachLoop();

            //TaskCanceledException();

            //WaitAll();
            //WaitAny();

            Task.Run(() => Sequential());
           
            Console.ReadKey();
        }

        static async Task Sequential()
        {
            var tasks = Enumerable.Range(0, 3).Select(i =>
            {
                Console.WriteLine(i);
                return Task.Delay(1000);
            }).ToList();

            foreach (var task in tasks)
            {
                await task;
            }
        }

        private static void WaitAll()
        {
            var ctr = new CancellationTokenSource(2000);

            const int size = 4;

            var tasks = new Task<double>[size];

            for (var i = 0; i < size; i++)
            {
                var tempI = i;
                var task = new Task<double>(() => Math.Sqrt(tempI), ctr.Token);
                task.Start();

                tasks[i] = task;
            }
          
            Task.WaitAll(tasks, 2000, ctr.Token);
            
            //Task.WaitAll(new[]{ t1, t2, t3, t4});
            Console.WriteLine($"a={tasks[0].Result :N3} / b={tasks[1].Result:N3} / c={tasks[2].Result:N3} / d={tasks[3].Result:N3}");
        }

        private static void WaitAny()
        {
            var ctr = new CancellationTokenSource(2000);

            const int size = 4;

            var tasks = new Task<double>[size];

            for (var i = 0; i < size; i++)
            {
                var tempI = i;
                var task = new Task<double>(() => Math.Sqrt(tempI), ctr.Token);
                task.Start();

                tasks[i] = task;
            }

            var index = Task.WaitAny(tasks);
            ctr.Cancel();

            Console.WriteLine($"x={tasks[index].Result:N3}");
        }


        //how not to do it
        private static void FactoryForeachLoop()
        {
            foreach (var i in Enumerable.Range(0, 10))
            {
                Task.Factory.StartNew(() => Console.WriteLine(i));
            }
        }

        //how not to do it either 
        private static void FactoryForLoop()
        {
            for (var i = 0; i < 10; i++)
            {
                Task.Factory.StartNew(() => Console.WriteLine(i));
            }
        }

        private static void TaskCanceledException()
        {
            //will throw AggregateException follow by TaskCanceledException
            var ctr = new CancellationTokenSource(2);
            var result = ExecuteOnTask(x => x * 2, 2, 1000000, ctr.Token);
            Console.WriteLine(result);
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
