using Jot.Configuration;

namespace TestWPF.Settings
{
    public class AppSettings : ITrackingAware<AppSettings>
    {
        public DisplaySettings DisplaySettings { get; set; }
        public GeneralSettings GeneralSettings { get; set; }

        public AppSettings()
        {
            DisplaySettings = new DisplaySettings();
            GeneralSettings = new GeneralSettings();
        }

        public void ConfigureTracking(TrackingConfiguration<AppSettings> configuration)
        {
            configuration.Properties(s => new { s.DisplaySettings, s.GeneralSettings });
            System.Windows.Application.Current.Exit += (s, e) => { configuration.Tracker.Persist(this); };
        }
    }
}
