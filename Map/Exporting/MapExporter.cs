using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using PSS.MultiThreading;

namespace PSS.Mapping
{
    /// <summary>
    /// Handles the Exporting of newly imported Shapefile and CSV information into PSS_MAP format(AKA plaintext JSON becuase im lazy)
    /// </summary>
    public class MapExporter : BaseMultiThreaded, IMutliThreaded
    {

        public MapExporter(IMapPathInfo pathInfo)
        {
            PathInfo = pathInfo;
            // Make sure all the paths work
            bool failed = !Helpers.MapPathInfoHelpers.VerifyPaths(pathInfo);

            // we only want to spend the operations on setting up if the paths where valid.
            if (failed == true)
            {
                throw new Exception("Paths failed validation");
            }
        }

        /// <summary>
        /// The path information for this exporter
        /// </summary>
        public IMapPathInfo PathInfo { get; private set; }

        /// <summary>
        /// The full export path for the file
        /// </summary>
        private string ExportPath => PathInfo.OutputDirectory + PathInfo.FileName + ".PSS_MAP";

        /// <summary>
        /// The number of records that have been serialized so far
        /// </summary>
        public int RecordsSerialized => _RecordsSerialized;
        private int _RecordsSerialized;
        

        /// <summary>
        /// Whether or not the exporter is currently in the process of serializing data
        /// </summary>
        public bool SerializingRecords { get; private set; } = false;
        /// <summary>
        /// The number of records written to the file so far
        /// </summary>
        public int RecordsWritten => _RecordsWritten;
        private int _RecordsWritten;
        /// <summary>
        /// Whether or not the exporter is currently writing to the file
        /// </summary>
        public bool WritingRecords { get; set; } = false;

        private ConcurrentBag<string> SerializedRecords { get; set; } = new ConcurrentBag<string>();

        /// <summary>
        /// Exports the given records to the export path provided in the IMapPathInfo on construction
        /// </summary>
        /// <param name="RecordQueue"></param>
        /// <returns></returns>
        public void ExportMapRecords(ConcurrentQueue<IMapRecord> RecordQueue) {
            ///           MAIN THREAD 
            ///               ↓
            ///    MapExporter Worker (this)
            ///    ↓                       ↓
            ///    Serialize             Writer

            _Threads.Add(Task.Run(() => {
                SerializeAndBagRecords(RecordQueue,TokenSource.Token);
            }));

            _Threads.Add(Task.Run(() =>
            {
                ConsumeAndWriteRecordsFromBag(SerializedRecords,TokenSource.Token);
            }));

            try
            {
                Task.WaitAll(Threads.ToArray());
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
            finally {
                Cancel();
                WritingRecords = false;
                SerializingRecords = false;
            }

        }
        private void SerializeAndBagRecords(ConcurrentQueue<IMapRecord> RecordQueue, CancellationToken token) {
            SerializingRecords = true;
            while (!RecordQueue.IsEmpty)
            {
                token.ThrowIfCancellationRequested();
                IMapRecord record;
                if (RecordQueue.TryDequeue(out record))
                {
                    SerializedRecords.Add(record.Serialize());
                    System.Threading.Interlocked.Increment(ref _RecordsSerialized);
                }
            }
            SerializingRecords = false;
        }

        private void ConsumeAndWriteRecordsFromBag(ConcurrentBag<string> bag, CancellationToken token) {
            if (File.Exists(ExportPath))
            {
                File.Delete(ExportPath);
            }
            WritingRecords = true;
            using (StreamWriter writer = File.CreateText(ExportPath))
            {
                while (!bag.IsEmpty | SerializingRecords)
                {
                    token.ThrowIfCancellationRequested();
                    string record;
                    if (bag.TryTake(out record))
                    {
                        writer.WriteLine(record);
                        System.Threading.Interlocked.Increment(ref _RecordsWritten);
                    }
                }
            }
            WritingRecords = false;
        }
    }
}
