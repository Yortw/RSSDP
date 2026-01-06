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
			Assert.IsNull(header.Value);
			Assert.AreEqual("X-TestName: ", header.ToString());
		}

		[TestMethod()]
		public void CustomHttpHeader_ThrowsOnNullName()
		{
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				_ = new CustomHttpHeader(null, "Test Value");
			});
		}

		[TestMethod()]
		public void CustomHttpHeader_ThrowsOnEmptyName()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				_ = new CustomHttpHeader(String.Empty, "Test Value");
			});	
		}

		[TestMethod()]
		public void CustomHttpHeader_ThrowsOnValueWithLineFeed()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				_ = new CustomHttpHeader("X-TestName", "Test\nValue");
			});
		}

		[TestMethod()]
		public void CustomHttpHeader_ThrowsOnValueWithCarriageReturnFeed()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				_ = new CustomHttpHeader("X-TestName", "Test\r\nValue");
			});	
		}

		[TestMethod()]
		public void CustomHttpHeader_ThrowsOnNameWithColonFeed()
		{
			Assert.Throws<System.ArgumentException>(() =>
			{
				_ = new CustomHttpHeader("X:TestName", "Test Value");
			});	
		}

	}
}
