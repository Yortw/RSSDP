# (Really) Simple Service Discovery Protocol For .Net

## What is RSSDP ?
RSSDP is a 100% .Net implementation of the Simple Service Discovery (SSDP) protocol that is part of the Universal Plug and Play (UPnP) standard. SSDP allows you
to discover devices and services on a (local) network.

RSSDP is designed primarily to publish and discover custom or 'basic' devices, and as such does not implement the full UPnP device architecture. If you are 
looking to build a device for which a full UPnP device schema exists, this is not the library for you (sorry! though I guess you can fork and extend if you like).
If you are looking for a way to discover a custom service (such as a proprietary REST or SOAP service) from a device, RSSDP might be the solution for you.

[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/Yortw/RSSDP/blob/master/LICENSE.md) 

## Supported Platforms
Currently;

* .Net Framework 4.03+
* Windows Phone Silverlight (8.0+) 
* Xamarin.iOS Unified
* Xamarin.Android (Only tested with emulator)
* WinRT (Windows Store Apps 8.1)
* UWP 10+ (Windows 10 Universal Programs)
* .NET Core (ASP.NET Core 1.0+, .NET Standard 1.3+)

## Build Status
[![Build status](https://ci.appveyor.com/api/projects/status/f4e33as09yx0lsn4?svg=true)](https://ci.appveyor.com/project/Yortw/rssdp)

## How do I use RSSDP?
*We got your samples right here*

There is a sample console applicaton included in the repository. If you don't want to "read the source, Luke", then here's some tips and examples to get you started.

**One common gotcha to look out for:** SSDP root devices must publish an xml document describing themselves and any embedded devices, and this document must be published on a url that can be accessed via an HTTP GET.
RSSDP will return devices in search results and notifications regardless of whether this document is actually accessible (it is up to you to retrieve the document if you care, and handle any exceptions that occur doing so).
However, many other SSDP device locators (such as Intel's Device Spy application) will not report devices if the url cannot be accessed, the document is invalid, or data in the document (such as the UUID) does not 
match the associated notification or search request. For this reason, if you are using another tool to locate devices published with RSSDP, ensure you are publishing a correct document on the url specified in the Location
property of your root device, or else the device may not be found. 

Install the Nuget package like this;

```powershell
    PM> Install-Package Rssdp
```

[![NuGet Badge](https://buildstats.info/nuget/RSSDP)](https://www.nuget.org/packages/RSSDP/)

Or reference the Rssdp.Portable.dll assembly AND the assembly that matches your app's platform, i.e Rssdp.NetFX40.dll for .Net 4+.

### Publishing a Device
Only three steps to do this. Create a device definition, create a publisher, add the device to the publisher;

```C#

using Rssdp;
// Declare \_Publisher as a field somewhere, so it doesn't get GCed after the method finishes.
private SsdpDevicePublisher _Publisher;

// Call this method from somewhere to actually do the publish.
public void PublishDevice()
{
    // As this is a sample, we are only setting the minimum required properties.
    var deviceDefinition = new SsdpRootDevice()
    {
    	CacheLifetime = TimeSpan.FromMinutes(30), //How long SSDP clients can cache this info.
    	Location = new Uri("http://mydevice/descriptiondocument.xml"), // Must point to the URL that serves your devices UPnP description document. 
    	DeviceTypeNamespace = "my-namespace",
    	DeviceType = "MyCustomDevice",
    	FriendlyName = "Custom Device 1",
    	Manufacturer = "Me",
    	ModelName = "MyCustomDevice",
    	Uuid = GetPersistentUuid() // This must be a globally unique value that survives reboots etc. Get from storage or embedded hardware etc.
    };
}

//Note, you can use deviceDefinition.ToDescriptionDocumentText() to retrieve the data to 
//return from the Location end point, you just need to get that data to your service
//implementation somehow. Depends on how you've implemented your service.

_Publisher = new SsdpDevicePublisher();
_Publisher.AddDevice(deviceDefinition);    
```

### Discovering Devices
Basically, just create an SsdpDeviceLocator object and call the search method. By default the method will search for all devices, but you can specify a search target string in the following formats;

* ssdp:all
* upnp:rootdevice
* uuid:&lt;device's unique identifier&gt;
* urn:&lt;fully qualified device type&gt;

The format of a fully qualified device type is;
urn:&lt;device namespace&gt;:device:&lt;device type&gt;:&lt;device version&gt;

i.e

* uuid:CAA42739-8F87-4463-B747-6F6DDB301A06
* urn:schemas-upnp-org:device:Basic:1

#### Simple Search
Simple search is easy but requires you to wait for the full search to finish before getting any results back. 
This is asynchronous and  returns a task which you can choose to wait on (or not), but you must wait for the task to complete before accessing the results.

```C#
using Rssdp;

//Call this method from somewhere to begin the search.
public async void SearchForDevices()
{
    // This code goes in a method somewhere.
    using (var deviceLocator = new SsdpDeviceLocator())
    {
        var foundDevices = await deviceLocator.SearchAsync(); // Can pass search arguments here (device type, uuid). No arguments means all devices.

        foreach (var foundDevice in foundDevices)
        {
        	// Device data returned only contains basic device details and location ]
        	// of full device description.
        	Console.WriteLine("Found " + foundDevice.Usn + " at " + foundDevice.DescriptionLocation.ToString());
            
        	// Can retrieve the full device description easily though.
        	var fullDevice = await foundDevice.GetDeviceInfo();
        	Console.WriteLine(fullDevice.FriendlyName);
        	Console.WriteLine();
        }
    }
}
```

#### Event Driven Search & Discovery via Notifications
Event driven search is the same as 'simple' search but instead of looking at the task return value, you subscribe to the DeviceAvailable and DeviceUnavailable events to handle results.
These events are raised each time a search response is received, as well as whenever a status notification is broadcast from a device. By responding to the events you 
can process results sooner than waiting for all results to come back from a completed task. You can also monitor for new devices arriving on the network or existing devices disappearing 
without having to repeatedly call search. Notifications can also be used in conjunction with simple search, just call the StartListeningForNotifications method before searching and handle the events.

```C#
using Rssdp;
// Define _DeviceLocator as a field so it doesn't get GCed after the method ends, and it can
// continue to listen for notifications until it is explicitly stopped 
// (with a call to _DeviceLocator.StopListeningForNotifications();)
private SsdpDeviceLocator _DeviceLocator;

// Call this method from somewhere in your code to start the search.
public void BeginSearch()
{
    _DeviceLocator = new SsdpDeviceLocator();

    // (Optional) Set the filter so we only see notifications for devices we care about 
    // (can be any search target value i.e device type, uuid value etc - any value that appears in the 
    // DiscoverdSsdpDevice.NotificationType property or that is used with the searchTarget parameter of the Search method).
    _DeviceLocator.NotificationFilter = "upnp:rootdevice";

    // Connect our event handler so we process devices as they are found
    _DeviceLocator.DeviceAvailable += deviceLocator_DeviceAvailable;

    // Enable listening for notifications (optional)
    _DeviceLocator.StartListeningForNotifications();

    // Perform a search so we don't have to wait for devices to broadcast notifications 
    // again to get any results right away (notifications are broadcast periodically).
    _DeviceLocator.SearchAsync();

    Console.ReadLine();
}

// Process each found device in the event handler
async static void deviceLocator_DeviceAvailable(object sender, DeviceAvailableEventArgs e)
{
	//Device data returned only contains basic device details and location of full device description.
	Console.WriteLine("Found " + e.DiscoveredDevice.Usn + " at " + e.DiscoveredDevice.DescriptionLocation.ToString());
    
	//Can retrieve the full device description easily though.
	var fullDevice = await e.DiscoveredDevice.GetDeviceInfo();
	Console.WriteLine(fullDevice.FriendlyName);
	Console.WriteLine();
}
```

## Why RSSDP?
*Aren't there already lots of SSDP implementations?*

I needed to find a custom/proprietary service on local networks from a mobile device. I decided this had been done before and I shouldn't re-invent the wheel so I started looking for 
existing, standard protocols that did this. I decided Zeroconf and SSDP seemed like the best two, and Zeroconf looked like the more efficient, less overhead option. Unfortunately I also need a solution where

* I could publish a device. Many other libraries only focus on discovery.
* the publish component runs on (at least) .Net 4.0, without relying on any external services. Many other implementations are just wrappers around a Windows or Linux service, which I couldn't guarantee would be installed/enabled etc.
* the discovery component (at least) runs on Windows Phone and Xamarin.iOS. Preferably also .Net 4.0, Xamain.Android, WinRT and Compact Framework projects. A lot of other implementations don't support the Xamarin platforms.
* the API was consistent across platforms so I can write as little code with as little conditional compilation as possible (especially in Xamarin Forms projects).
* the library wasn't massive and didn't have huge numbers of dependencies, I want to keep my deployment footprint as small as possible.
* the library guided me (at least a little) towards publishing devices correctly, i.e correct device types, not leaving out required fields etc.

Sadly, I couldn't find a .Net implementation that met the criteria (I found some Node.Js and implementations in other languages that might have worked, but not in my environment/with my tools). Maybe I didn't look hard enough 
but that's where I ended up. Having failed on Zeroconf I went looking for SSDP implementations that met the same goals, and had exactly the same problem. I then looked at implementing each protocol and while Zeroconf looked 
better overall, it *seemed* less well documented and harder to implement. At the point where I decided this wheel needed reinventing** I chose SSDP.

 ** Have you ever thought about how many different, useful, kinds of wheel there are in the world? Train wheels won't work on a bicycle, and bicycle wheels won't work on a car etc. Often people who say don't re-invent the wheel 
haven't really considered how many variations of a wheel might be needed.

## References
Reference materials used while writing this library, or that may be useful to people working with RSSDP who are not familiar with SSDP/UPnP device types and protocols.

* [SSDP Draft 1.03 Specification](http://tools.ietf.org/html/draft-cai-ssdp-v1-03 "SSDP Draft Spec 1.03")
* [UPnP 1.0 Specification](http://tools.ietf.org/html/draft-cai-ssdp-v1-03 "UPnP 1.0 Spec")
* [UPnP 1.1 Specification](http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0-20080424.pdf "UPnP 1.1 Spec")
* [HTTP 1.1 Specification](http://tools.ietf.org/html/rfc2616 "HTTP 1.1 Spec")

* [UPnP Device Types and Descriptions](http://upnp.org/index.php/sdcps-and-certification/standards/sdcps/ "UPnP Device Types")
* [UPnP Developer Tools](http://opentools.homeip.net/dev-tools-for-upnp)

## Contributing
Contributing is encouraged! Please submit pull requests, open issues etc. However, to ensure we end up with a good result and to make my life a little easier, could I please request that;

* All changes be made in a feature branch, not in master, and please don't submit PR's directly against master.
* Make sure any PR contains (well named) new and/or updated unit tests to prove the new feature or bug fix. Failing this, please include enough sample data/problem description that I can write the tests myself.
  
Also, not required, but would be really great if;

* You could use tabs instead of spaces (and not argue about it).
* You could write the code in a similar style as what already exists. I'm not OCD about this so some deviation is fine, we all have different styles and I'm not suggesting mine is 'right', but it helps everybody 
undertand and maintain the code base when it is at least mostly uniform.

Thanks! I look forward to merging your awesomesauce.
