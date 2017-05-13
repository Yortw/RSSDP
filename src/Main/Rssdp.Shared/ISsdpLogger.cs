using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// Interface for a simple logging component used by RSSDP to record internal activity.
	/// </summary>
	public interface ISsdpLogger
	{

		/// <summary>
		/// Records a regular log message.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		void LogInfo(string message);

		/// <summary>
		/// Records a frequent or large log message usually only required when trying to trace a problem.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		void LogVerbose(string message);

		/// <summary>
		/// Records an important message, but one that may not neccesarily be an error.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		void LogWarning(string message);

		/// <summary>
		/// Records a message that represents an error.
		/// </summary>
		/// <param name="message">The text to be logged.</param>
		void LogError(string message);
	}
}