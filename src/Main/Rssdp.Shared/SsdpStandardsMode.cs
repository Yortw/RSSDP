using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rssdp
{
	/// <summary>
	/// An enum whose values control how strictly RSSDP follows the SSDP specification.
	/// </summary>
	public enum SsdpStandardsMode
	{
		/// <summary>
		/// Equivalent to <see cref="Relaxed"/>
		/// </summary>
		Default,
		/// <summary>
		/// RSSDP will not strictly follow the specification, but will instead behave in ways that are compatible with most SSDP devices.
		/// </summary>
		/// <remarks>
		/// <para>This mode provides maximum compatibility with other SSDP based systems.</para>
		/// </remarks>
		Relaxed,
		/// <summary>
		/// RSSDP will strictly follow the SSDP specification even where other implementations commonly deviate.
		/// </summary>
		Strict
	}
}
