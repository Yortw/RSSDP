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
			var prop = new SsdpDeviceProperty("TestNamespace", "TestPropName", "TestValue");

			Assert.AreEqual("TestNamespace:TestPropName", prop.FullName);
		}

		[TestMethod]
		public void DeviceProperty_FullNameIsCorrectWithNoNamespaceSpecified()
		{
			var prop = new SsdpDeviceProperty(string.Empty, "TestPropName", "TestValue");

			Assert.AreEqual("TestPropName", prop.FullName);
		}

	}
}
