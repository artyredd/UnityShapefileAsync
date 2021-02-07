using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    /// <summary>
    /// Data model that stores information to create a mesh
    /// </summary>
    public class MeshModel : IMesh
    {
        public Vector3[] Vertices { get; set; }

        public int[] Triangles { get; set; }
    }
}
