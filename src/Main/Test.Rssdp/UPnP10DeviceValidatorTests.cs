using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using Rssdp.Infrastructure;

namespace TestRssdp
{
	[TestClass]
	public class UPnP10DeviceValidatorTests
	{

		#region Root Device Validations

		[TestMethod]
		public void UPnP10DeviceValidator_ThrowsOnNullRootDevice()
		{
			SsdpRootDevice testDevice = null;

			var validator = new Upnp10DeviceValidator();
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				_ = validator.GetValidationErrors(testDevice);
			});	
		
		}

		[TestMethod]
		public void UPnP10DeviceValidator_PassesCorrectRootDevice()
		{
			var testDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};
			
			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_RootDeviceRequiresLocation()
		{
			var testDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("location", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_RootDeviceLocationMustBeAbsolute()
		{
			var testDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("/wherewouldthisbe", UriKind.Relative)
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("location", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Device Validations

		[TestMethod]
		public void UPnP10DeviceValidator_ThrowsOnNullDevice()
		{
			SsdpDevice testDevice = null;

			var validator = new Upnp10DeviceValidator();
			
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				_ = validator.GetValidationErrors(testDevice);
			});	
		}

		[TestMethod]
		public void UPnP10DeviceValidator_PassesCorrectEmbeddedDevice()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		#region Upc Code Tests

		[TestMethod]
		public void UPnP10DeviceValidator_UpcCodeIsOptional()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Upc = null
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNonNumericUpcCode()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Upc = "ABCDEFGHIJKL"
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("UPC", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsUpcCodeLessThan12Chars()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Upc = "12345"
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("UPC", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsUpcCodeMoreThan12Chars()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Upc = "1234567890123"
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("UPC", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Uuid Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullUuid()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = null
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("uuid", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyUuid()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = String.Empty
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("uuid", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Udn Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsUdnNotStartingWithPrefix()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Udn = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("udn", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsUdnNotUsingUuid()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Udn = "uuid: someothervalue"
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("uuid", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_IgnoresWhiteSpaceInUdn()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var uuid = System.Guid.NewGuid().ToString();
			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = uuid,
				Udn = "uuid:   " + uuid
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		#endregion

		#region Device Type Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullDeviceType()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceType = null,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceType", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyDeviceType()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceType = String.Empty,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceType", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Device Type Namespace Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsDeviceVersionZero()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceVersion = 0,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceVersion", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsDeviceVersionLessThanZero()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceVersion = -1,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceVersion", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Device Type Namespace Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsDeviceTypeNamespaceWithPeriods()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice.org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceTypeNamespace", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsDeviceTypeNamespaceOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = new String('A', 65),
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceTypeNamespace", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullDeviceTypeNamespace()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceTypeNamespace = null,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceTypeNamespace", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyDeviceTypeNamespace()
		{
			var rootDevice = new SsdpRootDevice()
			{
				DeviceTypeNamespace = String.Empty,
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("DeviceTypeNamespace", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region FriendlyName Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullFriendlyName()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = null,
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("FriendlyName", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyFriendlyName()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = String.Empty,
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("FriendlyName", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FriendlyNameOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Test Root Device",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = new string('A', 65),
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("FriendlyName", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Manufacturer Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullManufacturer()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = null,
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("Manufacturer", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyManufacturer()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = String.Empty,
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("Manufacturer", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsManufacturerOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = new string('A', 65),
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("Manufacturer", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region ModelName Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNullModelName()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = null,
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("ModelName", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsEmptyModelName()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manuafacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = String.Empty,
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("ModelName", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsModelNameOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manuafacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = new string('A', 33),
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("ModelName", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Model Number Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsModelNumberOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = new string('A', 33),
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manuafacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = String.Empty,
				ModelNumber = new string('A', 33),
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("ModelNumber", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Serial Number Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsSerialNumberOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = new string('A', 33),
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manuafacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = String.Empty,
				ModelNumber = "Test Model", 
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = new string('A', 65),
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("SerialNumber", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Serial Number Tests

		[TestMethod]
		public void UPnP10DeviceValidator_FailsModelDescriptionOverMaxLength()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = new string('A', 33),
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manuafacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = new string('A', 129),
				ModelName = String.Empty,
				ModelNumber = "Test Model",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-1234",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("ModelDescription", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Icon Validations

		[TestMethod]
		public void UPnP10DeviceValidator_PassesValidIcon()
		{
			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = 48,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsIconNullMimeType()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = 48,
				MimeType = null,
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("mime type", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsIconEmptyMimeType()
		{
			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = 48,
				MimeType = String.Empty,
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("mime type", StringComparison.OrdinalIgnoreCase));			
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsIconNullUri()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = 48,
				MimeType = "image/png",
				Url = null
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("url", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsZeroColorDepth()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 0,
				Width = 48,
				Height = 48,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("colordepth", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNegativeColorDepth()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = -1,
				Width = 48,
				Height = 48,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("colordepth", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsZeroWidth()
		{
			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 0,
				Height = 48,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("width", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNegativeWidth()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = -1,
				Height = 48,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("width", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsZeroHeight()
		{
			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = 0,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("width", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsNegativeHeight()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var icon = new SsdpDeviceIcon()
			{
				ColorDepth = 32,
				Width = 48,
				Height = -1,
				MimeType = "image/png",
				Url = new Uri("someimage.png", UriKind.Relative)
			};
			testDevice.Icons.Add(icon);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("width", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		[TestMethod]
		public void UPnP10DeviceValidator_ValidatesNestedChildDevices()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml"),
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				DeviceTypeNamespace = "testdevice-org",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString()
			};
			rootDevice.AddDevice(testDevice);

			var testDevice2 = new SsdpEmbeddedDevice();
			testDevice.AddDevice(testDevice2);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(testDevice);
			Assert.IsNotNull(results);
			Assert.AreNotEqual(0, results.Count());
			Assert.IsGreaterThanOrEqualTo(0, results.First().IndexOf("Embedded Device", StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Service Validations

		[TestMethod]
		public void UPnP10DeviceValidator_PassesCorrectServiceDevice()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ControlUrl = new Uri("http://192.168.1.1/control"),
				ScpdUrl = new Uri("http://192.168.1.1/scpd"),
				ServiceType = "TestServiceType",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(0, results.Count());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsOnMissingServiceId()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				ControlUrl = new Uri("http://192.168.1.1/control"),
				ScpdUrl = new Uri("http://192.168.1.1/scpd"),
				ServiceType = "TestServiceType",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual("ServiceId is missing", results.First());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsOnMissingServiceType()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ControlUrl = new Uri("http://192.168.1.1/control"),
				ScpdUrl = new Uri("http://192.168.1.1/scpd"),
				ServiceType = null,
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual("ServiceType is missing", results.First());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsOnServiceTypeWithHash()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ControlUrl = new Uri("http://192.168.1.1/control"),
				ScpdUrl = new Uri("http://192.168.1.1/scpd"),
				ServiceType = "My#TestServiceType",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual("ServiceType cannot contain #", results.First());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsOnMissingScpdUrl()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ControlUrl = new Uri("http://192.168.1.1/control"),
				ServiceType = "TestServiceType",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual("ScpdUrl is missing", results.First());
		}

		[TestMethod]
		public void UPnP10DeviceValidator_FailsOnMissingControlUrl()
		{
			var rootDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var testDevice = new SsdpEmbeddedDevice()
			{
				DeviceType = "TestEmbeddedDevice",
				FriendlyName = "Embedded Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
			};
			rootDevice.AddDevice(testDevice);

			var service = new SsdpService()
			{
				Uuid = System.Guid.NewGuid().ToString(),
				ScpdUrl = new Uri("http://192.168.1.1/scpd"),
				ServiceType = "TestServiceType",
				ServiceTypeNamespace = "my-test-namespace",
				ServiceVersion = 1
			};
			rootDevice.AddService(service);

			var validator = new Upnp10DeviceValidator();
			var results = validator.GetValidationErrors(rootDevice);
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual("ControlUrl is missing", results.First());
		}

		#endregion

		#region ThrowIfInvalidTests

		[TestMethod]
		public void UPnP10DeviceValidator_ThrowIfInvalidThrowsOnNullRootDevice()
		{
			SsdpRootDevice testDevice = null;

			var validator = new Upnp10DeviceValidator();

			Assert.Throws<System.ArgumentNullException>(() =>
			{
				validator.ThrowIfDeviceInvalid(testDevice);
			});
		}

		[TestMethod]
		public void UPnP10DeviceValidator_ThrowIfInvalidPassesCorrectRootDevice()
		{
			var testDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = System.Guid.NewGuid().ToString(),
				Location = new Uri("http://testdevice:1700/xml")
			};

			var validator = new Upnp10DeviceValidator();
			validator.ThrowIfDeviceInvalid(testDevice);
		}
		
		[TestMethod]
		public void UPnP10DeviceValidator_ThrowIfInvalidThrowsOnValidationError()
		{
			var testDevice = new SsdpRootDevice()
			{
				FriendlyName = "Basic Device 1",
				Manufacturer = "Test Manufacturer",
				ManufacturerUrl = new Uri("http://testmanufacturer.com"),
				ModelDescription = "A test model device",
				ModelName = "Test Model",
				ModelNumber = "Model #1234",
				ModelUrl = new Uri("http://modelurl.com"),
				SerialNumber = "SN-123",
				Uuid = null,
				Location = new Uri("http://testdevice:1700/xml")
			};

			var validator = new Upnp10DeviceValidator();
			Assert.Throws<System.InvalidOperationException>(() =>
			{
				validator.ThrowIfDeviceInvalid(testDevice);
			});		
		}

		#endregion

	}
}