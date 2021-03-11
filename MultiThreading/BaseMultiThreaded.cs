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
    public abstract class BaseMultiThreaded : IMultiThreaded
    {
        /// <summary>
        /// The worker threads of this object
        /// </summary>
        public IReadOnlyCollection<Task> Workers => _Workers.AsReadOnly();

        protected List<Task> _Workers { get; set; } = new List<Task>();

        /// <summary>
        /// The current status of this object, when instantiated it starts as <see cref="TaskStatus.Created"/>
        /// </summary>
        public TaskStatus Status { get; private set; } = TaskStatus.Created;

        /// <summary>
        /// The number of worker threads this object is currently in charge of
        /// </summary>
        public int RunningWorkers => _Workers.GetThreadStatusCount(TaskStatus.Running);

        /// <summary>
        /// The number of worker threads that have encountered an exception
        /// </summary>
        public int FaultedWorkers => _Workers.GetThreadStatusCount(TaskStatus.Faulted);

        /// <summary>
        /// The number of worker threads that have encountered an <seealso cref="OperationCanceledException"/> during runtime
        /// </summary>
        public int CancelledWorkers => _Workers.GetThreadStatusCount(TaskStatus.Canceled);

        /// <summary>
        /// The number of worker threads that are currently created and waiting to run
        /// </summary>
        public int WaitingWorkers => _Workers.GetThreadStatusCount(TaskStatus.WaitingForActivation, TaskStatus.WaitingToRun, TaskStatus.Created);

        /// <summary>
        /// The number of worker threads that have ran to completion
        /// </summary>
        public int CompletedWorkers => _Workers.GetThreadStatusCount(TaskStatus.RanToCompletion);

        /// <summary>
        /// The event handler that invokes every time the object's status changes
        /// </summary>
        public Progress<(object, TaskStatus)> TaskProgress { get; set; } = new Progress<(object, TaskStatus)>();

        /// <summary>
        /// The token source this object uses to handle thread cancellation
        /// </summary>
        protected CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();

        /// <summary>
        /// Starts all internal threads
        /// </summary>
        protected void StartAllTasks()
        {
            foreach (var item in _Workers)
            {
                item.Start();
            }
        }

        /// <summary>
        /// Awaits the completion of all threads of this object, automatically handles aggregate exceptions 
        /// </summary>
        /// <returns></returns>
        protected async Task AwaitTasks()
        {
            try
            {
                await Task.WhenAll(Workers.ToArray());
                UpdateStatus(TaskStatus.RanToCompletion);
            }
            catch (AggregateException e)
            {
                bool unexpected = e.ContainsUnexpectedExceptions(typeof(OperationCanceledException));
                if (unexpected)
                {
                    Factory.LogInnerExceptions(e);
                    UpdateStatus(TaskStatus.Faulted);
                    throw e;
                }
                else
                {
                    UpdateStatus(TaskStatus.Canceled);
                }
            }
            finally
            {
                Cancel();
            }
        }

        /// <summary>
        /// Waits for the completion of all threads of this object, automatically handles aggregate exceptions, this is THREAD BLOCKING
        /// </summary>
        protected void WaitTasks()
        {
            try
            {
                Task.WaitAll(Workers.ToArray(), TokenSource.Token);
                UpdateStatus(TaskStatus.RanToCompletion);
            }
            catch (AggregateException e)
            {
                bool unexpected = e.ContainsUnexpectedExceptions(typeof(OperationCanceledException));
                if (unexpected)
                {
                    Factory.LogInnerExceptions(e);
                    UpdateStatus(TaskStatus.Faulted);
                    throw e;
                }
                else
                {
                    UpdateStatus(TaskStatus.Canceled);
                }
            }
            finally
            {
                Cancel();
            }
        }

        /// <summary>
        /// Adds  the given task to the internal threads list
        /// </summary>
        /// <param name="newTask"></param>
        private Task Add(Task newTask)
        {
            _Workers.Add(newTask);
            return newTask;
        }

        /// <summary>
        /// Creates a new task with the given action and adds it to the internal task list
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected Task Add(Action action)
        {
            return Add(new Task(action, TokenSource.Token));
        }

        /// <summary>
        /// Creates a new task, adds it to the internal worker list, and begins executing it.
        /// </summary>
        /// <param name="action"></param>
        protected void Run(Action action)
        {
            Add(Task.Run(action, TokenSource.Token));
        }

        /// <summary>
        /// Manually set the <see cref="Status"/> of the object to the given value and raise <see cref="StatusUpdateEvent"/>
        /// </summary>
        /// <param name="status"></param>
        protected void UpdateStatus(TaskStatus status)
        {
            Status = status;
            ((IProgress<(object, TaskStatus)>)TaskProgress)?.Report((this, status));
        }

        public virtual void Cancel()
        {
            TokenSource.Cancel();
        }

        ~BaseMultiThreaded()
        {
            Cancel();
        }
    }
}
