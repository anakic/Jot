using System.ComponentModel;

namespace Jot
{
    /// <summary>
    /// Event args for a tracking operation. Enables the handler to cancel the operation and to modify the data that will be persisted/applied.
    /// </summary>
    public class TrackingOperationEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The TrackingConfiguration object that initiated the tracking operation.
        /// </summary>
        public TrackingConfiguration Configuration { get; private set; }

        /// <summary>
        /// The property that is being persisted or applied to.
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// The value that is being persited or applied.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Creates a new instance of TrackingOperationEventArgs.
        /// </summary>
        /// <param name="configuration">The TrackingConfiguration object that initiated the tracking operation.</param>
        /// <param name="property">The property that is being persisted or applied to.</param>
        /// <param name="value">The value that is being persited or applied.</param>
        public TrackingOperationEventArgs(TrackingConfiguration configuration, string property, object value)
        {
            Configuration = configuration;
            Property = property;
            Value = value;
        }
    }
}
