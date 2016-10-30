namespace Jot.Storage
{
    public interface IStoreFactory
    {
        IStore CreateStoreForObject(string objectId);
    }
}
