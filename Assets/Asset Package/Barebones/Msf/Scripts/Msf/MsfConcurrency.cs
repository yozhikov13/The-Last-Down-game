﻿using Barebones.Logging;
using Barebones.Networking;
using System;
using System.Threading;

namespace Barebones.MasterServer
{
    public class MsfConcurrency
    {
        public int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;

        public void RunInMainThread(Action action)
        {
            MsfTimer.RunInMainThread(action);
        }

		/// <summary>
		/// Sets the method that is to be executed on the separate thread
		/// </summary>
		/// <param name="expression">The method that is to be called on the newly created thread</param>
		private void RunInThreadPool(WaitCallback expression)
		{
			ThreadPool.QueueUserWorkItem(expression);
		}

		/// <summary>
		/// Used to run a method / expression on a separate thread
		/// </summary>
		/// <param name="expression">The method to be run on the separate thread</param>
		/// <param name="delayOrSleep">The amount of time to wait before running the expression on the newly created thread</param>
		/// <returns></returns>
		public void RunInThreadPool(Action expression, int delayOrSleep = 0)
		{
			// Wrap the expression in a method so that we can apply the delayOrSleep before and remove the task after it finishes
			WaitCallback inline = (state) =>
			{
				// Apply the specified delay
				if (delayOrSleep > 0)
					Sleep(delayOrSleep);

				// Call the requested method
				expression.Invoke();
			};

			// Set the method to be called on the separate thread to be the inline method we have just created
			RunInThreadPool(inline);
		}

#if WINDOWS_UWP
		public async void Sleep(int milliseconds)
		{
			await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(milliseconds));
		}
#else
		public void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}
#endif
	}
}