# Jot - a .NET library for managing application state

## Introduction 
Almost every application these days needs to keep track of its own state, regardless of what it otherwise does. Typically, this includes sizes and locations of movable and resizable elements of the UI, last entered data (e.g. username) application settings and user preferences. 

A common approach is to store this data in a .settings file, and read and update it as needed. This involves writing a lot of boilerplate code to copy that data back and forth. This code is generally tedious, error prone and no fun to write.
 
Jot's job is to reduce the amount of code, mental effort and time needed to satisfy this common requirement, and to make it almost a non-task for developers. 

The library starts off with reasonable defaults for everything but it gives the developer full control over when, how and where each piece of data will be stored and applied.


## Example: Persisting window size and location

**Step 1.** Create a StateTracker instance and expose it to the rest of the application (for simplicity's sake, let's expose it as a static property) 
``` C#
static class Services
{
    public static StateTracker Tracker = new StateTracker();//we can use constructor overloads to configure where & when data is stored
}
```
**Step 2**: apply tracking 

``` C#
public MainWindow()
{
    InitializeComponent();
    
	Services.Tracker.Configure(this, "main window")//the object to track and a string by which to identify it
        .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)//properties to track
        .RegisterPersistTrigger(nameof(SizeChanged))//when to persist data to store
        .Apply();//apply any previously stored data
}

```

The above code is simple enough and would certainly work for most cases, but for real world use, we would need to handle a few more edge cases. 

Jot already comes with configuration presets for tracking `Window` and `Form` objects. These presets are enabled by default, but we can certainly change/remove them or supply our own presets for any type (see [ConfigurationInitializers](#configuration-initializers)).

Since the preset for `Window` objects is included by default, all we need to do to set up tracking for a `Window` is this:

**Step 2. revisited - final version**

``` C#
public MainWindow()
{
    InitializeComponent();

	//Why SourceInitialized?
	//Subtle WPF issue: WPF will always maximize a window to the primary screen 
	//if WindowState is set too early (e.g. in the constructor), even
	//if the Left property says it should be on the 2nd screen. Setting
	//these values in SourceInitialized resolves the issue.
    this.SourceInitialized += (s,e) => Services.Tracker.Configure(this).Apply(); 
}

```

This will track the window's size, location and window state. The tracking configuration preset is defined in [WindowConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/WindowConfigurationInitializer.cs).
 

## Where & when data gets stored

The `StateTracker` class has an empty constructor which uses reasonable defaults, but the main constructor allows you to specify exactly **were** the data will be stored and **when**:
  
``` C#
StateTracker(IStoreFactory storeFactory, ITriggerPersist persistTrigger)
```

The two arguments are explained below.

### 1. Where data is stored
The `storeFactory` argument controls where data is stored. This factory will be used to create a data store for each tracked object. 

You can, of course, provide your own storage mechanism (by implementing `IStore` and `IStoreFactory`) and Jot will happily use it.

By default, Jot stores data in .json files in the following folder: `%AppData%\[company name]\[application name]` (*company name* and *application name* are read from the entry assembly's attributes). The default folder is a per-user folder, but you can use a per-machine folder like so:

``` C#
var tracker = new StateTracker() { StoreFactory = new JsonFileStoreFactory(false) };//true: per-user, false: per-machine
```

Or you can give it a specific folder like so:

``` C#
var tracker = new StateTracker() { StoreFactory = new JsonFileStoreFactory(@"c:\example\path\") };
```

For desktop applications, the per-user default is usually fine.

### 2. When data is stored
The StateTracker uses an object that implements `ITriggerPersist` to get notified when it should do a global save of all data. The `ITriggerPersist` interface has just one memeber: the `PersistRequired` event.

The only built-in implementation of this interface is the `DesktopPersistTrigger` class which fires the `PersistRequired` event when a desktop application is about to shut down. 

> Note: Objects that don't survive until application shutdown should be persisted earlier. This can be done by specifying the persist trigger (`RegisterPersistTrigger`) or by explicitly calling Persist on their `TrackingConfiguration` object when appropriate.  


## Configuration initializers

We've seen we can manipulate the tracking configuration of individual objects, like so:

``` C#
	tracker.Configure(target, "some id")
		.AddProperties(...)
		.RegisterPersistTrigger(...)
``` 

But, we can also configure tracking for **all instances of a given type** by using configuration initializers. 

We do this by implementing [IConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/IConfigurationInitializer.cs) and registering our implementation with the StateTracker by calling `StateTracker.AddConfigurationInitializer(cfgInitializerForFoo)`.

You can see (and modify) the included initializers in the `stateTracker.ConfigurationInitializers` property.

Jot includes these configuration initializers out of the box:
- [WindowConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/WindowConfigurationInitializer.cs)
- [FormConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/FormConfigurationInitializer.cs)
- [DefaultConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/DefaultInitializer/DefaultConfigurationInitializer.cs)

The first two initialize tracking configurations for `Window` and `Form` objects so we don't have to do it manually. They're simple enough to be used as examples for implementing your own initializers.

The `DefaultConfigurationInitializer` is applicable for type `object`, so it gets used for all objects that don't have a more specific initializer. It enables objects to use `[Trackable]` and `[TrackingKey]` attributes and the `ITrackingAware` interface to be self descriptive about how they wish to be tracked.

The `DefaultConfigurationInitializer` is included by default in the `StateTracker` so you can use the attributes and `ITrackingAware` in your classes and they will be honored.

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
All that's needed now to start tracking the object is to call: 

``` C#
tracker.Configure(target).Apply();
```

This is nice because we don't need to manipulate the tracking configuration from the outside (e.g. calls to `AddProperty`) for each instance. 

# Example of stored data

Each tracked object will have its own file where its tracked property values will be saved. Here's an example:

![](http://i.imgur.com/xUVaVMh.png)

The file name includes the type of the tracked object and the identifier: `[targetObjectType]_[identifier].json`.  
We can see, we're tracking three objects: AppSettings (id: not specified), MainWindow (id: myWindow) and a single TabControl (id: tabControl). 

# Demos

Demo projects are included in the repository. Playing around with them should be enough to get you started. 

# Contributing

You can contribute to this project in the usual way:

1. Fork the project
- Push your commits to your fork
- Make a pull request

# Links
Jot can be found on:
- Nuget: https://www.nuget.org/packages/Jot
- Codeproject: http://www.codeproject.com/Articles/475498/Easier-NET-settings (old but still mostly relevant) 


# TODO for this readme
- Web application scenarios

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
