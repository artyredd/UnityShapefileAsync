using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace PSS.MultiThreading.DataAccess
{

    public class Consumer<TResult, T, TOutput, TInput> : BaseMultiThreaded, IMultiThreaded, ITask where TOutput : IProducerConsumerCollection<TResult>, new() where TInput : IProducerConsumerCollection<T>, new()
    {
        /// <summary>
        /// Consumes items from <see cref="ConcurrentBag{U}"/> Input, runs <see cref="Func{U, T}}"/> Operation on the item and adds the result to <see cref="ConcurrentBag{T}"/> Output
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T"></typeparam>
        public Consumer()
        {

        }

        /// <summary>
        /// The items that have been consumed and operated on and ready for use
        /// </summary>
        public TOutput Output { get; set; } = new TOutput();

        /// <summary>
        /// The bag that the consumer, consumes items from to create TResult items
        /// </summary>
        public TInput Input { get; set; }

        /// <summary>
        /// The function that you want to run on the input data items to get the result of output items
        /// <para>
        /// Example:
        /// </para>
        /// <code>
        /// (input)=>{return input + 2;}
        /// </code>
        /// </summary>
        public Func<T, TResult> Operation { get; set; }

        /// <summary>
        /// The amount of items that have been consumed
        /// </summary>
        public int ItemsConsumed => _ItemsConsumed;
        private volatile int _ItemsConsumed = 0;

        /// <summary>
        /// Pauses or Unpauses the consumtion of items from Input, setting to true hangs all working threads until Paused = false is set, StopConsuming() or Cancel() is called.
        /// </summary>
        public bool Paused { get; private set; } = false;

        /// <summary>
        /// Whether or not this consumer should be kept alive
        /// </summary>
        public bool KeepAlive { get; set; } = false;

        /// <summary>
        /// Force the consumer to stop consuming and end all tasks.
        /// </summary>
        public void StopConsuming()
        {
            Paused = false;
            Cancel();
        }

        public void Pause()
        {
            SetPause(true);
        }

        public void Resume()
        {
            SetPause(false);
        }

        private void SetPause(bool pause)
        {
            Paused = pause;
            if (pause)
            {
                if (Status != TaskStatus.WaitingToRun)
                {
                    UpdateStatus(TaskStatus.WaitingToRun);
                }
            }
            else
            {
                if (Status != TaskStatus.Running)
                {
                    UpdateStatus(TaskStatus.Running);
                }
            }
        }

        /// <summary>
        /// Start <paramref name="workers"/> worker threads to start consuming items from the input bag and outputing the result to the output bag. By default worker threads end when all items have been consumed
        /// </summary>
        /// <param name="workers">The number of worker threads you want comsuming items, 1 recommeded unless the operation is slow/advanded or the producer/data set is too fast/large.</param>
        public void BeginConsuming(int workers = 1)
        {

            // create the worker threads to start consuming
            for (int i = 0; i < workers; i++)
            {
                Run(() => ConsumerWorker(TokenSource.Token));
            }

            WaitTasks();
        }

        /// <summary>
        /// Calls this.BeginConsuming(int numberOfWorkers) after the <see cref="IMultiThreaded"/> <paramref name="objectToWatch"/> reaches <paramref name="status"/>
        /// </summary>
        /// <param name="objectToWatch"></param>
        /// <param name="status"></param>
        public void DelayConsumeEvent(IMultiThreaded objectToWatch, TaskStatus status, int numberOfWorkers = 1)
        {
            InvokOnStatusChange(objectToWatch, status, () => BeginConsuming(numberOfWorkers));
        }

        /// <summary>
        /// Calls this.BeginConsuming(int numberOfWorkers) after the <see cref="IMultiThreaded"/> <paramref name="objectToWatch"/> reaches <paramref name="status"/>
        /// </summary>
        /// <param name="objectToWatch"></param>
        /// <param name="status"></param>
        public void SetKeepAliveEvent(IMultiThreaded objectToWatch, TaskStatus status, bool alive)
        {
            InvokOnStatusChange(objectToWatch, status, () => KeepAlive = alive);
        }

        /// <summary>
        /// Calls this.BeginConsuming(int numberOfWorkers) after the <see cref="IMultiThreaded"/> <paramref name="objectToWatch"/> reaches <paramref name="status"/>
        /// </summary>
        /// <param name="objectToWatch"></param>
        /// <param name="status"></param>
        public void InvokOnStatusChange(IMultiThreaded objectToWatch, TaskStatus status, Action action)
        {
            objectToWatch.TaskProgress.ProgressChanged +=
                (object caller, (object caller, TaskStatus status) taskStatus) =>
                {
                    if (taskStatus.status == status)
                    {
                        action?.Invoke();
                    }
                };
        }

        /// <summary>
        /// Begins Consuming items from ConsumerBag{U} and adds the result to ConsumerBag{T} Output;
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public void ConsumerWorker(CancellationToken token)
        {
            // if we arent monitoring tasks then just wait for items is fine, if we are in order to stay in the loop we have to 
            // waiting for items and NOT all mintor tasks are finished
            UpdateStatus(TaskStatus.Running);

            while (Input.Count > 0 || KeepAlive)
            {
                token.ThrowIfCancellationRequested();

                // Hang thread when paused = true
                while (Paused)
                {
                    token.ThrowIfCancellationRequested();

                }

                // consume items
                T item;
                if (Input.TryTake(out item))
                {
                    ConsumeItem(item, token);
                }
            }
        }

        /// <summary>
        /// Runs <see cref="Operation"/> on <paramref name="ItemToConsume"/> and adds the resulting <see cref="TResult"/> to <see cref="Output"/>
        /// </summary>
        /// <param name="ItemToConsume">The value that <see cref="Operation"/> is going to be run on</param>
        private void ConsumeItem(T ItemToConsume, CancellationToken token)
        {
            TResult result = Operation(ItemToConsume);
            Interlocked.Increment(ref _ItemsConsumed);
            while (Output.TryAdd(result) == false)
            {
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
