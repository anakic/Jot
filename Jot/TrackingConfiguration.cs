using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using Jot.Storage;

namespace Jot
{
    public sealed class TrackingConfiguration
    {
        public StateTracker StateTracker { get; private set; }
        public IStore TargetStore { get; private set; }

        public WeakReference TargetReference { get; private set; }
        public string Key { get; set; }
        public Dictionary<string, TrackedPropertyInfo> TrackedProperties { get; set; } = new Dictionary<string, TrackedPropertyInfo>();//todo: add DefaultValue property to trackable attribute
        public bool AutoPersistEnabled { get; set; } = true;

        #region apply/persist events

        public event EventHandler<TrackingOperationEventArgs> ApplyingProperty;
        private object OnApplyingState(string property, object value)
        {
            var handler = ApplyingProperty;
            if (handler != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property, value);
                handler(this, args);

                if (args.Cancel)
                    throw new OperationCanceledException();

                return args.Value;
            }
            else
                return value;
        }

        public event EventHandler StateApplied;
        private void OnStateApplied()
        {
            StateApplied?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<TrackingOperationEventArgs> PersistingProperty;
        private object OnPersistingState(string property, object value)
        {
            var handler = PersistingProperty;
            if (handler != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property, value);
                handler(this, args);
                if (args.Cancel)
                    throw new OperationCanceledException();
                else
                    return args.Value;
            }
            return value;
        }

        public event EventHandler StatePersisted;
        private void OnStatePersisted()
        {
            StatePersisted?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        internal TrackingConfiguration(object target, StateTracker tracker)
            :this(target, null, tracker)
        {
        }

        internal TrackingConfiguration(object target, string idenitifier, StateTracker tracker)
        {
            TargetReference = new WeakReference(target);
            Key = idenitifier;
            StateTracker = tracker;
        }

        /// <summary>
        /// Initialize is called by StateTracker after the configuration object has been prepared (properties added, triggers set etc...).
        /// </summary>
        internal void CompleteInitialization()
        {
            object target = TargetReference.Target;

            //use the object type plus the key to identify the object store
            string storeName = Key == null ? target.GetType().Name : string.Format("{0}_{1}", target.GetType().Name, Key);
            TargetStore = StateTracker.StoreFactory.CreateStoreForObject(storeName);
            TargetStore.Initialize();
        }

        public void Persist()
        {
            if (TargetReference.IsAlive)
            {
                foreach (string propertyName in TrackedProperties.Keys)
                {
                    var value = TrackedProperties[propertyName].Getter(TargetReference.Target);

                    try
                    {
                        value = OnPersistingState(propertyName, value);
                        TargetStore.Set(value, propertyName);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Trace.WriteLine(string.Format("Persisting cancelled, property key = '{0}', message='{1}'.", propertyName, ex.Message));
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

        bool _applied = false;
        public void Apply()
        {
            if (TargetReference.IsAlive)
            {
                foreach (string propertyName in TrackedProperties.Keys)
                {
                    TrackedPropertyInfo descriptor = TrackedProperties[propertyName];

                    if (TargetStore.ContainsKey(propertyName))
                    {
                        try
                        {
                            object storedValue = TargetStore.Get(propertyName);
                            object valueToApply = OnApplyingState(propertyName, storedValue);
                            descriptor.Setter(TargetReference.Target, valueToApply);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("TRACKING: Applying tracking to property with key='{0}' failed. ExceptionType:'{1}', message: '{2}'!", propertyName, ex.GetType().Name, ex.Message));
                            if(descriptor.IsDefaultSpecified)
                                descriptor.Setter(TargetReference.Target, descriptor.DefaultValue);
                        }
                    }
                    else if (descriptor.IsDefaultSpecified)
                    {
                        descriptor.Setter(TargetReference.Target, descriptor.DefaultValue);
                    }
                }

                OnStateApplied();
            }
            _applied = true;
        }

        public TrackingConfiguration AddProperties(params string[] properties)
        {
            foreach (string property in properties)
                TrackedProperties[property] = CreateDescriptor(property, false, null);
            return this;
        }
        public TrackingConfiguration AddProperties<T>(params Expression<Func<T, object>>[] properties)
        {
            AddProperties(properties.Select(p => GetPropertyNameFromExpression(p)).ToArray());
            return this;
        }

        public TrackingConfiguration AddProperty<T>(Expression<Func<T, object>> property, object defaultValue)
        {
            AddProperty(GetPropertyNameFromExpression(property), defaultValue);
            return this;
        }
        public TrackingConfiguration AddProperty<T>(Expression<Func<T, object>> property)
        {
            AddProperty(GetPropertyNameFromExpression(property));
            return this;
        }

        public TrackingConfiguration AddProperty(string property, object defaultValue)
        {
            TrackedProperties[property] = CreateDescriptor(property, true, defaultValue);
            return this;
        }

        public TrackingConfiguration AddProperty(string property)
        {
            TrackedProperties[property] = CreateDescriptor(property, false, null);
            return this;
        }

        public TrackingConfiguration RemoveProperties(params string[] properties)
        {
            foreach (string property in properties)
                TrackedProperties.Remove(property);
            return this;
        }
        public TrackingConfiguration RemoveProperties<T>(params Expression<Func<T, object>>[] properties)
        {
            RemoveProperties(properties.Select(p => GetPropertyNameFromExpression(p)).ToArray());
            return this;
        }

        public TrackingConfiguration RegisterPersistTrigger(string eventName)
        {
            return RegisterPersistTrigger(eventName, TargetReference.Target);
        }

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
                    Expression.Call(Expression.Constant(new Action(() => { if (_applied) Persist(); /*don't persist before applying stored value*/ })), "Invoke", Type.EmptyTypes),
                    parameters)
              .Compile();

            eventInfo.AddEventHandler(eventSourceObject, handler);
            return this;
        }

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