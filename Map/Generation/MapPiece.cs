using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    /// <summary>
    /// A piece of the map
    /// </summary>
    public class MapPiece : IMapPiece
    {
        public MapPiece(GameObject gameObject, IMapData data, IMapRecordInfo info)
        {
            this.gameObject = gameObject;
            this.RecordNumber = data.RecordNumber;
            this.points = data.points;
            this.Name = info.Name;
            this.PrimaryID = info.PrimaryID;
            this.SecondaryID = info.SecondaryID;
        }

        public int RecordNumber { get; private set; }

        public IEnumerable<Vector3> points { get; set; }

        public int PrimaryID { get; private set; }

        public int SecondaryID { get; private set; }

        public string Name { get; private set; }

        public GameObject gameObject { get; private set; }
    }
}
