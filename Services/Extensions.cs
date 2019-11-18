using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
	public static class Extensions
	{
		/// <summary>
		/// Waits asynchronously for the process to exit.
		/// </summary>
		/// <param name="process">The process to wait for cancellation.</param>
		/// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
		/// immediately as canceled.</param>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<object>();
			process.Exited += (sender, args) => tcs.TrySetResult(null);
            process.EnableRaisingEvents = true;
            if (cancellationToken != default(CancellationToken))
				cancellationToken.Register(tcs.SetCanceled);

			return tcs.Task;
		}


		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source.IndexOf(toCheck, comp) >= 0;
		}

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, inx) => new { item, inx })
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }
    }
}
