using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Assets.PipelineGenerator.Scripts.ESRI.ShapeImporter25D;
using System.Linq;
using TriangleNet.Topology.DCEL;
using MIConvexHull;
using PSS;
using static PSS.Debugging;
using System.Diagnostics;
using static MainThread;
using System.Threading.Tasks;
using static PSS.FileUtils;
using System;
using PSS.Mapping;
using System.Threading;

public class ShapefileImport : MonoBehaviour
{
    public bool loadFromFile = true;

    public string shapefileName;

    public bool cancel = false;

    public bool usePolyGons = false;

    [Header("CSV Index Positions")]
    public IndexPositions positions = new IndexPositions();

    [Header("Importing")]
    public TaskStatus importing;
    public int linesImported = 0;
    public int totalRecordsToImport = 0;

    [Header("Prefabs")]
    public Material mat;
    public GameObject shapePrefab;
    public GameObject linePrefab;

    [Header("Shape Generation")]
    public int ShapesGenerated = 1;
    public int TotalShapes = 1;
    public string ShapesSpeed = "/s";

    public int GenerationSpeed = 5;

    public double Tolerance = 1e-10;

    [Header("MapGenerator")]
    public TaskStatus GeneratorStatus;
    public int piecesGenerated = 0;

    [Header("MapReader")]
    public TaskStatus reading;
    public int linesLoaded = 0;
    public int linesDeser = 0;

    [Header("Exporter")]
    public TaskStatus exporting;
    public bool serializing = false;
    public bool writing = false;
    public int numSerialized = 0;
    public int numWritten = 0;

    [Header("Shapefile")]
    public TaskStatus SHPReading;
    public int shpRecordRead = 0;
    [Range(0, 100)]
    public int percentDoneBytes = 0;
    public long bytesRead = 0;
    public long totalBytes = 0;

    [Header("CSV")]
    public TaskStatus CSVReading;
    public int linesRead = 0;
    public int linesConverted = 0;
    public int totalLines = 0;

    [Header("Debug")]
    public bool saveStates = false;

    [Header("Path Constants")]
    public string defaultShapeFilePath = "Assets/MapData/";
    public string defaultAttributeFilePath = "MapData/";
    public string defaultOutputPath = "/MapData/Maps/";
    public string attributeFileExtension = ".csv";
    public string outputExtenstion = ".PSS_MAP";

    public string path => defaultShapeFilePath + shapefileName;
    public string attributePath => defaultAttributeFilePath + shapefileName + attributeFileExtension;
    public string @outputName => shapefileName + outputExtenstion;

    private CancellationTokenSource TokenSource = new CancellationTokenSource();
#pragma warning disable CS0414
    private int lastShapesCount = 0;
#pragma warning restore CS0414
    MapImporter importer;
    MapExporter exporter;
    MapReader reader;
    MapGenerator generator;

    private bool importingSpin = false;
    private bool loadingSpin = false;

    void Start()
    {

        Helpers.MapGenerationHelpers.CreatePrefabs();

        if (loadFromFile)
        {
            LoadRecords(@outputName);
            return;
        }
        if (!loadFromFile)
        {
            //ImportRecords();
            //StartCoroutine("ImportMultiple");
            StartCoroutine(nameof(LoadMultiple));

        }
    }
    private void OnDisable()
    {
        importer?.Cancel();
        exporter?.Cancel();
        reader?.Cancel();
        generator?.Cancel();
    }
    private void Update()
    {

        if (cancel)
        {
            cancel = false;
            importer?.Cancel();
            exporter?.Cancel();
            reader?.Cancel();
            generator?.Cancel();
        }

        if (generator != null)
        {
            GeneratorStatus = generator.Status;
            piecesGenerated = generator.PiecesGenerated;
        }
        if (reader != null)
        {
            reading = reader.Status;
            linesLoaded = reader.LinesRead;
            linesDeser = reader.LinesConverted;
        }
        if (importer != null)
        {
            importing = importer.Status;
            linesImported = importer.RecordsImported;
            totalRecordsToImport = importer.TotalRecords;

            if (importer.SReader != null)
            {
                SHPReading = importer.SReader.Status;
                shpRecordRead = importer.SReader.RecordsRead;
                bytesRead = ((ShapeFileReader)importer.SReader).BytesRead;
                totalBytes = ((ShapeFileReader)importer.SReader).FileSize;
                percentDoneBytes = (int)(bytesRead / (float)totalBytes * 100);
            }
            if (importer.CSVReader != null)
            {
                CSVReading = importer.CSVReader.Status;
                linesRead = importer.CSVReader.LinesRead;
                linesConverted = importer.CSVReader.LinesConverted;
                totalLines = importer.CSVReader.TotalLines;
            }
        }
        if (exporter != null)
        {
            exporting = exporter.Status;
            serializing = exporter.SerializingRecords;
            writing = exporter.WritingRecords;
            numSerialized = exporter.RecordsSerialized;
            numWritten = exporter.RecordsWritten;
        }
    }

    IEnumerator LoadMultiple()
    {
        Stopwatch watch = Stopwatch.StartNew();
        var states = Enum.GetValues(typeof(StateCode));
        foreach (int item in states)
        {
            string itemName = item.ToString().Length == 1 ? "0" + item.ToString() : item.ToString();
            string name = $"tl_2019_{itemName}_cousub.PSS_MAP";

            LoadRecords(name);

            while (loadingSpin)
            {
                yield return null;
            }
        }
        Debug($"TotalTime:{watch.ElapsedMilliseconds}");
        yield return null;
        //if (usePolyGons) {
        //    usePolyGons = false;
        //    StartCoroutine("LoadMultiple");
        //}
    }

    IEnumerator ImportMultiple()
    {
        Stopwatch watch = Stopwatch.StartNew();
        var states = Enum.GetValues(typeof(StateCode));
        foreach (int item in states)
        {
            string itemName = item.ToString().Length == 1 ? "0" + item.ToString() : item.ToString();
            string name = $"tl_2019_{itemName}_cousub";

            shapefileName = name;

            yield return ImportRecords();

            while (importingSpin)
            {
                yield return null;
            }
            Factory.Log($"Imported {item}");
        }
        Debug($"TotalTime:{watch.ElapsedMilliseconds}");
    }

    private void LoadRecords(string recordName)
    {
        loadingSpin = true;

        string path = Application.dataPath + defaultOutputPath + recordName;

        reader = new MapReader(path);
        generator = new MapGenerator();

        Task readerTask = Task.Run(() =>
        {

            reader.ReadMapMultiThreaded(2);
            TotalShapes += reader.LinesRead;
        });

        Task generatorTask = Task.Run(() =>
        {

            generator.GenerateMap(reader, usePolyGons, mat);
            Debug($"Finished Generating {recordName}");
            loadingSpin = false;
        });
    }


    private async Task ImportRecords()
    {
        importingSpin = true;
        string newPath = this.ExecutablePath() + '/' + defaultAttributeFilePath + shapefileName + ".shp";
        MapPathInfo info = new MapPathInfo()
        {
            CSVDirectory = this.ExecutablePath() + "/" + defaultAttributeFilePath,
            ShapeFileDirectory = this.ExecutablePath() + "/" + defaultAttributeFilePath,
            FileName = shapefileName,
            OutputDirectory = this.ExecutablePath() + defaultOutputPath,
        };
        importer = new MapImporter(info);
        Factory.Log("Started Importing");
        await importer.BeginReadingAsync(new RecordPositions(positions.PrimaryIdIndex, positions.SecondaryIdIndex, positions.NameIndex));

        await Task.Run(() =>
        {
            PSS.MultiThreading.Helpers.WaitForStatus(importer, TaskStatus.RanToCompletion, TokenSource.Token, long.MaxValue);
            Factory.Log("Finished Importing");
            exporter = new MapExporter(info);
            exporter.ExportMapRecords(importer.Records);
            PSS.MultiThreading.Helpers.WaitForStatus(exporter, TaskStatus.RanToCompletion, TokenSource.Token, long.MaxValue);
            Factory.Log("Finished Exporting");
            importingSpin = false;
        });
    }
    [System.Serializable]
    public class IndexPositions
    {
        public int PrimaryIdIndex;
        public int SecondaryIdIndex;
        public int NameIndex;
    }
}