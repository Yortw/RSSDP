using Rssdp.Infrastructure;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Rssdp
{
  /// <summary>
  /// Represents a 'root' device, a device that has no parent. Used for publishing devices and for the root device in a tree of discovered devices.
  /// </summary>
  /// <remarks>
  /// <para>Child (embedded) devices are represented by the <see cref="SsdpDevice"/> in the <see cref="SsdpDevice.Devices"/> property.</para>
  /// <para>Root devices contain some information that applies to the whole device tree and is therefore not present on child devices, such as <see cref="CacheLifetime"/> and <see cref="Location"/>.</para>
  /// </remarks>
  public class SsdpRootDevice : SsdpDevice
  {

    #region Fields

    private Uri _UrlBase;

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    public SsdpRootDevice() : base()
    {
    }

    /// <summary>
    /// Deserialisation constructor.
    /// </summary>
    /// <param name="location">The url from which the device description document was retrieved.</param>
    /// <param name="cacheLifetime">A <see cref="TimeSpan"/> representing the time maximum period of time the device description can be cached for.</param>
    /// <param name="deviceDescriptionXml">The device description XML as a string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="deviceDescriptionXml"/> or <paramref name="location"/> arguments are null.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="deviceDescriptionXml"/> argument is empty.</exception>
    public SsdpRootDevice(Uri location, TimeSpan cacheLifetime, string deviceDescriptionXml)
      : base(deviceDescriptionXml)
    {
      if (location == null) throw new ArgumentNullException(nameof(location));

      CacheLifetime = cacheLifetime;
      Location = location;

      LoadFromDescriptionDocument(deviceDescriptionXml);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Specifies how long clients can cache this device's details for. Optional but defaults to <see cref="TimeSpan.Zero"/> which means no-caching. Recommended value is half an hour.
    /// </summary>
    /// <remarks>
    /// <para>Specifiy <see cref="TimeSpan.Zero"/> to indicate no caching allowed.</para>
    /// <para>Also used to specify how often to rebroadcast alive notifications.</para>
    /// <para>The UPnP/SSDP specifications indicate this should not be less than 1800 seconds (half an hour), but this is not enforced by this library.</para>
    /// </remarks>
    public TimeSpan CacheLifetime { get; set; }

    /// <summary>
    /// Gets or sets the URL used to retrieve the description document for this device/tree. Required.
    /// </summary>
    public Uri Location { get; set; }


    /// <summary>
    /// The base URL to use for all relative url's provided in other propertise (and those of child devices). Optional.
    /// </summary>
    /// <remarks>
    /// <para>Defines the base URL. Used to construct fully-qualified URLs. All relative URLs that appear elsewhere in the description are combined with this base URL. If URLBase is empty or not given, the base URL is the URL from which the device description was retrieved (which is the preferred implementation; use of URLBase is no longer recommended). Specified by UPnP vendor. Single URL.</para>
    /// </remarks>
    public Uri UrlBase
    {
      get
      {
        return _UrlBase ?? Location;
      }
      set
      {
        _UrlBase = value;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Saves the property values of this device object to an a string in the full UPnP device description XML format, including child devices and outer root node and XML document declaration.
    /// </summary>
    /// <returns>A string containing XML in the UPnP device description format</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Dispsoing memory stream twice is 'safe' and easier to read than correct code for ensuring it is only closed once.")]
    public virtual string ToDescriptionDocument()
    {
      if (string.IsNullOrEmpty(Uuid)) throw new InvalidOperationException("Must provide a UUID value.");

      //This would have been so much nicer with Xml.Linq, but that's
      //not available until .NET 4.03 at the earliest, and I want to 
      //target 4.0 :(
      using (var ms = new MemoryStream())
      {
        using (var writer = XmlWriter.Create(ms, new XmlWriterSettings() { CloseOutput = false, Encoding = Encoding.UTF8, Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates }))
        {
          writer.WriteStartDocument();
          writer.WriteStartElement("root", SsdpConstants.SsdpDeviceDescriptionXmlNamespace);

          writer.WriteStartElement("specVersion");
          writer.WriteElementString("major", "1");
          writer.WriteElementString("minor", "0");
          writer.WriteEndElement();

          if (UrlBase != null && UrlBase != Location)
            writer.WriteElementString("URLBase", UrlBase.ToString());

          WriteDeviceDescriptionXml(writer, this);

          writer.WriteEndElement();
        }

        ms.Seek(0, SeekOrigin.Begin);

        using (var reader = new StreamReader(ms))
        {
          return reader.ReadToEnd();
        }
      }
    }

    #endregion

    #region Private Methods

    #region Deserialisation Methods

    private void LoadFromDescriptionDocument(string deviceDescriptionXml)
    {
      using (var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(deviceDescriptionXml)))
      {
        using (var reader = XmlReader.Create(ms))
        {
          while (!reader.EOF)
          {
            reader.Read();
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "root") continue;

            while (!reader.EOF)
            {
              reader.Read();

              if (reader.NodeType != XmlNodeType.Element) continue;

              if (reader.LocalName == "URLBase")
              {
                UrlBase = StringToUri(reader.ReadElementContentAsString());
                break;
              }
            }
          }
        }
      }
    }

    #endregion

    #endregion

  }
}