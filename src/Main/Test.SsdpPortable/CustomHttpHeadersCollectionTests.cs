using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace Test.RssdpPortable
{
	[TestClass]
	public class CustomHttpHeadersCollectionTests
	{

		#region Constructor Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_CapacityConstructor_Succeeds()
		{
			var properties = new CustomHttpHeadersCollection(10);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_CapacityConstructor_SucceedsWithZeroValue()
		{
			var properties = new CustomHttpHeadersCollection(0);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void CustomHttpHeadersCollection_CapacityConstructor_FailsWithNegativeValue()
		{
			var properties = new CustomHttpHeadersCollection(-1);
		}

		#endregion

		#region Add Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void CustomHttpHeadersCollection_Add_NullThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			properties.Add(null);
		}

		#endregion

		#region Remove Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void CustomHttpHeadersCollection_Remove_NullThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			properties.Remove((CustomHttpHeader)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeadersCollection_Remove_NullKeyThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			properties.Remove((string)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeadersCollection_Remove_EmptyKeyThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			properties.Remove(String.Empty);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveInstanceSucceeds()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.AreEqual(true, properties.Remove(p));
			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveInstanceForDifferentInstanceWithSameKeyReturnsFalse()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			var p2 = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.AreEqual(false, properties.Remove(p2));
			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveByKeySucceeds()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.AreEqual(true, properties.Remove(p.Name));
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

		#region Contains Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeadersCollection_Contains_NullNameThrows()
		{
			var properties = new CustomHttpHeadersCollection();
			properties.Contains((string)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void CustomHttpHeadersCollection_Contains_EmptyNameThrows()
		{
			var properties = new CustomHttpHeadersCollection();
			properties.Contains(String.Empty);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void CustomHttpHeadersCollection_Contains_NullPropertyThrows()
		{
			var properties = new CustomHttpHeadersCollection();
			properties.Contains((CustomHttpHeader)null);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsTrueForExistingKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(true, properties.Contains(prop.Name));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForNonExistentKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains("NotAValidKey"));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsTrueForExistingItem()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(true, properties.Contains(prop));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForExistingKeyDifferentItem()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			var prop2 = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains(prop2));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForNonExistentProperty()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			var prop2 = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains(prop2));
		}

		#endregion

		#region GetEnumerator Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_GenericGetEnumerator_Success()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);
			var enumerator = properties.GetEnumerator();

			Assert.AreEqual(true, enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.AreEqual(false, enumerator.MoveNext());
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_GetEnumerator_Success()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);
			var enumerator = ((IEnumerable)properties).GetEnumerator();

			Assert.AreEqual(true, enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.AreEqual(false, enumerator.MoveNext());
		}

		#endregion

		#region Indexer Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_Indexer_Succeeds()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(p);

			Assert.AreEqual(p, properties[p.Name]);
		}

		[ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
		[TestMethod]
		public void CustomHttpHeadersCollection_Indexer_ThrowsOnUnknownKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(p);

			Assert.AreEqual(p, properties["NotAValidKey"]);
		}

		#endregion

		#region Count Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_Count_ReturnsZeroForNewCollection()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Count_ReturnsOneAfterItemAdded()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Count_ReturnsZeroAfterLastItemRemoved()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Remove(prop);
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

	}
}