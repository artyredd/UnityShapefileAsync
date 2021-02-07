using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Facilitates deserialization of Map Records
    /// </summary>
    public static class MapRecordDeserializer
    {
        /// <summary>
        /// Deserializes maprecord and constructs a new one
        /// </summary>
        /// <param name="SerializedRecord"></param>
        /// <returns></returns>
        public static MapRecord DeserializeMapRecord(string SerializedRecord) {
            MapRecord.SerializedMapRecord tmp = SerializedRecord.FromJSON<MapRecord.SerializedMapRecord>();
            MapRecord mapRecord = Factory.CreateMapRecord();
            mapRecord.Name = tmp.Name;
            mapRecord.points = tmp.points.ToList();
            mapRecord.PrimaryID = tmp.PrimaryID;
            mapRecord.SecondaryID = tmp.SecondaryID;
            mapRecord.RecordNumber = tmp.RecordNumber;
            return mapRecord;
        }
    }
}
