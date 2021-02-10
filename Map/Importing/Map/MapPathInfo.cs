using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// The model class that holds information about a shapefile that needs to be imported
    /// </summary>
    public class MapPathInfo : IMapPathInfo
    {
        public string ShapeFileDirectory { get; set; }

        public string CSVDirectory { get; set; }

        public string OutputDirectory { get; set; }

        public string FileName { get; set; }

        public string ExportPath => OutputDirectory + FileName + ".PSS_MAP";
    }
}
