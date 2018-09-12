using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadLocalStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        [ThreadStatic]
        public static string threadStatic = Thread.CurrentThread.ManagedThreadId.ToString();

        public static ThreadLocal<string> threadlocal =
            new ThreadLocal<string>(() => Thread.CurrentThread.ManagedThreadId.ToString());

        public static void Run()
        {
            new Thread(() => { Console.WriteLine("First: {0} {1}", threadlocal.Value, threadStatic); }).Start();
            new Thread(() => { Console.WriteLine("Second: {0} {1}", threadlocal.Value, threadStatic); }).Start();
            new Thread(() => { Console.WriteLine("Third: {0} {1}", threadlocal.Value, threadStatic); }).Start();
            Console.ReadKey();
        }
    }
}
