using System;
using System.Collections.Generic;
using System.Linq;
using Jot.Storage;
using System.Runtime.CompilerServices;
using Jot.Configuration;

namespace Jot
{
    /// <summary>
    /// A StateTracker is an object responsible for tracking the specified properties of the specified target objects. 
    /// Tracking means persisting the values of the specified object properties, and restoring this data when appropriate.
    /// </summary>
    public class Tracker
    {
        // configurations for types
        Dictionary<Type, object> _typeConfigurations = new Dictionary<Type, object>();

        // Weak reference dictionary
        ConditionalWeakTable<object, ITrackingConfigurationInternal> _configurationsDict = new ConditionalWeakTable<object, ITrackingConfigurationInternal>();

        // Workaround:
        // ConditionalWeakTable does not support getting a list of all keys, which we need for a global persist
        List<WeakReference> _trackedObjects = new List<WeakReference>();

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

        public void Track<T>(T target)
        {
            if (!_configurationsDict.TryGetValue(target, out _))
            {
                // find a configuration for this type of the nearest base type, or create a new one
                ITrackingConfigurationInternal config = Configure<T>();

                // if the object or the caller want to customize the config for this type, copy the config so they don't mess with the config for the type
                if (target is ITrackingAware<T>)
                {
                    config = new TrackingConfiguration<T>(config);

                    // allow the object to adjust the configuration
                    if (target is ITrackingAware<T> ita)
                        ita?.ConfigureTracking((TrackingConfiguration<T>)config);
                }

                // keep track of the object
                _trackedObjects.Add(new WeakReference(target));
                _configurationsDict.Add(target, config);

                // apply any previously stored data
                config.Apply(target);
                // listen to persist trigger events
                config.StartTracking(target);
            }
        }

        public TrackingConfiguration<T> Configure<T>()
        {
            TrackingConfiguration<T> configuration;
            if (_typeConfigurations.ContainsKey(typeof(T)))
            {
                // if a config for this exact type exists return it
                configuration = (TrackingConfiguration<T>)_typeConfigurations[typeof(T)];
            }
            else
            {
                // if a config for this exact type does not exist, copy from base type's config or create a blank one
                var baseConfig = FindConfiguration(typeof(T));
                if (baseConfig != null)
                    configuration = new TrackingConfiguration<T>(baseConfig);
                else
                    configuration = new TrackingConfiguration<T>(this);
                _typeConfigurations[typeof(T)] = configuration;
            }
            return configuration;
        }

        private ITrackingConfigurationInternal FindConfiguration(Type type)
        {
            var config = _typeConfigurations.ContainsKey(type) ? _typeConfigurations[type] : null;
            if (config != null)
                return (ITrackingConfigurationInternal)config;
            else
            {
                if (type == typeof(object))
                    return null;
                else
                    return FindConfiguration(type.BaseType);
            }
        }

        public void StopTracking(object target)
        {
            if (_configurationsDict.TryGetValue(target, out ITrackingConfigurationInternal cfg))
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

        public void Persist(object target)
        {
            if (_configurationsDict.TryGetValue(target, out ITrackingConfigurationInternal config))
                config.Persist(target);
            else
                throw new ArgumentException("Target object is not being tracked", nameof(target));
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
