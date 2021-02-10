using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PSS.Mapping
{
    /// <summary>
    /// Defines an object that has information to generate a new mesh
    /// </summary>
    public interface IMesh
    {
        /// <summary>
        /// The Verticies of the mesh
        /// </summary>
        Vector3[] Vertices { get; set; }
        /// <summary>
        /// The Triangles of the mesh
        /// </summary>
        int[] Triangles { get; set; }
    }
}
