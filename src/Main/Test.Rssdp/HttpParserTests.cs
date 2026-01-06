
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp.Infrastructure;

namespace TestRssdp
{
	[TestClass]
	public class HttpParserTests
	{

		#region Response Parser Tests

		#region Argument Checking

		[TestMethod]
		public void HttpResponseParser_ThrowsOnNull()
		{
			var parser = new HttpResponseParser();
			Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
		}

		[TestMethod]
		public void HttpResponseParser_ThrowsOnEmpty()
		{
			var parser = new HttpResponseParser();
			Assert.Throws<ArgumentException>(() => parser.Parse(String.Empty));
		}

		[TestMethod]
		public void HttpResponseParser_ThrowsOnMisingCrLf()
		{
			var parser = new HttpRequestParser();
			Assert.Throws<ArgumentException>(() => parser.Parse("HTTP/1.1 200 OK"));
		}

		[TestMethod]
		public void HttpResponseParser_ThrowsOnInvalidHeader()
		{
			var parser = new HttpResponseParser();
			Assert.Throws<ArgumentException>(() => parser.Parse("HTTP1.1 200 OK" + Environment.NewLine));
		}

		#endregion

		#endregion

		#region Request Parser Tests

		#region Argument Checking

		[TestMethod]
		public void HttpRequestParser_ThrowsOnNull()
		{
			var parser = new HttpRequestParser();
			Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
		}

		[TestMethod]
		public void HttpRequestParser_ThrowsOnEmpty()
		{
			var parser = new HttpRequestParser();
			Assert.Throws<ArgumentException>(() => parser.Parse(String.Empty));
		}

		[TestMethod]
		public void HttpRequestParser_ThrowsOnMisingCrLf()
		{
			var parser = new HttpRequestParser();
			Assert.Throws<ArgumentException>(() => parser.Parse("GET * HTTP/1.1"));
		}

		[TestMethod]
		public void HttpRequestParser_ParsesMultilineHeaders()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: {2}
	ssdp:all

",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(2, message.Headers.GetValues("ST").Count());
		}

		[TestMethod]
		public void HttpRequestParser_ParsesMultilineHeadersWithLFLineTermination()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format("M-SEARCH * HTTP/1.1\nHOST: {0}:{1}\nMAN: \"ssdp:discover\"\nMX: 1\nST: {2}\n\tssdp:all\n",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(2, message.Headers.GetValues("ST").Count());
		}

		[TestMethod]
		public void HttpRequestParser_ParsesHeaderWithQuotedValues()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: ""{2}"", ""ssdp:all""

",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(2, message.Headers.GetValues("ST").Count());
			Assert.AreEqual(SsdpConstants.UpnpDeviceTypeRootDevice, message.Headers.GetValues("ST").First());
			Assert.AreEqual("ssdp:all", message.Headers.GetValues("ST").Last());
		}

		[TestMethod]
		public void HttpRequestParser_ParsesHeaderWithQuotedSeparatorValue()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: ""test, quoted comma""

",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(1, message.Headers.GetValues("ST").Count());
			Assert.AreEqual("test, quoted comma", message.Headers.GetValues("ST").First());
		}

		[TestMethod]
		public void HttpRequestParser_ParsesHeaderWithQuotedEmptyValue()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: """"

",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(1, message.Headers.GetValues("ST").Count());
			Assert.AreEqual(String.Empty, message.Headers.GetValues("ST").First());
		}

		[TestMethod]
		public void HttpRequestParser_ParsesMultivalueHeaderWithQuotedEmptyValue()
		{
			var parser = new HttpRequestParser();
			var message = parser.Parse(String.Format(@"M-SEARCH * HTTP/1.1
HOST: {0}:{1}
MAN: ""ssdp:discover""
MX: 1
ST: ""test1"", """"

",
 SsdpConstants.MulticastLocalAdminAddress,
 SsdpConstants.MulticastPort,
 SsdpConstants.UpnpDeviceTypeRootDevice));

			Assert.AreEqual(2, message.Headers.GetValues("ST").Count());
			Assert.AreEqual(String.Empty, message.Headers.GetValues("ST").Last());
		}

		#endregion

		#endregion

	}
}