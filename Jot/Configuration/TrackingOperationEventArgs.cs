using System.ComponentModel;

namespace Jot.Configuration
{
    /// <summary>
    /// Event args for a tracking operation. Enables the handler to cancel the operation and modify the data that will be persisted/applied.
    /// </summary>
    public class PropertyOperationData
    {
        public bool Cancel { get; set; }

        /// <summary>
        /// The property that is being persisted or applied to.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// The value that is being persited or applied. Has a setter to support converting/mapping/limiting values when applying/persisting. 
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Creates a new instance of PropertyData.
        /// </summary>
        /// <param name="property">The property that is being persisted or applied to.</param>
        /// <param name="value">The value that is being persited or applied.</param>
        public PropertyOperationData(string property, object value)
        {
            Property = property;
            Value = value;
        }
    }
}
