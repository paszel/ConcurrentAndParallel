using System;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<AccessTest>();
            var summary = BenchmarkRunner.Run<ThreadTest>();

            Console.ReadLine();
        }
    }

}
