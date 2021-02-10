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
        public bool Paused { get; set; } = false;

        /// <summary>
        /// Enables the worker threads to continously wait for items even after running out of items to consume. Set this value to true if you have producers that produce data slowly , or producers that dont start right away. This value MUST be set to false before worker threads can finish their work unless Cancel() is called.
        /// </summary>
        public bool WaitForItems { get; set; } = false;

        /// <summary>
        /// When this property is set this consumer will wait for the ITask to be either in a Running state or RanToCompletion state to begin consuming items
        /// </summary>
        public List<ITask> MonitoredITasks { get; set; } = new List<ITask>();

        /// <summary>
        /// When this list contains any Tasks this object will spin-wait for all of them to be in a running state before starting consumer tasks
        /// </summary>
        public List<Task> MonitoredTasks { get; set; } = new List<Task>();


        /// <summary>
        /// Force the consumer to stop consuming and end all tasks.
        /// </summary>
        public void StopConsuming()
        {
            Paused = false;
            Cancel();
        }

        /// <summary>
        /// Start <paramref name="workers"/> worker threads to start consuming items from the input bag and outputing the result to the output bag. By default worker threads end when all items have been consumed
        /// </summary>
        /// <param name="workers">The number of worker threads you want comsuming items, 1 recommeded unless the operation is slow/advanded or the producer/data set is too fast/large.</param>
        public void BeginConsuming(int workers = 1) {
            try
            {
                // should we wait for a task to start producing?
                if (MonitoredITasks.Count > 0)
                {
                    foreach (var item in MonitoredITasks)
                    {
                        Helpers.WaitForStatus(item, TaskStatus.Running, TokenSource.Token, long.MaxValue);
                    }
                }

                if(MonitoredTasks.Count > 0)
                {
                    // This 'spin waits' until the task we are monitoring is running, completed, canceled, or faulted
                    foreach (var item in MonitoredTasks)
                    {
                        Helpers.WaitForStatus(item, TaskStatus.Running, TokenSource.Token, long.MaxValue);
                    }
                }

                // create the worker threads to start consuming
                for (int i = 0; i < workers; i++)
                {
                    _Threads.Add(
                        Task.Run(() => {
                            ConsumerWorker(TokenSource.Token);
                        })
                    );
                }

                // wait for the tasks to finish consuming or fault/cancel 
                Task.WaitAll(Threads.ToArray());

                // this line only gets hit if all the tasks finish without fault/cancel
                UpdateStatus(TaskStatus.RanToCompletion);
            }
            catch (AggregateException e)
            {
                bool unexpectedFault = MultiThreading.Helpers.ContainsUnexpectedExceptions(e, typeof(OperationCanceledException));
                if (unexpectedFault)
                {
                    Factory.LogInnerExceptions(e);
                    UpdateStatus(TaskStatus.Faulted);
                    throw;
                }
                else
                {
                    // Don't throw if we expected a cancellation
                    UpdateStatus(TaskStatus.Canceled);
                }
            }
            finally
            {
                Cancel();
            }
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

            while (Input.Count > 0 || (MonitoredITasks.Count == 0 ? WaitForItems : (WaitForItems && !AllFinished())))
            {
                token.ThrowIfCancellationRequested();

                // Hang thread when paused = true
                while (Paused)
                {
                    token.ThrowIfCancellationRequested();

                    if (Status != TaskStatus.WaitingToRun)
                    {
                        UpdateStatus(TaskStatus.WaitingToRun);
                    }
                }

                if (Status != TaskStatus.Running)
                {
                    UpdateStatus(TaskStatus.Running);
                }

                // consume items
                T item;
                if (Input.TryTake(out item)) 
                {
                    ConsumeItem(item,token);
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
            System.Threading.Interlocked.Increment(ref _ItemsConsumed);
            while (Output.TryAdd(result) == false) 
            {
                token.ThrowIfCancellationRequested();
            }
        }


        private bool AllFinished() {
            bool anyNotFinished = false;
            foreach (var item in MonitoredITasks)
            {
                if (item == null)
                {
                    throw new NullReferenceException("Failed to get status of ITask, make sure the ITask is instanitated before the consumer begins consuming!");
                }
                if (item.Status != TaskStatus.RanToCompletion && item.Status != TaskStatus.Canceled && item.Status != TaskStatus.Faulted)
                {
                    anyNotFinished = true;
                }
            }
            return !anyNotFinished;
        }
    }
}
