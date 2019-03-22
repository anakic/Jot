using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Jot.Configuration
{
    /// <summary>
    /// A TrackingConfiguration is an object that determines how a target object will be tracked.
    /// </summary>
    public sealed class TrackingConfiguration<T> : ITrackingConfigurationInternal
    {
        Action<object> _appliedAction;

        Action<object, PropertyOperationData> _applyingPropertyAction;
        Action<object, PropertyOperationData> ITrackingConfigurationInternal.ApplyingPropertyAction { get => _applyingPropertyAction; }
        Action<object> ITrackingConfigurationInternal.AppliedAction { get => _appliedAction; }

        /// <summary>
        /// The StateTracker that owns this tracking configuration.
        /// </summary>
        public Tracker Tracker { get; }

        /// <summary>
        /// A dictionary containing the tracked properties.
        /// </summary>
        public Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; } = new Dictionary<string, TrackedPropertyInfo>();

        /// <summary>
        /// List containing the events that will trigger persisting
        /// </summary>
        public List<Trigger> PersistTriggers { get; } = new List<Trigger>();
        public Trigger StopTrackingTrigger { get; set; }

        internal TrackingConfiguration(Tracker tracker)
        {
            Tracker = tracker;
            _idFunc = target => target.GetType().Name;
        }

        internal TrackingConfiguration(ITrackingConfigurationInternal baseConfig)
        {
            Tracker = baseConfig.Tracker;

            _idFunc = baseConfig.IdFunc;

            _appliedAction = baseConfig.AppliedAction;
            _persistedAction = baseConfig.PersistedAction;
            _applyingPropertyAction = baseConfig.ApplyingPropertyAction;
            _persistingPropertyAction = baseConfig.PersistingPropertyAction;

            foreach (var kvp in baseConfig.TrackedProperties)
                TrackedProperties.Add(kvp.Key, kvp.Value);
            PersistTriggers.AddRange(baseConfig.PersistTriggers);
        }

        #region apply/persist events
        private bool OnApplyingProperty(object target, string property, ref object value)
        {
            var args = new PropertyOperationData(property, value);
            _applyingPropertyAction?.Invoke(target, args);
            value = args.Value;
            return !args.Cancel;
        }
        /// <summary>
        /// Allows value conversion and cancallation when applying a stored value to a property.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenApplyingProperty(Action<T, PropertyOperationData> action)
        {
            _applyingPropertyAction = (obj, prop) => action((T)obj, prop);
            return this;
        }

        private void OnStateApplied(T target)
        {
            _appliedAction?.Invoke(target);
        }

        /// <summary>
        /// Allows supplying a callback that will be called when all saved state is applied to a target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenAppliedState(Action<T> action)
        {
            _appliedAction = obj => action((T)obj);
            return this;
        }

        Action<object, PropertyOperationData> _persistingPropertyAction;
        Action<object, PropertyOperationData> ITrackingConfigurationInternal.PersistingPropertyAction { get => _persistingPropertyAction; }
        private bool OnPersistingProperty(object target, string property, ref object value)
        {
            var args = new PropertyOperationData(property, value);
            _persistingPropertyAction?.Invoke(target, args);
            value = args.Value;
            return !args.Cancel;
        }

        /// <summary>
        /// Allows value conversion and cancallation when persisting a property of the target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenPersistingProperty(Action<T, PropertyOperationData> action)
        {
            _persistingPropertyAction = (obj, prop) => action((T)obj, prop);
            return this;
        }


        private void OnStatePersisted(T target)
        {
            _persistedAction?.Invoke(target);
        }

        Action<object> _persistedAction;
        Action<object> ITrackingConfigurationInternal.PersistedAction { get => _persistedAction; }
        public TrackingConfiguration<T> WhenPersisted(Action<T> action)
        {
            _persistedAction = obj => action((T)obj);
            return this;
        }
        #endregion

        /// <summary>
        /// Reads the data from the tracked properties and saves it to the data store for the tracked object.
        /// </summary>
        void ITrackingConfigurationInternal.Persist(object target)
        {
            var name = _idFunc((T)target);

            IDictionary<string, object> originalValues = null;
            var values = new Dictionary<string, object>();
            foreach (string propertyName in TrackedProperties.Keys)
            {
                var value = TrackedProperties[propertyName].Getter((T)target);
                try
                {
                    var shouldPersist = OnPersistingProperty(target, propertyName, ref value);
                    if (shouldPersist)
                    {
                        values[propertyName] = value;
                    }
                    else
                    {
                        // keeping previously stored value in case persist cancelled
                        originalValues = originalValues ?? Tracker.Store.GetData(name);
                        values[propertyName] = originalValues[propertyName];
                        Trace.WriteLine($"Persisting cancelled, key='{name}', property='{propertyName}'.");
                    }
                }
                catch (Exception ex)
                {
                    // todo: replace with ILogger
                    Trace.WriteLine($"Persisting failed, property key = '{name}', property = {propertyName}, message='{ex.Message}'.");
                }
            }

            Tracker.Store.SetData(name, values);

            OnStatePersisted((T)target);
        }

        /// <summary>
        /// Applies any previously stored data to the tracked properties of the target object.
        /// </summary>
        void ITrackingConfigurationInternal.Apply(object target)
        {
            var name = _idFunc((T)target);
            var data = Tracker.Store.GetData(name);

            foreach (string propertyName in TrackedProperties.Keys)
            {
                var descriptor = TrackedProperties[propertyName];

                if (data?.ContainsKey(propertyName) == true)
                {
                    try
                    {
                        object value = data[propertyName];
                        var shouldApply = OnApplyingProperty(target, propertyName, ref value);
                        if (shouldApply)
                        {
                            descriptor.Setter((T)target, value);
                        }
                        else
                        {
                            Trace.WriteLine($"Persisting cancelled, key='{name}', property='{propertyName}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"TRACKING: Applying tracking to property with key='{propertyName}' failed. ExceptionType:'{ex.GetType().Name}', message: '{ex.Message}'!");
                    }
                }
                else if (descriptor.IsDefaultSpecified)
                {
                    descriptor.Setter((T)target, descriptor.DefaultValue);
                }
            }

            OnStateApplied((T)target);
        }


        Func<object, string> _idFunc;
        Func<object, string> ITrackingConfigurationInternal.IdFunc { get => _idFunc; }
        /// <summary>
        /// </summary>
        /// <param name="idFunc">The provided function will be used to get an identifier for a target object in order to identify the data that belongs to it.</param>
        /// <param name="namespaceSegments">Serves to distinguish objects with the same ids that are used in different contexts</param>
        /// <returns></returns>
        public TrackingConfiguration<T> Id(Func<T, string> idFunc, params object[] namespaceSegments)
        {
            _idFunc = target =>
            {
                StringBuilder idBuilder = new StringBuilder();
                foreach (var seg in namespaceSegments)
                    idBuilder.Append($"{seg}.");
                idBuilder.Append(idFunc((T)target));
                return idBuilder.ToString();
            };

            return this;
        }

        /// <summary>
        /// Registers the specified event of the target object as a trigger that will cause the target's data to be persisted.
        /// </summary>
        /// <example>
        /// For a Window object, "LocationChanged" and/or "SizeChanged" would be appropriate.
        /// </example>
        /// <remarks>
        /// Automatically persist a target object when it fires the specified name.
        /// </remarks>
        /// <param name="eventNames">The names of the events that will cause the target object's data to be persisted.</param>
        /// <returns></returns>
        public TrackingConfiguration<T> PersistOn(params string[] eventNames)
        {
            foreach (string eventName in eventNames)
                PersistTriggers.Add(new Trigger(eventName, s => s));
            return this;
        }

        /// <summary>
        /// Automatically persist a target object when the specified eventSourceObject fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSourceObject">If not provided, </param>
        /// <returns></returns>
        public TrackingConfiguration<T> PersistOn(string eventName, object eventSourceObject)
        {
            PersistOn(eventName, target => eventSourceObject);
            return this;
        }

        /// <summary>
        /// Automatically persist a target object when the specified eventSourceObject fires the specified event.
        /// </summary>
        /// <param name="eventName">The name of the event that should trigger persisting stete.</param>
        /// <param name="eventSourceGetter"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> PersistOn(string eventName, Func<T, object> eventSourceGetter)
        {
            PersistTriggers.Add(new Trigger(eventName, target => eventSourceGetter((T)target)));
            return this;
        }

        /// <summary>
        /// Stop tracking the target when it fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> StopTrackingOn(string eventName)
        {
            return StopTrackingOn(eventName, target => target);
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSource"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> StopTrackingOn(string eventName, object eventSource)
        {
            return StopTrackingOn(eventName, target => eventSource);
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSourceGetter"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> StopTrackingOn(string eventName, Func<T, object> eventSourceGetter)
        {
            StopTrackingTrigger = new Trigger(eventName, target => eventSourceGetter((T)target));
            return this;
        }

        void ITrackingConfigurationInternal.StopTracking(object target)
        {
            // unsubscribe from all trigger events
            foreach (var trigger in PersistTriggers)
                trigger.Unsubscribe((T)target);

            // unsubscribe from stoptracking trigger too
            StopTrackingTrigger?.Unsubscribe((T)target);

            Tracker.RemoveFromList(target);
        }

        void ITrackingConfigurationInternal.StartTracking(object target)
        {
            // listen for trigger events (for persisting)
            foreach (var trigger in PersistTriggers)
                trigger.Subscribe((T)target, () => (this as ITrackingConfigurationInternal).Persist(target));

            // listen to stoptracking event
            StopTrackingTrigger?.Subscribe((T)target, () => (this as ITrackingConfigurationInternal).StopTracking(target));
        }

        /// <summary>
        /// Set up tracking for the specified property.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="name"></param>
        /// <param name="propertyAccessExpression"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> Property<K>(Expression<Func<T, K>> propertyAccessExpression, string name = null)
        {
            return Property(name, propertyAccessExpression, false, default(K));
        }

        /// <summary>
        /// Set up tracking for the specified property.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="name">The name of the property in the store</param>
        /// <param name="propertyAccessExpression">The expression that points to the specified property. Can navigate multiple levels.</param>
        /// <param name="defaultValue">If there is no value in the store for the property, the defaultValue will be used.</param>
        /// <returns></returns>
        public TrackingConfiguration<T> Property<TProperty>(Expression<Func<T, TProperty>> propertyAccessExpression, TProperty defaultValue, string name = null)
        {
            return Property(name, propertyAccessExpression, true, defaultValue);
        }

        private TrackingConfiguration<T> Property<TProperty>(string name, Expression<Func<T, TProperty>> propertyAccessExpression, bool defaultSpecified, TProperty defaultValue)
        {
            if (name == null && propertyAccessExpression.Body is MemberExpression me)
            {
                name = me.Member.Name;
            }

            var membershipExpression = propertyAccessExpression.Body;
            var getter = propertyAccessExpression.Compile();

            var right = Expression.Parameter(typeof(object));
            var propType = membershipExpression.Type;
            var setter = Expression.Lambda(Expression.Block(Expression.Assign(membershipExpression, Expression.Convert(right, membershipExpression.Type)), Expression.Empty()), propertyAccessExpression.Parameters[0], right).Compile() as Action<T, object>;
            if (defaultSpecified)
                TrackedProperties[name] = new TrackedPropertyInfo(x => getter((T)x), (x, v) => setter((T)x, v), defaultValue);
            else
                TrackedProperties[name] = new TrackedPropertyInfo(x => getter((T)x), (x, v) => setter((T)x, v));
            return this;
        }

        /// <summary>
        /// Set up tracking for one or more properties. The projections can be property access expression (e.g. x => x.MyProp) or anonymous type projections (e.g. x => new { x.MyProp1, x.MyProp2 }). 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="projections"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> Properties(params Expression<Func<T, object>>[] projections)
        {
            foreach (var projection in projections)
            {
                if (projection.Body is NewExpression newExp)
                {
                    var accessors = newExp.Members.Select((m, i) =>
                    {
                        var right = Expression.Parameter(typeof(object));
                        var propType = (m as PropertyInfo).PropertyType;
                        return new
                        {
                            name = m.Name,
                            getter = (Expression.Lambda(Expression.Convert(newExp.Arguments[i] as MemberExpression, typeof(object)), projection.Parameters[0]).Compile() as Func<T, object>),
                            setter = Expression.Lambda(Expression.Block(Expression.Assign(newExp.Arguments[i], Expression.Convert(right, propType)), Expression.Empty()), projection.Parameters[0], right).Compile() as Action<T, object>
                        };
                    });

                    foreach (var a in accessors)
                    {
                        TrackedProperties[a.name] = new TrackedPropertyInfo(x => a.getter((T)x), (x, v) => a.setter((T)x, v));
                    }
                }
                else
                {
                    throw new ArgumentException("Expression must project properties as an anonymous class e.g. f => new { f.Height, f.Width } or access a single property e.g. f => f.Text.");
                }
            }
            return this;
        }
    }
}
