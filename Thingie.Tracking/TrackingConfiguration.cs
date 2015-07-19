using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.ComponentModel;
using Thingie.Tracking.Description;

namespace Thingie.Tracking
{
    public sealed class TrackingConfiguration
    {
        public SettingsTracker SettingsTracker { get; private set; }

        public string Key { get; set; }
        public Dictionary<string, TrackedPropertyDescriptor> Defaults { get; set; }//todo: add DefaultValue property to trackable attribute
        public WeakReference TargetReference { get; private set; }

        #region apply/persist events

        public event EventHandler<TrackingOperationEventArgs> ApplyingProperty;
        private bool OnApplyingState(string property)
        {
            if (ApplyingProperty != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property);
                ApplyingProperty(this, args);
                return !args.Cancel;
            }
            else
                return true;
        }

        public event EventHandler StateApplied;
        private void OnStateApplied()
        {
            if (StateApplied != null)
                StateApplied(this, EventArgs.Empty);
        }

        public event EventHandler<TrackingOperationEventArgs> PersistingProperty;
        private bool OnPersistingState(string property)
        {
            if (PersistingProperty != null)
            {
                TrackingOperationEventArgs args = new TrackingOperationEventArgs(this, property);
                PersistingProperty(this, args);
                return !args.Cancel;
            }
            return true;
        }

        public event EventHandler StatePersisted;
        private void OnStatePersisted()
        {
            if (StatePersisted != null)
                StatePersisted(this, EventArgs.Empty);
        }
        #endregion

        internal TrackingConfiguration(object target, SettingsTracker tracker)
        {
            SettingsTracker = tracker;
            this.TargetReference = new WeakReference(target);
            Defaults = new Dictionary<string, TrackedPropertyDescriptor>();
            AddMetaData();

            ITrackingAware trackingAwareTarget = target as ITrackingAware;
            if (trackingAwareTarget != null)
                trackingAwareTarget.InitConfiguration(this);

            INotifyPersistenceRequired asNotify = target as INotifyPersistenceRequired;
            if (asNotify != null)
                asNotify.PersistenceRequired += (s, e) => Persist();
        }

        public void Persist()
        {
            if (TargetReference.IsAlive)
            {
                foreach (string propertyName in Defaults.Keys)
                {
                    if (OnPersistingState(propertyName) == false)
                        continue;

                    try
                    {
                        SettingsTracker.ObjectStore.Persist(Defaults[propertyName].Getter(TargetReference.Target), ConstructPropertyKey(propertyName));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Persisting failed, property key = '{0}', message='{1}'.", ConstructPropertyKey(propertyName), ex.Message));
                    }
                }

                OnStatePersisted();
            }
        }

        bool _applied = false;
        public void Apply()
        {
            if (TargetReference.IsAlive)
            {
                foreach (string propertyName in Defaults.Keys)
                {
                    if (OnApplyingState(propertyName) == false)
                        continue;

                    string key = ConstructPropertyKey(propertyName);
                    TrackedPropertyDescriptor descriptor = Defaults[propertyName];

                    if (SettingsTracker.ObjectStore.ContainsKey(key))
                    {
                        try
                        {
                            object storedValue = SettingsTracker.ObjectStore.Retrieve(key);
                            descriptor.Setter(TargetReference.Target, storedValue);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("TRACKING: Applying tracking to property with key='{0}' failed. ExceptionType:'{1}', message: '{2}'!", key, ex.GetType().Name, ex.Message));
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
                Defaults[property] = CreateDescriptor(property, false, null);
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

        public TrackingConfiguration AddProperty(string property, object defaultValue)
        {
            Defaults[property] = CreateDescriptor(property, true, defaultValue);
            return this;
        }

        public TrackingConfiguration RemoveProperties(params string[] properties)
        {
            foreach (string property in properties)
                Defaults.Remove(property);
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
                    Expression.Call(Expression.Constant(new Action(() => { if (_applied) Persist(); })), "Invoke", Type.EmptyTypes),
                    parameters)
              .Compile();

            eventInfo.AddEventHandler(eventSourceObject, handler);
            return this;
        }

        public TrackingConfiguration SetId(string key)
        {
            this.Key = key;
            return this;
        }

        private TrackingConfiguration AddMetaData()
        {
            Type t = TargetReference.Target.GetType();

            PropertyInfo keyProperty = t.GetProperties().SingleOrDefault(pi => pi.IsDefined(typeof(TrackingKeyAttribute), true));
            if (keyProperty != null)
                Key = keyProperty.GetValue(TargetReference.Target, null).ToString();

            foreach (PropertyInfo pi in t.GetProperties())
            {
                //don't track the key property
                if (pi == keyProperty)
                    continue;

                TrackableAttribute propTrackableAtt = pi.GetCustomAttributes(true).OfType<TrackableAttribute>().Where(ta => ta.TrackerName == SettingsTracker.Name).SingleOrDefault();
                if (propTrackableAtt != null)
                {
                    DefaultValueAttribute defaultAtt = pi.CustomAttributes.OfType<DefaultValueAttribute>().SingleOrDefault();
                    if (defaultAtt != null)
                        Defaults[pi.Name] = CreateDescriptor(pi.Name, true, defaultAtt.Value);
                    else
                        Defaults[pi.Name] = CreateDescriptor(pi.Name, false, null);
                }
            }

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

        public TrackingConfiguration ClearSavedState()
        {
            foreach (string propertyName in Defaults.Keys)
            {
                string key = ConstructPropertyKey(propertyName);
                try
                {
                    SettingsTracker.ObjectStore.Remove(key);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("TRACKING: Failed to delete property '{0}'. ExceptionType:'{1}', message: '{2}'!", key, ex.GetType().Name, ex.Message));
                }
            }
            return this;
        }

        private TrackedPropertyDescriptor CreateDescriptor(string propertyName, bool isDefaultSpecifier, object defaultValue)
        {
            PropertyInfo pi = TargetReference.Target.GetType().GetProperty(propertyName);
            return new TrackedPropertyDescriptor(
                obj => pi.GetValue(obj, null),
                (obj, val) => pi.SetValue(obj, val, null),
                isDefaultSpecifier,
                defaultValue);
        }

        private string ConstructPropertyKey(string propertyName)
        {
            return string.Format("{0}_{1}.{2}", TargetReference.Target.GetType().Name, Key, propertyName);
        }
    }
}