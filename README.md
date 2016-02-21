# Introduction

*TL;DR* - Jot is a library for persisting and applying application data (window locations and sizes, config settings, last entered data e.g. username etc.) in a consistend developer-friendly way.  

Applications are forgetful people. When an application shuts down and goes beddy-bye it forgets everything it had on it's mind, and when it wakes(starts) up again, it starts completely fresh. This may seem nice, but some of the data it had on its mind was likely useful, and forgetting this data just won't do (Memento anyone?).

As a user, you don't want to keep telling an application where you want its windows to be located and what window sizes would be particularily feng shui to your keen eye. You want a well behaved application that will remember all this stuff. For example, you don't need to adjust Visual Studio's layout every time you start it. 

It's not just window layout that you may want it to remember, it's all UI layout (movable/resizabe elements, selected tabs etc.), configuration settings, last entered data (e.g. last entered username), basically everything that's not bulk data (data for which you'd typically use a database). 

This is quite a ubiquitous requirement for applications, but it's a pesky one. It forces the developer to spend time writing tedious code that copies data back and forth from a settings file (or god-knows-where) insted of letting them focus on the actual problem the application is supposed to solve. 

That's where Jot comes in. Jot is a nice little .NET library whose sole job it is to make it easy for the developer to take care of this pesky task in a consistent and easy way. Let not its small size fool you, it's very capable and highly customizable, useful in all sorts of applications (web included).  

Jot is short for „jot down“, because it allows the application to jot down bits and pieces of information so that it can reapply them later when it restarts. 

# How to use it

## 1. Create a state tracker instance
```c#
StateTracker tracker = new StateTracker(new FileStore(@"d:\settingsfile.xml"), new DesktopPersistTrigger());
```
This creates a state tracker which will save all data to a file, and persist all data just before the application closes.

Once created you should make this instance available to the rest of the application (e.g. as a public static property or better yet through IOC). 

You'll likely create only one StateTracker instance in your application. In some situations though, you might want to create more. For example, if you need certain settings to be per-machine and others per-user, you would create two instances, one using %appdata%, and the other using %allusersprofile% folders to store the settings file. 

## 2. Use the state tracker
```c#
public MainWindow()
{
    InitializeComponent();
    //Services.Tracker is a static property with the StateTracker we created previously
    //set up tracking and apply state for the main window
    Services.Tracker.Configure(this)
        .AddProperties<MainWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState)
        .IdentifyAs("MainWindow")//not really needed since only one instance of MainWindow will ever exist, the default id is the name of the type, included for completeness
        .RegisterPersistTrigger("Closed")//not really needed in main window since the tracker will detect the application is closing and persist automatically, included for completeness
        .Apply();
            
    //track a tabcontrol's selected index
    Services.Tracker.Configure(tabControl)
        .IdentifyAs(tabControl.Name)
        .AddProperties<TabControl>(tc => tc.SelectedIndex)
        .Apply();

    this.DataContext = _settings;
}
```

# Further info

The library started in 2012, there's an old article on codeproject I wrote describing it here: http://www.codeproject.com/Articles/475498/Easier-NET-settings 

The article is dated but mostly still applies. While this readme is unfinished this article is a good source of additional information. 

The library is available on NuGet: https://www.nuget.org/packages/Jot/

Current state is pretty mature as far as desktop use goes, haven't used it too much for web applications. The repo includes *demo projects* which you can check out to see examples of its use. 



# TODO - add following sections to readme: 
- types can be self describing with regards to tracking (by implementing ITrackableAware or by using Trackable and TrackingKey attributes)
- If using IOC, a custom step can be added that sets up tracking for all created objects that can imeplement ITrackingAware or have tracking attributes applied to properties.
- how to use in web applications 
- customization points
  - storage
      - isolated storage
      - asp.net session
      - asp.net userprofile
      - custom storage (database, registry or stone tablets)
  - serialization
      - binary
      - json
  - custom persist triggers
