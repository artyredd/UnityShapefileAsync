using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace PSS.Mapping.CSV
{
    public interface ICSVReader : IMutliThreaded
    {
        int LinesRead { get; }
        bool Reading { get; }
        int LinesConverted { get; }
        int TotalLines { get; }

        ConcurrentQueue<string[]> RowQueue { get; }

        void ReadCSV(params int[] columnsToKeep);
    }
}
