using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Holds the indexes where the important data in a CSV is found
    /// </summary>
    public class RecordPositions : IRecordPositions
    {
        public RecordPositions(int primaryIDIndex, int secondaryIDIndex, int nameIndex)
        {
            PrimaryIDIndex = primaryIDIndex;
            SecondaryIDIndex = secondaryIDIndex;
            NameIndex = nameIndex;
        }

        public int PrimaryIDIndex { get; private set; }

        public int SecondaryIDIndex { get; private set; }

        public int NameIndex { get; private set; }


    }
}
