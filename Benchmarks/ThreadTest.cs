using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class ThreadTest
    {

        [Params(100)]
        public static int Iterations{ get; set; }
        [Params(10)]
        public static int Delay{ get; set; }

        [Benchmark]
        public static void ThreadSleep()
        {
            for (int i = 0; i < Iterations; i++)
            {
                Thread.Sleep(Delay);
            }
        }

        [Benchmark]
        public static void ThreadSpin()
        {
            for (int i = 0; i < Iterations; i++)
            {
                Thread.SpinWait(Delay);
            }
        }
    }
}