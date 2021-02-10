using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSS.Mapping.CSV;

namespace PSS.Mapping
{
    public class MapRecordInfo : IMapRecordInfo
    {
        public int RecordNumber { get; private set; }

        public int PrimaryID { get; private set; }

        public int SecondaryID { get; private set; }

        public string Name { get; private set; }

        public MapRecordInfo(IRow row, IRecordPositions positions) {
            RecordNumber = row.Index;
            int tmp;

            if (int.TryParse(row.Cells.ElementAt(positions.PrimaryIDIndex).Value, out tmp))
            {
                PrimaryID = tmp;
            }

            if (int.TryParse(row.Cells.ElementAt(positions.SecondaryIDIndex).Value, out tmp))
            {
                SecondaryID = tmp;
            }

            Name = row.Cells.ElementAt(positions.NameIndex).Value;
        }
    }
}
