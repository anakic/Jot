using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TestWPF.Settings
{
    [Serializable]
    public class GeneralSettings
    {
        public int Property1 { get; set; }
        public string Property2 { get; set; }
        public IPAddress Property3 { get; set; }
        public bool Property4 { get; set; }
    }
}
