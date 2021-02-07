using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSS;

namespace PSS.Mapping
{
    /// <summary>
    /// Defines an object that contains information and data about a portion of a map.
    /// </summary>
    public interface IMapRecord : IMapData, IMapRecordInfo
    {
        string Serialize();
    }
}
