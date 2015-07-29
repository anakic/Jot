using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity.ObjectBuilder;
using Eidetic.Configuration;

namespace Eidetic.Unity
{
    /// <summary>
    /// Unity extension for adding (attribute based) state tracking to creted objects
    /// </summary>
    public class TrackingExtension : UnityContainerExtension
    {
        class TrackingStrategy : BuilderStrategy
        {
            static Dictionary<Type, bool> _trackabilityCache = new Dictionary<Type, bool>();

            IUnityContainer _container;
            Action<TrackingConfiguration> _customizeConfigAction;

            public TrackingStrategy(IUnityContainer container, Action<TrackingConfiguration> customizeConfigAction)
            {
                _container = container;
                _customizeConfigAction = customizeConfigAction;
            }

            public override void PostBuildUp(IBuilderContext context)
            {
                base.PostBuildUp(context);

                Type targetType = context.Existing.GetType();

                if (!_trackabilityCache.ContainsKey(targetType))
                    _trackabilityCache[targetType] = 
                        targetType.GetInterfaces().Contains(typeof(ITrackingAware)) || 
                        targetType.GetProperties().Any(p => p.GetCustomAttributes(true).OfType<TrackableAttribute>().Count() > 0);

                if (_trackabilityCache[targetType])
                {
                    List<StateTracker> trackers = new List<StateTracker>(_container.ResolveAll<StateTracker>());
                    if (_container.IsRegistered<StateTracker>())
                        trackers.Add(_container.Resolve<StateTracker>());

                    foreach (StateTracker tracker in trackers)
                    {
                        var config = tracker.Configure(context.Existing);
                        _customizeConfigAction(config);
                        config.Apply();
                    }
                }
            }
        }

        protected virtual void CustomizeConfiguration(TrackingConfiguration configuration){}

        protected override void Initialize()
        {
            Context.Strategies.Add(new TrackingStrategy(Container, CustomizeConfiguration), UnityBuildStage.Creation);
        }
    }
}
