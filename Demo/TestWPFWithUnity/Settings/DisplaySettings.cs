using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace TestWPFWithUnity.Settings
{
    //notice that I implemented INotifyPropertyChanged in my settings
    //which would be difficult to do with the appconfig approach

    [Serializable]
    public class DisplaySettings : INotifyPropertyChanged
    {
        private FontFamily _font;
        public FontFamily Font
        {
            get { return _font; }
            set { _font = value; OnPropertyChanged(); }
        }

        private decimal _fontSize = 15;
        public decimal FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged(); }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
