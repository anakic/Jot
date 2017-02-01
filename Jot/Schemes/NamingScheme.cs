using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot
{
    /// <summary>
    /// Naming scheme for Storaname (defines filename format, where is the configuration stored).
    /// </summary>
    public enum NamingScheme
    {
        /// <summary>
        /// {TypeName}_{key}
        /// </summary>
        TypeNameAndKey,
        /// <summary>
        /// {key}
        /// </summary>
        KeyOnly,
    }
}
