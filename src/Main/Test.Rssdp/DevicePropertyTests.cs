using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace TestRssdp
{
	[TestClass]
	public class DevicePropertyTests
	{

		[TestMethod]
		public void DeviceProperty_FullNameIsCorrectFormat()
		{
			var prop = new SsdpDeviceProperty();
			prop.Namespace = "TestNamespace";
			prop.Name = "TestPropName";
			prop.Value = "TestValue";

			Assert.AreEqual("TestNamespace:TestPropName", prop.FullName);
		}

		[TestMethod]
		public void DeviceProperty_FullNameIsCorrectWithNoNamespaceSpecified()
		{
			var prop = new SsdpDeviceProperty();
			prop.Name = "TestPropName";
			prop.Value = "TestValue";

			Assert.AreEqual("TestPropName", prop.FullName);
		}

	}
}
