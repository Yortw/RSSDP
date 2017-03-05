using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rssdp;
using Rssdp.Aggregatable;
using Rssdp.Infrastructure;
using Rssdp.Network;

namespace Test.RssdpPortable.DeviceLocator
{
	[TestClass]
	public class AggregatableSsdpDeviceLocatorTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WhenFirstArgumentIsNull_ThrowArgumentNullException1()
		{
			INetworkInfoProvider networkInfoProvider = null;
			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProvider, deviceLocatorFactoryMock.Object, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WhenSecondArgumentIsNull_ThrowArgumentNullException1()
		{
			var networkInfoProvider = new Mock<INetworkInfoProvider>();
			ISsdpDeviceLocatorFactory deviceLocatorFactoryMock = null;
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProvider.Object, deviceLocatorFactoryMock, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WhenFirstArgumentIsNull_ThrowArgumentNullException2()
		{
			List<string> unicastAddresses = null;
			var devicePublisherFactory = new Mock<ISsdpDeviceLocatorFactory>();
			var aggregatableLocator = new AggregatableDeviceLocator(unicastAddresses, devicePublisherFactory.Object, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WhenSecondArgumentIsNull_ThrowArgumentNullException2()
		{
			var unicastAddresses = new List<string>();
			ISsdpDeviceLocatorFactory deviceLocatorFactory = null;
			var aggregatableLocator = new AggregatableDeviceLocator(unicastAddresses, deviceLocatorFactory, 0);
		}

		[TestMethod]
		public void Ctor_WhenProvidedEmptyListOfUnicastAddresses_LocatorsAreNotCreated()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>());
			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();

			//# Act
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Assert
			Assert.IsTrue(!aggregatableLocator.Locators.Any());
		}

		[TestMethod]
		public void Ctor_WhenNoUnicastAddresses_LocatorsAreNotCreated()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>());
			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();

			//# Act
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Assert
			deviceLocatorFactoryMock.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
		}

		[TestMethod]
		public void Ctor1_WhenHasUnicastAddresses_LocatorsHasBeenCreatedForEachInterface()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});
			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(new SsdpDeviceLocator());

			//# Act
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Assert
			deviceLocatorFactoryMock.Verify(f => f.Create("127.0.0.1", 0), Times.Once);
			deviceLocatorFactoryMock.Verify(f => f.Create("::1", 0), Times.Once);
		}

		[TestMethod]
		public void Ctor2_WhenHasUnicastAddresses_LocatorsHasBeenCreatedForEachInterface()
		{
			//# Arrange
			var unicastAddresses = new List<string>
			{
				"127.0.0.1",
				"::1"
			};
			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(new SsdpDeviceLocator());

			//# Act
			var aggregatableLocator = new AggregatableDeviceLocator(unicastAddresses, deviceLocatorFactoryMock.Object, 0);

			//# Assert
			deviceLocatorFactoryMock.Verify(f => f.Create("127.0.0.1", 0), Times.Once);
			deviceLocatorFactoryMock.Verify(f => f.Create("::1", 0), Times.Once);
		}

		[TestMethod]
		public void Ctor3_WhenLocatorCreated_SubscrivedToDeviceAvailableEvent()
		{
			//# Arrange
			bool eventGenerated = false;

			var unicastAddresses = new List<string>
			{
				"127.0.0.1",
				"::1"
			};

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			var deviceLocatorMock = new Mock<ISsdpDeviceLocator>();
			deviceLocatorMock.Object.DeviceAvailable += (sender, args) =>
			{
				eventGenerated = true;
			};

			deviceLocatorFactoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(deviceLocatorMock.Object);

			var aggregatableLocator = new AggregatableDeviceLocator(unicastAddresses, deviceLocatorFactoryMock.Object, 0);
			
			//# Act
			deviceLocatorMock.Raise(l=>l.DeviceAvailable += null, new DeviceAvailableEventArgs(new DiscoveredSsdpDevice(), true));

			//# Assert
			Assert.IsTrue(eventGenerated);
		}

		[TestMethod]
		public void Ctor3_WhenLocatorCreated_SubscrivedToDeviceUnvailableEvent()
		{
			//# Arrange
			bool eventGenerated = false;

			var unicastAddresses = new List<string>
			{
				"127.0.0.1",
				"::1"
			};

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			var deviceLocatorMock = new Mock<ISsdpDeviceLocator>();
			deviceLocatorMock.Object.DeviceUnavailable += (sender, args) =>
			{
				eventGenerated = true;
			};

			deviceLocatorFactoryMock.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(deviceLocatorMock.Object);

			var aggregatableLocator = new AggregatableDeviceLocator(unicastAddresses, deviceLocatorFactoryMock.Object, 0);

			//# Act
			deviceLocatorMock.Raise(l => l.DeviceUnavailable += null, new DeviceUnavailableEventArgs(new DiscoveredSsdpDevice(), true));

			//# Assert
			Assert.IsTrue(eventGenerated);
		}

		[TestMethod]
		public void Dtor_WhenHasLocators_EachLocatorCalledStopListeningAndDispose()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});

			var deviceLocatorFirstMock = new Mock<ISsdpDeviceLocator>();
			var deviceLocatorSecondMock = new Mock<ISsdpDeviceLocator>();

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>(), It.IsAny<int>()))
				.Returns(deviceLocatorFirstMock.Object)
				.Returns(deviceLocatorSecondMock.Object);

			IAggregatableDeviceLocator aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Act
			aggregatableLocator.Dispose();

			//# Assert
			deviceLocatorFirstMock.Verify(p => p.StopListeningForNotifications());
			deviceLocatorSecondMock.Verify(p => p.StopListeningForNotifications());

			deviceLocatorFirstMock.Verify(p => p.Dispose());
			deviceLocatorSecondMock.Verify(p => p.Dispose());
		}

		[TestMethod]
		public void Locators_WhenHasSeveralLocators_ReturnsLocators()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});

			var deviceLocatorFirstMock = new Mock<ISsdpDeviceLocator>();
			var deviceLocatorSecondMock = new Mock<ISsdpDeviceLocator>();

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>(), It.IsAny<int>()))
				.Returns(deviceLocatorFirstMock.Object)
				.Returns(deviceLocatorSecondMock.Object);

			//# Act
			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Assert
			Assert.AreEqual(2, aggregatableLocator.Locators.Count());
			Assert.AreEqual(deviceLocatorFirstMock.Object, aggregatableLocator.Locators.ElementAt(0));
			Assert.AreEqual(deviceLocatorSecondMock.Object, aggregatableLocator.Locators.ElementAt(1));
		}

		[TestMethod]
		public void StartListening_WhenHasSeveralLocators_EachLocatorCalledStartListening()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});

			var deviceLocatorFirstMock = new Mock<ISsdpDeviceLocator>();
			var deviceLocatorSecondMock = new Mock<ISsdpDeviceLocator>();

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>(), It.IsAny<int>()))
				.Returns(deviceLocatorFirstMock.Object)
				.Returns(deviceLocatorSecondMock.Object);

			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);
			
			//# Act
			aggregatableLocator.StartListeningForNotifications();

			//# Assert
			deviceLocatorFirstMock.Verify(l=>l.StartListeningForNotifications());
		}

		[TestMethod]
		public void StopListening_WhenHasSeveralLocators_EachLocatorCalledStopListening()
		{
			//# Arrange
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});

			var deviceLocatorFirstMock = new Mock<ISsdpDeviceLocator>();
			var deviceLocatorSecondMock = new Mock<ISsdpDeviceLocator>();

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>(), It.IsAny<int>()))
				.Returns(deviceLocatorFirstMock.Object)
				.Returns(deviceLocatorSecondMock.Object);

			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Act
			aggregatableLocator.StopListening();

			//# Assert
			deviceLocatorFirstMock.Verify(l => l.StopListeningForNotifications());
		}

		[TestMethod]
		public async Task SearchAsync_WhenHasSeveralLocators_ReturnsAggregatedResultFromEachLocator()
		{
			//# Arrange
			var discoveredDevice1 = new DiscoveredSsdpDevice();
			var discoveredDevice2 = new DiscoveredSsdpDevice();
			
			var networkInfoProviderMock = new Mock<INetworkInfoProvider>();
			networkInfoProviderMock.Setup(n => n.GetIpAddressesFromAdapters()).Returns(new List<string>
			{
				"127.0.0.1",
				"::1"
			});

			var deviceLocatorFirstMock = new Mock<ISsdpDeviceLocator>();
			var deviceLocatorSecondMock = new Mock<ISsdpDeviceLocator>();

			deviceLocatorFirstMock.Setup(l => l.SearchAsync())
				.ReturnsAsync(new List<DiscoveredSsdpDevice>
				{
					discoveredDevice1
				});

			deviceLocatorSecondMock.Setup(l => l.SearchAsync())
				.ReturnsAsync(new List<DiscoveredSsdpDevice>
				{
					discoveredDevice2
				});

			var deviceLocatorFactoryMock = new Mock<ISsdpDeviceLocatorFactory>();
			deviceLocatorFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>(), It.IsAny<int>()))
				.Returns(deviceLocatorFirstMock.Object)
				.Returns(deviceLocatorSecondMock.Object);

			var aggregatableLocator = new AggregatableDeviceLocator(networkInfoProviderMock.Object, deviceLocatorFactoryMock.Object, 0);

			//# Act
			var devices = await aggregatableLocator.SearchAsync();

			//# Assert
			var discoveredSsdpDevices = devices as DiscoveredSsdpDevice[] ?? devices.ToArray();
			Assert.AreEqual(discoveredDevice1, discoveredSsdpDevices.ElementAt(0));
			Assert.AreEqual(discoveredDevice2, discoveredSsdpDevices.ElementAt(1));
		}
	}
}
