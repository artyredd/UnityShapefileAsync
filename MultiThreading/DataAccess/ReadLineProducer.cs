using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PSS.MultiThreading.DataAccess
{

    public class ReadLineProducer<T> : BaseMultiThreaded, IMultiThreaded, ITask where T : IProducerConsumerCollection<string>, new()
    {
        /// <summary>
        /// The path that this reader reads from
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Reads lines from a file and adds them to a <see cref="T"/>
        /// </summary>
        public ReadLineProducer(string path) => Path = path;

        /// <summary>
        /// The total number of items located in the file.
        /// </summary>
        public int TotalItems => SkipFirstLine ? _TotalItems - 1 : _TotalItems;
        private volatile int _TotalItems = 0;

        /// <summary>
        /// The number of items produced so far.
        /// </summary>
        public int ItemsProduced => _OperationsCompleted;
        private volatile int _OperationsCompleted = 0;

        /// <summary>
        /// The output of this producer
        /// </summary>
        public T Output { get; set; } = new T();


        /// <summary>
        /// Should this reader skip the first line of the file?
        /// </summary>
        public bool SkipFirstLine { get; set; } = false;

        /// <summary>
        /// Starts the producer worker on another thread
        /// </summary>
        public void StartWorker()
        {
            _OperationsCompleted = 0;

            Run(() => ReadLineWorker(TokenSource.Token));

            // This is after the worker thread task becuase get file line count takes ~5s to complete for files 3k+ lines
            _TotalItems = PSS.Mapping.Helpers.MapFileHelpers.GetFileLineCount(Path);

            WaitTasks();
        }

        /// <summary>
        /// Actual worker, reads line from file and adds it to the Output
        /// </summary>
        /// <param name="token"></param>
        private void ReadLineWorker(CancellationToken token)
        {
            using (StreamReader reader = File.OpenText(Path))
            {
                UpdateStatus(TaskStatus.Running);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    token.ThrowIfCancellationRequested();
                    if (SkipFirstLine)
                    {
                        SkipFirstLine = false;
                        continue;
                    }
                    ProduceLine(line, token);
                    Interlocked.Increment(ref _OperationsCompleted);
                }
            }
        }

        /// <summary>
        /// Calls this.StartWorker() after the <see cref="IMultiThreaded"/> <paramref name="objectToWatch"/> reaches <paramref name="status"/>
        /// </summary>
        /// <param name="objectToWatch"></param>
        /// <param name="status"></param>
        public void WaitToProduce(IMultiThreaded objectToWatch, TaskStatus status)
        {
            objectToWatch.TaskProgress.ProgressChanged +=
                (object caller, (object caller, TaskStatus status) taskStatus) =>
                {
                    if (taskStatus.status == status)
                    {
                        StartWorker();
                    }
                };
        }

        /// <summary>
        /// Adds the line to the output
        /// </summary>
        /// <param name="line"></param>
        /// <param name="token"></param>
        private void ProduceLine(string line, CancellationToken token)
        {
            while (Output.TryAdd(line) == false)
            {
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
