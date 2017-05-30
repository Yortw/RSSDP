using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// Implementation of <see cref="ISsdpLogger"/> that writes to the .Net tracing system on platforms that support it, or <see cref="System.Diagnostics.Debug"/> on those that don't.
	/// </summary>
	/// <remarks>
	/// <para>On platforms that only support <see cref="System.Diagnostics.Debug"/> no log entries will be output unless running a debug build, and this effectively becomes a null logger for release builds.</para>
	/// </remarks>
	public class SsdpTraceLogger : ISsdpLogger
	{
		/// <summary>
		/// Records a regular log message.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		public void LogInfo(string message)
		{
			WriteLogMessage("Information", message);
		}

		/// <summary>
		/// Records a frequent or large log message usually only required when trying to trace a problem.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		public void LogVerbose(string message)
		{
			WriteLogMessage("Verbose", message);
		}

		/// <summary>
		/// Records an important message, but one that may not neccesarily be an error.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		public void LogWarning(string message)
		{
			WriteLogMessage("Warning", message);
		}

		/// <summary>
		/// Records a message that represents an error.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		public void LogError(string message)
		{
			WriteLogMessage("Error", message);
		}

		private static void WriteLogMessage(string category, string message)
		{
#if SUPPORTS_TRACE
			System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("G") + " " + message, category);
#else
			System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("G") + " [" + category + "] " + message);
#endif
		}

	}
}