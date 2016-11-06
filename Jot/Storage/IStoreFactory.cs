namespace Jot.Storage
{
    /// <summary>
    /// Interface for objects that are responsible for creating data stores (implementations of IStore) for tracked objects.
    /// </summary>
    /// <remarks>
    /// Each tracked object has it's own data store, which is why the factory pattern is used.
    /// </remarks>
    public interface IStoreFactory
    {
        /// <summary>
        /// Creates a data store for the specified id.
        /// </summary>
        /// <param name="objectId">The id of othe object to which the store will belong.</param>
        /// <returns></returns>
        IStore CreateStoreForObject(string objectId);
    }
}
