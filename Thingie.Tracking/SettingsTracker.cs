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

namespace Thingie.Tracking
{
    public class SettingsTracker
    {
        List<TrackingConfiguration> _configurations = new List<TrackingConfiguration>();

        public string Name { get; set; }

        public IObjectStore ObjectStore { get; private set; }

        /// <summary>
        /// Uses a BinarySerializer as the serializer, and a FileDataStore as the data store.
        /// Uses the <see cref="AssemblyCompanyAttribute"/> and <see cref="AssemblyTitleAttribute"/>
        /// to construct the path for the settings file, which it combines with the user's ApplicationData
        /// folder.
        /// </summary>
        /// <param name="baseFolder"></param>
        public SettingsTracker()
            : this(new FileStore(Environment.SpecialFolder.ApplicationData), new JsonSerializer())
        {
        }

        public SettingsTracker(IDataStore store, ISerializer serializer)
            : this(new DefaultObjectStore(store, serializer))
        {
        }

        public SettingsTracker(IObjectStore objectStore)
        {
            ObjectStore = objectStore;
        }

        #region automatic persisting
        bool _isWiredUp = false;
        protected virtual void WireUpAutomaticPersist()
        {
            if (System.Windows.Application.Current != null)//wpf
                System.Windows.Application.Current.Exit += (s, e) => { PersistAutomaticTargets(); };
            else //winforms
                System.Windows.Forms.Application.ApplicationExit += (s, e) => { PersistAutomaticTargets(); };
            _isWiredUp = true;
        }
        #endregion

        /// <summary>
        /// Creates or retrieves the tracking configuration for the speficied object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public TrackingConfiguration Configure(object target)
        {
            if(!_isWiredUp)
                WireUpAutomaticPersist();

            TrackingConfiguration config = FindExistingConfig(target);
            if (config == null)
                _configurations.Add(config = new TrackingConfiguration(target, this));
            return config;
        }

        public void ApplyAllState()
        {
            _configurations.ForEach(c=>c.Apply());
        }

        public void ApplyState(object target)
        {
            TrackingConfiguration config = FindExistingConfig(target);
            Debug.Assert(config != null);
            config.Apply();
        }

        public void PersistState(object target)
        {
            TrackingConfiguration config = FindExistingConfig(target);
            Debug.Assert(config != null);
            config.Persist();
        }

        public void PersistAutomaticTargets()
        {
            foreach (TrackingConfiguration config in _configurations.Where(cfg => cfg.Mode == PersistModes.Automatic && cfg.TargetReference.IsAlive))
                PersistState(config.TargetReference.Target);
        }

        #region private helper methods

        private TrackingConfiguration FindExistingConfig(object target)
        {
            return _configurations.SingleOrDefault(cfg => cfg.TargetReference.Target == target);
        }
        
        #endregion
    }
}
