using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PSS.Mapping.CSV;

namespace PSS.Mapping
{
    [System.Serializable]
    public class MapRecord : IMapRecord
    {
        public int RecordNumber { get; set; }

        public IEnumerable<Vector3> points { get; set; }

        public int PrimaryID { get; set; }

        public int SecondaryID { get; set; }

        public string Name { get; set; }

        //                 holds point data  holds info from csv   tells where to find info in csv IRow
        public MapRecord(IMapData data, string[] row, IRecordPositions positions)
        {

            RecordNumber = data.RecordNumber;

            points = data.points;

            int tmp;

            if (int.TryParse(row[positions.PrimaryIDIndex], out tmp))
            {
                PrimaryID = tmp;
            }

            if (int.TryParse(row[positions.SecondaryIDIndex], out tmp))
            {
                SecondaryID = tmp;
            }

            Name = row[positions.NameIndex];
        }

        public MapRecord() { }

        public string Serialize()
        {
            return JsonUtility.ToJson(new SerializedMapRecord()
            {
                RecordNumber = RecordNumber,
                points = points.ToArray(),
                PrimaryID = PrimaryID,
                SecondaryID = SecondaryID,
                Name = Name
            });
        }

        [System.Serializable]
        public class SerializedMapRecord
        {
            public int RecordNumber;

            public Vector3[] points;

            public int PrimaryID;

            public int SecondaryID;

            public string Name;
        }
    }
}
