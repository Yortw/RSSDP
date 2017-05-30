using Rssdp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Rssdp
{
	/// <summary>
	/// Represents an SSDP service to be published.
	/// </summary>
	public class SsdpService
	{

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SsdpService()
		{
			this.ServiceTypeNamespace = SsdpConstants.UpnpDeviceTypeNamespace;
			this.ServiceVersion = 1;
		}

		/// <summary>
		/// Deserialisation constructor.
		/// </summary>
		/// <remarks><para>Uses the provided XML string to set the properties of the object. The XML provided must be a valid UPnP service description document.</para></remarks>
		/// <param name="serviceDescriptionXml">A UPnP service description XML document.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="serviceDescriptionXml"/> argument is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="serviceDescriptionXml"/> argument is empty.</exception>
		public SsdpService(string serviceDescriptionXml) : this()
		{
			if (serviceDescriptionXml == null) throw new ArgumentNullException(nameof(serviceDescriptionXml));
			if (serviceDescriptionXml.Length == 0) throw new ArgumentException(nameof(serviceDescriptionXml) + " cannot be an empty string.", nameof(serviceDescriptionXml));

			using (var ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(serviceDescriptionXml)))
			{
				var reader = XmlReader.Create(ms);

				LoadServiceProperties(reader, this);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Sets or returns the service type (not including namespace, version etc) of the exposed service. Required.
		/// </summary>
		///	<seealso cref="ServiceTypeNamespace"/>
		///	<seealso cref="ServiceVersion"/>
		///	<seealso cref="FullServiceType"/>
		public string ServiceType { get; set; }

		/// <summary>
		/// Sets or returns the namespace for the <seealso cref="ServiceType"/> of this service. Optional but defaults to the UPnP schema so should be changed if <seealso cref="ServiceType"/> is not an official UPnP service type.
		/// </summary>
		/// <seealso cref="ServiceType"/>
		/// <seealso cref="ServiceVersion"/>
		/// <seealso cref="FullServiceType"/>
		public string ServiceTypeNamespace { get; set; }

		/// <summary>
		/// Sets or returns the version of the service type. Optional, defaults to 1.
		/// </summary>
		/// <remarks><para>Defaults to a value of 1.</para></remarks>
		/// <seealso cref="ServiceType"/>
		/// <seealso cref="ServiceTypeNamespace"/>
		/// <seealso cref="FullServiceType"/>
		public int ServiceVersion { get; set; }

		/// <summary>
		/// Returns the full service type string.
		/// </summary>
		/// <remarks>
		/// <para>The format used is urn:<see cref="ServiceTypeNamespace"/>:service:<see cref="ServiceType"/>:<see cref="ServiceVersion"/></para>
		/// </remarks>
		public string FullServiceType
		{
			get
			{
				//From the spec; Period characters in the Vendor Domain Name MUST be replaced with hyphens in accordance with RFC 2141
				return String.Format("urn:{0}:service:{1}:{2}",
				(this.ServiceTypeNamespace ?? String.Empty).Replace(".", "-"),
				this.ServiceType ?? String.Empty,
				this.ServiceVersion);
			}
		}

		/// <summary>
		/// Sets or returns the universally unique identifier for this service (without the uuid: prefix). Required.
		/// </summary>
		/// <remarks>
		/// <para>Must be the same over time for a specific service instance (i.e. must survive reboots).</para>
		/// <para>For UPnP 1.0 this can be any unique string. For UPnP 1.1 this should be a 128 bit number formatted in a specific way, preferably generated using the time and MAC based algorithm. See section 1.1.4 of http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.1.pdf for details.</para>
		/// <para>Technically this library implements UPnP 1.0, so any value is allowed, but we advise using UPnP 1.1 compatible values for good behaviour and forward compatibility with future versions.</para>
		/// </remarks>
		public string Uuid { get; set; }

		/// <summary>
		/// Returns the full service type string.
		/// </summary>
		/// <remarks>
		/// <para>The format used is urn:<see cref="ServiceTypeNamespace"/>:serviceid:<see cref="ServiceType"/></para>
		/// </remarks>
		public string ServiceId
		{
			get
			{
				//From the spec; Period characters in the Vendor Domain Name MUST be replaced with hyphens in accordance with RFC 2141
				return String.Format
				(
					"urn:{0}:serviceId:{1}",
					(this.ServiceTypeNamespace == SsdpConstants.UpnpDeviceTypeNamespace ? "upnp-org" : this.ServiceTypeNamespace).Replace(".", "-"),
					this.Uuid ?? String.Empty
				);
			}
		}

		/// <summary>
		/// REQUIRED. URL for service description. (See section  2.5, “Service description” below.) MUST be relative to the URL at which the device description is located in accordance with section 5 of RFC 3986. Specified by UPnP vendor. Single URL. 
		/// </summary>
		public Uri ScpdUrl { get; set; }
		/// <summary>
		/// REQUIRED. URL for control (see section  3, “Control”). MUST be relative to the URL at which the device description is located in accordance with section 5 of RFC 3986. Specified by UPnP vendor. Single URL. 
		/// </summary>
		public Uri ControlUrl { get; set; }
		/// <summary>
		/// URL for eventing (see section  4, “Eventing”). MUST be relative to the URL at which the device description is located in accordance with section 5 of RFC 3986. MUST be unique within the device; any two services MUST NOT have the same URL for eventing. If the service has no evented variables, this element MUST be present but MUST be empty(i.e., <eventSubURL></eventSubURL>.) Specified by UPnP vendor.Single URL.
		/// </summary>
		public Uri EventSubUrl { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Writes this service to the specified <see cref="System.Xml.XmlWriter"/> as a service node and it's content.
		/// </summary>
		/// <param name="writer">The <see cref="System.Xml.XmlWriter"/> to output to.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="writer"/> argument is null.</exception>
		public virtual void WriteServiceDescriptionXml(XmlWriter writer)
		{
			if (writer == null) throw new ArgumentNullException("writer");

			writer.WriteStartElement("service");

			if (!String.IsNullOrEmpty(this.ServiceType))
				WriteNodeIfNotEmpty(writer, "serviceType", FullServiceType);

			WriteNodeIfNotEmpty(writer, "serviceId", ServiceId);
			WriteNodeIfNotEmpty(writer, "SCPDURL", ScpdUrl);
			WriteNodeIfNotEmpty(writer, "controlURL", ControlUrl);
			WriteNodeIfNotEmpty(writer, "eventSubURL", EventSubUrl);

			writer.WriteEndElement();
		}

		#endregion

		#region Private Methods

		private static void WriteNodeIfNotEmpty(XmlWriter writer, string nodeName, string value)
		{
			if (!String.IsNullOrEmpty(value))
				writer.WriteElementString(nodeName, value);
		}

		private static void WriteNodeIfNotEmpty(XmlWriter writer, string nodeName, Uri value)
		{
			if (value != null)
				writer.WriteElementString(nodeName, value.ToString());
		}

		private void LoadServiceProperties(XmlReader reader, SsdpService service)
		{
			ReadUntilServiceNode(reader);

			while (!reader.EOF)
			{
				if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "service")
				{
					reader.Read();
					break;
				}

				if (!SetPropertyFromReader(reader, service))
					reader.Read();
			}
		}

		private static void ReadUntilServiceNode(XmlReader reader)
		{
			while (!reader.EOF && (reader.LocalName != "service" || reader.NodeType != XmlNodeType.Element))
			{
				reader.Read();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Yes, there is a large switch statement, not it's not really complex and doesn't really need to be rewritten at this point.")]
		private bool SetPropertyFromReader(XmlReader reader, SsdpService service)
		{
			switch (reader.LocalName)
			{
				case "serviceType":
					SetServiceTypePropertiesFromFullDeviceType(service, reader.ReadElementContentAsString());
					break;

				case "serviceId":
					SetServiceIdPropertiesFromFullServiceId(service, reader.ReadElementContentAsString());
					break;

				case "SCPDURL":
					this.ScpdUrl = StringToUri(reader.ReadElementContentAsString());
					break;

				case "controlURL":
					this.ControlUrl = StringToUri(reader.ReadElementContentAsString());
					break;

				case "eventSubURL":
					this.EventSubUrl = StringToUri(reader.ReadElementContentAsString());
					break;

				default:
					return false;
			}
			return true;
		}

		private static void SetServiceIdPropertiesFromFullServiceId(SsdpService service, string value)
		{
			if (String.IsNullOrEmpty(value) || !value.Contains(":"))
				service.ServiceType = value;
			else
			{
				var parts = value.Split(':');
				if (parts.Length == 4)
					service.Uuid = parts[3];
				else
					service.Uuid = value;
			}
		}

		private static void SetServiceTypePropertiesFromFullDeviceType(SsdpService service, string value)
		{
			if (String.IsNullOrEmpty(value) || !value.Contains(":"))
				service.ServiceType = value;
			else
			{
				var parts = value.Split(':');
				if (parts.Length == 5)
				{
					int serviceVersion = 1;
					if (Int32.TryParse(parts[4], out serviceVersion))
					{
						service.ServiceTypeNamespace = parts[1];
						service.ServiceType = parts[3];
						service.ServiceVersion = serviceVersion;
					}
					else
						service.ServiceType = value;
				}
				else
					service.ServiceType = value;
			}
		}

		private static Uri StringToUri(string value)
		{
			if (!String.IsNullOrEmpty(value))
				return new Uri(value, UriKind.RelativeOrAbsolute);

			return null;
		}

		#endregion

	}
}