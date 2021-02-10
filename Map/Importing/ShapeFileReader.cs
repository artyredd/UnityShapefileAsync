using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using static PSS.Debugging;
using System.Diagnostics;
using PSS.MultiThreading;
using System.Linq;

namespace PSS.Mapping
{
    /// <summary>
    /// Enumeration defining the various shape types. Each shapefile
    /// contains only one type of shape (e.g., all polygons or all
    /// polylines).
    /// </summary>

    public enum ShapeType
    {
        /// <summary>
        /// Nullshape / placeholder record.
        /// </summary>
        NullShape = 0,

        /// <summary>
        /// Point record, for defining point locations such as a city.
        /// </summary>
        Point = 1,

        /// <summary>
        /// One or more sets of connected points. Used to represent roads,
        /// hydrography, etc.
        /// </summary>
        PolyLine = 3,

        /// <summary>
        /// One or more sets of closed figures. Used to represent political
        /// boundaries for countries, lakes, etc.
        /// </summary>
        Polygon = 5,

        /// <summary>
        /// A cluster of points represented by a single shape record.
        /// </summary>
        Multipoint = 8

        // Unsupported types:
        // PointZ = 11,
        // PolyLineZ = 13,
        // PolygonZ = 15,
        // MultiPointZ = 18,
        // PointM = 21,
        // PolyLineM = 23,
        // PolygonM = 25,
        // MultiPointM = 28,
        // MultiPatch = 31
    }

    public class ShapeFileReader : BaseMultiThreaded, IShapeFileReader
    {
        private readonly static byte[] intBytes = new byte[4];
        private readonly static byte[] doubleBytes = new byte[8];

        private string Path { get; set; }

        /// <summary>
        /// The number of bytes read so far by the stream
        /// </summary>
        public long BytesRead { get; set; }

        /// <summary>
        /// the file size of the shp file in bytes
        /// </summary>
        public long FileSize => _FileSizeInBits * 8;
        private long _FileSizeInBits { get; set; }

        /// <summary>
        /// The number of shape records that have been generated so far
        /// </summary>
        public int RecordsRead { get; private set; } = 0;

        /// <summary>
        /// The records read from the shape file
        /// </summary>
        public ConcurrentQueue<IMapData> Records { get; set; } = new ConcurrentQueue<IMapData>();

        /// <summary>
        /// The file stream reading the bytes from the shapefile
        /// </summary>
        public FileStream Stream { get; private set; }

        /// <summary>
        /// Begins the async reading of the shape file
        /// </summary>
        /// <param name="path"></param>
        public void BeginReadingAsync(CancellationToken token = default)
        {
            token = token != default ? token : TokenSource.Token;

            if (string.IsNullOrEmpty(Path))
            {
                throw new ArgumentNullException("fileName");
            }

            // Get the file size of the .shp file we are reading, in case of large files we want to be able to see how far we are in reading the file
            _FileSizeInBits = Helpers.MapFileHelpers.GetFileSize(Path);

            // reset map data if there is any
            Records = new ConcurrentQueue<IMapData>();

            _Threads.Add(Task.Run(() => StartWorkerThread(token)));

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
            }
            finally
            {
                Cancel();
            }
            Cancel();
        }

        private void StartWorkerThread(CancellationToken token)
        {
            var Progress = MultiThreading.Helpers.CreateProgress<ShapeFileProgressModel>(ReaderProgressEvent);
            using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                Stream = stream;
                UpdateStatus(TaskStatus.Running);
                this.ReadShapes(stream, Progress, token);
            }
        }

        private void ReaderProgressEvent(object caller, ShapeFileProgressModel status)
        {
            RecordsRead = status.RecordsRead;
            BytesRead = status.BytesRead;
        }

        private void ReadShapes(Stream stream, IProgress<ShapeFileProgressModel> progress, CancellationToken token)
        {

            ShapeFileProgressModel progressReport = new ShapeFileProgressModel();

            // Read the File Header.
            this.ReadShapeFileHeader(stream, ref progressReport);

            Stopwatch timeoutTimer = Stopwatch.StartNew();
            long timeout = 1000;
            int i = 0;
            ///the number of entries that can fail to read before throwing a hard exception
            int maxNullEntries = 1;
            int nullEntries = 0;
            while (true)
            {
                if (timeoutTimer.ElapsedMilliseconds >= timeout)
                {
                    throw new TimeoutException($"Failed to read shape file, no records read in last {timeout}ms");
                }
                token.ThrowIfCancellationRequested();
                IMapData data;
                try
                {
                    data = ReadShapeFileRecord(stream, ref progressReport);
                }
                catch (IOException)
                {
                    // IOException gets thrown when EOF occurs
                    break;
                }
                if (data != null)
                {
                    Records.Enqueue(data);
                    progressReport.RecordsRead = i++;
                    progress.Report(progressReport);
                    timeoutTimer.Restart();
                }
                else
                {
                    if (nullEntries++ >= maxNullEntries)
                    {
                        throw new FileLoadException("Shapefile corrupted/and or contains information that cannot be converted to IMapData records.");
                    }
                }
            }
        }

        /// <summary>
        /// Reads the file header but immediately discards information found information found is not used but moves the stream byte pos and we dont want to mess with the magic i dont want to re-write
        /// </summary>
        /// <param name="stream"></param>
        private void ReadShapeFileHeader(Stream stream, ref ShapeFileProgressModel progress)
        {
            // File Code.
            _ = ShapeFileReader.ReadInt32_BE(stream, ref progress);
            // 5 unused values.
            ShapeFileReader.ReadInt32_BE(stream, ref progress);
            ShapeFileReader.ReadInt32_BE(stream, ref progress);
            ShapeFileReader.ReadInt32_BE(stream, ref progress);
            ShapeFileReader.ReadInt32_BE(stream, ref progress);
            ShapeFileReader.ReadInt32_BE(stream, ref progress);

            // File Length.
            _ = ShapeFileReader.ReadInt32_BE(stream, ref progress);

            // Version.
            _ = ShapeFileReader.ReadInt32_LE(stream, ref progress);

            // Shape Type.
            _ = ShapeFileReader.ReadInt32_LE(stream, ref progress);

            // Bounding Box.
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);

            // Skip the rest of the file header.
            stream.Seek(100, SeekOrigin.Begin);
            progress.BytesRead += 100;
        }

        /// <summary>
        /// Read a shapefile record.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        private IMapData ReadShapeFileRecord(Stream stream, ref ShapeFileProgressModel progress)
        {
            int recordNumber;
            List<Vector3> points = new List<Vector3>();
            //  MainWindow mw = new MainWindow();
            // Record Header.

            recordNumber = ShapeFileReader.ReadInt32_BE(stream, ref progress);
            _ = ShapeFileReader.ReadInt32_BE(stream, ref progress);


            // Shape Type.
            var shapeType = ShapeFileReader.ReadInt32_LE(stream, ref progress);


            // Read the shape geometry, depending on its type.
            switch (shapeType)
            {
                case (int)ShapeType.NullShape:
                    // Do nothing.
                    break;
                case (int)ShapeType.Point:
                    points.Add(ShapeFileReader.ReadPoint(stream, ref progress));
                    break;
                case (int)ShapeType.PolyLine:
                    // PolyLine has exact same structure as Polygon in shapefile.
                    List<Vector3> a = ShapeFileReader.ReadPolygon(stream, ref progress);
                    foreach (var item in a)
                    {
                        points.Add(item);
                    }
                    break;
                case (int)ShapeType.Polygon:
                    List<Vector3> b = ShapeFileReader.ReadPolygon(stream, ref progress);
                    foreach (var item in b)
                    {
                        points.Add(item);
                    }
                    break;
                case (int)ShapeType.Multipoint:
                    List<Vector3> c = ShapeFileReader.ReadMultipoint(stream, ref progress);
                    foreach (var item in c)
                    {
                        points.Add(item);
                    }
                    break;
                default:
                    {
                        string msg = String.Format(System.Globalization.CultureInfo.InvariantCulture, "ShapeType {0} is not supported.", shapeType);
                        throw new ArgumentException(msg);
                    }
            }
            return Factory.CreateMapDataModel(recordNumber, points);
        }

        private static int ReadInt32_LE(Stream stream, ref ShapeFileProgressModel progress)
        {
            for (int i = 0; i < 4; i++)
            {
                int b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }

                intBytes[i] = (byte)b;
            }
            progress.BytesRead += 32;
            return BitConverter.ToInt32(intBytes, 0);
        }

        private static int ReadInt32_BE(Stream stream, ref ShapeFileProgressModel progress)
        {
            for (int i = 3; i >= 0; i--)
            {

                int b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }

                intBytes[i] = (byte)b;

            }
            progress.BytesRead += 32;

            return BitConverter.ToInt32(intBytes, 0);
        }

        private static double ReadDouble64_LE(Stream stream, ref ShapeFileProgressModel progress)
        {
            for (int i = 0; i < 8; i++)
            {
                int b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }

                doubleBytes[i] = (byte)b;
            }
            progress.BytesRead += 64;
            return BitConverter.ToDouble(doubleBytes, 0);
        }
        public DataSet dataSet = new DataSet();

        public ShapeFileReader(string path)
        {
            Path = path;
        }

        private static Vector3 ReadPoint(Stream stream, ref ShapeFileProgressModel progress)
        {
            // Points - add a single point.
            //,double XMax,double XMin,double YMax,double YMin
            Vector3 p = new Vector3();
            p.x = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            p.y = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            p.z = 3;

            return p;
        }

        private static List<Vector3> ReadMultipoint(Stream stream, ref ShapeFileProgressModel progress)
        {
            // Bounding Box.
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            //record.ZMin = ShapeFile.ReadDouble64_LE(stream);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            //record.ZMax = ShapeFile.ReadDouble64_LE(stream);


            // Num Points.
            int numPoints = ShapeFileReader.ReadInt32_LE(stream, ref progress);

            List<Vector3> points = new List<Vector3>();

            // Points.
            for (int i = 0; i < numPoints; i++)
            {
                Vector3 p = new Vector3();
                p.x = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
                p.y = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
                p.z = 4;
                points.Add(p);
            }

            return points;
        }

        private static List<Vector3> ReadPolygon(Stream stream, ref ShapeFileProgressModel progress)
        {
            // Bounding Box.
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            //record.ZMin = ShapeFile.ReadDouble64_LE(stream);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            _ = ShapeFileReader.ReadDouble64_LE(stream, ref progress);
            //record.ZMax = ShapeFile.ReadDouble64_LE(stream);

            List<Vector3> points = new List<Vector3>();

            // Num Parts and Points.
            int numParts = ShapeFileReader.ReadInt32_LE(stream, ref progress);
            int numPoints = ShapeFileReader.ReadInt32_LE(stream, ref progress);

            // Parts.

            for (int i = 0; i < numParts; i++)
            {
                _ = ShapeFileReader.ReadInt32_LE(stream, ref progress);
            }

            // Points.
            float a = 0;
            float b = 0;
            float c = 0;
            int nombrePoint = 0;
            int nombrePointDu = 0;

            for (int i = 0; i < numPoints; i++)
            {
                Vector3 p = new Vector3();
                p.x = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
                p.y = (float)ShapeFileReader.ReadDouble64_LE(stream, ref progress);
                p.z = (float)1;
                nombrePointDu += 1;

                if (p.x != a || p.y != b || p.z != c)
                {


                    points.Add(p);

                    nombrePoint += 1;
                }
                a = p.x;
                b = p.y;
                c = p.z;
            }

            return points;
        }
    }
}