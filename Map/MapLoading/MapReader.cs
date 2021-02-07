using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PSS.MultiThreading;
using PSS.MultiThreading.DataAccess;

namespace PSS.Mapping
{
    /// <summary>
    /// Loads a map from a file
    /// </summary>
    public class MapReader : BaseMultiThreaded, IMapReader, IMultiThreaded
    {
        public MapReader(string Path)
        {
            this.Path = Path;
        }

        /// <summary>
        /// The path of this map reader
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The number of lines that this reader has already converted
        /// </summary>
        public int LinesConverted => Consumer.ItemsConsumed;

        /// <summary>
        /// Whether or not this reader is currently reading from a file
        /// </summary>
        public bool ReadingFile { get; set; }

        /// <summary>
        /// The number of lines that this reader has read so far
        /// </summary>
        public int LinesRead => Producer.ItemsProduced;

        /// <summary>
        /// The map records that have been deserialized and that are ready for use. Thread safe.
        /// </summary>
        public ConcurrentBag<IMapRecord> MapRecords { get; private set; } = new ConcurrentBag<IMapRecord>();

        private ConcurrentBag<string> SerializedRecords { get; set; } = new ConcurrentBag<string>();

        private Consumer<IMapRecord,string,ConcurrentBag<IMapRecord>, ConcurrentBag<string>> Consumer { get; set; } = new Consumer<IMapRecord, string, ConcurrentBag<IMapRecord>, ConcurrentBag<string>>();

        private ReadLineProducer<ConcurrentBag<string>> Producer { get; set; }

        public override void Cancel()
        {
            base.Cancel();
            Consumer?.Cancel();
            Producer?.Cancel();
        }

        /// <summary>
        /// Begins reading map data with 1 deserializerThread
        /// </summary>
        /// <param name="maxThreads"></param>
        /// <returns></returns>
        public void ReadMapMultiThreaded(int maxThreads = 2)
        {
            Stopwatch watch = Stopwatch.StartNew();

            // keep track of the reader task
            _Threads.Add(StartProducer(maxThreads));

            try
            {
                Task.WaitAll(Threads.ToArray(),TokenSource.Token);
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
            finally {
                Cancel();
            }

            watch.Stop();

            // cancel all threads incase we have a hanging thread somewhere
            Cancel();

            //Factory.Log($"Finished Loading Map File {LinesRead}/{LinesConverted} Lines Time={watch.ElapsedMilliseconds} Status: {Status}");
        }

        private Task StartProducer(int maxThreads)
        {
            //BEgin reading from the file
            Task ReaderTask = Task.Run(() => {
                
                // create a producer
                Producer = new ReadLineProducer<ConcurrentBag<string>>(Path);

                // route the output of the producer to this object
                Producer.Output = SerializedRecords;

                // keep track of the deserializer task
                _Threads.Add(StartConsumer(maxThreads));

                // notify self and others that we are now reading lines
                UpdateStatus(TaskStatus.Running);

                // start reading lines
                Producer.StartWorker();

                // after we're done reading lines tell the consumer that its okay to end its threads when its done consuming
                Consumer.WaitForItems = false;
            });
            return ReaderTask;
        }

        private Task StartConsumer(int maxThreads) 
        {
            Task deserializerTask = Task.Run(() =>
            {
                // give the consumer something to consume from
                Consumer.Input = SerializedRecords;

                // route the output to this object
                Consumer.Output = MapRecords;

                // tell the consumer what to do with the items it consumes
                Consumer.Operation = ConsumeLinesOperation;

                // tell the consumer to wait for the producer to start before creating new threads that will just hang without items to consume
                Consumer.MonitoredITasks.Add(Producer);

                // tell the consumer to wait for items to consume and not exit its threads until told no more items are going to be sent
                Consumer.WaitForItems = true;

                // tell the consumer to begin consuming
                Consumer.BeginConsuming(maxThreads - 1);
            });
            return deserializerTask;
        }

        private IMapRecord ConsumeLinesOperation(string line) 
        {
            IMapRecord deserializedRecord = MapRecordDeserializer.DeserializeMapRecord(line);
            return deserializedRecord;
        }
    }
}
