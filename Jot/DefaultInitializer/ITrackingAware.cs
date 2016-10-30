namespace Jot.DefaultInitializer
{
    /// <summary>
    /// Allows the object that is being tracked to customize
    /// its persitence
    /// </summary>
    public interface ITrackingAware
    {
        /// <summary>
        /// Called when the object's tracking configuration is first created.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Return false to cancel applying state</returns>
        void InitConfiguration(TrackingConfiguration configuration);
    }
}
