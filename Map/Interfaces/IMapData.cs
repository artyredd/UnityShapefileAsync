using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    /// <summary>
    /// Object containing point information for a portion of the map
    /// </summary>
    public interface IMapData
    {
        /// <summary>
        /// The record number of the data, usually the line# the record was found on in the .shp file.
        /// </summary>
        int RecordNumber { get; }
        /// <summary>
        /// An <see cref="IEnumerable"/> object containing Vector3s needed to draw a portion of map.
        /// </summary>
        IEnumerable<Vector3> points { get; set; }
    }
}
