# Jot - A .NET library for managing application state

## Introduction 
Jot is a .NET library for persisting and applying application state. Typically, this includes sizes and locations of movable and resizable elements of the UI, last entered data (e.g. username), user settings, etc... 

Almost every application these days needs to keep track of its own state, regardless of what it otherwise does. A common approach is to store this data in a .settings file, and read and update it as needed. This involves writing a lot of boilerplate code to copy that data back and fourth. This code is tedious, error prone and generally no fun to write.  
 
Jot's job is to reduce the amount of code, mental effort and time needed to satisfy this requirement and to make it almost a non-task for developers. The library is quite flexible, it provides reasonable defaults for everything, but allows the developer to configure in detail when, how and where each piece of data will be stored and applied.


## Example: Persisting window size and location

Step 1. Create a StateTracker instance and expose it to the rest of the application (Purely for simplicity of the example, exposing the instance as a static member.) 
``` C#
static class Services
{
    public static StateTracker Tracker = new StateTracker();//use constructor overloads to configure how data is stored
}
```
Step2. Use the state tracker to keep track of a window's size, state and location.
``` C#
public MainWindow()
{
    InitializeComponent();
    
    Services.Tracker.Configure(this)//get or create a TrackingConfiguration object for the window
        .IdentifyAs("MyMainWindow")//arbitrary unique string to identify the target object
        .AddProperties<MainWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState)
        .RegisterPersistTrigger(nameof(Closed))//window.Closed will trigger persisting data
        .Apply();//apply any previous state straight away
}

```

## Configuring the StateTracker

The `StateTracker` class has several convenience constructors which provide reasonable defaults, but the main constructor allows you to specify exactly **were** the data will be stored and **when**:
  
``` C#
StateTracker(IObjectStore objectStore, ITriggerPersist globalAutoPersistTrigger)
```

### Where data is stored
The `objectStore` argument determines where data will be stored. There are several implementations of `IObjectStore` built into Jot:
- `FileStore`
- `IsolatedStorageStore`
- `AspNetSessionStore`
- `AspNetUserProfileStore `

You are, of course, free to make additional implementations of `IObjectStore` yourself and pass them to the StateTracker. 

For desktop applications, `FileStore` is commonly the appropriate choice. The file path will determine if the settings are per-user (AppData) or per-machine (AllUsersProfile). FileStore and IsolatedStorageStore will use `JsonSerialization` by default to serialize data, but there are several other serializer implementations that can be used if needed.

### When data is stored
The StateTracker will use an object that implements `ITriggerPersist` to notify it when it should do a global save of all data. For desktop applications this will be just before the application closes. 

The `ITriggerPersist` interface has just one memeber: the `PersistRequired` event and the only built-in implementation of this interface is the `DesktopPersistTrigger` class which fires the `PersistRequired` event when a desktop application is about to shut down. 

This interface serves another purpose too: you can implement it in any object you want to track, to enable the object to trigger its own persistence. This is useful because some objects might be garbage collected before the application closes. These objects should either implement `ITriggerPersist` themselves -or- you can call `stateTracker.Persist(target)` manually for them when appropriate -or- you can use any other event they expose as a trigger by calling `RegisterPersistTrigger(eventName)`, as in the example above where we use the `Window.Closed` event to trigger persisting the window's properties.      

## Types can specify their own tracking configuration 
Types can be self descriptive about how they want their instances to be tracked. They can do this in two ways: 
- using attributes (`[TrackingKey]` and `[Trackable]` --> equivalent to `IdentifyAs` and `AddProperty`)
- implementing `ITrackingAware` and/or `ITriggerPersist`

### Example: Using tracking attributes

``` C#
public class GeneralSettings
{
	[Trackable]
    public int Property1 { get; set; }
	[Trackable]
	public string Property2 { get; set; }
	[Trackable]
	public SomeComplexType Property3 { get; set; }
}
```

### Example: Using the ITrackingAware interface
``` C#
public class GeneralSettings : ITrackingAware
	{
		public int Property1 { get; set; }
		public string Property2 { get; set; }
		public SomeComplexType Property3 { get; set; }

		public void InitConfiguration(TrackingConfiguration configuration)
		{
			configuration.AddProperties<GeneralSettings>(s => s.Property1, s => s.Property2, s => s.Property3);
		}
	}
```

When a type is self descriptive about the way it wishes to be tracked, all that's needed to handle tracking is to call: 

``` C#
tracker.Configure(target).Apply();
```

This is nice because we don't need to call `AddProperty`, `IdentifyAs` etc. for each instance, the object's type determines everything itself. 

What's even more cool, when using an IOC container, we can add this line as a post-resolve step (not every container will support this but e.g. Unity and Ninject do). **If we do, all resolved objects will have tracking applied automatically**. Neat, huh?

# Example of stored data

If we're using a `FileStore` (e.g. when using the default `StateTracker` constructor), the data is serialized and saved in a file, most likely in the %appdata% folder. 

Each property value will be stored in its own XML node. The id of the property is composed of three elements:

1. Target object type (the type that owns the property)
- Target object name (what we supplied when calling `IdentifyAs` or the value of the property that has `[TrackingKey]` applied)
- Property name  

So basically the format is as follows: `TargetType`_`TargetName`.`PropertyName`

For example if the id is `MainWindow_.Left` that means:
- Target object type is `MainWindow`
- Target object name is empty (this is OK if there's only ever going to be one instance of `MainWindow`)
- The name of the property is `Left` 

Here is a full example of a storage file for a tiny demo application:

``` XML
<Data>
  <Item Id="MainWindow_.Left" Type="System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">760.0</Item>
  <Item Id="MainWindow_.Top" Type="System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">368.0</Item>
  <Item Id="MainWindow_.Height" Type="System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">583.0</Item>
  <Item Id="MainWindow_.Width" Type="System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">729.0</Item>
  <Item Id="MainWindow_.WindowState" Type="System.Windows.WindowState, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35">0</Item>
  <Item Id="AppSettings_.DisplaySettings" Type="TestWPFWithUnity.Settings.DisplaySettings, TestWPFWithUnity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">{"Font":"Corbel","FontSize":125.0}</Item>
  <Item Id="AppSettings_.GeneralSettings" Type="TestWPFWithUnity.Settings.GeneralSettings, TestWPFWithUnity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null">{"Property1":123,"Property2":"test string","Property3":true}</Item>
  <Item Id="TabControl_.SelectedIndex" Type="System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">1</Item>
  <Item Id="ColumnDefinition_.Width" Type="System.Windows.GridLength, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35">"Auto"</Item>
</Data>
```

# Demos

Demo projects are included in the repository. Playing around with them should be enough to get you started. 

# Contributing

You can contribute to this project in the standard way:

1. Fork the project
- Push your commits to your fork
- Make a pull request

# Links
Jot can be found on:
- Nuget: https://www.nuget.org/packages/Jot
- Codeproject: http://www.codeproject.com/Articles/475498/Easier-NET-settings (old but still mostly relevant) 


# TODO for this readme
- Web application scenarios (making controllers/pages statefull)

# License
MIT License

Copyright (c) 2016 Jot

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
