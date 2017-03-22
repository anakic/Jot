using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using Jot.Storage;

namespace Jot
{
    /// <summary>
    /// A TrackingConfiguration is an object that determines how a target object will be tracked.
    /// </summary>
    public sealed class TrackingConfiguration
    {
        /// <summary>
        /// Indicates if previously stored data has been applied to the target object.
        /// </summary>
        public bool IsApplied { get; private set; }

        /// <summary>
        /// The StateTracker that owns this tracking configuration.
        /// </summary>
        public StateTracker StateTracker { get; private set; }

        /// <summary>
        /// The store that is used to save and retrieve the target's data.
        /// </summary>
        public IStore TargetStore { get; private set; }

        /// <summary>
        /// A weak reference to the target object.
        /// </summary>
        public WeakReference TargetReference { get; private set; }

        /// <summary>
        /// The identity of the target. This severs to identify which stored data belongs to which object. If not specified,
        /// only the type name is used, which is fine for singletons. (check out NamingScheme)
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Defines format of store name. Default value is TypeNameAndKey. In that case the storename is "{typename}_{key}".
        /// </summary>
        public NamingScheme StoreNamingScheme { get; set; } = NamingScheme.TypeNameAndKey;

        /// <summary>
        /// A dictionary containing the tracked properties.
        /// </summary>
        public Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; set; } = new Dictionary<string, TrackedPropertyInfo>();

        /// <summary>
        /// Should the target object be persisted when a global persist trigger is fired.
        /// </summary>
        public bool AutoPersistEnabled { get; set; } = true;

        #region apply/persist events

        /// <summary>
        /// Fired before previously persisted data is applied to a property of the target object. 
        /// Allows the handler to cancel applying the data to the property, as well as to modify the data that gets applied.
        /// </summary>
        public event EventHandler<TrackingOperationEventArgs> ApplyingProperty;
        private bool OnApplyingState(string property, ref object value)
        {
            var handler = ApplyingProperty;
            if (handler != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property, value);
                handler(this, args);
                value = args.Value;
                return !args.Cancel;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Fired when previously persisted data is applied to target object. 
        /// </summary>
        public event EventHandler StateApplied;
        private void OnStateApplied()
        {
            StateApplied?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fired when the a property of the object is being persisted. Allows the handler to cancel persisting the property, as well as to modify the data that gets persisted.
        /// </summary>
        public event EventHandler<TrackingOperationEventArgs> PersistingProperty;
        private bool OnPersistingState(string property, ref object value)
        {
            var handler = PersistingProperty;
            if (handler != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property, value);
                handler(this, args);
                value = args.Value;
                return !args.Cancel;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Fired when the data for the target object is persisted.
        /// </summary>
        public event EventHandler StatePersisted;
        private void OnStatePersisted()
        {
            StatePersisted?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        internal TrackingConfiguration(object target, StateTracker tracker)
            : this(target, null, tracker)
        {
        }

        internal TrackingConfiguration(object target, string idenitifier, StateTracker tracker)
        {
            TargetReference = new WeakReference(target);
            Key = idenitifier;
            StateTracker = tracker;
        }

        /// <summary>
        /// Reads the data from the tracked properties and saves it to the data store for the tracked object.
        /// </summary>
        public void Persist()
        {
            if (TargetReference.IsAlive)
            {
                if (TargetStore == null)
                    TargetStore = InitStore();

                foreach (string propertyName in TrackedProperties.Keys)
                {
                    var value = TrackedProperties[propertyName].Getter(TargetReference.Target);
                    try
                    {
                        var shouldPersist = OnPersistingState(propertyName, ref value);
                        if (shouldPersist)
                        {
                            TargetStore.Set(value, propertyName);
                        }
                        else
                        {
                            Trace.WriteLine(string.Format("Persisting cancelled, key='{0}', property='{1}'.", Key, propertyName));
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Persisting failed, property key = '{0}', message='{1}'.", propertyName, ex.Message));
                    }
                }

                TargetStore.CommitChanges();

                OnStatePersisted();
            }
        }

        /// <summary>
        /// Applies any previously stored data to the tracked properties of the target object.
        /// </summary>
        public void Apply()
        {
            if (TargetReference.IsAlive)
            {
                if (TargetStore == null)
                    TargetStore = InitStore();

                foreach (string propertyName in TrackedProperties.Keys)
                {
                    TrackedPropertyInfo descriptor = TrackedProperties[propertyName];

                    if (TargetStore.ContainsKey(propertyName))
                    {
                        try
                        {
                            object value = TargetStore.Get(propertyName);
                            var shouldApply = OnApplyingState(propertyName, ref value);
                            if (shouldApply)
                            {
                                descriptor.Setter(TargetReference.Target, value);
                            }
                            else
                            {
                                Trace.WriteLine(string.Format("Persisting cancelled, key='{0}', property='{1}'.", Key, propertyName));
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("TRACKING: Applying tracking to property with key='{0}' failed. ExceptionType:'{1}', message: '{2}'!", propertyName, ex.GetType().Name, ex.Message));
                        }
                    }
                    else if (descriptor.IsDefaultSpecified)
                    {
                        descriptor.Setter(TargetReference.Target, descriptor.DefaultValue);
                    }
                }

                OnStateApplied();
            }
            IsApplied = true;
        }

        /// <summary>
        /// Sets the identity (Key) of the object. It is important to set the identity in situations
        /// where you want to track multiple objects of the same type, so Jot can know which data belongs 
        /// to what object. Otherwise, they will use the same data which is usually not what you want.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="storeNamingScheme"></param>
        /// <returns></returns>
        public TrackingConfiguration IdentifyAs(string key, NamingScheme storeNamingScheme = NamingScheme.TypeNameAndKey)
        {
            if (TargetStore != null)
                throw new InvalidOperationException("Can't set key after TargetStore has been set (which happens the first time Apply() or Persist() is called).");

            Key = key;
            StoreNamingScheme = storeNamingScheme;

            return this;
        }

        /// <summary>
        /// Adds a list of properties (of the target object) to track.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperties(params string[] properties)
        {
            foreach (string property in properties)
                TrackedProperties[property] = CreateDescriptor(property, false, null);
            return this;
        }

        /// <summary>
        /// Adds a property (of the target object) to track.
        /// </summary>
        /// <typeparam name="T">The type of the target object</typeparam>
        /// <param name="properties">A list of expressions that point to the properties of the target object that you want to track.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperties<T>(params Expression<Func<T, object>>[] properties)
        {
            AddProperties(properties.Select(p => GetPropertyNameFromExpression(p)).ToArray());
            return this;
        }

        /// <summary>
        /// Adds a property (of the target object) to track.
        /// </summary>
        /// <typeparam name="T">The type of the target object</typeparam>
        /// <param name="property">Expression that points to the property of the target object that you want to track.</param>
        /// <param name="defaultValue">The value that will be applied to the property if no previously stored data exists.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperty<T>(Expression<Func<T, object>> property, object defaultValue)
        {
            AddProperty(GetPropertyNameFromExpression(property), defaultValue);
            return this;
        }

        /// <summary>
        /// Adds a property (of the target object) to track.
        /// </summary>
        /// <typeparam name="T">The type of the target object</typeparam>
        /// <param name="property">Expression that points to the property of the target object that you want to track.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperty<T>(Expression<Func<T, object>> property)
        {
            AddProperty(GetPropertyNameFromExpression(property));
            return this;
        }

        /// <summary>
        /// Adds a property (of the target object) to track.
        /// </summary>
        /// <param name="property">The name the property of the target object that you want to track.</param>
        /// <param name="defaultValue">The value that will be applied to the property if no previously stored data exists.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperty(string property, object defaultValue)
        {
            TrackedProperties[property] = CreateDescriptor(property, true, defaultValue);
            return this;
        }

        /// <summary>
        /// Adds a property (of the target object) to track.
        /// </summary>
        /// <param name="property">The name the property of the target object that you want to track.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration AddProperty(string property)
        {
            TrackedProperties[property] = CreateDescriptor(property, false, null);
            return this;
        }

        /// <summary>
        /// Removes a list of properties from the list of tracked properties.
        /// </summary>
        /// <param name="properties">The list of properties to remove from the list of tracked properties.</param>
        /// <returns>The tracking configuration object itself, for further method chaining.</returns>
        public TrackingConfiguration RemoveProperties(params string[] properties)
        {
            foreach (string property in properties)
                TrackedProperties.Remove(property);
            return this;
        }
        /// <summary>
        /// Removes a list of properties from the list of tracked properties.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="properties">The list of expressions that point to properties (of the target object) to remove from the list of tracked properties.</param>
        /// <returns></returns>
        public TrackingConfiguration RemoveProperties<T>(params Expression<Func<T, object>>[] properties)
        {
            RemoveProperties(properties.Select(p => GetPropertyNameFromExpression(p)).ToArray());
            return this;
        }

        /// <summary>
        /// Registers the specified event of the target object as a trigger that will cause the target's data to be persisted.
        /// </summary>
        /// <example>
        /// For a Window object, "LocationChanged" and/or "SizeChanged" would be appropriate.
        /// </example>
        /// <remarks>
        /// The tracking configuration will subscribe to the specified even of the target object and will call Persist() when the event is fired.
        /// </remarks>
        /// <param name="eventName">The name of the event that will cause the target object's data to be persisted.</param>
        /// <returns></returns>
        public TrackingConfiguration RegisterPersistTrigger(string eventName)
        {
            return RegisterPersistTrigger(eventName, TargetReference.Target);
        }

        /// <summary>
        /// Registers the specified event of the specified object as a trigger that will cause the target's data to be persisted.
        /// </summary>
        /// <remarks>
        /// The tracking configuration will subscribe to the specified even of the specified object and will call Persist() when the event is fired.
        /// </remarks>
        /// <param name="eventName">The name of the event that will cause the target object's data to be persisted.</param>
        /// <param name="eventSourceObject">The object that owns the event.</param>
        /// <returns></returns>
        public TrackingConfiguration RegisterPersistTrigger(string eventName, object eventSourceObject)
        {
            EventInfo eventInfo = eventSourceObject.GetType().GetEvent(eventName);
            var parameters = eventInfo.EventHandlerType
                .GetMethod("Invoke")
                .GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType))
                .ToArray();

            var handler = Expression.Lambda(
                    eventInfo.EventHandlerType,
                    Expression.Call(Expression.Constant(new Action(() => { if (IsApplied) Persist(); /*don't persist before applying stored value*/ })), "Invoke", Type.EmptyTypes),
                    parameters)
              .Compile();

            eventInfo.AddEventHandler(eventSourceObject, handler);
            return this;
        }

        /// <summary>
        /// Specifies if the object should be persisted during a global persist (usually fired just before application shutdown).
        /// </summary>
        /// <param name="shouldAutoPersist">If true, will be persisted when global persist trigger is fired.</param>
        /// <returns></returns>
        public TrackingConfiguration SetAutoPersistEnabled(bool shouldAutoPersist)
        {
            AutoPersistEnabled = shouldAutoPersist;
            return this;
        }

        private static string GetPropertyNameFromExpression<T>(Expression<Func<T, object>> exp)
        {
            MemberExpression membershipExpression;
            if (exp.Body is UnaryExpression)
                membershipExpression = (exp.Body as UnaryExpression).Operand as MemberExpression;
            else
                membershipExpression = exp.Body as MemberExpression;
            return membershipExpression.Member.Name;
        }

        private IStore InitStore()
        {
            object target = TargetReference.Target;

            //use the object type plus the key to identify the object store based on NamingScheme
            string storeName;
            switch (StoreNamingScheme)
            {
                case NamingScheme.KeyOnly: storeName = Key; break;
                //default is reserved for NamingScheme.TypeNameAndKey
                default: storeName = Key == null ? target.GetType().Name : string.Format("{0}_{1}", target.GetType().Name, Key); break;
            }
            return StateTracker.StoreFactory.CreateStoreForObject(storeName);
        }

        private TrackedPropertyInfo CreateDescriptor(string propertyName, bool isDefaultSpecifier, object defaultValue)
        {
            PropertyInfo pi = TargetReference.Target.GetType().GetProperty(propertyName);
            return new TrackedPropertyInfo(
                obj => pi.GetValue(obj, null),
                (obj, val) => pi.SetValue(obj, val, null),
                isDefaultSpecifier,
                defaultValue);
        }
    }
}
