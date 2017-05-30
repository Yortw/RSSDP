using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rssdp
{
	/// <summary>
	/// Provides extension methods to <see cref="System.Exception"/> and derived objects.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Returns true of the specified exception is one that indicates some form of memory corruption, out of memory state or other fatal exception that should *never* be handled by user code.
		/// </summary>
		/// <param name="exception">The exception to check.</param>
		/// <remarks>
		/// <para>Doesn't check for System.StackOverflowExceptions as if the stack really is full calling this method might check, therefore calling code must explicitly handle that exception type itself.</para>
		/// <para>Specifically checks for the following exception types;
		/// 
		/// <list type="Bullet">
		/// <item>System.AccessViolationException</item>
		/// <item>System.OutOfMemoryException</item>
		/// <item>System.InvalidProgramException</item>
		/// </list>
		/// </para>
		/// </remarks>
		/// <returns>True if the specified exception is considered critical and should be re-thrown and not otherwise handled by user code.</returns>
		public static bool IsCritical(this Exception exception)
		{
#if NETSTANDARD
			// Unrecoverable exceptions should not be getting caught and will be dealt with on a broad level by a high-level catch-all handler
			// https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/breaking-change-rules.md#exceptions
			return (exception is System.OutOfMemoryException)
				|| (exception is System.InvalidProgramException);
#elif WINRT || PORTABLE
			return (exception is System.OutOfMemoryException);
#else
			return (exception is System.AccessViolationException)
				|| (exception is System.OutOfMemoryException)
				|| (exception is System.InvalidProgramException);

#endif
		}
	}
}