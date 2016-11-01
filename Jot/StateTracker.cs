using System;
using System.Collections.Generic;
using System.Linq;
using Jot.Storage;
using Jot.Triggers;
using Jot.DefaultInitializer;
using Jot.CustomInitializers;

namespace Jot
{
    public class StateTracker
    {
        ITriggerPersist _autoPersistTrigger;
        List<TrackingConfiguration> _configurations = new List<TrackingConfiguration>();

        public string Name { get; set; }
        public IStoreFactory StoreFactory { get; set; }
        public Dictionary<Type, IConfigurationInitializer> ConfigurationInitializers { get; private set; } = new Dictionary<Type, IConfigurationInitializer>();

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
        /// Creates a StateTracker that uses a per-user json file to store the data
        /// and does a global persist when it detects the desktop application is closing. 
        /// This constructor is appropriate for most desktop application use cases. 
        /// Both ObjectStoreFactory and AutoPersistTrigger properties can be set/modified.
        /// </summary>
        public StateTracker()
            : this(new JsonFileStoreFactory(), new DesktopPersistTrigger())
        {
        }

        /// <summary>
        /// Creates a new instance of the state tracker. 
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
            AddConfigurationInitializer(new DefaultConfigurationInitializer()); //the default, will be used for all objects that don't have a more specific initializer
            AddConfigurationInitializer(new FormConfigurationInitializer());    //will be used for initializing configuration for forms (WinForms)
            AddConfigurationInitializer(new WindowConfigurationInitializer());  //will be used for initializing configuration for windows (WPF)
        }

        public void AddConfigurationInitializer(IConfigurationInitializer cfgInitializer)
        {
            ConfigurationInitializers[cfgInitializer.ForType] = cfgInitializer;
        }

        private void AutoPersistTrigger_PersistRequired(object sender, EventArgs e)
        {
            RunAutoPersist();
        }

        public TrackingConfiguration Configure(object target)
        {
            return Configure(target, null);
        }

        /// <summary>
        /// Creates or retrieves the tracking configuration for the speficied object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public TrackingConfiguration Configure(object target, string identifier)
        {
            TrackingConfiguration config = FindExistingConfig(target);
            if (config == null)
            {
                config = new TrackingConfiguration(target, identifier, this);
                FindInitializer(target.GetType()).InitializeConfiguration(config);
                config.CompleteInitialization();
                _configurations.Add(config);
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

        public void ApplyAllState()
        {
            _configurations.ForEach(c => c.Apply());
        }

        public void RunAutoPersist()
        {
            foreach (TrackingConfiguration config in _configurations.Where(cfg => cfg.AutoPersistEnabled && cfg.TargetReference.IsAlive))
                config.Persist();
        }

        #region private helper methods

        private TrackingConfiguration FindExistingConfig(object target)
        {
            return _configurations.SingleOrDefault(cfg => cfg.TargetReference.Target == target);
        }

        #endregion
    }
}
