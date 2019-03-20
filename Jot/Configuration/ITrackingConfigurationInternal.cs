using System;
using System.Collections.Generic;

namespace Jot.Configuration
{
    internal interface ITrackingConfigurationInternal
    {
        Tracker Tracker { get; }
        List<Trigger> PersistTriggers { get; }

        void Persist(object target);
        void Apply(object target);
        void StartTracking(object target);
        void StopTracking(object target);
        Func<object, string> IdFunc { get; }
        Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; }

        Action<object> AppliedAction { get; }
        Action<object> PersistedAction { get; }
        Action<object, PropertyOperationData> PersistingPropertyAction { get; }
        Action<object, PropertyOperationData> ApplyingPropertyAction { get; }
    }

}
