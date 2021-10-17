﻿using Rssdp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Rssdp
{
  /// <summary>
  /// Base class representing the common details of a (root or embedded) device, either to be published or that has been located.
  /// </summary>
  /// <remarks>
  /// <para>Do not derive new types directly from this class. New device classes should derive from either <see cref="SsdpRootDevice"/> or <see cref="SsdpEmbeddedDevice"/>.</para>
  /// </remarks>
  /// <seealso cref="SsdpRootDevice"/>
  /// <seealso cref="SsdpEmbeddedDevice"/>
  public abstract class SsdpDevice
  {

    #region Fields

    private string _Udn;
    private readonly SsdpDevicePropertiesCollection _CustomProperties;
    private readonly CustomHttpHeadersCollection _CustomResponseHeaders;

    private readonly IList<SsdpDevice> _Devices;
    private readonly IList<SsdpService> _Services;

    #endregion

    #region Events

    /// <summary>
    /// Raised when a new child device is added.
    /// </summary>
    /// <seealso cref="AddDevice"/>
    /// <seealso cref="DeviceAdded"/>
    public event EventHandler<DeviceEventArgs> DeviceAdded;

    /// <summary>
    /// Raised when a child device is removed.
    /// </summary>
    /// <seealso cref="RemoveDevice"/>
    /// <seealso cref="DeviceRemoved"/>
    public event EventHandler<DeviceEventArgs> DeviceRemoved;

    /// <summary>
    /// Raised when a new service is added.
    /// </summary>
    /// <seealso cref="AddService"/>
    /// <seealso cref="ServiceAdded"/>
    public event EventHandler<ServiceEventArgs> ServiceAdded;

    /// <summary>
    /// Raised when a service is removed.
    /// </summary>
    /// <seealso cref="RemoveService"/>
    /// <seealso cref="OnServiceRemoved"/>
    public event EventHandler<ServiceEventArgs> ServiceRemoved;

    #endregion

    #region Constructors

    /// <summary>
    /// Derived type constructor, allows constructing a device with no parent. Should only be used from derived types that are or inherit from <see cref="SsdpRootDevice"/>.
    /// </summary>
    protected SsdpDevice()
    {
      DeviceTypeNamespace = SsdpConstants.UpnpDeviceTypeNamespace;
      DeviceType = SsdpConstants.UpnpDeviceTypeBasicDevice;
      DeviceVersion = 1;

      this.Icons = new List<SsdpDeviceIcon>();
      _Devices = new List<SsdpDevice>();
      this.Devices = new ReadOnlyEnumerable<SsdpDevice>(_Devices);
      _CustomResponseHeaders = new CustomHttpHeadersCollection();
      _CustomProperties = new SsdpDevicePropertiesCollection();
      _Services = new List<SsdpService>();
      this.Services = new ReadOnlyEnumerable<SsdpService>(_Services);
    }

    /// <summary>
    /// Deserialisation constructor.
    /// </summary>
    /// <remarks><para>Uses the provided XML string and parent device properties to set the properties of the object. The XML provided must be a valid UPnP device description document.</para></remarks>
    /// <param name="deviceDescriptionXml">A UPnP device description XML document.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="deviceDescriptionXml"/> argument is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown if the <paramref name="deviceDescriptionXml"/> argument is empty.</exception>
    protected SsdpDevice(string deviceDescriptionXml)
      : this()
    {
      if (deviceDescriptionXml == null) throw new ArgumentNullException("deviceDescriptionXml");
      if (deviceDescriptionXml.Length == 0) throw new ArgumentException("deviceDescriptionXml cannot be an empty string.", "deviceDescriptionXml");

      using (var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(deviceDescriptionXml)))
      {
        using (var reader = XmlReader.Create(ms))
        {
          LoadDeviceProperties(reader, this);
        }
      }
    }

    #endregion

    #region Public Properties

    #region UPnP Device Description Properties

    /// <summary>
    /// Sets or returns the core device type (not including namespace, version etc.). Required.
    /// </summary>
    /// <remarks><para>Defaults to the UPnP basic device type.</para></remarks>
    /// <seealso cref="DeviceTypeNamespace"/>
    /// <seealso cref="DeviceVersion"/>
    /// <seealso cref="FullDeviceType"/>
    public string DeviceType { get; set; }

    /// <summary>
    /// Sets or returns the namespace for the <see cref="DeviceType"/> of this device. Optional, but defaults to UPnP schema so should be changed if <see cref="DeviceType"/> is not a UPnP device type.
    /// </summary>
    /// <remarks><para>Defaults to the UPnP standard namespace.</para></remarks>
    /// <seealso cref="DeviceType"/>
    /// <seealso cref="DeviceVersion"/>
    /// <seealso cref="FullDeviceType"/>
    public string DeviceTypeNamespace { get; set; }

    /// <summary>
    /// Sets or returns the version of the device type. Optional, defaults to 1.
    /// </summary>
    /// <remarks><para>Defaults to a value of 1.</para></remarks>
    /// <seealso cref="DeviceType"/>
    /// <seealso cref="DeviceTypeNamespace"/>
    /// <seealso cref="FullDeviceType"/>
    public int DeviceVersion { get; set; }

    /// <summary>
    /// Returns the full device type string.
    /// </summary>
    /// <remarks>
    /// <para>The format used is urn:<see cref="DeviceTypeNamespace"/>:device:<see cref="DeviceType"/>:<see cref="DeviceVersion"/></para>
    /// </remarks>
    public string FullDeviceType
    {
      get
      {
        return string.Format("urn:{0}:device:{1}:{2}",
        this.DeviceTypeNamespace ?? string.Empty,
        this.DeviceType ?? string.Empty,
        this.DeviceVersion);
      }
    }

    /// <summary>
    /// Sets or returns the universally unique identifier for this device (without the uuid: prefix). Required.
    /// </summary>
    /// <remarks>
    /// <para>Must be the same over time for a specific device instance (i.e. must survive reboots).</para>
    /// <para>For UPnP 1.0 this can be any unique string. For UPnP 1.1 this should be a 128 bit number formatted in a specific way, preferably generated using the time and MAC based algorithm. See section 1.1.4 of http://upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.1.pdf for details.</para>
    /// <para>Technically this library implements UPnP 1.0, so any value is allowed, but we advise using UPnP 1.1 compatible values for good behaviour and forward compatibility with future versions.</para>
    /// </remarks>
    public string Uuid { get; set; }

    /// <summary>
    /// Returns (or sets*) a unique device name for this device. Optional, not recommended to be explicitly set.
    /// </summary>
    /// <remarks>
    /// <para>* In general you should not explicitly set this property. If it is not set (or set to null/empty string) the property will return a UDN value that is correct as per the UPnP specification, based on the other device properties.</para>
    /// <para>The setter is provided to allow for devices that do not correctly follow the specification (when we discover them), rather than to intentionally deviate from the specification.</para>
    /// <para>If a value is explicitly set, it is used verbatim, and so any prefix (such as uuid:) must be provided in the value.</para>
    /// </remarks>
    public string Udn
    {
      get
      {
        if (String.IsNullOrEmpty(_Udn) && !String.IsNullOrEmpty(this.Uuid))
          return "uuid:" + this.Uuid;
        else
          return _Udn;
      }
      set
      {
        _Udn = value;
      }
    }

    /// <summary>
    /// Sets or returns a friendly/display name for this device on the network. Something the user can identify the device/instance by, i.e Lounge Main Light. Required.
    /// </summary>
    /// <remarks><para>A short description for the end user. </para></remarks>
    public string FriendlyName { get; set; }

    /// <summary>
    /// Sets or returns the name of the manufacturer of this device. Required.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Sets or returns a URL to the manufacturers web site. Optional.
    /// </summary>
    public Uri ManufacturerUrl { get; set; }

    /// <summary>
    /// Sets or returns a description of this device model. Recommended.
    /// </summary>
    /// <remarks><para>A long description for the end user.</para></remarks>
    public string ModelDescription { get; set; }

    /// <summary>
    /// Sets or returns the name of this model. Required.
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Sets or returns the number of this model. Recommended.
    /// </summary>
    public string ModelNumber { get; set; }

    /// <summary>
    /// Sets or returns a URL to a web page with details of this device model. Optional.
    /// </summary>
    /// <remarks>
    /// <para>Optional. May be relative to base URL.</para>
    /// </remarks>
    public Uri ModelUrl { get; set; }

    /// <summary>
    /// Sets or returns the serial number for this device. Recommended.
    /// </summary>
    public string SerialNumber { get; set; }

    /// <summary>
    /// Sets or returns the universal product code of the device, if any. Optional.
    /// </summary>
    /// <remarks>
    /// <para>If not blank, must be exactly 12 numeric digits.</para>
    /// </remarks>
    public string Upc { get; set; }

    /// <summary>
    /// Sets or returns the URL to a web page that can be used to configure/manager/use the device. Recommended.
    /// </summary>
    /// <remarks>
    /// <para>May be relative to base URL. </para>
    /// </remarks>
    public Uri PresentationUrl { get; set; }

    #endregion

    /// <summary>
    /// Returns a list of icons (images) that can be used to display this device. Optional, but recommended you provide at least one at 48x48 pixels.
    /// </summary>
    public IList<SsdpDeviceIcon> Icons
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns a read-only enumerable set of <see cref="SsdpDevice"/> objects representing children of this device. Child devices are optional.
    /// </summary>
    /// <seealso cref="AddDevice"/>
    /// <seealso cref="RemoveDevice"/>
    public IEnumerable<SsdpDevice> Devices
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns a dictionary of <see cref="SsdpDeviceProperty"/> objects keyed by <see cref="SsdpDeviceProperty.FullName"/>. Each value represents a custom property in the device description document.
    /// </summary>
    public SsdpDevicePropertiesCollection CustomProperties
    {
      get
      {
        return _CustomProperties;
      }
    }

    /// <summary>
    /// Provides a list of additional information to provide about this device in search response and notification messages.
    /// </summary>
    /// <remarks>
    /// <para>The headers included here are included in the (HTTP headers) for search response and alive notifications sent in relation to this device.</para>
    /// <para>Only values specified directly on this <see cref="SsdpDevice"/> instance will be included, headers from ancestors are not automatically included.</para>
    /// </remarks>
    public CustomHttpHeadersCollection CustomResponseHeaders
    {
      get
      {
        return _CustomResponseHeaders;
      }
    }

    /// <summary>
    /// Returns a read-only enumerable set of <see cref="SsdpService"/> objects representing services associated with this device.
    /// </summary>
    /// <seealso cref="AddService"/>
    /// <seealso cref="RemoveService"/>
    public IEnumerable<SsdpService> Services
    {
      get;
      private set;
    }

    #endregion

    #region Public Methods

    #region Child Device Methods

    /// <summary>
    /// Adds a child device to the <see cref="Devices"/> collection.
    /// </summary>
    /// <param name="device">The <see cref="SsdpEmbeddedDevice"/> instance to add.</param>
    /// <remarks>
    /// <para>If the device is already a member of the <see cref="Devices"/> collection, this method does nothing.</para>
    /// <para>Also sets the <see cref="SsdpEmbeddedDevice.RootDevice"/> property of the added device and all descendant devices to the relevant <see cref="SsdpRootDevice"/> instance.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="device"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <paramref name="device"/> is already associated with a different <see cref="SsdpRootDevice"/> instance than used in this tree. Can occur if you try to add the same device instance to more than one tree. Also thrown if you try to add a device to itself.</exception>
    /// <seealso cref="DeviceAdded"/>
    public void AddDevice(SsdpEmbeddedDevice device)
    {
      if (device == null) throw new ArgumentNullException(nameof(device));
      if (device.RootDevice != null && device.RootDevice != this.ToRootDevice()) throw new InvalidOperationException("This device is already associated with a different root device (has been added as a child in another branch).");
      if (device == this) throw new InvalidOperationException("Can't add device to itself.");

      bool wasAdded = false;
      lock (_Devices)
      {
        if (!ChildDeviceExists(device))
        {
          device.RootDevice = this.ToRootDevice();
          _Devices.Add(device);
          wasAdded = true;
        }
      }

      if (wasAdded)
        OnDeviceAdded(device);
    }

    /// <summary>
    /// Removes a child device from the <see cref="Devices"/> collection.
    /// </summary>
    /// <param name="device">The <see cref="SsdpEmbeddedDevice"/> instance to remove.</param>
    /// <remarks>
    /// <para>If the device is not a member of the <see cref="Devices"/> collection, this method does nothing.</para>
    /// <para>Also sets the <see cref="SsdpEmbeddedDevice.RootDevice"/> property to null for the removed device and all descendant devices.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="device"/> argument is null.</exception>
    /// <seealso cref="DeviceRemoved"/>
    public void RemoveDevice(SsdpEmbeddedDevice device)
    {
      if (device == null) throw new ArgumentNullException(nameof(device));

      bool wasRemoved = false;
      lock (_Devices)
      {
        wasRemoved = _Devices.Remove(device);
      }

      if (wasRemoved)
      {
        device.RootDevice = null;
        OnDeviceRemoved(device);
      }
    }

    /// <summary>
    /// Raises the <see cref="DeviceAdded"/> event.
    /// </summary>
    /// <param name="device">The <see cref="SsdpEmbeddedDevice"/> instance added to the <see cref="Devices"/> collection.</param>
    /// <seealso cref="AddDevice"/>
    /// <seealso cref="DeviceAdded"/>		
    protected virtual void OnDeviceAdded(SsdpEmbeddedDevice device)
    {
      DeviceAdded?.Invoke(this, new DeviceEventArgs(device));
    }

    /// <summary>
    /// Raises the <see cref="DeviceRemoved"/> event.
    /// </summary>
    /// <param name="device">The <see cref="SsdpEmbeddedDevice"/> instance removed from the <see cref="Devices"/> collection.</param>
    /// <seealso cref="RemoveDevice"/>
    /// <see cref="DeviceRemoved"/>
    protected virtual void OnDeviceRemoved(SsdpEmbeddedDevice device)
    {
      DeviceRemoved?.Invoke(this, new DeviceEventArgs(device));
    }

    #endregion

    #region Service Methods

    /// <summary>
    /// Adds a service to the <see cref="Services"/> collection.
    /// </summary>
    /// <param name="service">The <see cref="SsdpService"/> instance to add.</param>
    /// <remarks>
    /// <para>If the service is already a member of the <see cref="Services"/> collection, this method does nothing.</para>
    /// <para>Services should be added to the device before it is added to a publisher.</para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="service"/> argument is null.</exception>
    /// <seealso cref="ServiceAdded"/>
    public void AddService(SsdpService service)
    {
      if (service == null) throw new ArgumentNullException(nameof(service));

      bool wasAdded = false;
      lock (_Services)
      {
        if (!ChildServiceExists(service))
        {
          _Services.Add(service);
          wasAdded = true;
        }
      }

      if (wasAdded)
        OnServiceAdded(service);
    }

    /// <summary>
    /// Removes a service from the <see cref="Services"/> collection.
    /// </summary>
    /// <param name="service">The <see cref="SsdpService"/> instance to remove.</param>
    /// <remarks>
    /// <para>If the service is not a member of the <see cref="Services"/> collection, this method does nothing.</para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="service"/> argument is null.</exception>
    /// <seealso cref="ServiceRemoved"/>
    public void RemoveService(SsdpService service)
    {
      if (service == null) throw new ArgumentNullException(nameof(service));

      bool wasRemoved = false;
      lock (_Services)
      {
        wasRemoved = _Services.Remove(service);
      }

      if (wasRemoved)
        OnServiceRemoved(service);
    }

    /// <summary>
    /// Raises the <see cref="ServiceAdded"/> event.
    /// </summary>
    /// <param name="service">The <see cref="SsdpService"/> instance added to the <see cref="Services"/> collection.</param>
    /// <seealso cref="AddService"/>
    /// <seealso cref="ServiceAdded"/>		
    protected virtual void OnServiceAdded(SsdpService service)
    {
      ServiceAdded?.Invoke(this, new ServiceEventArgs(service));
    }

    /// <summary>
    /// Raises the <see cref="ServiceRemoved"/> event.
    /// </summary>
    /// <param name="service">The <see cref="SsdpService"/> instance removed from the <see cref="Services"/> collection.</param>
    /// <seealso cref="RemoveService"/>
    /// <see cref="ServiceRemoved"/>
    protected virtual void OnServiceRemoved(SsdpService service)
    {
      ServiceRemoved?.Invoke(this, new ServiceEventArgs(service));
    }

    #endregion

    /// <summary>
    /// Writes this device to the specified <see cref="System.Xml.XmlWriter"/> as a device node and it's content.
    /// </summary>
    /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> to output to.</param>
    /// <param name="device">The <see cref="SsdpDevice"/> to write out.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="writer"/> or <paramref name="device"/> argument is null.</exception>
    protected virtual void WriteDeviceDescriptionXml(XmlWriter writer, SsdpDevice device)
    {
      if (writer == null) throw new ArgumentNullException(nameof(writer));
      if (device == null) throw new ArgumentNullException(nameof(device));

      writer.WriteStartElement("device");

      if (!string.IsNullOrEmpty(device.FullDeviceType))
        WriteNodeIfNotEmpty(writer, "deviceType", device.FullDeviceType);

      WriteNodeIfNotEmpty(writer, "friendlyName", device.FriendlyName);
      WriteNodeIfNotEmpty(writer, "manufacturer", device.Manufacturer);
      WriteNodeIfNotEmpty(writer, "manufacturerURL", device.ManufacturerUrl);
      WriteNodeIfNotEmpty(writer, "modelDescription", device.ModelDescription);
      WriteNodeIfNotEmpty(writer, "modelName", device.ModelName);
      WriteNodeIfNotEmpty(writer, "modelNumber", device.ModelNumber);
      WriteNodeIfNotEmpty(writer, "modelURL", device.ModelUrl);
      WriteNodeIfNotEmpty(writer, "presentationURL", device.PresentationUrl);
      WriteNodeIfNotEmpty(writer, "serialNumber", device.SerialNumber);
      WriteNodeIfNotEmpty(writer, "UDN", device.Udn);
      WriteNodeIfNotEmpty(writer, "UPC", device.Upc);

      WriteCustomProperties(writer, device);
      WriteIcons(writer, device);
      WriteChildDevices(writer, device);
      WriteServices(writer, device);

      writer.WriteEndElement();
    }

    /// <summary>
    /// Converts a string to a <see cref="Uri"/>, or returns null if the string provided is null.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A <see cref="Uri"/>.</returns>
    protected static Uri StringToUri(string value)
    {
      if (!string.IsNullOrEmpty(value))
        return new Uri(value, UriKind.RelativeOrAbsolute);

      return null;
    }

    #endregion

    #region Private Methods

    #region Serialisation Methods

    private static void WriteCustomProperties(XmlWriter writer, SsdpDevice device)
    {
      foreach (var prop in device.CustomProperties)
      {
        writer.WriteElementString(prop.Namespace, prop.Name, SsdpConstants.SsdpDeviceDescriptionXmlNamespace, prop.Value);
      }
    }

    private static void WriteIcons(XmlWriter writer, SsdpDevice device)
    {
      if (device.Icons.Any())
      {
        writer.WriteStartElement("iconList");

        foreach (var icon in device.Icons)
        {
          writer.WriteStartElement("icon");

          writer.WriteElementString("mimetype", icon.MimeType);
          writer.WriteElementString("width", icon.Width.ToString());
          writer.WriteElementString("height", icon.Height.ToString());
          writer.WriteElementString("depth", icon.ColorDepth.ToString());
          writer.WriteElementString("url", icon.Url.ToString());

          writer.WriteEndElement();
        }

        writer.WriteEndElement();
      }
    }

    private void WriteChildDevices(XmlWriter writer, SsdpDevice parentDevice)
    {
      if (parentDevice.Devices.Any())
      {
        writer.WriteStartElement("deviceList");

        foreach (var device in parentDevice.Devices)
        {
          WriteDeviceDescriptionXml(writer, device);
        }

        writer.WriteEndElement();
      }
    }

    private static void WriteNodeIfNotEmpty(XmlWriter writer, string nodeName, string value)
    {
      if (!string.IsNullOrEmpty(value))
        writer.WriteElementString(nodeName, value);
    }

    private static void WriteNodeIfNotEmpty(XmlWriter writer, string nodeName, Uri value)
    {
      if (value != null)
        writer.WriteElementString(nodeName, value.ToString());
    }

    private static void WriteServices(XmlWriter writer, SsdpDevice device)
    {
      if (device.Services.Any())
      {
        writer.WriteStartElement("serviceList");
        foreach (var service in device.Services)
        {
          WriteService(writer, service);
        }
        writer.WriteEndElement();
      }
    }

    private static void WriteService(XmlWriter writer, SsdpService service)
    {
      writer.WriteStartElement("service");

      WriteNodeIfNotEmpty(writer, "serviceType", service.FullServiceType);
      WriteNodeIfNotEmpty(writer, "serviceId", service.ServiceId);
      WriteNodeIfNotEmpty(writer, "SCPDURL", service.ScpdUrl);
      WriteNodeIfNotEmpty(writer, "controlURL", service.ControlUrl);
      WriteNodeIfNotEmpty(writer, "eventSubURL", service.EventSubUrl);

      writer.WriteEndElement();
    }

    #endregion

    #region Deserialisation Methods

    private void LoadDeviceProperties(XmlReader reader, SsdpDevice device)
    {
      ReadUntilDeviceNode(reader);

      while (!reader.EOF)
      {
        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "device")
        {
          reader.Read();
          break;
        }

        if (!SetPropertyFromReader(reader, device))
          reader.Read();
      }
    }

    private static void ReadUntilDeviceNode(XmlReader reader)
    {
      while (!reader.EOF && (reader.LocalName != "device" || reader.NodeType != XmlNodeType.Element))
      {
        reader.Read();
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Yes, there is a large switch statement, not it's not really complex and doesn't really need to be rewritten at this point.")]
    private bool SetPropertyFromReader(XmlReader reader, SsdpDevice device)
    {
      switch (reader.LocalName)
      {
        case "friendlyName":
          device.FriendlyName = reader.ReadElementContentAsString();
          break;

        case "manufacturer":
          device.Manufacturer = reader.ReadElementContentAsString();
          break;

        case "manufacturerURL":
          device.ManufacturerUrl = StringToUri(reader.ReadElementContentAsString());
          break;

        case "modelDescription":
          device.ModelDescription = reader.ReadElementContentAsString();
          break;

        case "modelName":
          device.ModelName = reader.ReadElementContentAsString();
          break;

        case "modelNumber":
          device.ModelNumber = reader.ReadElementContentAsString();
          break;

        case "modelURL":
          device.ModelUrl = StringToUri(reader.ReadElementContentAsString());
          break;

        case "presentationURL":
          device.PresentationUrl = StringToUri(reader.ReadElementContentAsString());
          break;

        case "serialNumber":
          device.SerialNumber = reader.ReadElementContentAsString();
          break;

        case "UDN":
          device.Udn = reader.ReadElementContentAsString();
          SetUuidFromUdn(device);
          break;

        case "UPC":
          device.Upc = reader.ReadElementContentAsString();
          break;

        case "deviceType":
          SetDeviceTypePropertiesFromFullDeviceType(device, reader.ReadElementContentAsString());
          break;

        case "iconList":
          reader.Read();
          LoadIcons(reader, device);
          break;

        case "deviceList":
          reader.Read();
          LoadChildDevices(reader, device);
          break;

        case "serviceList":
          reader.Read();
          LoadServices(reader, device);
          if (!reader.EOF && reader.NodeType == XmlNodeType.EndElement && reader.Name == "serviceList")
            reader.Read();
          break;

        default:
          if (reader.NodeType == XmlNodeType.Element && reader.Name != "device" && reader.Name != "icon")
          {
            AddCustomProperty(reader, device);
            break;
          }
          else
            return false;
      }
      return true;
    }

    private static void LoadServices(XmlReader reader, SsdpDevice device)
    {
      while (!reader.EOF && reader.NodeType != XmlNodeType.Element)
      {
        reader.Read();
      }

      while (!reader.EOF)
      {
        while (!reader.EOF && reader.NodeType != XmlNodeType.Element && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "serviceList"))
        {
          reader.Read();
        }

        if (reader.LocalName == "service")
        {
          var service = new SsdpService(reader.ReadOuterXml());
          device.AddService(service);
        }
        else
          break;
      }
    }

    private static void SetDeviceTypePropertiesFromFullDeviceType(SsdpDevice device, string value)
    {
      if (string.IsNullOrEmpty(value) || !value.Contains(":"))
        device.DeviceType = value;
      else
      {
        var parts = value.Split(':');
        if (parts.Length == 5)
        {
          if (int.TryParse(parts[4], out int deviceVersion))
          {
            device.DeviceTypeNamespace = parts[1];
            device.DeviceType = parts[3];
            device.DeviceVersion = deviceVersion;
          }
          else
            device.DeviceType = value;
        }
        else
          device.DeviceType = value;
      }
    }

    private static void SetUuidFromUdn(SsdpDevice device)
    {
      if (device.Udn != null && device.Udn.StartsWith("uuid:", StringComparison.OrdinalIgnoreCase))
        device.Uuid = device.Udn.Substring(5).Trim();
      else
        device.Uuid = device.Udn;
    }

    private static void LoadIcons(XmlReader reader, SsdpDevice device)
    {
      while (!reader.EOF && reader.NodeType != XmlNodeType.Element)
      {
        reader.Read();
      }

      while (!reader.EOF)
      {
        while (!reader.EOF && reader.NodeType != XmlNodeType.Element && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "iconList"))
        {
          reader.Read();
        }

        if (reader.LocalName == "icon")
        {
          var icon = new SsdpDeviceIcon();
          LoadIconProperties(reader, icon);
          device.Icons.Add(icon);
        }
        else
          break;
      }
    }

    private static void LoadIconProperties(XmlReader reader, SsdpDeviceIcon icon)
    {
      while (!reader.EOF && (reader.LocalName != "icon" || reader.NodeType != XmlNodeType.Element))
      {
        reader.Read();
      }

      while (!reader.EOF)
      {
        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "icon")
        {
          reader.Read();
          break;
        }

        switch (reader.LocalName)
        {
          case "depth":
            icon.ColorDepth = reader.ReadElementContentAsInt();
            break;

          case "height":
            icon.Height = reader.ReadElementContentAsInt();
            break;

          case "width":
            icon.Width = reader.ReadElementContentAsInt();
            break;

          case "mimetype":
            icon.MimeType = reader.ReadElementContentAsString();
            break;

          case "url":
            icon.Url = StringToUri(reader.ReadElementContentAsString());
            break;

          default:
            reader.Read();
            break;
        }
      }
    }

    private void LoadChildDevices(XmlReader reader, SsdpDevice device)
    {
      while (!reader.EOF && reader.NodeType != XmlNodeType.Element)
      {
        reader.Read();
      }

      while (!reader.EOF)
      {
        while (!reader.EOF && reader.NodeType != XmlNodeType.Element)
        {
          reader.Read();
        }

        if (reader.LocalName == "device")
        {
          var childDevice = new SsdpEmbeddedDevice();
          LoadDeviceProperties(reader, childDevice);
          device.AddDevice(childDevice);
        }
        else
          break;
      }
    }

    private static void AddCustomProperty(XmlReader reader, SsdpDevice device)
    {
      // If the property is an empty element, there is no value to read
      // Advance the reader and return
      if (reader.IsEmptyElement)
      {
        reader.Read();
        return;
      }

      var newProp = new SsdpDeviceProperty() { Namespace = reader.Prefix, Name = reader.LocalName };
      int depth = reader.Depth;
      reader.Read();
      while (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment)
      {
        reader.Read();
      }

      if (reader.NodeType != XmlNodeType.CDATA && reader.NodeType != XmlNodeType.Text)
      {
        while (!reader.EOF && (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != newProp.Name || reader.Prefix != newProp.Namespace || reader.Depth != depth))
        {
          reader.Read();
        }
        if (!reader.EOF)
          reader.Read();
        return;
      }

      newProp.Value = reader.Value;

      // We don't support complex nested types or repeat/multi-value properties
      if (!device.CustomProperties.Contains(newProp.FullName))
        device.CustomProperties.Add(newProp);
    }

    #endregion

    private bool ChildDeviceExists(SsdpDevice device)
    {
      return (from d in _Devices where device.Uuid == d.Uuid select d).Any();
    }

    private bool ChildServiceExists(SsdpService service)
    {
      return (from s in _Services where service.Uuid == s.Uuid select s).Any();
    }

    #endregion

  }
}
