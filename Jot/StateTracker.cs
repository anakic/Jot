using System;
using System.Collections.Generic;
using System.Linq;
using Jot.Storage;
using Jot.Triggers;
using Jot.DefaultInitializer;
using Jot.CustomInitializers;
using System.Runtime.CompilerServices;

namespace Jot
{
    /// <summary>
    /// A StateTracker is an object responsible for tracking the specified properties of the specified target objects. 
    /// Tracking means persisting the values of the specified object properties, and restoring this data when appropriate.
    /// </summary>
    public class StateTracker
    {
        ITriggerPersist _autoPersistTrigger;

        //Weak reference dictionary
        ConditionalWeakTable<object, TrackingConfiguration> _configurationsDict = new ConditionalWeakTable<object, TrackingConfiguration>();

        //Workaround:
        //ConditionalWeakTable does not support getting a list of all keys, which we need for a global persist
        List<WeakReference> _trackedObjects = new List<WeakReference>();

        /// <summary>
        /// The name of the StateTracker. Useful in tandem with [Trackable] attributes in situations with multiple state trackers, where each StateTracker is responsible for a different set of properties. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The object that will create a data store (an implementation of IStore) for each tracked object.
        /// </summary>
        public IStoreFactory StoreFactory { get; set; }

        /// <summary>
        /// A list of configuration initializers, that set up the default configuration for a given type.
        /// </summary>
        /// <remarks>
        ///Useful for centrally setting up configurations for all instances of a type whose code you do not control. 
        ///If you do control the code of the object, a more appropriate solution is to use [Trackable] attributes or implement ITrackingAware. That way, the class is self descriptive about tracking.
        /// </remarks>
        public Dictionary<Type, IConfigurationInitializer> ConfigurationInitializers { get; private set; } = new Dictionary<Type, IConfigurationInitializer>();

        /// <summary>
        /// The object that tells the StateTracker when to do a global Persist() of data. This will usually be on application shutdown, but this is not mandatory (e.g. can be a timer instead).
        /// </summary>
        public ITriggerPersist AutoPersistTrigger
        {
            get { return _autoPersistTrigger; }
            set
            {
                if (_autoPersistTrigger != null)
                    AutoPersistTrigger.PersistRequired -= AutoPersistTrigger_PersistRequired;

                _autoPersistTrigger = value;
                _autoPersistTrigger.PersistRequired += AutoPersistTrigger_PersistRequired;
            }
        }

        /// <summary>
        /// Creates a StateTracker that uses json files in a per-user folder to store the data.
        /// Does a global persist when it detects the desktop application is closing. 
        /// </summary>
        /// <remarks>
        /// This constructor is appropriate for most desktop application use cases. 
        /// Both ObjectStoreFactory and AutoPersistTrigger properties can be set/modified.
        /// </remarks>
        public StateTracker()
            : this(new JsonFileStoreFactory(), new DesktopPersistTrigger())
        {
        }

        /// <summary>
        /// Creates a new instance of the state tracker with the specified storage mechanism, and global persist trigger. 
        /// </summary>
        /// <remarks>
        /// Even though both arguments can be set via properties, this constructor is here to make the dependencies explicit.
        /// </remarks>
        /// <param name="storeFactory">The factory that will create an IStore for each tracked object's data.</param>
        /// <param name="persistTrigger">The object that will notify the state tracker when it should run a global persist operation. This will usually be when the application is shutting down.</param>
        public StateTracker(IStoreFactory storeFactory, ITriggerPersist persistTrigger)
        {
            StoreFactory = storeFactory;
            AutoPersistTrigger = persistTrigger;

            //add the basic configuration initializers
            RegisterConfigurationInitializer(new DefaultConfigurationInitializer()); //the default, will be used for all objects that don't have a more specific initializer
            RegisterConfigurationInitializer(new FormConfigurationInitializer());    //will be used for initializing configuration for forms (WinForms)
            RegisterConfigurationInitializer(new WindowConfigurationInitializer());  //will be used for initializing configuration for windows (WPF)
        }

        /// <summary>
        /// Registers an object that will initialize the configuration for all instances of a type.
        /// </summary>
        /// <remarks>
        /// Only the most specific initialier will be used (for the most derived type). 
        /// E.g. if there are initializers for types Window and Object, and a window is being tracked, only the Window initializer will be used. 
        /// </remarks>
        /// <param name="cfgInitializer">The configuration initializer to register.</param>
        public void RegisterConfigurationInitializer(IConfigurationInitializer cfgInitializer)
        {
            ConfigurationInitializers[cfgInitializer.ForType] = cfgInitializer;
        }

        private void AutoPersistTrigger_PersistRequired(object sender, EventArgs e)
        {
            RunAutoPersist();
        }

        /// <summary>
        /// Gets or creates a configuration object what will control how the target object is going to be tracked (which properties, when to persist, when to apply, validation).
        /// For a given target object, always returns the same configuration instance.
        /// </summary>
        /// <param name="target">The object whose properties your want to track.</param>
        /// <returns>The tracking configuration object.</returns>
        public TrackingConfiguration Configure(object target)
        {
            TrackingConfiguration config = FindExistingConfig(target);
            if (config == null)
            {
                config = new TrackingConfiguration(target, this);
                var initializer = FindInitializer(target.GetType());
                initializer.InitializeConfiguration(config);
                _trackedObjects.Add(new WeakReference(target));
                _configurationsDict.Add(target, config);
            }
            return config;
        }

        private IConfigurationInitializer FindInitializer(Type type)
        {
            IConfigurationInitializer initializer = ConfigurationInitializers.ContainsKey(type) ? ConfigurationInitializers[type] : null;

            if (initializer != null || type == typeof(object))
                return initializer;
            else
                return FindInitializer(type.BaseType);
        }

        /// <summary>
        /// Runs a global persist for all objects that are still alive and have AutoPersistEnabled=true in their TrackingConfiguration.
        /// </summary>
        public void RunAutoPersist()
        {
            foreach (var target in _trackedObjects.Where(o => o.IsAlive).Select(o => o.Target))
            {
                TrackingConfiguration configuration;
                if (_configurationsDict.TryGetValue(target, out configuration) && configuration.AutoPersistEnabled)
                    configuration.Persist();
            }
        }

        #region private helper methods

        private TrackingConfiguration FindExistingConfig(object target)
        {
            TrackingConfiguration configuration;
            _configurationsDict.TryGetValue(target, out configuration);
            return configuration;
        }

        #endregion
    }
}
