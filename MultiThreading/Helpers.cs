using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.MultiThreading
{
    /// <summary>
    /// Helper methods commonly used for multithreading
    /// </summary>
    public static partial class Helpers
    {
        /// <summary>
        /// Creates <seealso cref="Progress{T}"/>, attaches the provided Event(caller,T) <seealso cref="Action{object, T}"/>, and returns the <seealso cref="Progress{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ProgressMode"></param>
        /// <param name="UpdaterEvent"></param>
        /// <returns></returns>
        public static Progress<T> CreateProgress<T>(Action<object, T> UpdaterEvent) where T : class, new()
        {
            Progress<T> newProgress = new Progress<T>();
            newProgress.ProgressChanged += (object a, T b) => UpdaterEvent(a,b);
            return newProgress;
        }

        /// <summary>
        /// Returns the number of threads in <see cref="IEnumerable{Task}"/> that are <see cref="TaskStatus"/> <paramref name="status"/>
        /// </summary>
        /// <param name="Tasks"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetThreadStatusCount(this IEnumerable<Task> Tasks, TaskStatus status) {
            return Tasks.Where((x) => x.Status == status).Count();
        }
        /// <summary>
        /// Returns the number of threads in <see cref="IEnumerable{Task}"/> that are <see cref="TaskStatus"/> <paramref name="status"/>
        /// </summary>
        /// <param name="Tasks"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetThreadStatusCount(this IEnumerable<Task> Tasks, TaskStatus S1, TaskStatus S2)
        {
            return Tasks.Where((x) => x.Status == S1 || x.Status == S2).Count();
        }
        /// <summary>
        /// Returns the number of threads in <see cref="IEnumerable{Task}"/> that are <see cref="TaskStatus"/> <paramref name="status"/>
        /// </summary>
        /// <param name="Tasks"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetThreadStatusCount(this IEnumerable<Task> Tasks, TaskStatus S1, TaskStatus S2, TaskStatus S3)
        {
            return Tasks.Where((x) => x.Status == S1 || x.Status == S2 || x.Status == S3).Count();
        }
        /// <summary>
        /// Returns the number of threads in <see cref="IEnumerable{Task}"/> that are <see cref="TaskStatus"/> <paramref name="status"/>
        /// </summary>
        /// <param name="Tasks"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int GetThreadStatusCount(this IEnumerable<Task> Tasks, TaskStatus S1, TaskStatus S2, TaskStatus S3, TaskStatus S4)
        {
            return Tasks.Where((x) => x.Status == S1 || x.Status == S2 || x.Status == S3 || x.Status == S4).Count();
        }

        /// <summary>
        /// <para>
        ///     Waits for the provided <see cref="ITask"/> to change its <see cref="TaskStatus"/> to <paramref name="status"/>.
        /// </para>
        /// </summary>
        /// <param name="task">Any <see cref="ITask"/> object, can be used on <see cref="Task"/> by casting <see cref="Task"/> to <see cref="ITask"/> explcitily, see example.</param>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <param name="MaxTimeout"></param>
        /// <returns><see cref="true"/> when <see cref="TaskStatus"/> <paramref name="status"/> is reached</returns>
        /// <exception cref="OperationCanceledException"> Thrown when task is in the TaskStatus.Canceled state, but your weren't waiting for TaskStatus.Canceled</exception>
        /// <exception cref="TimeoutException">Thrown when MaxTimeout has been exceeded by waiting.</exception>
        /// <exception cref="Exception">Thrown when task has faulted, but you weren't waiting for TaskStatus.Exception</exception>
        public static bool WaitForStatus(ITask task, TaskStatus status, CancellationToken token, long MaxTimeout = 2000)
        {
            Stopwatch watch = Stopwatch.StartNew();

            if (task == null)
            {
                throw new NullReferenceException("Failed to wait for Task/ITask null object, make sure the task is instantiated before waiting for it.");
            }

            while (task.Status != status & task.Status != TaskStatus.RanToCompletion)
            {
                token.ThrowIfCancellationRequested();

                if (status != TaskStatus.Faulted && task.Status == TaskStatus.Faulted)
                {
                    throw new Exception("Monitored ITask faulted.");
                }

                if (status != TaskStatus.Canceled && task.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException("Monitored ITask was canceled.");
                }

                if (watch.ElapsedMilliseconds > MaxTimeout)
                {
                    throw new TimeoutException($"Monitored ITask's Status never Reached {status} before the timeout of {MaxTimeout}ms was reached.");
                }
            }
            return true;
        }

        /// <summary>
        /// <para>
        ///     Waits for the provided <see cref="ITask"/> to change its <see cref="TaskStatus"/> to <paramref name="status"/>.
        /// </summary>
        /// <param name="task">Any <see cref="ITask"/> object, can be used on <see cref="Task"/> by casting <see cref="Task"/> to <see cref="ITask"/> explcitily, see example.</param>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <param name="MaxTimeout"></param>
        /// <returns><see cref="true"/> when <see cref="TaskStatus"/> <paramref name="status"/> is reached</returns>
        /// <exception cref="OperationCanceledException"> Thrown when task is in the TaskStatus.Canceled state, but your weren't waiting for TaskStatus.Canceled</exception>
        /// <exception cref="TimeoutException">Thrown when MaxTimeout has been exceeded by waiting.</exception>
        /// <exception cref="Exception">Thrown when task has faulted, but you weren't waiting for TaskStatus.Exception</exception>
        public static bool WaitForStatus(Task task, TaskStatus status, CancellationToken token, long MaxTimeout = 2000)
        {
            Stopwatch watch = Stopwatch.StartNew();

            if (task == null)
            {
                throw new NullReferenceException("Failed to wait for Task/ITask null object, make sure the task is instantiated before waiting for it.");
            }

            while (task.Status != status & task.Status != TaskStatus.RanToCompletion)
            {
                token.ThrowIfCancellationRequested();

                if (status != TaskStatus.Faulted && task.Status == TaskStatus.Faulted)
                {
                    throw new Exception("Monitored ITask faulted.");
                }

                if (status != TaskStatus.Canceled && task.Status == TaskStatus.Canceled)
                {
                    throw new OperationCanceledException("Monitored ITask was canceled.");
                }

                if (watch.ElapsedMilliseconds > MaxTimeout)
                {
                    watch.Stop();

                    throw new TimeoutException($"Monitored ITask's Status never Reached {status} before the timeout of {MaxTimeout}ms was reached.");
                }
            }
            return true;
        }

        /// <summary>
        /// Spin waits for T task to become instantiated before continuing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <param name="MaxTimeout"></param>
        /// <returns></returns>
        public static bool WaitForInstantiation<T>(T task, CancellationToken token, long MaxTimeout = 2000) where T : class
        {
            Stopwatch watch = Stopwatch.StartNew();

            while (task == null)
            {
                token.ThrowIfCancellationRequested();

                if (watch.ElapsedMilliseconds > MaxTimeout)
                {
                    watch.Stop();

                    throw new TimeoutException($"Monitored ITask's never instantitaed before the timeout of {MaxTimeout}ms was reached.");
                }
            }
            return true;
        }

        /// <summary>
        /// Checks to see if any exceptions in the Aggregate Exception ARE the given exception
        /// <para>
        /// Usage:
        /// <code>
        ///     bool hasException = aggregateException.ContainsException(typeof(OperationCanceledException));
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="expectedExceptions"></param>
        /// <returns></returns>
        public static bool ContainsException<T>(this AggregateException e, params T[] expectedExceptions) where T : Type
        {
            foreach (var ex in e.InnerExceptions)
            {
                Type exType = ex.GetType();
                foreach (var expectedException in expectedExceptions)
                {
                    if (exType.Equals(expectedException))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if any exceptions in the Aggregate Exception are NOT the given exceptions
        /// <para>
        /// Usage:
        /// <code>
        ///     bool unexpectedException = aggregateException.ContainsUnexpectedExceptions(typeof(OperationCanceledException));
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="expectedExceptions"></param>
        /// <returns></returns>
        public static bool ContainsUnexpectedExceptions<T>(this AggregateException e, params T[] expectedExceptions) where T : Type
        {
            foreach (var ex in e.InnerExceptions)
            {
                Type exType = ex.GetType();
                foreach (var expectedException in expectedExceptions)
                {
                    if (exType.Equals(expectedException) == false) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
