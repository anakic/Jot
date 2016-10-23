using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.IO;
using Jot.Configuration;
using Jot.Storage;
using Jot.Triggers;
using System.IO.IsolatedStorage;

namespace Jot
{
    public class StateTracker
    {
        List<TrackingConfiguration> _configurations = new List<TrackingConfiguration>();

        public string Name { get; set; }
        public IStoreFactory ObjectStoreFactory { get; set; } = new JsonFileStoreFactory();
        public ITriggerPersist AutoPersistTrigger { get; set; } = new DesktopPersistTrigger(); 

        public StateTracker()
        {
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
    }
}
