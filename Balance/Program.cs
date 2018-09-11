using System;
using System.Threading;

namespace Balance
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Init balance : {balance}.");
            ThreadPool.QueueUserWorkItem(DoWork, 1);
            ThreadPool.QueueUserWorkItem(DoWork, 20);
            ThreadPool.QueueUserWorkItem(DoWork, 324);
            ThreadPool.QueueUserWorkItem(DoWork, 444);
            Console.WriteLine($"Finish balance : {balance}.");
            Console.ReadLine();
        }

        private static void DoWork(object state)
        {
            Console.WriteLine($"[{state}] start working.");
            var rand = new Random((int)state);

            for (var i = 0; i < 1000; i++)
            {
                var amount = rand.Next(0, 100);
                if (rand.NextDouble() > 0.3)
                {
                    var newBalance = Debit(amount);
                    Console.WriteLine($"[{state}][{i}] debit by {amount}. Balance = {newBalance }.");
                }
                else
                {
                    Credit(amount);
                    Console.WriteLine($"[{state}][{i}] credit by {amount}. Balance = {balance}.");
                }

                if (balance < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{state}][{i}] detect balance < 0! ({balance})");
                    Console.ResetColor();
                }
            }
        }

        private static readonly object Obj = new object();
        static int balance = 10;

        public static int Debit(int amount)
        {
            lock (Obj)
            {
                if (balance >= amount)
                {
                    Thread.Sleep(100);
                    balance = balance - amount;
                    Console.WriteLine($"Balance after debit: {balance}");
                    return balance;
                }
            }
            return 0;
        }

        public static void Credit(int amount)
        {
            lock (Obj)
            {
                balance = balance + amount;
            }
            Console.WriteLine($"Balance after credit: {balance}");
        }
    }
}
