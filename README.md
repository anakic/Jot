# Jot - A .NET library for managing application state

## Introduction 
Jot is a .NET library for persisting and applying application state. Typically, this state includes sizes and locations of movable and resizable elements of the UI, last entered data (e.g. username), user settings, etc...

This is a rather ubiquitous requirement. Almost every application these days needs to keep track of its own state, regardless of what it otherwise does. It's quite a tedious requirement for developers though, as it has little to do with the purpose of the application they're building. Still, it's important to do, since it enhances user experience and for a professional grade application, skipping it simply won't do. 

A common approach is to store this data in a .settings file when appropriate, and read it from there as needed. This involves writing a lot of boilerplate code to copy that data back and fourth. This code is tedious, error prone and generally no fun to write.  
 
Jot's job is to reduce the amount of code, mental effort and time needed to satisfy this requirement and to make it almost a non-task for developers. It's very flexible, and while it provides decent defaults for everything, it does allow the developer to configure when, how and where each piece of data will be stored and applied.


## Example: Persisting window size and location

Step 1. Create a StateTracker instance and expose it to the rest of the application
``` C#
static class Services
{
    public static StateTracker Tracker = new StateTracker();//use constructor overloads to configure how data is stored
}
```
Step2. Use the state tracker to keep track of the main window's size and location.
``` C#
public MainWindow()
{
    InitializeComponent();
    
    Services.Tracker.Configure(this)
        .IdentifyAs("MyMainWindow")
        .AddProperties<MainWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState)
        .RegisterPersistTrigger(nameof(Closed))//window.Closed will trigger persist
        .Apply();//apply any previous state 
}

```

### Short discussion of the example
For each object you want to track, there will need to be exactly one `TrackingConfiguration` object. This configuration object is created the first time you call `settingsTracker.Configure(object target)` for a given target object. Subsequent calls with the same target object will return the existing `TrackingConfiguration` object. The `TrackingConfiguration` object has a number of methods and properties that determine how the target object will be tracked. 

The code in the example above reads as follows:  
- `Configure(this)`: get or create the tracking configuration for `this` (the `MainWindow` instance)
- `IdentifyAs("MyMainWindow")`: identify the target object as `"MyMainWindow"` (arbitrary unique string)
- `AddProperties<MainWindow>(...)`: keep track of the target's Height/Width/Top/Left/WindowState properties
- `RegisterPersistTrigger(nameof(Closed))`: Save data when the target's Closed event fires (in this case mainWindow.Close)
- `Apply()`: apply any previous state right away
 
The `RegisterPersistTrigger` method is optional as there is a global persist trigger that fires just before the application closes. If there's a chance the target object will be garbage collected before that, you should specify the `RegisterPersistTrigger` or call `stateTracker.Persist(trigger)` manually when appropriate.

## Configuring when and where data will be stored
The StateTracker has a constructor that lets you specify **were** the data will be stored and **when** (the global persist trigger).

The primary constructor overload looks like this:  
``` C#
StateTracker(IObjectStore objectStore, ITriggerPersist globalAutoPersistTrigger)
```
The `objectStore` controls where data will be stored, the `globalAutoPersistTrigger` controls when all data should be persisted. 

There are several implementations of `IObjectStore` built into Jot:
- `FileStore`
- `IsolatedStorageStore`
- `AspNetSessionStore`
- `AspNetUserProfileStore `

You are, of course, free to make additional implementations of `IObjectStore` yourself and pass them to the StateTracker. 

For desktop applications, the `FileStore` is commonly the appropriate choice. The file path will determine if the settings are per-user (AppData) or per-machine (AllUsersProfile). FileStore and IsolatedStorageStore will use `JsonSerialization` by default to serialize data, but there are several other serializer implementations that can be used if needed.  

The `ITriggerPersist` interface has just one memeber: the `PersistRequired` event. There is only one built in implementation of this interface: the `DesktopPersistTrigger` class. This implementation fires the `PersistRequired` event when a desktop application is shutting down. 

This interface serves another purpose: you can implement it in any object you want to track, to enable the object to trigger its own persistence.      

## Types can specify their own tracking configuration 
Types can be self descriptive regarding tracking in two ways: 
- using attributes (`TrackingKeyAttribute` and `TrackableAttribute`)
- implementing `ITrackingAware` and/or `ITriggerPersist`

There are several benefits of this:
- we don't need to specify a list of properties to persist, triggers and identity for each instance of a type
- if we're using an IOC container, we can use this to enable automatic tracking for each object (that has `[Trackable]` attributes or implements `ITrackingAware`) as soon as the object is resolved by hooking into the container's post-resolve phase (not every container will support this but Unity and Ninject for instance do).    

### Self descriptive type using tracking attributes

``` C#
public class AppSettings
{
	[TrackingKey]
	public string Id { get { return "MyApplicationSettings"; } }

    [Trackable]
    public DisplaySettings DisplaySettings { get; set; }
    [Trackable]
    public GeneralSettings GeneralSettings { get; set; }

    public AppSettings()
    {
        DisplaySettings = new DisplaySettings();
        GeneralSettings = new GeneralSettings();
    }
}
```

### Self descriptive type using tracking interfaces
``` C#
public class AppSettings: ITrackingAware, ITriggerPersist
    {
		public DisplaySettings DisplaySettings { get; set; }
        public GeneralSettings GeneralSettings { get; set; }

        public AppSettings()
        {
            DisplaySettings = new DisplaySettings();
            GeneralSettings = new GeneralSettings();

			DisplaySettings.PropertyChanged += Settings_PropertyChanged;
			GeneralSettings.PropertyChanged += Settings_PropertyChanged
		}

		#region ITriggerPersist implementation
		public event EventHandler PersistRequired;
		#endregion

		#region ITrackingAware implementation
		public void InitConfiguration(TrackingConfiguration configuration)
		{
			configuration.IdentifyAs("MyApplicationSettings").AddProperties<AppSettings>(s => s.DisplaySettings, s => s.GeneralSettings);
		}
		#endregion

		private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//persist whenever something changes in DisplaySettings or GeneralSettings
			PersistRequired?.Invoke(this, EventArgs.Empty);
		}
	}
```

If a type is self descriptive about the way it wishes to be persisted, it is not necessary to manipulate it's TrackingConfiguration from the outside. All that's needed is to call: 

``` C#
tracker.Configure(target).Apply();
```

## Automatically setting up tracking through IOC object
When types are self descriptive about their tracking, there's a nice trick we can do. We can hook into the IOC continer, so that after it resolves an object, it automatically sets up tracking. Since the type can define which properties to track, and when to persist, all we need to do is call `tracker.Configure(target).Apply();`.

Once we set this up, all that's needed is make a property persisten is to decorate it with [Trackable]   
