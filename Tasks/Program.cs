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

            //Task.Run(() => Sequential());
            //Task.Run(() => AwaitOnCancelledTask());
            //Task.Run(() => ConcurrentAsync());
            //Task.Run(() => ConcurrentFirstAsync());
            //Task.Run(() => WhenAny());

            //Task.Run(() => TimeoutExtensionTest());



            Console.ReadKey();
        }

        private static async Task TimeoutExtensionTest()
        {
            //won't throw exception
            var task = Task.Run(() => { Console.WriteLine("Done"); }).TimeoutAfter(100);
            await task;
            //will throw exception but task will execute as well

            try
            {
                var exceptionTask = Task.Run(() =>
                {
                    Thread.Sleep(500);
                    Console.WriteLine("Done!");
                }).TimeoutAfter(10);

                await exceptionTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test method exception {ex.Message}");
            }
        }

        private static async Task WhenAny()
        {
            var t1 = Task.CompletedTask;
            var t2 = Task.CompletedTask;
            var finished = Task.WhenAny(t1, t2);
            await finished; //even if void tasks, await will check if exception is thrown  
        }

        static async Task ConcurrentAsync()
        {
            var tasks = Enumerable.Range(0, 3).Select(i =>
            {
                Debug($"Started {i}");
                var task = Task.Delay(1000);
                Debug($"Finished {i}");
                return task;
            });

            await Task.WhenAll(tasks);
            Debug("Done");
        }
        static async Task ConcurrentFirstAsync()
        {
            var cts = new CancellationTokenSource(2000);
            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                Debug($"Started {i}");
                await Task.Delay((i+1)*1000, cts.Token);
                Debug($"Finished {i}");
                return i;
            });


            //pro tip: will work \/
            //var index = await await Task.WhenAny(tasks);

            var finished = await Task.WhenAny(tasks);
            cts.Cancel();

            var index = await finished;

            Debug($"Finished i={index}");
            Debug("Done");
        }

        private static void Debug<T>(T arg) =>
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {arg}");

        static async Task<int> DelaySecondsAndReturnAsync(int seconds, CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds), token);
            return seconds;
        }
        static async Task AwaitOnCancelledTask()
        {
            Debug("Start");
            var cts = new CancellationTokenSource(2000);
            var task = DelaySecondsAndReturnAsync(4, cts.Token);
            Thread.Sleep(1000);
            //cts.Cancel();
            var result = await task;  // Throws in case of cancellation both from timeout or manual Cancel
            Debug($"Done. Cancelled: {task.IsCanceled}");
            Debug($"Result: {result}");
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
