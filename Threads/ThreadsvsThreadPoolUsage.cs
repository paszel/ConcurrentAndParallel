using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Threads
{
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(printAsm: true, printIL: true, printSource: false)]
    public class ThreadsvsThreadPoolUsage
    {

        [Benchmark]
        public int PlainUsage()
        {
            return Execute(x => x * 2, 10, 10);
        }

        [Benchmark]
        public int ThreadUsage()
        {
            return ExecuteOnThreadWithCoordinator(x => x * 2, 10, 10, CancellationToken.None);
        }

        [Benchmark]
        public int ThreadPoolUsage()
        {
            return ExecuteOnThreadPoll(x => x * 2, 10, 10, CancellationToken.None);
        }

        [Benchmark]
        public int TaskUsage()
        {
            return ExecuteOnTask(x => x * 2, 10, 10, CancellationToken.None);
        }

        int ExecuteOnThreadWithCoordinator(Func<int, int> func, int arg, int times, CancellationToken token)
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

        int ExecuteOnThreadPoll(Func<int, int> func, int arg, int times,
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

        int Execute(Func<int, int> func, int arg, int times)
        {
            var result = 0;
            for (var i = 0; i < times; ++i)
            {
                result += func(arg);
            }
            return result;
        }

        int ExecuteOnTask(Func<int, int> func, int arg, int times, CancellationToken token)
        {
            var result = 0;
            var task = new Task(() =>
            {
                for (var i = 0; i < times; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Abort!");
                        break;
                    }
                    //Debug.WriteLine(result);
                    result += func(arg);
                }
            }, token);

            task.Start();
            task.Wait(2000);

            //Debug.WriteLine(result);
            return result;
        }
    }
}