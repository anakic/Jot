using Jot.DefaultInitializer;
using System;
using System.Windows;

namespace Jot.CustomInitializers
{
    /// <summary>
    /// Sets up the tracking configuration for WPF Window objects
    /// </summary>
    public class WindowConfigurationInitializer : DefaultConfigurationInitializer
    {
        /// <summary>
        /// Applies to object of type System.windows.Window
        /// </summary>
        public override Type ForType { get { return typeof(Window); } }

        /// <summary>
        /// Adds Height/Width/Top/Left/WindowState to list of tracked properties. Uses
        /// the "Sizechanged" event to trigger persistence. Handles validation for edge
        /// cases (2nd display disconnected between application shutdown and restart).
        /// </summary>
        /// <param name="configuration"></param>
        public override void InitializeConfiguration(TrackingConfiguration configuration)
        {
            Window window = configuration.TargetReference.Target as Window;

            configuration
                .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)
                .RegisterPersistTrigger(nameof(window.SizeChanged))
                .RegisterPersistTrigger(nameof(window.LocationChanged))
                .IdentifyAs(window.Name);

            configuration.ApplyingProperty += (sender, args) =>
            {
                //We don't want to restore the form off screeen.
                //This can happen in case of a multi-display setup i.e. the form was closed on 2nd display, but restored after the 2nd display was disconnected
                if (args.Property == "Left")
                    args.Value = Math.Min(Math.Max(SystemParameters.VirtualScreenLeft, (double)args.Value), SystemParameters.VirtualScreenWidth - window.Width);
            };

            base.InitializeConfiguration(configuration);
        }
    }
}
