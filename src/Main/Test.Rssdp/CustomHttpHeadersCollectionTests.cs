using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rssdp;

namespace TestRssdp
{
	[TestClass]
	public class CustomHttpHeadersCollectionTests
	{

		#region Constructor Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_CapacityConstructor_Succeeds()
		{
			_ = new CustomHttpHeadersCollection(10);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_CapacityConstructor_SucceedsWithZeroValue()
		{
			_ = new CustomHttpHeadersCollection(0);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_CapacityConstructor_FailsWithNegativeValue()
		{
			Assert.Throws<System.ArgumentOutOfRangeException>(() =>
			{
				_ = new CustomHttpHeadersCollection(-1);
			});
		}

		#endregion

		#region Add Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_Add_NullThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentNullException>(() =>
			{
				properties.Add((CustomHttpHeader)null);
			});
		}

		#endregion

		#region Remove Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_NullThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentNullException>(() =>
			{
				properties.Remove((CustomHttpHeader)null);
			});
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_NullKeyThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Remove((string)null);
			});
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_EmptyKeyThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Remove(String.Empty);
			});	
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveInstanceSucceeds()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.IsTrue(properties.Remove(p));
			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveInstanceForDifferentInstanceWithSameKeyReturnsFalse()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			var p2 = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.IsFalse(properties.Remove(p2));
			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Remove_RemoveByKeySucceeds()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestProp1", "Test Value");

			properties.Add(p);

			Assert.IsTrue(properties.Remove(p.Name));
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

		#region Contains Tests

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_NullNameThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Contains((string)null);
			});	
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_EmptyNameThrows()
		{
			var properties = new CustomHttpHeadersCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Contains(String.Empty);
			});
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_NullPropertyThrows()
		{
			var properties = new CustomHttpHeadersCollection();
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				_ = properties.Contains((CustomHttpHeader)null);
			});
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsTrueForExistingKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.IsTrue(properties.Contains(prop.Name));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForNonExistentKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains("NotAValidKey"));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsTrueForExistingItem()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.IsTrue(properties.Contains(prop));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForExistingKeyDifferentItem()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			var prop2 = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains(prop2));
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_Contains_ReturnsFalseForNonExistentProperty()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			var prop2 = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains(prop2));
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

			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[TestMethod]
		public void CustomHttpHeadersCollection_GetEnumerator_Success()
		{
			var properties = new CustomHttpHeadersCollection();
			var prop = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(prop);
			var enumerator = ((IEnumerable)properties).GetEnumerator();

			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.IsFalse(enumerator.MoveNext());
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

		[TestMethod]
		public void CustomHttpHeadersCollection_Indexer_ThrowsOnUnknownKey()
		{
			var properties = new CustomHttpHeadersCollection();
			var p = new CustomHttpHeader("TestHeader", "Test Value");

			properties.Add(p);

			Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
			{
				_ = properties["NotAValidKey"];
			});
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