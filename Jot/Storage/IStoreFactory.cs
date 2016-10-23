using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jot.Storage
{
    public interface IStoreFactory
    {
        IObjectStore CreateStoreForObject(string objectId);
    }
}
