using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    public interface IMapReader : IMultiThreaded
    {
        ConcurrentBag<IMapRecord> MapRecords { get; }
    }
}
