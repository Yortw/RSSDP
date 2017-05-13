using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	internal class NullLogger : ISsdpLogger
	{

		private static ISsdpLogger s_Instance;

		private NullLogger()
		{
		}

		public static ISsdpLogger Instance
		{
			get { return s_Instance ?? (s_Instance = new NullLogger()); }
		}

		public void LogError(string message)
		{
		}

		public void LogInfo(string message)
		{
		}

		public void LogVerbose(string message)
		{
		}

		public void LogWarning(string message)
		{
		}
	}
}