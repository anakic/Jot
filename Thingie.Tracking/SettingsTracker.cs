using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.IO;
using Thingie.Tracking.DefaultObjectStoreUtil.SerializedStorage;
using Thingie.Tracking.DefaultObjectStoreUtil.Serialization;
using Thingie.Tracking.DefaultObjectStoreUtil;
using Thingie.Tracking.SessionEndNotification;

namespace Thingie.Tracking
{
    public class SettingsTracker
    {
        List<TrackingConfiguration> _configurations = new List<TrackingConfiguration>();

        public string Name { get; set; }
        public IObjectStore ObjectStore { get; set; }
        public ISessionEndNotifier SessionEndNotifier { get; set; }

        public SettingsTracker(IDataStore store, ISerializer serializer, ISessionEndNotifier sessionEndNotifier)
            : this(new DefaultObjectStore(store, serializer), sessionEndNotifier)
        {
        }

        public SettingsTracker(IObjectStore objectStore, ISessionEndNotifier sessionEndNotifier)
        {
            ObjectStore = objectStore;
            SessionEndNotifier = sessionEndNotifier;

            if (SessionEndNotifier != null)
                SessionEndNotifier.SessionEnd += (s, e) => PersistAll();
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

        public void PersistAll()
        {
            foreach (TrackingConfiguration config in _configurations.Where(cfg => cfg.TargetReference.IsAlive))
                config.Persist();
        }

        #region private helper methods

        private TrackingConfiguration FindExistingConfig(object target)
        {
            return _configurations.SingleOrDefault(cfg => cfg.TargetReference.Target == target);
        }

        #endregion

        #region convenience methods for constructing a SettingsTracker
    
        public static SettingsTracker CreateTrackerForDesktop()
        {
            return CreateTrackerForDesktop(Environment.SpecialFolder.ApplicationData);
        }

        public static SettingsTracker CreateTrackerForDesktop(Environment.SpecialFolder folder)
        {
            return new SettingsTracker(new FileStore(folder), new JsonSerializer(), new DesktopSessionEndNotifier());
        }
        #endregion
    }
}
