using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// To be thrown when a socket is unexpectedly closed, or accessed in a closed state.
	/// </summary>
#if SUPPORTS_SERIALISATION
	[Serializable]
#endif
	public class SocketClosedException : Exception
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public SocketClosedException() : this("The socket is closed.") { }
		/// <summary>
		/// Partial constructor.
		/// </summary>
		/// <param name="message">The error message associated with the error.</param>
		public SocketClosedException(string message) : base(message) { }
		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="message">The error message associated with the error.</param>
		/// <param name="inner">Any inner exception that is wrapped by this exception.</param>
		public SocketClosedException(string message, Exception inner) : base(message, inner) { }

#if SUPPORTS_SERIALISATION
		/// <summary>
		/// Deserialisation constructor.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected SocketClosedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}