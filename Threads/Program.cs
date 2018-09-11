using System;
using System.Threading;
using BenchmarkDotNet.Running;

namespace Threads
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var result = 0;
            var ctr = new CancellationTokenSource(200);

            Basic(args);
            
            result = ExecuteOnThread(x => x * 2, 2, 10);
            Console.WriteLine($"ExecuteOnThread => {result}");

            
            result = ExecuteOnThreadWithCancellationToken(x => x * 2, 2, 10, ctr.Token);
            Console.WriteLine($"ExecuteOnThreadWithCancellationToken => {result}");

            result = ExecuteOnThreadWithCoordinator(x => x * 2, 2, 10, ctr.Token);
            Console.WriteLine($"ExecuteOnThreadWithCoordinator => {result}");

            result = ExecuteOnThreadPoll(x => x * 2, 2, 10000, ctr.Token);
            Console.WriteLine($"ExecuteOnThreadPoll => {result}");

            */

            var summary = BenchmarkRunner.Run<ThreadsvsThreadPoolUsage>();

            Console.ReadLine();
        }

        private static void Basic(string[] args)
        {
            var thread = new Thread(Hello);

            thread.Start(args);
            thread.Join();
        }

        static void Hello(object args)
        {
            Console.WriteLine($"Hello {args}");
        }


        public static int ExecuteOnThread(Func<int, int> func, int arg, int times)
        {
            var result = 0;
            var thread = new Thread(() =>
            {
                for (var i = 0; i < times; i++)
                {
                    //Debug.WriteLine(result);
                    result += func(arg);
                }
            });

            thread.Start();
            thread.Join(2000);

            //Debug.WriteLine(result);
            return result;
        }

        public static int ExecuteOnThreadWithCancellationToken(Func<int, int> func, int arg, int times, CancellationToken cancellationToken)
        {

            var result = 0;
            var thread = new Thread(() =>
            {
                for (var i = 0; i < times; i++)
                {
                    //better for Threads because couldn't be handled easy from main task
                    if (cancellationToken.IsCancellationRequested)
                    {
                        //Debug.WriteLine("Cancellation token abort!");
                        return;
                    }


                    //Debug.WriteLine(result);
                    result += func(arg);
                }
            });

            thread.Start();
            thread.Join(2000);

            //Debug.WriteLine(result);
            return result;
        }

        public static int ExecuteOnThreadWithCoordinator(Func<int, int> func, int arg, int times,
            CancellationToken token)
        {
            var autoEvent = new AutoResetEvent(false);
            var result = 0;

            var thread = new Thread(() =>
            {
                for (var i = 0; i < times; ++i)
                {
                    if (token.IsCancellationRequested)
                        break;
                    result += func(arg);
                }

                autoEvent.Set();
            });
            thread.Start();
            autoEvent.WaitOne(2000);
            autoEvent.Close();

            return result;
        }

        public static int ExecuteOnThreadPoll(Func<int, int> func, int arg, int times,
            CancellationToken token)
        {
            var autoEvent = new AutoResetEvent(false);
            var result = 0;

            ThreadPool.QueueUserWorkItem(x =>
            {
                try
                {
                    for (var i = 0; i < times; ++i)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        result += func((int)x);

                        //Debug.WriteLine(result);
                    }

                    autoEvent.Set();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");

                }
            }, arg);


            autoEvent.WaitOne(2000);
            autoEvent.Close();

            return result;
        }
    }
}
