using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using PSS.Mapping.CSV;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections;
using PSS.MultiThreading;
using PSS.MultiThreading.DataAccess;

namespace PSS.Mapping
{
    /// <summary>
    /// Reads from a CSV
    /// </summary>
    public class CSVReader : BaseMultiThreaded, ICSVReader, IMultiThreaded
    {
        /// <summary>
        /// The path that this CSV reader reads from.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// How many lines have been read so far by the reader;
        /// </summary>
        public int LinesRead => Producer != null ? Producer.ItemsProduced : 0;

        public int TotalLines => Producer != null ? Producer.TotalItems : 0;

        public int LinesConverted => Consumer != null ? Consumer.ItemsConsumed : 0;

        public bool Reading { get; private set; } = false;

        public ConcurrentQueue<string[]> RowQueue { get; set; } = new ConcurrentQueue<string[]>();

        private int NumberOfColumnsToKeep => ColumnsToKeep.Length;
        private int[] ColumnsToKeep { get; set; }

        private ConcurrentQueue<string> LineQueue { get; set; } = new ConcurrentQueue<string>();

        ReadLineProducer<ConcurrentQueue<string>> Producer { get; set; }

        Consumer<string[], string, ConcurrentQueue<string[]>, ConcurrentQueue<string>> Consumer { get; set; }

        public CSVReader(string Path)
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new NullReferenceException("Path");
            }
            this.Path = Path;
        }

        /// <summary>
        /// Reads the CSV file to the concurrent queue
        /// </summary>
        /// <returns></returns>
        public void ReadCSV(params int[] columnsToKeep)
        {
            Reading = true;

            ColumnsToKeep = columnsToKeep;

            StartReaderWorker();

            try
            {
                Task.WaitAll(Workers.ToArray());
                UpdateStatus(TaskStatus.RanToCompletion);
            }
            catch (AggregateException e)
            {
                bool unexpected = e.ContainsUnexpectedExceptions(typeof(OperationCanceledException));
                if (unexpected)
                {
                    Factory.LogInnerExceptions(e);
                    UpdateStatus(TaskStatus.Faulted);
                    throw;
                }
                else
                {
                    UpdateStatus(TaskStatus.Canceled);
                }
            }

            Reading = false;
        }

        private void StartReaderWorker()
        {
            Run(() =>
            {
                // setup a new producer
                Producer = new ReadLineProducer<ConcurrentQueue<string>>(Path);

                UpdateStatus(TaskStatus.Running);

                // route the producers output to this object
                Producer.Output = LineQueue;

                // tell the producer that we dont need the first line, its a garbo header
                Producer.SkipFirstLine = true;

                StartConsumerWorker();

                // start producing lines
                Producer.StartWorker();

                // tell the consumer that its okay to stop its threads when its done consuming
                Consumer.KeepAlive = false;
            });
        }

        private string[] ConsumerOperation(string line)
        {
            string[] split = line.Split(',');
            string[] result = new string[NumberOfColumnsToKeep];
            for (int i = 0; i < NumberOfColumnsToKeep; i++)
            {
                result[i] = split[ColumnsToKeep[i]];
            }
            return result;
        }

        private void StartConsumerWorker()
        {
            Run(() =>
            {
                UpdateStatus(TaskStatus.Running);

                Consumer = new Consumer<string[], string, ConcurrentQueue<string[]>, ConcurrentQueue<string>>();

                // tell the consumer what to consume
                Consumer.Input = LineQueue;

                // route the output of the consumer to this object
                Consumer.Output = RowQueue;

                // tell the consumer what to do with the items
                Consumer.Operation = ConsumerOperation;

                // tell the consumer to keep trying to consume until the producer says to stop
                Consumer.KeepAlive = true;

                // begin consuming
                Consumer.DelayConsumeEvent(Producer, TaskStatus.Running, 1);
                //Consumer.BeginConsuming(1);
            });
        }

        public override void Cancel()
        {
            base.Cancel();
            Consumer.Cancel();
            Producer.Cancel();
        }

        ~CSVReader()
        {
            Cancel();
            Reading = false;
        }
    }
}
