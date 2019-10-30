using System;
using System.Linq.Expressions;

namespace Jot.Configuration
{
    /*
     * Derives from TrackingConfiguration and adds a generic strongly typed API for configuring tracking.
     * This class does not provide any new functionality nor store any additional state. All calls are forwarded to the base class.
     */

    /// <summary>
    /// A TrackingConfiguration is an object that determines how a target object will be tracked.
    /// </summary>
    public sealed class TrackingConfiguration<T> : TrackingConfiguration
    {
        internal TrackingConfiguration(Tracker tracker) : base(tracker, typeof(T))
        {
        }

        internal TrackingConfiguration(TrackingConfiguration baseConfig) 
            : base(baseConfig, typeof(T))
        {
        }

        /// <summary>
        /// Allows value conversion and cancallation when applying a stored value to a property.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenApplyingProperty(Action<T, PropertyOperationData> action)
        {
            base.WhenApplyingProperty((obj, prop) => action((T)obj, prop));
            return this;
        }

        /// <summary>
        /// Allows supplying a callback that will be called when all saved state is applied to a target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenAppliedState(Action<T> action)
        {
            base.WhenAppliedState(obj => action((T)obj));
            return this;
        }

        /// <summary>
        /// Allows value conversion and cancallation when persisting a property of the target object.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> WhenPersistingProperty(Action<T, PropertyOperationData> action)
        {
            base.WhenPersistingProperty((obj, prop) => action((T)obj, prop));
            return this;
        }

        public TrackingConfiguration<T> WhenPersisted(Action<T> action)
        {
            base.WhenPersisted(obj => action((T)obj));
            return this;
        }
        
        /// <summary>
        /// </summary>
        /// <param name="idFunc">The provided function will be used to get an identifier for a target object in order to identify the data that belongs to it.</param>
        /// <param name="includeType">If true, the name of the type will be included in the id. This prevents id clashes with different types.</param>
        /// <param name="namespace">Serves to distinguish objects with the same ids that are used in different contexts.</param>
        /// <returns></returns>
        public TrackingConfiguration<T> Id(Func<T, string> idFunc, object @namespace = null, bool includeType = true)
        {
            base.Id(t => idFunc((T)t), @namespace, includeType);
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
        public new TrackingConfiguration<T> PersistOn(params string[] eventNames)
        {
            base.PersistOn(eventNames);
            return this;
        }

        /// <summary>
        /// Automatically persist a target object when the specified eventSourceObject fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSourceObject">If not provided, </param>
        /// <returns></returns>
        public new TrackingConfiguration<T> PersistOn(string eventName, object eventSourceObject)
        {
            base.PersistOn(eventName, eventSourceObject);
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
            base.PersistOn(eventName, t => eventSourceGetter((T)t));
            return this;
        }

        /// <summary>
        /// Stop tracking the target when it fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public new TrackingConfiguration<T> StopTrackingOn(string eventName)
        {
            base.StopTrackingOn(eventName);
            return this;
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSource"></param>
        /// <returns></returns>
        public new TrackingConfiguration<T> StopTrackingOn(string eventName, object eventSource)
        {
            base.StopTrackingOn(eventName, eventSource);
            return this;
        }

        /// <summary>
        /// Stop tracking the target when the specified eventSource object fires the specified event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventSourceGetter"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> StopTrackingOn(string eventName, Func<T, object> eventSourceGetter)
        {
            base.StopTrackingOn(eventName, t => eventSourceGetter((T)t));
            return this;
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
            base.Property(name, propertyAccessExpression, defaultSpecified, defaultValue);
            return this;
        }

        /// <summary>
        /// Set up tracking for one or more properties. The expression should be an anonymous type projection (e.g. x => new { x.MyProp1, x.MyProp2 }). 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="projections"></param>
        /// <returns></returns>
        public TrackingConfiguration<T> Properties(Expression<Func<T, object>> projection)
        {
            base.Properties(projection);
            return this;
        }
    }
}
