using System.Threading.Tasks;
using System.Collections.Generic;
using PSS.MultiThreading;

namespace PSS.Mapping
{
    public interface IMutliThreaded : ITask
    {
        IReadOnlyCollection<Task> Threads { get; }
        void Cancel();
    }
}