using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    public interface IShapeFileReader : IMutliThreaded
    {
        int RecordsRead { get; }
        ConcurrentQueue<IMapData> Records { get; set; }
        void BeginReadingAsync(CancellationToken token = default);
    }
}