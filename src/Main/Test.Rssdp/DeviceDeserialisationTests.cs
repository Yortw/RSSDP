using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace TestRssdp
{
	[TestClass]
	public class DeviceDeserialisationTests
	{

		private const string UpnpDeviceXmlNamespace = "urn:schemas-upnp-org:device-1-0";

		[TestMethod]
		public void ToDescriptionDocument_DeserialisesChildlessRootDevice()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, descriptionDocument);

			AssertDevicesAreSame(rootDevice, deserialisedDevice);

			Assert.AreEqual(descriptionDocument, deserialisedDevice.ToDescriptionDocument());
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialisesEmbeddedDevices()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, descriptionDocument);

			AssertDevicesAreSame(rootDevice.Devices.First(), deserialisedDevice.Devices.First());
			AssertDevicesAreSame(rootDevice.Devices.Last(), deserialisedDevice.Devices.Last());

			Assert.AreEqual(descriptionDocument, deserialisedDevice.ToDescriptionDocument());
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseServiceList()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var service = new SsdpService()
			{
				ControlUrl = new Uri("/test/control", UriKind.Relative),
				EventSubUrl = new Uri("/test/events", UriKind.Relative),
				ScpdUrl = new Uri("/test", UriKind.Relative),
				ServiceType = "mytestservicetype",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1,
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddService(service);
			var service2 = new SsdpService()
			{
				ControlUrl = new Uri("/test/control", UriKind.Relative),
				EventSubUrl = new Uri("/test/events", UriKind.Relative),
				ScpdUrl = new Uri("/test", UriKind.Relative),
				ServiceType = "mytestservicetype",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1,
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddService(service2);

			var service3 = new SsdpService()
			{
				ControlUrl = new Uri("/test/control", UriKind.Relative),
				EventSubUrl = new Uri("/test/events", UriKind.Relative),
				ScpdUrl = new Uri("/test", UriKind.Relative),
				ServiceType = "mytestservicetype",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1,
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.Devices.First().AddService(service3);

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());

			AssertDevicesAreSame(rootDevice.Devices.First(), deserialisedDevice.Devices.First());
			AssertDevicesAreSame(rootDevice.Devices.Last(), deserialisedDevice.Devices.Last());
			Assert.AreEqual(2, deserialisedDevice.Services.Count());
			Assert.AreEqual(1, deserialisedDevice.Devices.First().Services.Count());
			AssertServicesAreSame(service, deserialisedDevice.Services.First());
			AssertServicesAreSame(service2, deserialisedDevice.Services.Last());
			AssertServicesAreSame(service3, deserialisedDevice.Devices.First().Services.First());

			Assert.AreEqual(descriptionDocument, deserialisedDevice.ToDescriptionDocument());
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseEmptyDeviceTypeWithoutError()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);
			var deviceTypeNode = doc.Descendants(XName.Get("deviceType", UpnpDeviceXmlNamespace)).First();
			deviceTypeNode.Value = String.Empty;
			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());

			Assert.AreEqual(String.Empty, deserialisedDevice.DeviceType);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseDeviceTypeWithNoSeparatorWithoutError()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);
			var deviceTypeNode = doc.Descendants(XName.Get("deviceType", UpnpDeviceXmlNamespace)).First();
			deviceTypeNode.Value = "invaliddevicetype";
			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());

			Assert.AreEqual("invaliddevicetype", deserialisedDevice.DeviceType);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseDeviceTypeWithInvalidVersionWithoutError()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);
			var deviceTypeNode = doc.Descendants(XName.Get("deviceType", UpnpDeviceXmlNamespace)).First();
			deviceTypeNode.Value = "urn:custom-devicetype-ns:device:customdevicetype:A";
			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());

			Assert.AreEqual("urn:custom-devicetype-ns:device:customdevicetype:A", deserialisedDevice.DeviceType);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialisesUrlBase()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.Udn = "testudn";

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, descriptionDocument);

			Assert.AreEqual(rootDevice.Udn, deserialisedDevice.Udn);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseInvalidUdn()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.UrlBase = new Uri("http://testdevice:1700/baseurl");

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, descriptionDocument);

			Assert.AreEqual(rootDevice.UrlBase, deserialisedDevice.UrlBase);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialiseInvalidDeviceType()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);
			var deviceTypeNode = doc.Descendants(XName.Get("deviceType", UpnpDeviceXmlNamespace)).First();
			deviceTypeNode.Value = "invalid:devicetype";

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());

			Assert.AreEqual("invalid:devicetype", deserialisedDevice.DeviceType);
		}

		[TestMethod]
		public void ToDescriptionDocument_DeserialisesEmptyUrlNodesSuccessfully()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			var doc = XDocument.Parse(descriptionDocument);
			var modelUrlNode = doc.Descendants(XName.Get("modelURL", UpnpDeviceXmlNamespace)).First();
			modelUrlNode.Value = String.Empty;

			var deserialisedDevice = new SsdpRootDevice(rootDevice.Location, rootDevice.CacheLifetime, doc.ToString());
			Assert.AreEqual(null, deserialisedDevice.ModelUrl);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeserialisationConstructor_ThrowsOnNullDocument()
		{
			_ = new SsdpEmbeddedDevice(null, new System.Xml.XmlReaderSettings());
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void DeserialisationConstructor_ThrowsOnEmptyDocument()
		{
			_ = new SsdpEmbeddedDevice(String.Empty, new System.Xml.XmlReaderSettings());
		}

		[ExpectedException(typeof(System.ArgumentException))]
		[TestMethod]
		public void RootDeviceDeserialisationConstructor_ThrowsOnEmptyDocument()
		{
			_ = new SsdpRootDevice(new Uri("http://somedevice:1700"), TimeSpan.FromMinutes(30), String.Empty);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void RootDeviceDeserialisationConstructor_ThrowsOnNullDocument()
		{
			_ = new SsdpRootDevice(new Uri("http://somedevice:1700"), TimeSpan.FromMinutes(30), null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void RootDeviceDeserialisationConstructor_ThrowsOnLocation()
		{
			_ = new SsdpRootDevice(null, TimeSpan.FromMinutes(30), "<root />");
		}

		private void AssertDevicesAreSame(SsdpRootDevice originalDevice, SsdpRootDevice deserialisedDevice)
		{
			Assert.AreEqual(originalDevice.CacheLifetime, deserialisedDevice.CacheLifetime);
			Assert.AreEqual(originalDevice.Location, deserialisedDevice.Location);
			Assert.AreEqual(originalDevice.UrlBase, deserialisedDevice.UrlBase);

			AssertDevicesAreSame((SsdpDevice)originalDevice, (SsdpDevice)deserialisedDevice);
		}

		private static void AssertDevicesAreSame(SsdpDevice originalDevice, SsdpDevice deserialisedDevice)
		{
			Assert.AreEqual(originalDevice.DeviceType, deserialisedDevice.DeviceType);
			Assert.AreEqual(originalDevice.DeviceTypeNamespace, deserialisedDevice.DeviceTypeNamespace);
			Assert.AreEqual(originalDevice.DeviceVersion, deserialisedDevice.DeviceVersion);
			Assert.AreEqual(originalDevice.FriendlyName, deserialisedDevice.FriendlyName);
			Assert.AreEqual(originalDevice.FullDeviceType, deserialisedDevice.FullDeviceType);
			Assert.AreEqual(originalDevice.Manufacturer, deserialisedDevice.Manufacturer);
			Assert.AreEqual(originalDevice.ManufacturerUrl, deserialisedDevice.ManufacturerUrl);
			Assert.AreEqual(originalDevice.ModelDescription, deserialisedDevice.ModelDescription);
			Assert.AreEqual(originalDevice.ModelName, deserialisedDevice.ModelName);
			Assert.AreEqual(originalDevice.ModelNumber, deserialisedDevice.ModelNumber);
			Assert.AreEqual(originalDevice.ModelUrl, deserialisedDevice.ModelUrl);
			Assert.AreEqual(originalDevice.PresentationUrl, deserialisedDevice.PresentationUrl);
			Assert.AreEqual(originalDevice.SerialNumber, deserialisedDevice.SerialNumber);
			Assert.AreEqual(originalDevice.Udn, deserialisedDevice.Udn);
			Assert.AreEqual(originalDevice.Upc, deserialisedDevice.Upc);
			Assert.AreEqual(originalDevice.Uuid, deserialisedDevice.Uuid);
		}


		private void AssertServicesAreSame(SsdpService service, SsdpService service2)
		{
			Assert.AreEqual(service.ControlUrl, service2.ControlUrl);
			Assert.AreEqual(service.EventSubUrl, service2.EventSubUrl);
			Assert.AreEqual(service.FullServiceType, service2.FullServiceType);
			Assert.AreEqual(service.ScpdUrl, service2.ScpdUrl);
			Assert.AreEqual(service.ServiceId, service2.ServiceId);
			Assert.AreEqual(service.ServiceType, service2.ServiceType);
			Assert.AreEqual(service.ServiceTypeNamespace, service2.ServiceTypeNamespace);
			Assert.AreEqual(service.ServiceVersion, service2.ServiceVersion);
			Assert.AreEqual(service.Uuid, service2.Uuid);
		}

		private SsdpRootDevice CreateSampleRootDevice()
		{
			var retVal = new SsdpRootDevice()
			{
				CacheLifetime = TimeSpan.FromMinutes(30),
				DeviceType = "TestDeviceType",
				DeviceTypeNamespace = "test-device-ns",
				FriendlyName = "Test Device 1",
				Location = new Uri("http://testdevice:1700"),
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testman.com"),
				ModelDescription = "A test device",
				ModelName = "Test Model",
				ModelNumber = "1234",
				ModelUrl = new Uri("http://testmodel.com"),
				PresentationUrl = new Uri("http://testmodel.com/presentation"),
				SerialNumber = "TM-12345",
				Upc = "123456789012",
				UrlBase = new Uri("http://testdevice:1700"),
				Uuid = Guid.NewGuid().ToString()
			};

			var customProp = new SsdpDeviceProperty("custom-ns", "TestProp1", "Test");
			retVal.CustomProperties.Add(customProp);
			customProp = new SsdpDeviceProperty("custom-ns", "TestProp2", "Test");
			retVal.CustomProperties.Add(customProp);

			var icon = new SsdpDeviceIcon() { ColorDepth = 32, Height = 48, Width = 48, MimeType = "image/png", Url = new Uri("icons/48", UriKind.Relative) };
			retVal.Icons.Add(icon);
			icon = new SsdpDeviceIcon() { ColorDepth = 32, Height = 120, Width = 120, MimeType = "image/png", Url = new Uri("icons/120", UriKind.Relative) };
			retVal.Icons.Add(icon);

			return retVal;
		}

		private SsdpEmbeddedDevice CreateEmbeddedDevice(SsdpRootDevice rootDevice)
		{
			var retVal = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDeviceType",
				DeviceTypeNamespace = "test-device-ns",
				FriendlyName = "Test Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testman.com"),
				ModelDescription = "A test embeddeddevice",
				ModelName = "Test Model",
				ModelNumber = "1234",
				ModelUrl = new Uri("http://testmodel.com"),
				PresentationUrl = new Uri("http://testmodel.com/embedded/presentation"),
				SerialNumber = "TM-12345",
				Upc = "123456789012",
				Uuid = Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(retVal);

			return retVal;
		}

	}

}