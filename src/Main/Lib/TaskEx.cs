using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rssdp
{
	internal static class TaskEx
	{

		// Sadly Task.Run() is missing from this PCL profile,
		// so attempt to build our own for convenience.
		// According to;
		// http://blogs.msdn.com/b/pfxteam/archive/2011/10/24/10229468.aspx
		// "Task.Run is exactly equivalent to" Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
		// Sadly, we don't have DenyChildAttach either, so I guess this is as good as it gets.
		public static Task Run(Action work)
		{
			return Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		public static Task<T> Run<T>(Func<T> work)
		{
			return Task.Factory.StartNew<T>(work, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}


		public static Task Delay(TimeSpan period)
		{
			if (period.TotalMilliseconds > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(period), String.Format(System.Globalization.CultureInfo.InvariantCulture, "period cannot be more than {0} millseconds.", period.TotalMilliseconds));
			
			return Delay(Convert.ToInt32(period.TotalMilliseconds));
		}

		public static Task Delay(int millisecondsDelay)
		{
			if (millisecondsDelay < -1) throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), "millisecondsDelay must be -1 or greater.");

			var tcs = new TaskCompletionSource<object>();
			var timer = new Timer((state) => 
				{
					tcs.SetResult(null);
				}, 
				null, 
				millisecondsDelay, 
				System.Threading.Timeout.Infinite);
			
			return tcs.Task.ContinueWith((t) => timer.Dispose());
		}

	}
}
