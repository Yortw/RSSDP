using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.RssdpPortable
{
	[TestClass]
	public class DeviceSerialisationTests
	{

		private const string UpnpDeviceXmlNamespace = "urn:schemas-upnp-org:device-1-0";

		[ExpectedException(typeof(System.InvalidOperationException))]
		[TestMethod]
		public void ToDescriptionDocument_ThrowsWithNullUuid()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.Uuid = null;

			var descriptionDocument = rootDevice.ToDescriptionDocument();
		}

		[ExpectedException(typeof(System.InvalidOperationException))]
		[TestMethod]
		public void ToDescriptionDocument_ThrowsWithEmptyUuid()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.Uuid = String.Empty;

			var descriptionDocument = rootDevice.ToDescriptionDocument();
		}

		[TestMethod]
		public void ToDescriptionDocument_CompletesWithNonEmptyResult()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();
			Assert.AreNotEqual(null, descriptionDocument);
			Assert.AreNotEqual(String.Empty, descriptionDocument);
		}

		[TestMethod]
		public void ToDescriptionDocument_StartsWithXmlDeclaration()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			Assert.AreEqual(true, descriptionDocument.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
		}

		[TestMethod]
		public void ToDescriptionDocument_RootNodeHasCorrectXmlns()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			var rootNode = doc.Descendants(XName.Get("root", UpnpDeviceXmlNamespace));
			Assert.IsNotNull(rootNode);
		}

		[TestMethod]
		public void ToDescriptionDocument_ChilldlessRootDeviceContainsSingleDeviceNode()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			var deviceNodes = doc.Descendants(XName.Get("device", UpnpDeviceXmlNamespace));
			Assert.AreEqual(1, deviceNodes.Count());
		}

		[TestMethod]
		public void ToDescriptionDocument_ContainsCorrectNumberOfIcons()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			var deviceNodes = doc.Descendants(XName.Get("icon", UpnpDeviceXmlNamespace));
			Assert.AreEqual(2, deviceNodes.Count());
		}

		[TestMethod]
		public void ToDescriptionDocument_ContainsCorrectCustomProperties()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			var customPropNode = doc.Descendants(XName.Get("TestProp1", UpnpDeviceXmlNamespace)).FirstOrDefault();
			Assert.IsNotNull(customPropNode);

			customPropNode = doc.Descendants(XName.Get("TestProp2", UpnpDeviceXmlNamespace)).FirstOrDefault();
			Assert.IsNotNull(customPropNode);
		}

		[TestMethod]
		public void ToDescriptionDocument_ContainsSpecVersion10()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			var specVersionNodes = doc.Descendants(XName.Get("specVersion", UpnpDeviceXmlNamespace));
			Assert.AreEqual(1, specVersionNodes.Count());

			var specVersionNode = specVersionNodes.Single();
			var majorNode = specVersionNode.Elements(XName.Get("major", UpnpDeviceXmlNamespace)).Single();

			var minorNode = specVersionNode.Elements(XName.Get("minor", UpnpDeviceXmlNamespace)).Single();

			Assert.AreEqual("1", majorNode.Value);
			Assert.AreEqual("0", minorNode.Value);
		}

		[TestMethod]
		public void ToDescriptionDocument_ContainsSingleUrlBase()
		{
			var rootDevice = CreateSampleRootDevice();

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			doc.Descendants(XName.Get("URLBase", UpnpDeviceXmlNamespace)).Single();
		}

		[TestMethod]
		public void ToDescriptionDocument_BlankUrlBaseDoesNotSerialise()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.UrlBase = null;

			var descriptionDocument = rootDevice.ToDescriptionDocument();

			var doc = XDocument.Parse(descriptionDocument);
			Assert.AreEqual(0, doc.Descendants(XName.Get("URLBase", UpnpDeviceXmlNamespace)).Count());
		}

		[TestMethod]
		public void ToDescriptionDocument_ContainsEmbeddedDevices()
		{
			var rootDevice = CreateSampleRootDevice();
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));
			rootDevice.AddDevice(CreateEmbeddedDevice(rootDevice));

			var descriptionDocument = XDocument.Parse(rootDevice.ToDescriptionDocument());
			var deviceNodes = descriptionDocument.Descendants(XName.Get("device", UpnpDeviceXmlNamespace));

			Assert.AreEqual(3, deviceNodes.Count());
			Assert.IsTrue(deviceNodes.Where((n) => n.Elements(XName.Get("UDN", UpnpDeviceXmlNamespace)).First().Value == rootDevice.Udn).Any());
			Assert.IsTrue(deviceNodes.Where((n) => n.Elements(XName.Get("UDN", UpnpDeviceXmlNamespace)).First().Value == rootDevice.Devices.First().Udn).Any());
			Assert.IsTrue(deviceNodes.Where((n) => n.Elements(XName.Get("UDN", UpnpDeviceXmlNamespace)).First().Value == rootDevice.Devices.Last().Udn).Any());
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void ToDescriptionDocument_WriteDeviceDescriptionXml_ThrowsIfWriterNull()
		{
			var device = new MockCustomDevice();
			device.DoInvalidWrite(null, CreateSampleRootDevice());
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void ToDescriptionDocument_WriteDeviceDescriptionXml_ThrowsIfDeviceNull()
		{
			var device = new MockCustomDevice();
			using (var ms = new System.IO.MemoryStream())
			{
				using (var writer = XmlWriter.Create(ms))
				{
					device.DoInvalidWrite(writer, null);
				}
			}
		}

		private SsdpRootDevice CreateSampleRootDevice()
		{
			var retVal = new SsdpRootDevice()
			{
				CacheLifetime = TimeSpan.FromMinutes(30),
				DeviceType = "TestDeviceType",
				DeviceTypeNamespace = "test-device-ns",
				FriendlyName = "Test Device 1",
				Location = new Uri("http://testdevice:1700/xml"),
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

			var customProp = new SsdpDeviceProperty() { Namespace = "custom-ns", Name = "TestProp1", Value = "Test" };
			retVal.CustomProperties.Add(customProp.FullName, customProp);
			customProp = new SsdpDeviceProperty() { Namespace = "custom-ns", Name = "TestProp2", Value = "Test" };
			retVal.CustomProperties.Add(customProp.FullName, customProp);

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

		private class MockCustomDevice : SsdpDevice
		{

			public void DoInvalidWrite(XmlWriter writer, SsdpDevice device)
			{
				this.WriteDeviceDescriptionXml(writer, device);
			}

			protected override void WriteDeviceDescriptionXml(System.Xml.XmlWriter writer, SsdpDevice device)
			{
				base.WriteDeviceDescriptionXml(writer, device);
			}

		}
	}
}