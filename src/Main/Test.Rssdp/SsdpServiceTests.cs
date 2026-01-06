using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRssdp
{
	[TestClass]
	public class SsdpServiceTests
	{

		#region Constructor Tests

		[TestMethod]
		public void SsdpService_ConstructorThrowsArgumentNullIfXmlStringNull()
		{
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				var device = new SsdpService(null);
			});
		}

		[TestMethod]
		public void SsdpService_ConstructorThrowsArgumentNullIfXmlStringEmpty()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				var device = new SsdpService(String.Empty);
			});	
		}

		[TestMethod]
		public void SsdpService_ConstructsOkWithDefaultConstructor()
		{
			_ = new SsdpService();
		}

		#endregion

		#region ServiceTypeNamespace Tests

		[TestMethod]
		public void SsdpService_NullServiceTypeNamespaceReturnsNull()
		{
			var service = new SsdpService
			{
				ServiceTypeNamespace = null
			};
			Assert.IsNull(service.ServiceTypeNamespace);
		}

		[TestMethod]
		public void SsdpService_EmptyDeviceTypeNamespaceReturnsEmpty()
		{
			var service = new SsdpService
			{
				ServiceTypeNamespace = String.Empty
			};
			Assert.AreEqual(String.Empty, service.ServiceTypeNamespace);
		}

		#endregion

		#region ServiceType Tests

		[TestMethod]
		public void SsdpService_NullServiceTypeReturnsNull()
		{
			var service = new SsdpService
			{
				ServiceType = null
			};
			Assert.IsNull(service.ServiceType);
		}

		[TestMethod]
		public void SsdpService_EmptyServiceTypeReturnsEmpty()
		{
			var service = new SsdpService
			{
				ServiceType = String.Empty
			};
			Assert.AreEqual(String.Empty, service.ServiceType);
		}

		[TestMethod]
		public void SsdpService_FullServiceTypesReturnsStringWithNullValues()
		{
			var service = new SsdpService
			{
				ServiceType = null,
				ServiceTypeNamespace = null
			};
			Assert.AreEqual("urn::service::1", service.FullServiceType);
		}

		[TestMethod]
		public void SsdpService_FullServiceTypesReturnsExpectedString()
		{
			var service = new SsdpService
			{
				ServiceType = "testservicetype",
				ServiceTypeNamespace = "my-test-namespace"
			};
			Assert.AreEqual("urn:my-test-namespace:service:testservicetype:1", service.FullServiceType);
		}

		[TestMethod]
		public void SsdpService_FullServiceTypesReplacesDotsInVendorNamespace()
		{
			var service = new SsdpService
			{
				ServiceType = "testservicetype",
				ServiceTypeNamespace = "my.test.namespace.org"
			};
			Assert.AreEqual("urn:my-test-namespace-org:service:testservicetype:1", service.FullServiceType);
		}

		#endregion

		#region Device Document Deserialisation Tests

		private const string ServiceXmlDescription = @"<service>
        <serviceType>urn:schemas-upnp-org:service:SystemProperties:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:SystemProperties</serviceId>
        <controlURL>/SystemProperties/Control</controlURL>
        <eventSubURL>/SystemProperties/Event</eventSubURL>
        <SCPDURL>/xml/SystemProperties1.xml</SCPDURL>
				<CustomProperty1>test value</CustomProperty1>
      </service>    
";

		[TestMethod]
		public void SsdpDevice_Constructor_DeserialisesComplexDeviceDocumentAndSkipsComplexCustomProperties()
		{
			var service = new SsdpService(ServiceXmlDescription);
			Assert.AreEqual("urn:schemas-upnp-org:service:SystemProperties:1", service.FullServiceType);
			Assert.AreEqual("urn:upnp-org:serviceId:SystemProperties", service.ServiceId);
			Assert.AreEqual(new Uri("/SystemProperties/Control", UriKind.Relative), service.ControlUrl);
			Assert.AreEqual(new Uri("/SystemProperties/Event", UriKind.Relative), service.EventSubUrl);
			Assert.AreEqual(new Uri("/xml/SystemProperties1.xml", UriKind.Relative), service.ScpdUrl);
		}

		#endregion

	}
}