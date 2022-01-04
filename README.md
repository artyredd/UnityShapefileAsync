
# UnityShapefileAsync [DEPRECATED]
Converts [Shapefiles](https://en.wikipedia.org/wiki/Shapefile), commonly used for representing geographical or demographic bodies in statistical analysis programs, to Unity3d(MonoDevelop C#7.3) Meshes and Vector3's.

### DEPRECATED
This library relied on implementation details in Unity ~2018's Task Scheduler and as such is no longer reliable for modern .NET Core, Framework or Unity 2019+. This project is deprecated and no features or updates are planned.

I apologize for the inconvenience. In the mean time you can use the following work around to convert your shapefile and .csv's into JSON flat files:

```csharp
ShapeFileReader reader = new ShapeFileReader("yourShapeFile.shp");

reader.BeginReadingAsync();

while (reader.Status != TaskStatus.RanToCompletion)
{
    continue;
}

CSVReader otherReader = Factory.CreateCSVReader("yourRecordFile.csv");

// the number's 0, 3 and 5 represent the key information(columns in the csv) in the records you want added to the JSON
// see IMapRecord
otherReader.ReadCSV(0, 3, 5);

var positions = new RecordPositions(0, 3, 5);

using (var writer = new StreamWriter(File.OpenWrite("outputFile.json")))
{
    while (reader.Records.TryDequeue(out IMapData data))
    {
        while (otherReader.LineQueue.TryDequeue(out string line))
        {
            writer.WriteLine(Factory.CreateMapRecord(data, line.Split(','), positions).Serialize());
        }
    }
}
```

### Features

- Implements Object Oriented Classes and helpers to simplify integration into your solution.
- As of release Beta-b2, Speed is approx ~1ms per shape (that's inclusive of instantiation of the GameObject and rendering, source: 35k shapes in 34 seconds)

### Showcase

![](https://i.imgur.com/48gVaca.png)](https://i.imgur.com/I5grYTY.mp4)

### Dependencies
- Unity 5.0+ (Optional)
- C# 7.3+ (Required)
- [MIConvexHull](https://github.com/DesignEngrLab/MIConvexHull) (for Mesh generation) (optional)

### Installation
- Copy Files to Assets Directory,
- Alternatively for Non-Unity Usage Add a reference to the files in Visual Studio. Avoid Map Generator as it references Unity Dependencies.

### Usage
#### Source Files:
- Source Files must be a vanilla .SHP(Shapefile) and a .csv(Converted from .DBF, using a tool such as Excel or online converters) for shapefile shape information.
#### Importing:
- Create a MapPathInfo object that contains pertinent path information for the importer
	<pre>MapPathInfo pathInfo= new MapPathInfo()
  {
      CSVDirectory = @"C:/TestDirectory/CSVs/",
      ShapeFileDirectory = @"C:/TestDirectory/Shapefiles/",
      FileName = florida_counties,
      OutputDirectory = @"C:/TestDirectory/Output",
  };</pre>
- Instantiate a new MapImporter
	<pre>
	var importer = Factory.CreateImporter(pathInfo);
	or
	var importer = new MapImporter(pathInfo);
	</pre>
- Create a RecordsPositions object that contains information about the pertinent information to associate with each shape. This specifically refers to the position in the CSV, for example if the CSV has the columns "StateCode", "GEOID", "AREA", "CountyName" and you only wanted the state code, GEOID, and county name you would use
	<pre>
	RecordPostions pertinentLabels = new RecordPositions
	{
		PrimaryIDIndex = 0,
		SecondaryIDIndex= 1,
		NameIndex= 3,
	};
	</pre>
	
- Begin Importing Records from the source files
	<pre>
	importer.BeginReadingAsync(pertinentLabels );
	</pre>
#### Exporting
- The Records generated from reading the shapefile are read asyncronously and can be accessed imemdiately during runtime using importer.Records. These records can be consumed directly and immediately.
	<pre>
	/// < summary>
    /// The queue of records in this importer
    /// < /summary>
    public ConcurrentQueue< IMapRecord> Records { get; private set; }
	</pre>
- Records contain the shapefile information imported. By default these records are of type IMapRecord which has only one public method Serialize(), which serializes the record to a valid JSON object.
	<pre>
	string serializedRecord = record.Serialize();
	Console.Write(serializedRecord) => (System.String)
	{
		RecordNumber: 1,
		Points: {
					{x: 1, y: 2, z: 3},
					{x: 4, y: 5, z: 6},
					...
				},
		PrimaryID: 12,
		SecondaryID: 44,
		Name: The Sate of Florida
	}
	</pre>
- The records generated are currently intended to be used with the MapExporter, which generates flat files containing the JSON objects. The exporter consumes the items asyncronously and exports them asyncronously as well. The exporter can be used concurrently with the Importer.  Basic usage would be:
	<pre>
		var exporter = new MapExporter(pathInfo);
		exporter.ExportMapRecords(importer.Records);
	</pre>

#### Reading and Loading Exported Records
- Saved Records using the MapExporter class can be loaded asyncronously as well using:
	<pre>
	var reader = MapReader( pathInfo );
	reader.ReadMapMultiThreaded( maxThreads: 4 );
	</pre>
- Saved records can not be loaded or read concurrently with exporting at this time.
- Read Records are read asyncronously and can be accessessed and consumed immediately.
	<pre>
		/// < summary>
      /// The map records that have been deserialized and that are ready for use. Thread safe.
      /// < /summary>
      public ConcurrentBag< IMapRecord> MapRecords { get; private set; } = new ConcurrentBag<IMapRecord>();
	</pre>
- Read records are intended to be used with MapGenerator class that concurrently and asyncronously generates UnityEngine.GameObjects, either in mesh(polygon planes) or line-renderer versions(using UnityEngine.LineRenderer). This automatically generates the meshes and/or line-renderers and instantiates them asyncronously.
	<pre>
	var generator = new MapGenerator( );
	generator.GenerateMap( reader, usePolygons: true, material: default(UnityEngine.Material) );
	</pre>
	
### Upcoming Changes
Minor Refactor, Dependency Cleanup, Deprecated Code Removal
Simplify API for classes

### If you like my Work please consider donating to the Autism Research Institute 
https://www.autism.org/donate-autism-research-institute/

### Bug Reports
If you have an issue, discover a bug, or have a recommendation please drop me a line directly through GitHub!

### Change Log
Release b2
	- Major Refactor of Multithreading, speed increase of ~10% 35k shapefile shapes loaded in 34 seconds, ~ 1ms/shape (including instantiation in the game engine)
First Beta Release: b1
