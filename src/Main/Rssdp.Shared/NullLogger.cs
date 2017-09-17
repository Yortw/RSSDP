using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// Provides a <see cref="ISsdpLogger"/> implementation that does nothing, effectively disabling logging. Use the <see cref="Instance"/> property to obtain an instance as the constructor is private.
	/// </summary>
	/// <remarks>
	/// <para>This logger is inherently thread-safe and the <see cref="Instance"/> value can be shared among multiple components.</para>
	/// </remarks>
	public class NullLogger : ISsdpLogger
	{

		private static ISsdpLogger s_Instance;

		private NullLogger()
		{
		}

		/// <summary>
		/// Provides a single instance of <see cref="NullLogger"/>.
		/// </summary>
		public static ISsdpLogger Instance
		{
			get { return s_Instance ?? (s_Instance = new NullLogger()); }
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <param name="message">Unused as this implementation does not log.</param>
		public void LogError(string message)
		{
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <param name="message">Unused as this implementation does not log.</param>
		public void LogInfo(string message)
		{
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <param name="message">Unused as this implementation does not log.</param>
		public void LogVerbose(string message)
		{
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <param name="message">Unused as this implementation does not log.</param>
		public void LogWarning(string message)
		{
		}
	}
}