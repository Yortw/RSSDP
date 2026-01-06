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
	public class SsdpDevicePropertiesCollectionTests
	{

		#region Constructor Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_CapacityConstructor_Succeeds()
		{
			_ = new SsdpDevicePropertiesCollection(10);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_CapacityConstructor_SucceedsWithZeroValue()
		{
			_ = new SsdpDevicePropertiesCollection(0);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void SsdpDevicePropertiesCollection_CapacityConstructor_FailsWithNegativeValue()
		{
			_ = new SsdpDevicePropertiesCollection(-1);
		}

		#endregion

		#region Add Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void SsdpDevicePropertiesCollection_Add_NullThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			properties.Add(null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Add_EmptyFullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);

			properties.Add(p);
		}

		#endregion

		#region Remove Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void SsdpDevicePropertiesCollection_Remove_NullThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			properties.Remove((SsdpDeviceProperty)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Remove_NullKeyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			properties.Remove((string)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Remove_EmptyKeyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			properties.Remove(String.Empty);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Remove_EmptyFullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);
			properties.Remove(p);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveInstanceSucceeds()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);

			properties.Add(p);

			Assert.AreEqual(true, properties.Remove(p));
			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveInstanceForDifferentInstanceWithSameKeyReturnsFalse()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);
			var p2 = new SsdpDeviceProperty("TestNamespace", "TestProp1");

			properties.Add(p);

			Assert.AreEqual(false, properties.Remove(p2));
			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveByKeySucceeds()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);

			properties.Add(p);

			Assert.AreEqual(true, properties.Remove(p.FullName));
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

		#region Contains Tests

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Contains_NullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			properties.Contains((string)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Contains_EmptyNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			properties.Contains(String.Empty);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void SsdpDevicePropertiesCollection_Contains_NullPropertyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			properties.Contains((SsdpDeviceProperty)null);
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void SsdpDevicePropertiesCollection_Contains_PropertyWithEmptyNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);

			properties.Contains(p);
		}
		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsTrueForExistingKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.AreEqual(true, properties.Contains(prop.FullName));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForNonExistentKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains("NotAValidKey"));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsTrueForExistingItem()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.AreEqual(true, properties.Contains(prop));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForExistingKeyDifferentItem()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			var prop2 = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains(prop2));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForNonExistentProperty()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");
			var prop2 = new SsdpDeviceProperty("MyNamespace", "TestProperty1", "1.0");

			properties.Add(prop);

			Assert.AreEqual(false, properties.Contains(prop2));
		}

		#endregion

		#region GetEnumerator Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_GenericGetEnumerator_Success()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);
			var enumerator = properties.GetEnumerator();

			Assert.AreEqual(true, enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.AreEqual(false, enumerator.MoveNext());
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_GetEnumerator_Success()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);
			var enumerator = ((IEnumerable)properties).GetEnumerator();

			Assert.AreEqual(true, enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.AreEqual(false, enumerator.MoveNext());
		}

		#endregion

		#region Indexer Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Indexer_Succeeds()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "Test", "some value");
			properties.Add(p);

			Assert.AreEqual(p, properties[p.FullName]);
		}

		[ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
		[TestMethod]
		public void SsdpDevicePropertiesCollection_Indexer_ThrowsOnUnknownKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "Test", "some value");

			properties.Add(p);

			Assert.AreEqual(p, properties["NotAValidKey"]);
		}

		#endregion

		#region Count Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Count_ReturnsZeroForNewCollection()
		{
			var properties = new SsdpDevicePropertiesCollection();

			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Count_ReturnsOneAfterItemAdded()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Count_ReturnsZeroAfterLastItemRemoved()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Remove(prop);
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

	}
}