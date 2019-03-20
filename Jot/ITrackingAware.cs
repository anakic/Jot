namespace Jot.Configuration
{
    /// <summary>
    /// Allows the object that is being tracked to customize
    /// its persitence
    /// </summary>
    public interface ITrackingAware<T>
    {
        /// <summary>
        /// Called when the object's tracking configuration is first created.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Return false to cancel applying state</returns>
        void ConfigureTracking(TrackingConfiguration<T> configuration);
    }
}
