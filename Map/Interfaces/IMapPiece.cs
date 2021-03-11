using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    public interface IMapPiece : IMapData, IMapRecordInfo, IPhysicalObject
    {

    }
    public interface IPhysicalObject
    {
        GameObject gameObject { get; }
    }
}
