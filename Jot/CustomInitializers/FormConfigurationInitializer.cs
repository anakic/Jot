using Jot.DefaultInitializer;
using System;
using System.Windows;
using System.Windows.Forms;

namespace Jot.CustomInitializers
{
    /// <summary>
    /// Initializes the tracking configuration for a (WinForms) Form object. 
    /// </summary>
    public class FormConfigurationInitializer : DefaultConfigurationInitializer
    {
        /// <summary>
        /// Applies to objects of type System.Windows.Forms.Form
        /// </summary>
        public override Type ForType { get { return typeof(Form); } }
        
        /// <summary>
        /// Adds Height/Width/Top/Left/Windows state to the list of properties to track. Uses
        /// the "ResizeEnd" to trigger persist. Handles validation for edge cases (2nd display
        /// disconnected, saving size info while minimized/maximized).
        /// </summary>
        /// <param name="configuration"></param>
        public override void InitializeConfiguration(TrackingConfiguration configuration)
        {
            var form = configuration.TargetReference.Target as Form;

            configuration
                .AddProperties<Form>(f => f.Height, f => f.Width, f => f.Top, f => f.Left, f => f.WindowState)
                .RegisterPersistTrigger(nameof(form.ResizeEnd))
                .RegisterPersistTrigger(nameof(form.LocationChanged))
                .IdentifyAs(form.Name);

            configuration.PersistingProperty += (sender, args) =>
            {
                //do not save height/width/top/left when the form is maximized or minimized
                args.Cancel = form.WindowState != FormWindowState.Normal && args.Property != nameof(form.WindowState);
            };

            configuration.ApplyingProperty += (sender, args) =>
            {
                //We don't want to restore the form off screeen.
                //This can happen in case of a multi-display setup i.e. the form was closed on 2nd display, but restored after the 2nd display was disconnected
                if (args.Property == "Left")
                    args.Value = (int)Math.Min(Math.Max(SystemParameters.VirtualScreenLeft, (int)args.Value), SystemParameters.VirtualScreenWidth - form.Width);
            };

            base.InitializeConfiguration(configuration);
        }
    }
}
