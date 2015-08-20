using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.IO;
using Ursus.Configuration;
using Ursus.Storage;
using Ursus.Storage.Serialization;
using Ursus.Triggers;

namespace Ursus
{
    public class StateTracker
    {
        List<TrackingConfiguration> _configurations = new List<TrackingConfiguration>();

        public string Name { get; set; }
        public IObjectStore ObjectStore { get; set; }
        public ITriggerPersist AutoPersistTrigger { get; set; }

        public StateTracker(IObjectStore objectStore, ITriggerPersist globalAutoPersistTrigger)
        {
            ObjectStore = objectStore;
            AutoPersistTrigger = globalAutoPersistTrigger;

            if (AutoPersistTrigger != null)
                AutoPersistTrigger.PersistRequired += (s, e) => RunAutoPersist();
        }

        /// <summary>
        /// Creates or retrieves the tracking configuration for the speficied object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public TrackingConfiguration Configure(object target)
        {
            TrackingConfiguration config = FindExistingConfig(target);
            if (config == null)
                _configurations.Add(config = new TrackingConfiguration(target, this));
            return config;
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

        #region convenience methods for constructing a SettingsTracker

        /// <summary>
        /// Shorthand for: new StateTracker(new PersistentObjectStore(new FileStore(Environment.SpecialFolder.ApplicationData), new JsonSerializer()), new DesktopPersistTrigger());
        /// </summary>
        public static StateTracker CreateTrackerForDesktop()
        {
            return CreateTrackerForDesktop(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Shorthand for: new StateTracker(new PersistentObjectStore(new FileStore(<see cref="folder">folder</see>), new JsonSerializer()), new DesktopPersistTrigger());
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static StateTracker CreateTrackerForDesktop(Environment.SpecialFolder folder)
        {
            return new StateTracker(new FileStore(folder), new DesktopPersistTrigger());
        }

        #endregion
    }
}
