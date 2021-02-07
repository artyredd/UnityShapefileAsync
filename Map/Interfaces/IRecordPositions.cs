using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Defines an object that contains information that provides the column numbers for the required data to construct an IMapRecord
    /// </summary>
    public interface IRecordPositions
    {
        int PrimaryIDIndex { get; }
        int SecondaryIDIndex { get; }
        int NameIndex { get; }
    }
}
