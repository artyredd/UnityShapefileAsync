using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSS;
using UnityEngine;
using static PSS.Debugging;
using Assets.PipelineGenerator.Scripts.ESRI.ShapeImporter25D;
using System.IO;
using PSS.Mapping.CSV;
namespace PSS.Mapping
{
    /// <summary>
    /// Produces new instances for classes and methods within the PSS.Mapping Namespace
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Logs message to console
        /// </summary>
        /// <param name="Message"></param>
        public static void Log(string Message) {
            Warn(Message);
        }

        /// <summary>
        /// Gets the data path where the executable is going to be
        /// </summary>
        /// <returns></returns>
        public static string GetDataPath() {
            return Application.dataPath;
        }


        public static FileInfo CreateFileInfo(string path) {
            return new FileInfo(path);
        }

        /// <summary>
        /// Creates and retruns a <see cref="CSV.IColumnHeader"/> object
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="Title"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public static Mapping.CSV.ColumnHeader CreateHeader(int Index, string Title, int Length) {
            return new Mapping.CSV.ColumnHeader(Index, Title, Length);
        }

        /// <summary>
        /// Creates and returns a new ICell object
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="Column"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Mapping.CSV.Cell CreateCell(int Row, int Column, string Value) {
            return new Mapping.CSV.Cell(Row, Column, Value);
        }

        /// <summary>
        /// Creates and returns a new CSV Row object
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Mapping.CSV.Row CreateRow(int index) {
            return new Mapping.CSV.Row(index);
        }

        public static Mapping.CSV.CSVModel CreateCSV() {
            return new Mapping.CSV.CSVModel();
        }

        public static Mapping.CSVReader CreateCSVReader(string Path) {
            return new CSVReader(Path);
        }

        public static Mapping.MapPathInfo CreatePathInfo() {
            return new MapPathInfo();
        }

        public static MapDataModel CreateMapDataModel(int recordNumber, IEnumerable<Vector3> points) {
            return new MapDataModel(recordNumber, points);
        }

        public static ShapeFileReader CreateShapeFileReader(string Path) {
            return new ShapeFileReader(Path);
        }

        public static MapRecord CreateMapRecord(IMapData data, string[] row, IRecordPositions positions) {
            return new MapRecord(data, row, positions);
        }
        public static MapRecord CreateMapRecord()
        {
            return new MapRecord();
        }

        public static MeshModel CreateMeshModel() {
            return new MeshModel();
        }

        public static MapPiece CreateMapPiece(GameObject gameObject, IMapData data, IMapRecordInfo info) {
            return new MapPiece(gameObject, data, info);
        }

        public static void LogException(Exception e){
            Factory.Log($"Error: {e.Message}\n{e.StackTrace}");
        }

        /// <summary>
        /// Determines if an aggregrgate error contains any <see cref="Exception"/> that is NOT <see cref="OperationCanceledException"/> and logs it and returns <see langword="true"/> if all faults were <see cref="OperationCanceledException"/> and <see langword="false"/> if any faults were found that were not <see cref="OperationCanceledException"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static void LogInnerExceptions(AggregateException e) {
            foreach (var ex in e.InnerExceptions)
            {
                LogException(ex);
            }
            LogException(e);
        }
    }
}
