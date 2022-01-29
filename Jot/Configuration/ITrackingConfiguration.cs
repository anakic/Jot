using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Jot.Configuration
{
    public interface ITrackingConfiguration
    {
        List<Trigger> PersistTriggers { get; }
        Trigger StopTrackingTrigger { get; set; }
        Type TargetType { get; }
        Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; }
        Tracker Tracker { get; }

        ITrackingConfiguration CanPersist(Func<object, bool> canPersistFunc);
        string GetStoreId(object target);
        ITrackingConfiguration Id(Func<object, string> idFunc, object @namespace = null, bool includeType = true);
        ITrackingConfiguration PersistOn(params string[] eventNames);
        ITrackingConfiguration PersistOn(string eventName, Func<object, object> eventSourceGetter);
        ITrackingConfiguration PersistOn(string eventName, object eventSourceObject);
        ITrackingConfiguration StopTrackingOn(string eventName);
        ITrackingConfiguration StopTrackingOn(string eventName, Func<object, object> eventSourceGetter);
        ITrackingConfiguration StopTrackingOn(string eventName, object eventSource);
        ITrackingConfiguration WhenAppliedState(Action<object> action);
        ITrackingConfiguration WhenApplyingProperty(Action<object, PropertyOperationData> action);
        ITrackingConfiguration WhenPersisted(Action<object> action);
        ITrackingConfiguration WhenPersistingProperty(Action<object, PropertyOperationData> action);
    }
}