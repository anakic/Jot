using System;

namespace Jot.Configuration
{
    public class TrackingEventArgs<T> : EventArgs
    {
        public T Target { get; }
        public TrackingEventArgs(T target)
        {
            Target = target;
        }
    }
}
