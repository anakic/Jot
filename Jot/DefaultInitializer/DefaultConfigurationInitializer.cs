using Jot.Triggers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Jot.DefaultInitializer
{
    /// <summary>
    /// Default initializer that will be used if a more specific initializer is not specified. 
    /// Enables [Trackable] and [TrackingKey] attributes, ITrackingAware and ITriggerPersist interfaces.
    /// Can be overriden to allow additional initialization logic for a specific type. If you do not wish 
    /// to keep the logic that deals with [Trackable], [TrackingKey], ITrackingAware and ITriggerPersist, 
    /// implement IConfigurationInitializer directly instead.
    /// </summary>
    public class DefaultConfigurationInitializer : IConfigurationInitializer
    {
        public virtual Type ForType { get { return typeof(object); } }

        public virtual void InitializeConfiguration(TrackingConfiguration configuration)
        {
            object target = configuration.TargetReference.Target;

            //set key if [TrackingKey] detected
            Type targetType = target.GetType();
            PropertyInfo keyProperty = targetType.GetProperties().SingleOrDefault(pi => pi.IsDefined(typeof(TrackingKeyAttribute), true));
            if (keyProperty != null)
                configuration.Key = keyProperty.GetValue(target, null).ToString();

            //add properties that have [Trackable] applied
            foreach (PropertyInfo pi in targetType.GetProperties())
            {
                TrackableAttribute propTrackableAtt = pi.GetCustomAttributes(true).OfType<TrackableAttribute>().Where(ta => ta.TrackerName == configuration.StateTracker.Name).SingleOrDefault();
                if (propTrackableAtt != null)
                {
                    //use [DefaultValue] if present
                    DefaultValueAttribute defaultAtt = pi.CustomAttributes.OfType<DefaultValueAttribute>().SingleOrDefault();
                    if (defaultAtt != null)
                        configuration.AddProperty(pi.Name, defaultAtt.Value);
                    else
                        configuration.AddProperty(pi.Name);
                }
            }

            //allow the object to alter its configuration
            ITrackingAware trackingAwareTarget = target as ITrackingAware;
            if (trackingAwareTarget != null)
                trackingAwareTarget.InitConfiguration(configuration);

            //allow the object to reqest persistence
            ITriggerPersist asNotify = target as ITriggerPersist;
            if (asNotify != null)
                asNotify.PersistRequired += (s, e) => configuration.Persist();
        }
    }
}
