using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;

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
            set { _font = value; OnPropertyChanged("Font"); }
        }

        private decimal _fontSize = 15;
        public decimal FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged("FontSize"); }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
