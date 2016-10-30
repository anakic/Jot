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
        public IStoreFactory ObjectStoreFactory { get; set; }
        public List<IConfigurationInitializer> ConfigurationInitializers { get; private set; }

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


        public StateTracker()
        {
            //by default store data in a peruser jsonfile
            ObjectStoreFactory = new JsonFileStoreFactory();

            //run auto persist on all configurations when application is closing
            AutoPersistTrigger = new DesktopPersistTrigger();

            //add the basic configuration initializers
            ConfigurationInitializers = new List<IConfigurationInitializer>()
            {
                new DefaultConfigurationInitializer(),//the default, will be used for all objects that don't have a more specific initializer
                new FormConfigurationInitializer(),//will be used for initializing configuration for forms (WinForms)
                new WindowConfigurationInitializer(),//will be used for initializing configuration for windows (WPF)
            };
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
            var initializer = ConfigurationInitializers.SingleOrDefault(i => i.ForType == type);

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
