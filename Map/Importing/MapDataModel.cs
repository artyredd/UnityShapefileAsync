using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    /// <summary>
    /// The model that holds information about a record in a shapefile
    /// </summary>
    public class MapDataModel : IMapData
    {
        public MapDataModel(int recordNumber, IEnumerable<Vector3> points)
        {
            RecordNumber = recordNumber;
            this.points = points;
        }

        public int RecordNumber { get; private set; }

        public IEnumerable<Vector3> points { get; set; }

    }
}
