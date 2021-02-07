using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS
{
    /// <summary>
    /// Defines an object that contains information needed to import map data
    /// </summary>
    public interface IMapPathInfo
    {
        /// <summary>
        /// The directory that contains the map shape data (.shp) file
        /// </summary>
        string ShapeFileDirectory { get; }
        /// <summary>
        /// The directory that contains the map information data (.csv) (converted from DBASE) file
        /// </summary>
        string CSVDirectory { get; }
        /// <summary>
        /// The output path that the newly imported files should be placed
        /// </summary>
        string OutputDirectory { get; }
        /// <summary>
        /// The name of the new map piece
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// The full path of the MapFile to be exported
        /// </summary>
        string ExportPath { get; }
    }
}
