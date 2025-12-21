using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Rssdp
{
	[TestClass]
	public class CustomHttpHeaderTests
	{

		[TestMethod()]
		public void CustomHttpHeader_CanCreateWithValidNameAndValue()
		{
			var header = new CustomHttpHeader("X-TestName", "Test Value");
			Assert.AreEqual("X-TestName", header.Name);
			Assert.AreEqual("Test Value", header.Value);
			Assert.AreEqual("X-TestName: Test Value", header.ToString());
		}

		[TestMethod()]
		public void CustomHttpHeader_CanCreateWithEmptyValue()
		{
			var header = new CustomHttpHeader("X-TestName", String.Empty);
			Assert.AreEqual("X-TestName", header.Name);
			Assert.AreEqual(String.Empty, header.Value);
			Assert.AreEqual("X-TestName: ", header.ToString());
		}

		[TestMethod()]
		public void CustomHttpHeader_CanCreateWithNullValue()
		{
			var header = new CustomHttpHeader("X-TestName", null);
			Assert.AreEqual("X-TestName", header.Name);
			Assert.AreEqual(null, header.Value);
			Assert.AreEqual("X-TestName: ", header.ToString());
		}

		[TestMethod()]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void CustomHttpHeader_ThrowsOnNullName()
		{
			var header = new CustomHttpHeader(null, "Test Value");
		}

		[TestMethod()]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeader_ThrowsOnEmptyName()
		{
			var header = new CustomHttpHeader(String.Empty, "Test Value");
		}

		[TestMethod()]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeader_ThrowsOnValueWithLineFeed()
		{
			var header = new CustomHttpHeader("X-TestName", "Test\nValue");
		}

		[TestMethod()]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeader_ThrowsOnValueWithCarriageReturnFeed()
		{
			var header = new CustomHttpHeader("X-TestName", "Test\rValue");
		}

		[TestMethod()]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeader_ThrowsOnNameWithColonFeed()
		{
			var header = new CustomHttpHeader("X:TestName", "Test Value");
		}

	}
}
