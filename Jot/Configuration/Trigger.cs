using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jot.Configuration
{
    public class Trigger
    {
        ConditionalWeakTable<object, Delegate> _handlers = new ConditionalWeakTable<object, Delegate>();

        public string EventName { get; }
        public Func<object, object> SourceGetter { get; }

        public Trigger(string eventName, Func<object, object> sourceGetter)
        {
            EventName = eventName;
            SourceGetter = sourceGetter;
        }

        public void Subscribe(object target, Action action)
        {
            // clear a possible previous subscription for the same target/event
            Unsubscribe(target);

            var source = SourceGetter(target);

            EventInfo eventInfo = source.GetType().GetEvent(EventName);
            var parameters = eventInfo.EventHandlerType
                .GetMethod("Invoke")
                .GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType))
                .ToArray();

            var handler = Expression.Lambda(
                    eventInfo.EventHandlerType,
                    Expression.Call(Expression.Constant(action), "Invoke", Type.EmptyTypes),
                    parameters)
              .Compile();

            eventInfo.AddEventHandler(source, handler);

            _handlers.Add(source, handler);
        }

        public void Unsubscribe(object target)
        {
            if (_handlers.TryGetValue(target, out Delegate handler))
            {
                var source = SourceGetter(target);
                EventInfo eventInfo = source.GetType().GetEvent(EventName);
                eventInfo.RemoveEventHandler(source, handler);
                _handlers.Remove(target);
            }
        }
    }
}
