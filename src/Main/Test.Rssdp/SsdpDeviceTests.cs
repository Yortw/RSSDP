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
	public class SsdpDeviceTests
	{

		#region AddDevice Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_AddDevice_ThrowsArgumentNullOnNullDevice()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.AddDevice(null);
		}

		[TestMethod]
		public void SsdpDevice_AddDevice_RaisesDeviceAdded()
		{
			var rootDevice = new SsdpRootDevice();

			bool eventRaised = false;
			SsdpDevice eventDevice = null;
			rootDevice.DeviceAdded += (sender, e) =>
				{
					eventRaised = true;
					eventDevice = e.Device;
				};

			var embeddedDevice = new SsdpEmbeddedDevice();
			rootDevice.AddDevice(embeddedDevice);
			Assert.IsTrue(eventRaised);
			Assert.AreEqual(embeddedDevice, eventDevice);
		}

		[TestMethod]
		public void SsdpDevice_AddDevice_DuplicateAddDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var embeddedDevice = new SsdpEmbeddedDevice();
			rootDevice.AddDevice(embeddedDevice);
			rootDevice.AddDevice(embeddedDevice);
			Assert.AreEqual(1, rootDevice.Devices.Count());
		}

		[TestMethod]
		public void SsdpDevice_AddDevice_SetsRootDeviceOnDescendants()
		{
			var rootDevice = new SsdpRootDevice();

			var embeddedDevice = new SsdpEmbeddedDevice();
			var embeddedDevice2 = new SsdpEmbeddedDevice();
			embeddedDevice.AddDevice(embeddedDevice2);
			Assert.IsNull(embeddedDevice2.RootDevice);

			rootDevice.AddDevice(embeddedDevice);

			Assert.AreEqual(rootDevice, embeddedDevice.RootDevice);
			Assert.AreEqual(rootDevice, embeddedDevice2.RootDevice);
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[TestMethod]
		public void SsdpDevice_AddDevice_ThrowsAddingDeviceToSelf()
		{
			var embeddedDevice = new SsdpEmbeddedDevice();

			embeddedDevice.AddDevice(embeddedDevice);
		}

		[ExpectedException(typeof(InvalidOperationException))]
		[TestMethod]
		public void SsdpDevice_AddDevice_ThrowsAddingDeviceToMultipleParents()
		{
			var rootDevice1 = new SsdpRootDevice();
			var rootDevice2 = new SsdpRootDevice();

			var embeddedDevice = new SsdpEmbeddedDevice();


			rootDevice1.AddDevice(embeddedDevice);
			rootDevice2.AddDevice(embeddedDevice);
		}

		#endregion

		#region RemoveDevice Tests

		[TestMethod]
		public void SsdpDevice_RemoveDevice_RaisesDeviceRemoved()
		{
			var rootDevice = new SsdpRootDevice();

			bool eventRaised = false;
			SsdpDevice eventDevice = null;
			rootDevice.DeviceRemoved += (sender, e) =>
			{
				eventRaised = true;
				eventDevice = e.Device;
			};

			var embeddedDevice = new SsdpEmbeddedDevice() { Uuid = System.Guid.NewGuid().ToString() };
			rootDevice.AddDevice(embeddedDevice);
			rootDevice.RemoveDevice(embeddedDevice);

			Assert.IsTrue(eventRaised);
			Assert.AreEqual(embeddedDevice, eventDevice);
		}

		[TestMethod]
		public void SsdpDevice_RemoveDevice_DuplicateRemoveDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var embeddedDevice = new SsdpEmbeddedDevice() { Uuid = System.Guid.NewGuid().ToString() };
			rootDevice.AddDevice(embeddedDevice);
			Assert.AreEqual(1, rootDevice.Devices.Count());
			rootDevice.RemoveDevice(embeddedDevice);
			rootDevice.RemoveDevice(embeddedDevice);
			Assert.AreEqual(0, rootDevice.Devices.Count());
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_RemoveDevice_ThrowsArgumentNullOnNullDevice()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.RemoveDevice(null);
		}

		#endregion

		#region Constructor Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_ConstructorThrowsArgumentNullIfNotRootDevice()
		{
			_ = new SsdpEmbeddedDevice(null);
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void DeviceEventArgs_ConstructorThrowsOnNullDevice()
		{
			_ = new DeviceEventArgs(null);
		}

		#endregion

		#region DeviceTypeNamespace Tests

		[TestMethod]
		public void SsdpDevice_NullDeviceTypeNamespaceReturnsNull()
		{
			var rootDevice = new SsdpRootDevice
			{
				DeviceTypeNamespace = null
			};
			Assert.AreEqual(null, rootDevice.DeviceTypeNamespace);
		}

		[TestMethod]
		public void SsdpDevice_EmptyDeviceTypeNamespaceReturnsEmpty()
		{
			var rootDevice = new SsdpRootDevice
			{
				DeviceTypeNamespace = String.Empty
			};
			Assert.AreEqual(String.Empty, rootDevice.DeviceTypeNamespace);
		}

		#endregion

		#region DeviceType Tests

		[TestMethod]
		public void SsdpDevice_NullDeviceTypeReturnsNull()
		{
			var rootDevice = new SsdpRootDevice
			{
				DeviceType = null
			};
			Assert.AreEqual(null, rootDevice.DeviceType);
		}

		[TestMethod]
		public void SsdpDevice_EmptyDeviceTypeReturnsEmpty()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceType = String.Empty;
			Assert.AreEqual(String.Empty, rootDevice.DeviceType);
		}

		[TestMethod]
		public void SsdpDevice_FullDeviceTypesReturnsStringWithNullValues()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.DeviceType = null;
			rootDevice.DeviceTypeNamespace = null;
			Assert.AreEqual("urn::device::1", rootDevice.FullDeviceType);
		}

		#endregion

		#region RootDevice Tests

		[TestMethod]
		public void SsdpDevice_RootDeviceToRootDeviceReturnsSelf()
		{
			var rootDevice = new SsdpRootDevice();
			Assert.AreEqual(rootDevice, rootDevice.ToRootDevice());
		}

		[TestMethod]
		public void SsdpDevice_DeviceToRootDeviceReturnsAssignedRootDevice()
		{
			var rootDevice = new SsdpRootDevice();
			var device = new SsdpEmbeddedDevice();
			rootDevice.AddDevice(device);

			Assert.AreEqual(rootDevice, device.RootDevice);
		}

		[TestMethod]
		public void SsdpDevice_DeviceToRootDeviceReturnsNullWhenNoRootAssigned()
		{
			var device = new SsdpEmbeddedDevice();

			Assert.AreEqual(null, device.RootDevice);
		}

		[TestMethod]
		public void SsdpDevice_RootDeviceFromDeviceDescriptionXml()
		{
			var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root xmlns=\"urn:schemas-upnp-org:device-1-0\"><specVersion><major>1</major><minor>0</minor></specVersion><URLBase>http://192.168.0.1:54243</URLBase><device><deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType><friendlyName>Friendly Player</friendlyName><manufacturer>RSSDP</manufacturer><manufacturerURL>https://github.com/Yortw/RSSDP</manufacturerURL><modelDescription>UPnP Renderer</modelDescription><modelName>RSSDP</modelName><modelNumber>6</modelNumber><modelURL>https://github.com/Yortw/RSSDP</modelURL><serialNumber>0</serialNumber><UDN>uuid:uuid:4422acaa-c5b4-4a8e-a1ff-382656833d43</UDN><UPC>00000000</UPC><iconList><icon><mimetype>image/png</mimetype><url>/icons/sm.png</url><width>48</width><height>48</height><depth>24</depth></icon><icon><mimetype>image/png</mimetype><url>/icons/lrg.png</url><width>120</width><height>120</height><depth>24</depth></icon><icon><mimetype>image/jpeg</mimetype><url>/icons/sm.jpg</url><width>48</width><height>48</height><depth>24</depth></icon><icon><mimetype>image/jpeg</mimetype><url>/icons/lrg.jpg</url><width>120</width><height>120</height><depth>24</depth></icon></iconList><serviceList><service><serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType><serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId><controlURL>/service/ConnectionManager/control</controlURL><eventSubURL>/service/ConnectionManager/event</eventSubURL><SCPDURL>/service/ConnectionManager/scpd</SCPDURL></service><service><serviceType>urn:schemas-upnp-org:service:AVTransport:1</serviceType><serviceId>urn:upnp-org:serviceId:AVTransport</serviceId><controlURL>/service/AVTransport/control</controlURL><eventSubURL>/service/AVTransport/event</eventSubURL><SCPDURL>/service/AVTransport/scpd</SCPDURL></service><service><serviceType>urn:schemas-upnp-org:service:RenderingControl:1</serviceType><serviceId>urn:upnp-org:serviceId:RenderingControl</serviceId><controlURL>/service/RenderingControl/control</controlURL><eventSubURL>/service/RenderingControl/event</eventSubURL><SCPDURL>/service/RenderingControl/scpd</SCPDURL></service></serviceList></device></root>";
			var rootDevice = new SsdpRootDevice(new Uri("http://192.168.0.1:54243/device.xml"), TimeSpan.FromSeconds(30), xml);
			Assert.AreEqual("Friendly Player", rootDevice.FriendlyName);
		}

		#endregion

		#region Extension Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_ToRootDevice_ThrowsOnNullSource()
		{
			SsdpDevice device = null;

			device.ToRootDevice();
		}

		#endregion

		#region Device Document Deserialisation Tests

		private const string DocumentWithComplexCustomProperties = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<root xmlns=""urn:schemas-upnp-org:device-1-0"">
  <specVersion>
    <major>1</major>
    <minor>0</minor>
  </specVersion>
  <device>
    <deviceType>urn:schemas-upnp-org:device:ZonePlayer:1</deviceType>
    <friendlyName>192.168.1.69 - Sonos PLAY:3</friendlyName>
    <manufacturer>Sonos, Inc.</manufacturer>
    <manufacturerURL>http://www.sonos.com</manufacturerURL>
    <modelNumber>S3</modelNumber>
    <modelDescription>Sonos PLAY:3</modelDescription>
    <modelName>Sonos PLAY:3</modelName>
    <modelURL>http://www.sonos.com/products/zoneplayers/S3</modelURL>
    <softwareVersion>29.5-91030</softwareVersion>
    <hardwareVersion>1.8.1.3-2</hardwareVersion>
    <serialNum>B8-E9-37-3A-D9-06:D</serialNum>
    <UDN>uuid:RINCON_B8E9373AD90601400</UDN>
    <iconList>
      <icon>
        <id>0</id>
        <mimetype>image/png</mimetype>
        <width>48</width>
        <height>48</height>
        <depth>24</depth>
        <url>/img/icon-S3.png</url>
      </icon>
    </iconList>
    <minCompatibleVersion>28.0-00000</minCompatibleVersion>
    <legacyCompatibleVersion>24.0-0000</legacyCompatibleVersion>
    <displayVersion>5.4</displayVersion>
    <extraVersion>OTP: </extraVersion>
    <roomName>Portable</roomName>
    <displayName>PLAY:3</displayName>
    <zoneType>7</zoneType>
    <feature1>0x00000000</feature1>
    <feature2>0x00006332</feature2>
    <feature3>0x0003002e</feature3>
    <internalSpeakerSize>4</internalSpeakerSize>
    <bassExtension>60.000</bassExtension>
    <satGainOffset>3.000</satGainOffset>
    <memory>128</memory>
    <flash>64</flash>
    <ampOnTime>425</ampOnTime>
    <serviceList>
      <service>
        <serviceType>urn:schemas-upnp-org:service:AlarmClock:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:AlarmClock</serviceId>
        <controlURL>/AlarmClock/Control</controlURL>
        <eventSubURL>/AlarmClock/Event</eventSubURL>
        <SCPDURL>/xml/AlarmClock1.xml</SCPDURL>
      </service>    
      <service>
        <serviceType>urn:schemas-upnp-org:service:MusicServices:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:MusicServices</serviceId>
        <controlURL>/MusicServices/Control</controlURL>
        <eventSubURL>/MusicServices/Event</eventSubURL>
        <SCPDURL>/xml/MusicServices1.xml</SCPDURL>
      </service>    
      <service>
        <serviceType>urn:schemas-upnp-org:service:DeviceProperties:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:DeviceProperties</serviceId>
        <controlURL>/DeviceProperties/Control</controlURL>
        <eventSubURL>/DeviceProperties/Event</eventSubURL>
        <SCPDURL>/xml/DeviceProperties1.xml</SCPDURL>
      </service>    
      <service>
        <serviceType>urn:schemas-upnp-org:service:SystemProperties:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:SystemProperties</serviceId>
        <controlURL>/SystemProperties/Control</controlURL>
        <eventSubURL>/SystemProperties/Event</eventSubURL>
        <SCPDURL>/xml/SystemProperties1.xml</SCPDURL>
      </service>    
      <service>
        <serviceType>urn:schemas-upnp-org:service:ZoneGroupTopology:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:ZoneGroupTopology</serviceId>
        <controlURL>/ZoneGroupTopology/Control</controlURL>
        <eventSubURL>/ZoneGroupTopology/Event</eventSubURL>
        <SCPDURL>/xml/ZoneGroupTopology1.xml</SCPDURL>
      </service>    
      <service>
        <serviceType>urn:schemas-upnp-org:service:GroupManagement:1</serviceType>
        <serviceId>urn:upnp-org:serviceId:GroupManagement</serviceId>
        <controlURL>/GroupManagement/Control</controlURL>
        <eventSubURL>/GroupManagement/Event</eventSubURL>
        <SCPDURL>/xml/GroupManagement1.xml</SCPDURL>
      </service>
      <service>
        <serviceType>urn:schemas-tencent-com:service:QPlay:1</serviceType>
        <serviceId>urn:tencent-com:serviceId:QPlay</serviceId>
        <controlURL>/QPlay/Control</controlURL>
        <eventSubURL>/QPlay/Event</eventSubURL>
        <SCPDURL>/xml/QPlay1.xml</SCPDURL>
      </service>
    </serviceList>
    <deviceList>
      <device>
  <deviceType>urn:schemas-upnp-org:device:MediaServer:1</deviceType>
  <friendlyName>192.168.1.69 - Sonos PLAY:3 Media Server</friendlyName>
  <manufacturer>Sonos, Inc.</manufacturer>
  <manufacturerURL>http://www.sonos.com</manufacturerURL>
  <modelNumber>S3</modelNumber>
  <modelDescription>Sonos PLAY:3 Media Server</modelDescription>
  <modelName>Sonos PLAY:3</modelName>
  <modelURL>http://www.sonos.com/products/zoneplayers/S3</modelURL>
  <UDN>uuid:RINCON_B8E9373AD90601400_MS</UDN>
  <serviceList>
    <service>
      <serviceType>urn:schemas-upnp-org:service:ContentDirectory:1</serviceType>
      <serviceId>urn:upnp-org:serviceId:ContentDirectory</serviceId>
      <controlURL>/MediaServer/ContentDirectory/Control</controlURL>
      <eventSubURL>/MediaServer/ContentDirectory/Event</eventSubURL>
      <SCPDURL>/xml/ContentDirectory1.xml</SCPDURL>
    </service>
    <service>
      <serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType>
	    <serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId>
	    <controlURL>/MediaServer/ConnectionManager/Control</controlURL>
	    <eventSubURL>/MediaServer/ConnectionManager/Event</eventSubURL>
	    <SCPDURL>/xml/ConnectionManager1.xml</SCPDURL>
	  </service>
	</serviceList>
      </device>
      <device>
	<deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType>
  <friendlyName>Portable - Sonos PLAY:3 Media Renderer</friendlyName>
  <manufacturer>Sonos, Inc.</manufacturer>
  <manufacturerURL>http://www.sonos.com</manufacturerURL>
  <modelNumber>S3</modelNumber>
  <modelDescription>Sonos PLAY:3 Media Renderer</modelDescription>
  <modelName>Sonos PLAY:3</modelName>
  <modelURL>http://www.sonos.com/products/zoneplayers/S3</modelURL>
	<UDN>uuid:RINCON_B8E9373AD90601400_MR</UDN>
	<serviceList>
	  <service>
	    <serviceType>urn:schemas-upnp-org:service:RenderingControl:1</serviceType>
	    <serviceId>urn:upnp-org:serviceId:RenderingControl</serviceId>
	    <controlURL>/MediaRenderer/RenderingControl/Control</controlURL>
	    <eventSubURL>/MediaRenderer/RenderingControl/Event</eventSubURL>
	    <SCPDURL>/xml/RenderingControl1.xml</SCPDURL>
	  </service>
	  <service>
	    <serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType>
	    <serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId>
	    <controlURL>/MediaRenderer/ConnectionManager/Control</controlURL>
	    <eventSubURL>/MediaRenderer/ConnectionManager/Event</eventSubURL>
	    <SCPDURL>/xml/ConnectionManager1.xml</SCPDURL>
	  </service>
	  <service>
	    <serviceType>urn:schemas-upnp-org:service:AVTransport:1</serviceType>
	    <serviceId>urn:upnp-org:serviceId:AVTransport</serviceId>
	    <controlURL>/MediaRenderer/AVTransport/Control</controlURL>
	    <eventSubURL>/MediaRenderer/AVTransport/Event</eventSubURL>
	    <SCPDURL>/xml/AVTransport1.xml</SCPDURL>
	  </service>
	  <service>
	    <serviceType>urn:schemas-sonos-com:service:Queue:1</serviceType>
	    <serviceId>urn:sonos-com:serviceId:Queue</serviceId>
	    <controlURL>/MediaRenderer/Queue/Control</controlURL>
	    <eventSubURL>/MediaRenderer/Queue/Event</eventSubURL>
	    <SCPDURL>/xml/Queue1.xml</SCPDURL>
	  </service>
          <service>
            <serviceType>urn:schemas-upnp-org:service:GroupRenderingControl:1</serviceType>
            <serviceId>urn:upnp-org:serviceId:GroupRenderingControl</serviceId>
            <controlURL>/MediaRenderer/GroupRenderingControl/Control</controlURL>
            <eventSubURL>/MediaRenderer/GroupRenderingControl/Event</eventSubURL>
            <SCPDURL>/xml/GroupRenderingControl1.xml</SCPDURL>
          </service>
	</serviceList>
        <X_Rhapsody-Extension xmlns=""http://www.real.com/rhapsody/xmlns/upnp-1-0"">
          <deviceID>urn:rhapsody-real-com:device-id-1-0:sonos_1:RINCON_B8E9373AD90601400</deviceID>
            <deviceCapabilities>
              <interactionPattern type=""real-rhapsody-upnp-1-0""/>
            </deviceCapabilities>
        </X_Rhapsody-Extension>
        <qq:X_QPlay_SoftwareCapability xmlns:qq=""http://www.tencent.com"">QPlay:2</qq:X_QPlay_SoftwareCapability>
        <iconList>
          <icon>
            <mimetype>image/png</mimetype>
            <width>48</width>
            <height>48</height>
            <depth>24</depth>
            <url>/img/icon-S3.png</url>
          </icon>
        </iconList>
      </device>
    </deviceList>
  </device>
</root>";

		[TestMethod]
		public void SsdpDevice_Constructor_DeserialisesComplexDeviceDocumentAndSkipsComplexCustomProperties()
		{
			var device = new SsdpRootDevice(new Uri("http://192.168.1.2"), TimeSpan.FromMinutes(1), DocumentWithComplexCustomProperties);
			Assert.AreEqual(19, device.CustomProperties.Count);
			var subDevice = device.Devices.Last();

			Assert.AreEqual("QPlay:2", subDevice.CustomProperties["qq:X_QPlay_SoftwareCapability"].Value);
			Assert.IsFalse(subDevice.CustomProperties.Contains("X_Rhapsody-Extension"));
		}

	    [TestMethod]
	    public void Deserialisation_XmlWithNewlines_HandlesIconListAndFollowingProperties()
	    {
	        var docString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root xmlns=""urn:schemas-upnp-org:device-1-0"">
    <specVersion>
        <major>1</major>
        <minor>1</minor>
    </specVersion>
    <device>
        <deviceType>urn:schemas-upnp-org:device:MediaServer:1</deviceType>
        <iconList>
            <icon>
                <mimetype>image/png</mimetype>
                <width>120</width>
                <height>120</height>
                <depth>24</depth>
                <url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-120.png</url>
            </icon>
            <icon>
                <mimetype>image/png</mimetype>
                <width>48</width>
                <height>48</height>
                <depth>24</depth>
                <url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-48.png</url>
            </icon>
            <icon>
                <mimetype>image/jpeg</mimetype>
                <width>120</width>
                <height>120</height>
                <depth>24</depth>
                <url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-120.jpg</url>
            </icon>
            <icon>
                <mimetype>image/jpeg</mimetype>
                <width>48</width>
                <height>48</height>
                <depth>24</depth>
                <url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-48.jpg</url>
            </icon>
        </iconList>
        <friendlyName>MinimServer[RIEMANN]</friendlyName>
        <manufacturer>minimserver.com</manufacturer>
        <modelName>MinimServer</modelName>
        <UDN>uuid:df5bda28-1b1a-4a62-89ed-0acc041ee8e4</UDN>
        <serviceList>
            <service>
                <serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType>
                <serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId>
                <SCPDURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/upnp.org-ConnectionManager-1/service.xml</SCPDURL>
                <controlURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ConnectionManager-1/control</controlURL>
                <eventSubURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ConnectionManager-1/event</eventSubURL>
            </service>
            <service>
                <serviceType>urn:schemas-upnp-org:service:ContentDirectory:1</serviceType>
                <serviceId>urn:upnp-org:serviceId:ContentDirectory</serviceId>
                <SCPDURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/upnp.org-ContentDirectory-1/service.xml</SCPDURL>
                <controlURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ContentDirectory-1/control</controlURL>
                <eventSubURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ContentDirectory-1/event</eventSubURL>
            </service>
        </serviceList>
        <presentationURL>http://127.0.0.1:9790/</presentationURL>
    </device>
</root>";

	        var device = new SsdpRootDevice(new Uri("http://192.168.1.11/UPnP/DeviceDescription"), TimeSpan.FromMinutes(30), docString);
	        Assert.AreEqual("MinimServer[RIEMANN]", device.FriendlyName);
	        Assert.AreEqual("minimserver.com", device.Manufacturer);
	        Assert.AreEqual("MinimServer", device.ModelName);
	        Assert.AreEqual("uuid:df5bda28-1b1a-4a62-89ed-0acc041ee8e4", device.Udn);
	        Assert.AreEqual(4, device.Icons.Count);
	        Assert.AreEqual(4, device.Icons.Select(icon => icon.Url.ToString()).Distinct().Count());
        }

	    [TestMethod]
	    public void Deserialisation_XmlWithoutNewlines_HandlesIconListAndFollowingProperties()
	    {
	        var docString = @"<?xml version=""1.0"" encoding=""utf-8""?><root xmlns=""urn:schemas-upnp-org:device-1-0""><specVersion><major>1</major><minor>1</minor></specVersion><device><deviceType>urn:schemas-upnp-org:device:MediaServer:1</deviceType><iconList><icon><mimetype>image/png</mimetype><width>120</width><height>120</height><depth>24</depth><url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-120.png</url></icon><icon><mimetype>image/png</mimetype><width>48</width><height>48</height><depth>24</depth><url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-48.png</url></icon><icon><mimetype>image/jpeg</mimetype><width>120</width><height>120</height><depth>24</depth><url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-120.jpg</url></icon><icon><mimetype>image/jpeg</mimetype><width>48</width><height>48</height><depth>24</depth><url>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/resource/minimicon-48.jpg</url></icon></iconList><friendlyName>MinimServer[RIEMANN]</friendlyName><manufacturer>minimserver.com</manufacturer><modelName>MinimServer</modelName><UDN>uuid:df5bda28-1b1a-4a62-89ed-0acc041ee8e4</UDN><serviceList><service><serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType><serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId><SCPDURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/upnp.org-ConnectionManager-1/service.xml</SCPDURL><controlURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ConnectionManager-1/control</controlURL><eventSubURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ConnectionManager-1/event</eventSubURL></service><service><serviceType>urn:schemas-upnp-org:service:ContentDirectory:1</serviceType><serviceId>urn:upnp-org:serviceId:ContentDirectory</serviceId><SCPDURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/Upnp/upnp.org-ContentDirectory-1/service.xml</SCPDURL><controlURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ContentDirectory-1/control</controlURL><eventSubURL>/df5bda28-1b1a-4a62-89ed-0acc041ee8e4/upnp.org-ContentDirectory-1/event</eventSubURL></service></serviceList><presentationURL>http://127.0.0.1:9790/</presentationURL></device></root>";

	        var device = new SsdpRootDevice(new Uri("http://192.168.1.11/UPnP/DeviceDescription"), TimeSpan.FromMinutes(30), docString);
	        Assert.AreEqual("MinimServer[RIEMANN]", device.FriendlyName);
	        Assert.AreEqual("minimserver.com", device.Manufacturer);
	        Assert.AreEqual("MinimServer", device.ModelName);
	        Assert.AreEqual("uuid:df5bda28-1b1a-4a62-89ed-0acc041ee8e4", device.Udn);
	        Assert.AreEqual(4, device.Icons.Count);
	        var d = device.Icons.Select(icon => icon.Url.ToString()).Distinct();
	        Assert.AreEqual(4, device.Icons.Select(icon => icon.Url.ToString()).Distinct().Count());
	    }

        [TestMethod]
		public void DeserialisationHandlesEmptyCustomProperties()
		{
			//See issue #70 in repo - empty custom properties would cause
			//all following properties to be skipped.
			var docString = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns=""urn:schemas-upnp-org:device-1-0"">
  <specVersion>
    <major>1</major>
    <minor>0</minor>
  </specVersion>
  <device>
    <deviceType>urn:schemas-upnp-org:device:MediaServer:1</deviceType>
    <UDN>uuid:55076f6e-6b79-1d65-a472-00059a3c7a00</UDN>
    <friendlyName>Twonky :)</friendlyName>
    <pv:extension xmlns:pv=""http://www.pv.com/pvns/""></pv:extension>
    <pv:empty_extension xmlns:pv=""http://www.pv.com/pvns/"" />
    <manufacturer>PacketVideo</manufacturer>
    <manufacturerURL>http://www.pv.com</manufacturerURL>
    <modelName>TwonkyServer</modelName>
    <modelURL>http://www.twonky.com</modelURL>
    <modelDescription>TwonkyServer (Windows, T-206)</modelDescription>
    <modelNumber>8.4</modelNumber>
    <serialNumber>8.4</serialNumber>
  </device>
</root>";


			var device = new SsdpRootDevice(new Uri("http://192.168.1.11/UPnP/DeviceDescription"), TimeSpan.FromMinutes(30), docString);
			Assert.IsFalse(device.CustomProperties.Contains("pv:extension"));
			Assert.AreEqual(device.Manufacturer, "PacketVideo");
			Assert.AreEqual(device.ManufacturerUrl, new Uri("http://www.pv.com"));
			Assert.AreEqual(device.ModelName, "TwonkyServer");
			Assert.AreEqual(device.ModelUrl, new Uri("http://www.twonky.com"));
			Assert.AreEqual(device.ModelDescription, "TwonkyServer (Windows, T-206)");
			Assert.AreEqual(device.ModelNumber, "8.4");
			Assert.AreEqual(device.SerialNumber, "8.4");
		}

		#endregion

		#region Service Tests

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_AddService_ThrowsArgumentNullOnNullService()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.AddService(null);
		}

		[TestMethod]
		public void SsdpDevice_AddService_RaisesServiceAdded()
		{
			var rootDevice = new SsdpRootDevice();

			bool eventRaised = false;
			SsdpService eventService = null;
			rootDevice.ServiceAdded += (sender, e) =>
			{
				eventRaised = true;
				eventService = e.Service;
			};

			var service = new SsdpService();
			rootDevice.AddService(service);
			Assert.IsTrue(eventRaised);
			Assert.AreEqual(service, eventService);
		}

		[TestMethod]
		public void SsdpDevice_AddService_DuplicateAddDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var service = new SsdpService();
			rootDevice.AddService(service);
			rootDevice.AddService(service);
			Assert.AreEqual(1, rootDevice.Services.Count());
		}

		[ExpectedException(typeof(System.ArgumentNullException))]
		[TestMethod]
		public void SsdpDevice_RemoveService_ThrowsArgumentNullOnNullService()
		{
			var rootDevice = new SsdpRootDevice();
			rootDevice.AddService(null);
		}

		[TestMethod]
		public void SsdpDevice_RemoveService_RaisesServiceRemoved()
		{
			var rootDevice = new SsdpRootDevice();

			bool eventRaised = false;
			SsdpService eventService = null;
			rootDevice.ServiceRemoved += (sender, e) =>
			{
				eventRaised = true;
				eventService = e.Service;
			};

			var service = new SsdpService();
			rootDevice.AddService(service);

			rootDevice.RemoveService(service);
			Assert.IsTrue(eventRaised);
			Assert.AreEqual(service, eventService);
		}

		[TestMethod]
		public void SsdpDevice_RemoveService_DuplicateRemoveDoesNothing()
		{
			var rootDevice = new SsdpRootDevice();

			var service = new SsdpService();
			rootDevice.AddService(service);
			Assert.AreEqual(1, rootDevice.Services.Count());
			rootDevice.RemoveService(service);
			rootDevice.RemoveService(service);
			Assert.AreEqual(0, rootDevice.Devices.Count());
		}

		#endregion

	}
}