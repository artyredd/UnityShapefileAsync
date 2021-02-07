using PSS.MultiThreading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    /// <summary>
    /// The base class multithreaded classes inherit from in PSS
    /// </summary>
    public abstract class BaseMultiThreaded: IMultiThreaded
    {
        /// <summary>
        /// The worker threads of this object
        /// </summary>
        public IReadOnlyCollection<Task> Threads => _Threads.AsReadOnly();
        protected List<Task> _Threads { get; set; } = new List<Task>();

        /// <summary>
        /// The current status of this object, when instantiated it starts as <see cref="TaskStatus.Created"/>
        /// </summary>
        public TaskStatus Status { get; private set; } = TaskStatus.Created;

        /// <summary>
        /// The number of worker threads this object is currently in charge of
        /// </summary>
        public int RunningThreads => _Threads.GetThreadStatusCount(TaskStatus.Running);

        /// <summary>
        /// The number of worker threads that have encountered an exception
        /// </summary>
        public int FaultedThreads => _Threads.GetThreadStatusCount(TaskStatus.Faulted);

        /// <summary>
        /// The number of worker threads that have encountered an <seealso cref="OperationCanceledException"/> during runtime
        /// </summary>
        public int CancelledThreads => _Threads.GetThreadStatusCount(TaskStatus.Canceled);

        /// <summary>
        /// The number of worker threads that are currently created and waiting to run
        /// </summary>
        public int WaitingThreads => _Threads.GetThreadStatusCount(TaskStatus.WaitingForActivation, TaskStatus.WaitingToRun, TaskStatus.Created);

        /// <summary>
        /// The number of worker threads that have ran to completion
        /// </summary>
        public int FinishedThreads => _Threads.GetThreadStatusCount(TaskStatus.RanToCompletion);

        public IProgress<ITaskProgress> TaskProgress { get; set; }

        private TaskProgressModel _TaskProgress { get; set; } = new TaskProgressModel();

        /// <summary>
        /// The token source this object uses to handle thread cancellation
        /// </summary>
        protected CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();


        /// <summary>
        /// Manually set the <see cref="Status"/> of the object to the given value and raise <see cref="StatusUpdateEvent"/>
        /// </summary>
        /// <param name="status"></param>
        protected void UpdateStatus(TaskStatus status)
        {
            Status = status;
            _TaskProgress.Status = Status;
            TaskProgress?.Report(_TaskProgress);
        }

        public virtual void Cancel()
        {
            TokenSource.Cancel();
        }

        ~BaseMultiThreaded() {
            Cancel();
        }
    }
}
