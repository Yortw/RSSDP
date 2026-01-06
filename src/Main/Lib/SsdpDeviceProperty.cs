using System;

namespace Rssdp
{
	/// <summary>
	/// Represents a custom property of an <see cref="SsdpDevice"/>.
	/// </summary>
	public sealed class SsdpDeviceProperty
	{

		/// <summary>
		/// Partial constructor.
		/// </summary>
		/// <param name="namespace">The namespace this property exists in.</param>
		/// <param name="name">The name of this property.</param>
		public SsdpDeviceProperty(string @namespace, string name) : this (@namespace, name, null)
		{
		}

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="namespace">The namespace this property exists in.</param>
		/// <param name="name">The name of this property.</param>
		/// <param name="value">The value of this property.</param>
		public SsdpDeviceProperty(string? @namespace, string name, string? value) 
		{
			this.Namespace = @namespace;
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Sets or returns the namespace this property exists in.
		/// </summary>
		public string? Namespace { get; private set; }

		/// <summary>
		/// Sets or returns the name of this property.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the full name of this property (namespace and name).
		/// </summary>
		public string FullName { get { return String.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + ":" + this.Name; } }

		/// <summary>
		/// Sets or returns the value of this property.
		/// </summary>
		public string? Value { get; set; }

	}
}