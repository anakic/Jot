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
**Step 2**: Set up tracking 

``` C#
public MainWindow()
{
    InitializeComponent();
    
	Services.Tracker.Configure(this)//the object to track
		.IdenitifyAs("main window")//a string by which to identify the target object
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

This will track the window's size, location and window state. The deafult tracking configuration for `Window` objects is defined in [WindowConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/WindowConfigurationInitializer.cs).
 

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

Or you can specify a folder path:

``` C#
var tracker = new StateTracker() { StoreFactory = new JsonFileStoreFactory(@"c:\example\path\") };
```

For desktop applications, the per-user default is usually fine.

### 2. When data is stored
The StateTracker uses an object that implements `ITriggerPersist` to get notified when it should do a global save of all data. The `ITriggerPersist` interface has just one memeber: the `PersistRequired` event.

The only built-in implementation of this interface is the `DesktopPersistTrigger` class which fires the `PersistRequired` event when a desktop application is about to shut down. 

> Note: Objects that don't survive until application shutdown should be persisted earlier. This can be done by specifying the persist trigger (`RegisterPersistTrigger`) or by explicitly calling Persist on their `TrackingConfiguration` object when appropriate.  


## Which properties of which object to track?
Since Jot doesn't know anything about our objects, we need to introduce them and tell Jot which property of which object we want to track.

There are 4 ways of initializing `TrackingConfiguration` objects, each being advantageous for certain scenarios. 

Here they are...    

### 1. Direct manipulation of TrackingConfiguration

The most basic way to manipulate the TrackingConfiguration is directly.

This is the usual pattern:

``` C#
	tracker.Configure(target)
		.IdentifyAs("some id")
		.AddProperties(...)
		.RegisterPersistTrigger(...)
```

Once we've set up the tracking configuration, we just need to call `Apply()` on it. This will cause it to look up stored data for the object, and apply any previously stored data to its tracked properties.

**Advantages**: 

1. Flexibility

**Limitations**: 

1. Per-instance, we need to repeat this for all instances we want to track

### 2. Configuration initializers

Say we want to track all window objects in our application in the same way. We don't want to repeat the TrackingConfiguration setup for each window that our application creates. 

With configuration initializers we can configure tracking for **all instances of a given (base) type**, even if we don't own the code of that type. 

For example, here's how we might create a configuration initializer for TabControl objects:

``` C#
public class TabControlCfgInitializer : IConfigurationInitializer
{
    public Type ForType
    {
        get
        {
            return typeof(TabControl);
        }
    }

    public void InitializeConfiguration(TrackingConfiguration configuration)
    {
        configuration
            .AddProperties(nameof(TabControl.SelectedIndex))
            .RegisterPersistTrigger(nameof(TabControl.SelectedIndexChanged));
    }
}
```

We can register it like so:

``` C#
_tracker.RegisterConfigurationInitializer(new TabControlCfgInitializer());
```

Now when we want to track a TabControl, all we need to do is:
``` C#
_tracker.Configure(tabControl1).Apply();
```

This is good because, now the `StateTracker` knows how to track `TabControl` objects, without us having to repeat the configuration setup for every TabControl instance.   

Jot includes these configuration initializers out of the box:
- [WindowConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/WindowConfigurationInitializer.cs) (for WPF Window objects)
- [FormConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/CustomInitializers/FormConfigurationInitializer.cs) (for WinForms Form objects)
- [DefaultConfigurationInitializer](https://github.com/anakic/Jot/blob/master/Jot/DefaultInitializer/DefaultConfigurationInitializer.cs) (enables `[Trackable]` attributes and `ITrackingAware` for all objects)

**Advantages**: 

1. Flexibility
2. Centralized setup for all instances of a type
2. We don't need to own the code of the target type

**Disadvantages**: 

1. Requires a bit of code to set up


### 3. Using tracking attributes

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

All that's needed now to start tracking the object is: 

``` C#
tracker.Configure(settings).Apply();
```


**Advantages**: 

1. Class is self descriptive about tracking
2. Centralized setup for all instances of a type
2. Simple

**Limitations**: 

1. Not as flexible as using `ITrackingAware`
2. We need to own the code of the target type (to place the attributes)

**Notes**: 

1. Relies on `DefaultConfigurationInitializer` being present in the StateTracker (which it is by default).

### 4. Using the ITrackingAware interface
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
tracker.Configure(settings).Apply();
```

**Advantages**: 
1. Class is self descriptive about tracking
2. Centralized setup for tracking all instances of a type
2. More flexibility compared to using attributes 

**Limitations**: 
1. Not as simple as applying attributes 
2. We need to own the code of the target type (to place the attributes)

**Notes**: 
1. Relies on `DefaultConfigurationInitializer` being present in the StateTracker (which it is by default).

# IOC integration
When using an IOC container, many objects in the application will be created by the container. This gives us an opportunity to automatically set up tracking for all created objects by hooking into the container.

For example, with [SimpleInjector](https://simpleinjector.org/index.html) we can do this quite easily, with a single line of code:

``` C#
var stateTracker = new Jot.StateTracker();
var container = new SimpleInjector.Container();

//configure tracking and apply previously stored data to all created objects
container.RegisterInitializer(d => { stateTracker.Configure(d.Instance).Apply(); }, cx => true);
```

Since the container does't know anything about how to track specific types, we can specify the tracking configuration for target objects by:
- using configuration initializers
- using `[Trackable]` and `[TrackingKey]` attributes
- implementing `ITrackingAware` 

So basically, **we can now track any property of any object just by putting a [Trackable] attribute on it**! Pretty neat, huh?


 

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
