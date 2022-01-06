using System;
using System.Collections.Generic;
using System.Linq;
using Jot.Storage;
using System.Runtime.CompilerServices;
using Jot.Configuration;
using System.Reflection;
using System.Globalization;

namespace Jot
{
    /// <summary>
    /// A StateTracker is an object responsible for tracking the specified properties of the specified target objects. 
    /// Tracking means persisting the values of the specified object properties, and restoring this data when appropriate.
    /// </summary>
    public class Tracker
    {
        // configurations for types
        readonly Dictionary<Type, TrackingConfiguration> _typeConfigurations = new Dictionary<Type, TrackingConfiguration>();

        // Weak reference dictionary
        readonly ConditionalWeakTable<object, TrackingConfiguration> _configurationsDict = new ConditionalWeakTable<object, TrackingConfiguration>();

        // Workaround:
        // ConditionalWeakTable does not support getting a list of all keys, which we need for a global persist
        readonly List<WeakReference> _trackedObjects = new List<WeakReference>();

        /// <summary>
        /// The object that is used to store and retrieve tracked data.
        /// </summary>
        public IStore Store { get; set; }

        /// <summary>
        /// Creates a StateTracker that uses json files in a per-user folder to store the data.
        /// </summary>
        public Tracker()
            : this(new JsonFileStore())
        {
        }

        /// <summary>
        /// Creates a new instance of the state tracker with the specified storage. 
        /// </summary>
        /// <param name="store">The factory that will create an IStore for each tracked object's data.</param>
        public Tracker(IStore store)
        {
            Store = store;
        }

        // todo: allow caller to configure via action argument

        /// <summary>
        /// Track a target object. This will apply any previously stored state to the target and
        /// start listening for events that indicate persisting new data is required.
        /// </summary>
        /// <param name="target"></param>
        public void Track(object target)
        {
            // find configuration for the target
            TrackingConfiguration config = Configure(target);

            // apply any previously stored data
            config.Apply(target);
            
            // listen for persist requests
            config.StartTracking(target);

            // add to list of objects to track
            _trackedObjects.Add(new WeakReference(target));
        }

        /// <summary>
        /// Apply any previously stored data to the target object.
        /// </summary>
        /// <param name="target"></param>
        public void Apply(object target)
        {
            this.Configure(target)
                .Apply(target);
        }

        /// <summary>
        /// Apply specified defaults to the tracked properties of the target object.
        /// </summary>
        public void ApplyDefaults(object target)
        {
            this.Configure(target)
                .ApplyDefaults(target);
        }

        /// <summary>
        /// Forget any saved state for the object with the specified id.
        /// </summary>
        public void Forget(string id)
        {
            Store.ClearData(id);
        }

        /// <summary>
        /// Forget any saved state for the target object.
        /// </summary>
        public void Forget(object target)
        {
            var id = this.Configure(target).GetStoreId(target);
            Forget(id);
        }

        /// <summary>
        /// Forget all saved state.
        /// </summary>
        public void ForgetAll()
        {
            Store.ClearAll();
        }

        /// <summary>
        /// Gets or creates a tracking configuration for the target object. 
        /// </summary>
        public TrackingConfiguration Configure(object target)
        {
            TrackingConfiguration config;
            if (_configurationsDict.TryGetValue(target, out TrackingConfiguration cfg))
                config = cfg;
            else
            {
                config = Configure(target.GetType());

                // if the object or the caller want to customize the config for this type, copy the config so they don't mess with the config for the type
                if (target is ITrackingAware)
                {
                    config = new TrackingConfiguration(config, target.GetType());

                    // allow the object to adjust the configuration
                    if (target is ITrackingAware ita)
                        ita.ConfigureTracking(config);
                }

                _configurationsDict.Add(target, config);

            }
            return config;
        }

        /// <summary>
        /// Gets or creates a tracking configuration for the specified type. Objects of the
        /// specified type will be tracked according to the settings that are defined in the 
        /// configuration object.
        /// </summary>
        public TrackingConfiguration<T> Configure<T>()
        {
            return new TrackingConfiguration<T>(Configure(typeof(T)));
        }

        /// <summary>
        /// Gets or creates a tracking configuration for the specified type. Objects of the
        /// specified type will be tracked according to the settings that are defined in the 
        /// configuration object.
        /// </summary>
        public TrackingConfiguration Configure(Type t)
        {
            TrackingConfiguration configuration;
            if (_typeConfigurations.ContainsKey(t))
            {
                // if a config for this exact type exists return it
                configuration = _typeConfigurations[t];
            }
            else
            {
                // todo: we should make a config for each base type recursively, in case at a later point we add config for a base type
                // tbd : should configurtions delegate work to base classes, rather than copying their config data?
                // if a config for this exact type does not exist, copy from base type's config or create a blank one
                var baseConfig = FindConfiguration(t);
                if (baseConfig != null)
                    configuration = new TrackingConfiguration(baseConfig, t);
                else
                    configuration = new TrackingConfiguration(this, t);
                _typeConfigurations[t] = configuration;
            }
            return configuration;
        }

        private TrackingConfiguration FindConfiguration(Type type)
        {
            var config = _typeConfigurations.ContainsKey(type) ? _typeConfigurations[type] : null;
            if (config != null)
                return config;
            else
            {
                if (type == typeof(object) || type.BaseType == null)
                    return null;
                else
                    return FindConfiguration(type.BaseType);
            }
        }

        /// <summary>
        /// Stop tracking the target object. This prevents the persisting 
        /// the target's properties when PersistAll is called on the tracker.
        /// It is used to prevent saving invalid data when the target object 
        /// still exists but is in an invalid state (e.g. disposed forms).
        /// </summary>
        public void StopTracking(object target)
        {
            if (_configurationsDict.TryGetValue(target, out TrackingConfiguration cfg))
            {
                cfg.StopTracking(target);
            }
        }

        // allows the tracking configuration to remove an object from the lists (so that it's not hit by global persist)
        internal void RemoveFromList(object target)
        {
            _configurationsDict.Remove(target);
            _trackedObjects.RemoveAll(t => t.Target == target);
        }

        /// <summary>
        /// Persists the tracked properties of the target object.
        /// </summary>
        /// <param name="target"></param>
        public void Persist(object target)
        {
            Configure(target).Persist(target);
        }

        /// <summary>
        /// Runs a global persist for all objects that are still alive and tracked. Waits for finalizers to complete first.
        /// </summary>
        public void PersistAll()
        {
            GC.WaitForPendingFinalizers();

            foreach (var target in _trackedObjects.Where(o => o.IsAlive).Select(o => o.Target))
            {
                if (_configurationsDict.TryGetValue(target, out var configuration))
                    configuration.Persist(target);
            }
        }
    }
}
