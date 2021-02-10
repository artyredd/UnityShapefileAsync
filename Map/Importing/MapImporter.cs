using Assets.PipelineGenerator.Scripts.ESRI.ShapeImporter25D;
using PSS.Mapping.CSV;
using PSS.MultiThreading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace PSS.Mapping
{
    /// <summary>
    /// Defines an object that helps import SHP files
    /// 
    /// psuedo explanation:
    /// Verify paths are correct before we do anything
    /// import the csv file that contains information about the shapefile
    /// 
    /// </summary>
    public class MapImporter : BaseMultiThreaded, IMutliThreaded
    {
        /// <summary>
        /// Contains the path information for the source and output files
        /// </summary>
        public IMapPathInfo PathInfo { get; private set; }

        /// <summary>
        /// The CSV reader of this importer
        /// </summary>
        public ICSVReader CSVReader { get; set; }

        /// <summary>
        /// The Shape file reader of this importer
        /// </summary>
        public IShapeFileReader SReader { get; set; }

        public MapImporter(IMapPathInfo mapFileDetails) {
            PathInfo = mapFileDetails;
            // Make sure all the paths work
            bool failedValidation = !Helpers.MapPathInfoHelpers.VerifyPaths(mapFileDetails);
            if (failedValidation)
            {
                throw new UnauthorizedAccessException();
            }
        }

        /// <summary>
        /// The queue of records in this importer
        /// </summary>
        public ConcurrentQueue<IMapRecord> Records { get; private set; } = new ConcurrentQueue<IMapRecord>();

        /// <summary>
        /// The number of records imported both from the Shapefile and CSV file
        /// </summary>
        public int RecordsImported { get; private set; } = 0;

        /// <summary>
        /// The total number of records that are being imported.
        /// </summary>
        public int TotalRecords { get; private set; } = 0;

        /// <summary>
        /// The percent dont this importer is in importing a shapefile and CSV data
        /// </summary>
        public int PercentDone => MathHelpers.Percentages.PercentageComplete(RecordsImported,TotalRecords);

        /// <summary>
        /// Whether or not the CSVReader is finished reading its data
        /// </summary>
        public bool CSVReaderFinished => CSVReader?.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// Whether or not the CSVReader is finished reading its data
        /// </summary>
        /// [System.Obsolete("Use ShapeFile.Status == TaskStatus.RanToCompletion instead")]
        public bool ShapeFileReaderFinished => SReader?.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// Cancel all threads and tasks that this importer controls.
        /// </summary>
        public override void Cancel() {
            TokenSource.Cancel();
            CSVReader.Cancel();
        }

        /// <summary>
        /// IRecordPositions tells the importer which CSV columns hold important information about the map like the entity name, its ID and its parent ID, Example: Florida(nameIndex), State code(SecondaryID), Country Code(PrimaryID)
        /// </summary>
        public void BeginReadingAsync(IRecordPositions positions) {
            Progress<MapImporterProgressModel> progress = MultiThreading.Helpers.CreateProgress<MapImporterProgressModel>(ImporterProgressEvent);

            CSVReader = Factory.CreateCSVReader(PathInfo.CSVDirectory + PathInfo.FileName + ".csv");
            SReader = Factory.CreateShapeFileReader(PathInfo.ShapeFileDirectory + PathInfo.FileName + ".shp");

            _Threads.Add(StartShapeFileWorker(TokenSource.Token));
            _Threads.Add(StartCSVWorker(positions));

            _Threads.Add(Task.Run(() => {
                UpdateStatus(TaskStatus.Running);
                StartQueueConsumer(positions, progress, TokenSource.Token);
            }));

            try {
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

        private void ImporterProgressEvent(object caller, MapImporterProgressModel status) {
            RecordsImported = status.RecordsCompleted;
            TotalRecords = status.TotalRecords;
        }

        private void StartQueueConsumer(IRecordPositions positions, IProgress<MapImporterProgressModel> progress, CancellationToken token) {
            // the progress report for this async task
            MapImporterProgressModel progressReport = new MapImporterProgressModel();

            // make sure the other tasks have started before continuing
            MultiThreading.Helpers.WaitForStatus(CSVReader, TaskStatus.Running, token);
            MultiThreading.Helpers.WaitForStatus(SReader, TaskStatus.Running, token);

            while (token.IsCancellationRequested == false)
            {
                progressReport.TotalRecords = CSVReader.TotalLines;

                Tuple<IMapData, string[]> tuple = WaitForMultipleQueues(SReader.Records, CSVReader.RowQueue, token);

                IMapData data = tuple.Item1;

                string[] row = tuple.Item2;

                if (EnqueueRecord(data, row, positions))
                {
                    progressReport.RecordsCompleted++;
                    progress.Report(progressReport);
                }
                else
                {
                    /// Only Break if
                    /// → Nothing in queue
                    /// → Nothing Being Read
                    /// → Number of data read is the number of data imported
                    bool nothingInQueue = SReader.Records.IsEmpty & CSVReader.RowQueue.IsEmpty;
                    bool misMatch = SReader.Records.Count == 0 && CSVReader.RowQueue.Count != 0;
                    bool misMatch2 = SReader.Records.Count != 0 && CSVReader.RowQueue.Count == 0;
                    bool nothingBeingRead = ShapeFileReaderFinished & CSVReaderFinished;
                    bool importedAllData = RecordsImported >= TotalRecords - 1;
                    if ((nothingInQueue || ((misMatch || misMatch2) && nothingBeingRead)) & nothingBeingRead & !importedAllData)
                    {
                        Factory.Log($"Tried to exit but {TotalRecords - RecordsImported} records left to import. SHP Queue: {SReader.Records.Count()} CSV Queue: {CSVReader.RowQueue.Count()}");
                    }
                    if ((nothingInQueue || ((misMatch || misMatch2) && nothingBeingRead)) & nothingBeingRead & importedAllData)
                    {
                        //Factory.Log($"No more rows or data to enqueue.{RecordsImported}");
                        break;
                    }
                }
            }
        }
        private Tuple<T, U> WaitForMultipleQueues<T, U>(ConcurrentQueue<T> queue, ConcurrentQueue<U> queue2, CancellationToken token, long timeout = 10) where T : class where U : class {
            Stopwatch watch = Stopwatch.StartNew();
            T tmp1;
            U tmp2;
            // see if we can yoink some values, if we cant we should wait until timeout
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    // if there is values to yoink try to yoink them
                    if (queue.TryPeek(out tmp1) & queue2.TryPeek(out tmp2)) {

                        while (queue.TryDequeue(out tmp1) == false) {
                            // wait to yoink first value
                            token.ThrowIfCancellationRequested();
                        }

                        while (queue2.TryDequeue(out tmp2) == false) {
                            // wait to yoink second value
                            token.ThrowIfCancellationRequested();
                        }

                        // return tuple
                        return new Tuple<T, U>(tmp1, tmp2);
                    }
                    if (watch.ElapsedMilliseconds > timeout)
                    {
                        throw new OperationCanceledException();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return new Tuple<T, U>(null, null);
            }
            catch (Exception) {
                throw;
            }
        }


        private bool EnqueueRecord(IMapData data, string[] row, IRecordPositions positions) {
            if (data != null & row != null)
            {
                Records.Enqueue(Factory.CreateMapRecord(data, row, positions));
                return true;
            }
            return false;
        }

        private Task StartCSVWorker(IRecordPositions positions) {
            return Task.Run(()=> {
                CSVReader.ReadCSV(positions.PrimaryIDIndex,positions.SecondaryIDIndex,positions.NameIndex);
            });
        }

        private Task StartShapeFileWorker(CancellationToken token) {
            return Task.Run(() => {
                SReader.BeginReadingAsync(token);
            });
        }

        ~MapImporter() {
            Cancel();
        }
    }
}
