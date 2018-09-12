using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await Downloads();
            await ShortestTaskHandledFirst();
            Console.ReadKey();
        }

        static async Task ShortestTaskHandledFirst()
        {
            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                Debug($"Started {i}");
                await Task.Delay((3 - i) * 1000);
                Debug($"Finished {i}");
                return i;
            }).ToList();
            /* works
            foreach (var task in tasks)
            {
                var idx = await Task.WhenAny(tasks.Where(x=>!x.IsCompletedSuccessfully));
                var j = await idx;
                //var isRemove = temp.Remove(idx);
                Debug($"Processing {j}");
                //Debug($"j={isRemove}");
            }
            */

            //better approach
            while(tasks.Count >0)
            {
                var idx = await Task.WhenAny(tasks);
                var j = await idx;
                tasks.Remove(idx);
                Debug($"Processing {j}");
            }

            Debug("Done.");
        }


        static async Task InWhatOrder()
        {
            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                Debug($"Started {i}");
                await Task.Delay((3 - i) * 1000);
                Debug($"Finished {i}");
                return i;
            }).ToList();

            foreach (var task in tasks)
            {
                var idx = await task;
                Debug($"Processing {idx}");
            }
            Debug("Done.");
        }

        private static async Task Downloads()
        {
            Debug("Start");
            Debug(await DownloadsTest(1000,2000));
            Debug(await DownloadsTest(2000, 1000));

            Debug("End");
        }
        
        private static async Task<string> DownloadsTest(int millisecondsTimeout,
            int cancellationTokenMillisecondsTimeout)
        {
            var urls = new[]
            {
                "https://postman-echo.com/delay/3",
                "https://postman-echo.com/delay/6",
                "https://postman-echo.com/delay/9",
                "https://postman-echo.com/delay/12",
                "https://postman-echo.com/delay/15"
            };

            var cts = new CancellationTokenSource(cancellationTokenMillisecondsTimeout);

            try
            {
                return await ConcurrentDownloadAsync(urls, millisecondsTimeout, cts.Token);
            }
            catch (TimeoutException tex)
            {
                Debug($"Cancel all remaining tasks: {tex.Message}");
            }
            catch (Exception ex)
            {
                Debug($"Exception: {ex.Message}");
            }

            return "Nothing.";
        }

        private static async Task<string> ConcurrentDownloadAsync(string[] urls, int millisecondsTimeout,
            CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var client = new HttpClient();

            var tasks = urls.Select(x => client.GetAsync(x, cts.Token));
            var delay = Task.Delay(millisecondsTimeout, cts.Token);

            var tasks2 = tasks.Append(delay);
            var finished = await Task.WhenAny(tasks2);

            if (finished != delay)
            {
                Debug($"Not timeout, getting data");

                var completedTask = finished as Task<HttpResponseMessage>;
                var message = await completedTask;
                var result = await message.Content.ReadAsStringAsync();
                cts.Cancel();

                return result;
            }
            else
            {
                //Debug($"Cancel all remaining tasks: {tex.Message}");
                throw new TimeoutException($"Timeout after {millisecondsTimeout} ms");
            }
        }

        private static void Debug<T>(T arg) =>
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {arg}");
    }
}
