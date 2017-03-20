using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestWPF
{
    class IPValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            IPAddress dummy;
            return IPAddress.TryParse(value as string, out dummy) ? ValidationResult.ValidResult : new ValidationResult(false, "Invalid IP address");
        }
    }
}
