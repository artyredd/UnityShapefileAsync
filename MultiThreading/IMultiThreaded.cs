using PSS.MultiThreading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    public interface IMultiThreaded : ITask
    {
        int CancelledWorkers { get; }
        int FaultedWorkers { get; }
        int CompletedWorkers { get; }
        int RunningWorkers { get; }
        IReadOnlyCollection<Task> Workers { get; }
        int WaitingWorkers { get; }

        Progress<(object, TaskStatus)> TaskProgress { get; set; }

        void Cancel();
    }
}