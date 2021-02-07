using PSS.MultiThreading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    public interface IMultiThreaded : ITask
    {
        int CancelledThreads { get; }
        int FaultedThreads { get; }
        int FinishedThreads { get; }
        int RunningThreads { get; }
        IReadOnlyCollection<Task> Threads { get; }
        int WaitingThreads { get; }

        IProgress<ITaskProgress> TaskProgress { get; set; }

        void Cancel();
    }
}