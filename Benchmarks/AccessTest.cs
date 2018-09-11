using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class AccessTest
    {
        [Benchmark]
        public static void IJAccessPattern()
        {
            const int n = 5000;
            const int m = 5000;

            var tab = new int[n, m];

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    tab[i, j] = 1;
                }
            }
        }

        [Benchmark]
        public static void JIAccessPattern()
        {
            const int n = 5000;
            const int m = 5000;

            var tab = new int[n, m];

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    tab[j, i] = 1;
                }
            }
        }
    }
}