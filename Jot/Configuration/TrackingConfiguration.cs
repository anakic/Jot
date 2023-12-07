using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Jot.Configuration.Attributes;
using System.ComponentModel;

namespace Jot.Configuration
{
    /// <summary>
    /// A TrackingConfiguration is an object that determines how a target object will be tracked.
    /// </summary>
    public class TrackingConfiguration : ITrackingConfiguration
    {
        public Type TargetType { get; }

        Func<object, string> idFunc;
        Func<object, bool> canPersistFunc = x => true;

        Action<object, PropertyOperationData> applyingPropertyAction;
        Action<object, PropertyOperationData> persistingPropertyAction;
        Action<object> appliedAction;
        Action<object> persistedAction;

        /// <summary>
        /// The StateTracker that owns this tracking configuration.
        /// </summary>
        public virtual Tracker Tracker { get; }

        /// <summary>
        /// A dictionary containing the tracked properties.
        /// </summary>
        public Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; } = new Dictionary<string, TrackedPropertyInfo>();

        /// <summary>
        /// List containing the events that will trigger persisting
        /// </summary>
        public List<Trigger> PersistTriggers { get; } = new List<Trigger>();
        public Trigger StopTrackingTrigger { get; set; }

        internal TrackingConfiguration()
        {
        }

        internal TrackingConfiguration(
            Tracker tracker,
            Type targetType)
        {
            TargetType = targetType;
            Tracker = tracker;
            idFunc = target => target.GetType().Name;

            ReadAttributes();
        }

        internal TrackingConfiguration(
            TrackingConfiguration baseConfig,
            Type targetType)
        {
            TargetType = targetType;
            Tracker = baseConfig.Tracker;

            idFunc = baseConfig.idFunc;

            appliedAction = baseConfig.appliedAction;
            persistedAction = baseConfig.persistedAction;
            applyingPropertyAction = baseConfig.applyingPropertyAction;
            persistingPropertyAction = baseConfig.persistingPropertyAction;

            foreach (var kvp in baseConfig.TrackedProperties)
                TrackedProperties.Add(kvp.Key, kvp.Value);
            PersistTriggers.AddRange(baseConfig.PersistTriggers);

            ReadAttributes();
        }

        private void ReadAttributes()
        {
            // todo: use Expression API to generate getters/setters instead of reflection 
            // [low priority due to low likelyness of 1M+ invocations]

            // todo: add [CanPersist] attribute and use it for canReadFunc

            //set key if [TrackingKey] detected
            PropertyInfo keyProperty = TargetType.GetProperties().SingleOrDefault(pi => pi.IsDefined(typeof(TrackingIdAttribute), true));
            if (keyProperty != null)
                idFunc = (t) => keyProperty.GetValue(t, null).ToString();

            //add properties that have [Trackable] applied
            foreach (PropertyInfo pi in TargetType.GetProperties())
            {
                TrackableAttribute propTrackableAtt = pi.GetCustomAttributes(true).OfType<TrackableAttribute>().SingleOrDefault();
                if (propTrackableAtt != null)
                {
                    //use [DefaultValue] if present
                    DefaultValueAttribute defaultAtt = pi.GetCustomAttribute<DefaultValueAttribute>();
                    if (defaultAtt != null)
                        TrackedProperties[pi.Name] = new TrackedPropertyInfo(x => pi.GetValue(x), (x, v) => SetValue(x, pi, v), defaultAtt.Value);
                    else
                        TrackedProperties[pi.Name] = new TrackedPropertyInfo(x => pi.GetValue(x), (x, v) => SetValue(x, pi, v));
                }
            }

            foreach (EventInfo eventInfo in TargetType.GetEvents())
            {
                var attributes = eventInfo.GetCustomAttributes(true);

                if (attributes.OfType<PersistOnAttribute>().Any())
                    PersistOn(eventInfo.Name);

                if (attributes.OfType<StopTrackingOnAttribute>().Any())
                    StopTrackingOn(eventInfo.Name);
            }
        }

        private void SetValue(object target, PropertyInfo pi, object value)
        {
            var valueToWrite = Convert(value, pi.Name, pi.PropertyType);
            pi.SetValue(target, valueToWrite);
        }

        private static object Convert(object value, string propertyName, Type t)
        {
            if (value == null)
            {
                if (t.IsValueType)
                    throw new ArgumentException($"Cannot write null into non-nullable property {propertyName}");
            }
            else
            {
                var typeOfValue = value.GetType();

                // This can happen if we're trying to write an Int64 to an Int32 property (in case of overflow it will throw).
                // Also can happen for enums.
                if (typeOfValue != t && !t.IsAssignableFrom(typeOfValue))
                {
                    var converter = TypeDescriptor.GetConverter(t);
                    if (converter.CanConvertFrom(typeOfValue))
                        return converter.ConvertFrom(value);
                    else
                    {
                        if (t.IsEnum)
                            return Enum.ToObject(t, value);
                        else
                            return System.Convert.ChangeType(value, t);
                    }
                }
            }

            return value;
        }

        #region apply/persist events
        private bool OnApplyingProperty(object target, string property, ref object value)
        {
            var args = new PropertyOperationData(property, value);
            applyingPropertyAction?.Invoke(target, args);
            value = args.Value;
            return !args.Cancel;
        }

        /// <summary>
        /// Allows value conversion and cancallation when applying a stored value to a property.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ITrackingConfiguration WhenApplyingProperty(Action<object, PropertyOperationData> action)
        {
            applyingPropertyAction = action;
            return this;
        }

        private void OnStateApplied(object target)
        {
            appliedAction?.Invoke(target);
        }

        /// <summary>
        /// Allows supplying a callback that will be called when all saved state is applied to a target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ITrackingConfiguration WhenAppliedState(Action<object> action)
        {
            appliedAction = action;
            return this;
        }

        private bool OnPersistingProperty(object target, string property, ref object value)
        {
            var args = new PropertyOperationData(property, value);
            persistingPropertyAction?.Invoke(target, args);
            value = args.Value;
            return !args.Cancel;
        }

        /// <summary>
        /// Allows value conversion and cancallation when persisting a property of the target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ITrackingConfiguration WhenPersistingProperty(Action<object, PropertyOperationData> action)
        {
            persistingPropertyAction = action;
            return this;
        }

        private void OnStatePersisted(object target)
        {
            persistedAction?.Invoke(target);
        }

        public ITrackingConfiguration WhenPersisted(Action<object> action)
        {
            persistedAction = obj => action(obj);
            return this;
        }
        #endregion

        /// <summary>
        /// Reads the data from the tracked properties and saves it to the data store for the tracked object.
        /// </summary>
        internal void Persist(object target)
        {
            if (canPersistFunc(target))
            {
                var name = idFunc(target);

                IDictionary<string, object> originalValues = null;
                var values = new Dictionary<string, object>();
                foreach (string propertyName in TrackedProperties.Keys)
                {
                    try
                    {
                        var value = TrackedProperties[propertyName].Getter(target);
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
                        Trace.WriteLine($"Persisting failed, property key = '{name}', property = {propertyName}, message='{ex.Message}'.");
                    }
                }

                Tracker.Store.SetData(name, values);

                OnStatePersisted(target);
            }
        }

        public TrackingConfiguration<T> AsGeneric<T>()
            => new TrackingConfiguration<T>(this);

        /// <summary>
        /// Applies any previously stored data to the tracked properties of the target object.
        /// </summary>
        internal void Apply(object target)
        {
            if (this.TrackedProperties.Count == 0)
                return;

            var name = idFunc(target);
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
                            descriptor.Setter(target, value);
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
                    descriptor.Setter(target, descriptor.DefaultValue);
                }
            }

            OnStateApplied(target);
        }

        /// <summary>
        /// Apply specified defaults to the tracked properties of the target object.
        /// </summary>
        internal void ApplyDefaults(object target)
        {
            if (this.TrackedProperties.Count == 0)
                return;

            var name = idFunc(target);
            var data = Tracker.Store.GetData(name);

            foreach (string propertyName in TrackedProperties.Keys)
            {
                var descriptor = TrackedProperties[propertyName];

                if (descriptor.IsDefaultSpecified)
                {
                    descriptor.Setter(target, descriptor.DefaultValue);
                }
            }

            OnStateApplied(target);
        }

        public string GetStoreId(object target) => idFunc(target);


        /// <summary>
        /// </summary>
        /// <param name="idFunc">The provided function will be used to get an identifier for a target object in order to identify the data that belongs to it.</param>
        /// <param name="includeType">If true, the name of the type will be included in the id. This prevents id clashes with different types.</param>
        /// <param name="namespace">Serves to distinguish objects with the same ids that are used in different contexts.</param>
        /// <returns></returns>
        public ITrackingConfiguration Id(Func<object, string> idFunc, object @namespace = null, bool includeType = true)
        {
            this.idFunc = target =>
            {
                StringBuilder idBuilder = new StringBuilder();
                if (includeType)
                    idBuilder.Append($"[{target.GetType()}]");
                if (@namespace != null)
                    idBuilder.Append($"{@namespace}.");
                idBuilder.Append($"{idFunc(target)}");
                return idBuilder.ToString();
            };

            return this;
        }

        public ITrackingConfiguration CanPersist(Func<object, bool> canPersistFunc)
        {
            this.canPersistFunc = canPersistFunc;
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
        public ITrackingConfiguration PersistOn(params string[] eventNames)
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
        public ITrackingConfiguration PersistOn(string eventName, object eventSourceObject)
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
        public ITrackingConfiguration PersistOn(string eventName, Func<object, object> eventSourceGetter)
        {
            PersistTriggers.Add(new Trigger(eventName, target => eventSourceGetter(target)));
            return this;
        }

        /// <summary>
        /// Stop tracking the target when it fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public ITrackingConfiguration StopTrackingOn(string eventName)
        {
            return StopTrackingOn(eventName, target => target);
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSource"></param>
        /// <returns></returns>
        public ITrackingConfiguration StopTrackingOn(string eventName, object eventSource)
        {
            return StopTrackingOn(eventName, target => eventSource);
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSourceGetter"></param>
        /// <returns></returns>
        public ITrackingConfiguration StopTrackingOn(string eventName, Func<object, object> eventSourceGetter)
        {
            StopTrackingTrigger = new Trigger(eventName, target => eventSourceGetter(target));
            return this;
        }

        internal void StopTracking(object target)
        {
            // unsubscribe from all trigger events
            foreach (var trigger in PersistTriggers)
                trigger.Unsubscribe(target);

            // unsubscribe from stoptracking trigger too
            StopTrackingTrigger?.Unsubscribe(target);

            Tracker.RemoveFromList(target);
        }

        internal void StartTracking(object target)
        {
            // listen for trigger events (for persisting)
            foreach (var trigger in PersistTriggers)
                trigger.Subscribe(target, () => Persist(target));

            // listen to stoptracking event
            StopTrackingTrigger?.Subscribe(target, () => StopTracking(target));
        }

        /// <summary>
        /// Set up tracking for the specified property. Allows supplying a name for the property. 
        /// This overload is used when the target object has a list of child objects whose properties
        /// it wishes to track. Each child object's properties can be tracked with a different name,
        /// e.g. by including the index in the name.
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="name">Name to use when tracking the property's data.</param>
        /// <param name="propertyAccessExpression">The expression that points to the property to track. Supports accessing properties of nested objects.</param>
        /// <returns></returns>
        public ITrackingConfiguration Property<T, TProperty>(Expression<Func<T, TProperty>> propertyAccessExpression, string name = null)
        {
            return Property(name, propertyAccessExpression, false, default(TProperty));
        }

        /// <summary>
        /// Set up tracking for the specified property. Allows supplying a name for the property. 
        /// This overload is used when the target object has a list of child objects whose properties
        /// it wishes to track. Each child object's properties can be tracked with a different name,
        /// e.g. by including the index in the name.
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <typeparam name="TProperty">Type of property</typeparam>
        /// <param name="name">Name to use when tracking the property's data.</param>
        /// <param name="propertyAccessExpression">The expression that points to the property to track. Supports accessing properties of nested objects.</param>
        /// <param name="defaultValue">If there is no value in the store for the property, the defaultValue will be used.</param>
        /// <returns></returns>
        public ITrackingConfiguration Property<T, TProperty>(Expression<Func<T, TProperty>> propertyAccessExpression, TProperty defaultValue, string name = null)
        {
            return Property(name, propertyAccessExpression, true, defaultValue);
        }

        internal ITrackingConfiguration Property<T, TProperty>(string name, Expression<Func<T, TProperty>> propertyAccessExpression, bool defaultSpecified, TProperty defaultValue)
        {
            if (name == null && propertyAccessExpression.Body is MemberExpression me)
            {
                // If not specified, use the entire expression as the name of the property.
                // Note: we don't use just the member name because it might conflict with
                // another property that uses a different expression but the same member name
                // e.g. "firstCol.Width" and "secondCol.Width".
                name = me.ToString();
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
        /// Set up tracking for one or more properties. The expression should be an anonymous type projection (e.g. x => new { x.MyProp1, x.MyProp2 }). 
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="projection">A projection of properties to track. Allows providing nested object properties.</param>
        /// <returns></returns>
        public ITrackingConfiguration Properties<T>(Expression<Func<T, object>> projection)
        {
            NewExpression newExp = projection.Body as NewExpression;

            // VB.NET encapsulates the new expression in a convert-to-object expression
            if (newExp == null && projection.Body is UnaryExpression ue && ue.NodeType == ExpressionType.Convert && ue.Type == typeof(object))
                newExp = ue.Operand as NewExpression;

            if (newExp != null)
            {
                var accessors = newExp.Members.Select((m, i) =>
                {
                    var right = Expression.Parameter(typeof(object));
                    var propType = (m as PropertyInfo).PropertyType;
                    return new
                    {
                        name = m.Name,
                        type = propType,
                        getter = (Expression.Lambda(Expression.Convert(newExp.Arguments[i] as MemberExpression, typeof(object)), projection.Parameters[0]).Compile() as Func<T, object>),
                        // todo: call the Convert method instead of using Expression.Convert which will not work for enums
                        setter = Expression.Lambda(Expression.Block(Expression.Assign(newExp.Arguments[i], Expression.Convert(right, propType)), Expression.Empty()), projection.Parameters[0], right).Compile() as Action<T, object>
                    };
                });

                foreach (var a in accessors)
                {
                    TrackedProperties[a.name] = new TrackedPropertyInfo(x => a.getter((T)x), (x, v) => a.setter((T)x, Convert(v, a.name, a.type)));
                }
            }
            else
            {
                throw new ArgumentException("Expression must project properties as an anonymous class e.g. f => new { f.Height, f.Width } or access a single property e.g. f => f.Text.");
            }
            return this;
        }
    }
}
