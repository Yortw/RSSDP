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
		public void SsdpDevicePropertiesCollection_CapacityConstructor_FailsWithNegativeValue()
		{
			Assert.Throws<System.ArgumentOutOfRangeException>(() =>
			{
				_ = new SsdpDevicePropertiesCollection(-1);
			});
		}

		#endregion

		#region Add Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Add_NullThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			Assert.Throws<System.ArgumentNullException>(() =>
			{
				properties.Add(null);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Add_EmptyFullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Add(p);
			});
		}

		#endregion

		#region Remove Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_NullThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			Assert.Throws<System.ArgumentNullException>(() =>
			{
				properties.Remove((SsdpDeviceProperty)null);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_NullKeyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Remove((string)null);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_EmptyKeyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Remove(String.Empty);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_EmptyFullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);
			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Remove(p);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveInstanceSucceeds()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);

			properties.Add(p);

			Assert.IsTrue(properties.Remove(p));
			Assert.AreEqual(0, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveInstanceForDifferentInstanceWithSameKeyReturnsFalse()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);
			var p2 = new SsdpDeviceProperty("TestNamespace", "TestProp1");

			properties.Add(p);

			Assert.IsFalse(properties.Remove(p2));
			Assert.AreEqual(1, properties.Count);
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Remove_RemoveByKeySucceeds()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "TestProp1", null);

			properties.Add(p);

			Assert.IsTrue(properties.Remove(p.FullName));
			Assert.AreEqual(0, properties.Count);
		}

		#endregion

		#region Contains Tests

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_NullNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Contains((string)null);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_EmptyNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Contains(String.Empty);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_NullPropertyThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			Assert.Throws<System.ArgumentNullException>(() =>
			{
				properties.Contains((SsdpDeviceProperty)null);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_PropertyWithEmptyNameThrows()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty(string.Empty, string.Empty, null);

			Assert.Throws<System.ArgumentException>(() =>
			{
				properties.Contains(p);
			});
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsTrueForExistingKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.IsTrue(properties.Contains(prop.FullName));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForNonExistentKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains("NotAValidKey"));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsTrueForExistingItem()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.IsTrue(properties.Contains(prop));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForExistingKeyDifferentItem()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			var prop2 = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains(prop2));
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Contains_ReturnsFalseForNonExistentProperty()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");
			var prop2 = new SsdpDeviceProperty("MyNamespace", "TestProperty1", "1.0");

			properties.Add(prop);

			Assert.IsFalse(properties.Contains(prop2));
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

			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[TestMethod]
		public void SsdpDevicePropertiesCollection_GetEnumerator_Success()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var prop = new SsdpDeviceProperty("MyNamespace", "TestProperty", "1.0");

			properties.Add(prop);
			var enumerator = ((IEnumerable)properties).GetEnumerator();

			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual(prop, enumerator.Current);
			Assert.IsFalse(enumerator.MoveNext());
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

		[TestMethod]
		public void SsdpDevicePropertiesCollection_Indexer_ThrowsOnUnknownKey()
		{
			var properties = new SsdpDevicePropertiesCollection();
			var p = new SsdpDeviceProperty("TestNamespace", "Test", "some value");

			properties.Add(p);

			Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
			{
				var value = properties["NotAValidKey"];
			});
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