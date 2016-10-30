namespace Jot.Storage
{
    public interface IStoreFactory
    {
        IObjectStore CreateStoreForObject(string objectId);
    }
}
