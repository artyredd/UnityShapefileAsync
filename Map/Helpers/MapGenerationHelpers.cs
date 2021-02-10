using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static MainThread;

namespace PSS.Mapping
{
    public static partial class Helpers
    {
        /// <summary>
        /// Helper methods that assist in Map generation
        /// </summary>
        public static class MapGenerationHelpers
        {
            private static System.Random random = new System.Random();
            private static GameObject MapLinePrefab;
            private static GameObject MapShapePrefab;

            public static void CreatePrefabs()
            {
                MainThreadOperation operation = MainThread.Add(() =>
                {
                    MapLinePrefab = new GameObject();

                    LineRenderer r = MapLinePrefab.AddComponent<LineRenderer>();
                    r.receiveShadows = false;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    MapShapePrefab = new GameObject();
                    MapShapePrefab.AddComponent<MeshFilter>();
                    MapShapePrefab.AddComponent<MeshRenderer>();
                    MapShapePrefab.gameObject.transform.Translate(new Vector3(0, 0, 3f));
                });
            }

            /// <summary>
            /// Creates a random UnityEngine.Color and returns it, thread safe
            /// </summary>
            /// <returns></returns>
            public static Color GetRandomColor()
            {
                float x, y, z;
                lock (random)
                {
                    x = (float)random.NextDouble();
                    y = (float)random.NextDouble();
                    z = (float)random.NextDouble();
                }
                return new Color(x, y, z);
            }

            /// <summary>
            /// Generates an <see cref="IMesh"/> using the given points, thread independant, can be used off the main thread
            /// </summary>
            /// <param name="stars"></param>
            /// <param name="tolerance"></param>
            /// <returns></returns>
            public static IMesh CreateMesh(IEnumerable<Vector3> stars)
            {
                if (stars == null || stars.Count() == 0)
                {
                    Factory.Log("Null points provided to CreateMesh");
                    throw new ArgumentNullException("IEnumerable<Vector3> stars");
                }
                IMesh mesh = Factory.CreateMeshModel();

                List<int> triangles = new List<int>();

                var vertices = stars.Select(x => new MIConvexHull.DefaultVertex2D(x.x, x.y)).ToList();

                //var r = MIConvexHull.ConvexHull.Create2D(vertices, tolerance);

                var r = MIConvexHull.ConvexHull.Create2D(vertices);

                if (r.ErrorMessage != string.Empty)
                {
                    //return null;
                    throw new NullReferenceException(r.ErrorMessage);
                }

                //r.Outcome.Debug();
                var result = r.Result;
                mesh.Vertices = result.Select(x => new Vector3((float)x.X, (float)x.Y, 0)).ToArray();
                var xxx = result.ToList();

                int[] meshIndices = new int[xxx.Count * 3];
                for (int i = 0; i < xxx.Count - 2; i++)
                {
                    meshIndices[3 * i] = 0;
                    meshIndices[(3 * i) + 1] = i + 1;
                    meshIndices[(3 * i) + 2] = i + 2;
                }

                mesh.Triangles = meshIndices.Reverse().ToArray();
                return mesh;
            }

            /// <summary>
            /// Creates a gameobject on the mainthread that has a line renderer instead of a mesh
            /// </summary>
            /// <param name="record"></param>
            /// <param name="name"></param>
            /// <param name="color"></param>
            /// <param name="tolerance"></param>
            /// <returns></returns>
            public static MainThreadOperation CreateGameObjectWithLine(int uniqueId, IMapRecord record, float size, Color color, float tolerance = 0.1f)
            {

                record = SimplifyRecord(record, tolerance);

                return MainThread.Add(() =>
                {
                    GameObject n = GameObject.Instantiate(MapLinePrefab);
                    n.name = uniqueId.ToString();
                    LineRenderer r = n.GetComponent<LineRenderer>();
                    r.startWidth = size;
                    r.endWidth = size;
                    r.positionCount = record.points.Count();
                    r.SetPositions(record.points.ToArray());
                    r.material.color = color;
                    r.startColor = color;
                    r.endColor = color;
                });
            }

            /// <summary>
            /// Creates a gameobject on the mainthread, attaches the mesh, names the object, and colors the material
            /// </summary>
            /// <param name="name"></param>
            /// <param name="mesh"></param>
            /// <param name="mat"></param>
            /// <param name="color"></param>
            /// <returns></returns>
            public static MainThreadOperation CreateGameObjectWithMesh(string uniqueID, IMesh mesh, Material mat, Color color)
            {
                if (mesh == null)
                {
                    throw new ArgumentNullException(nameof(mesh), "Mesh cannot be null.");
                }

                MainThreadOperation mainThreadOperation = MainThread.Add(() =>
                {
                    GameObject selection = GameObject.Instantiate(MapShapePrefab);
                    selection.name = uniqueID;


                    MeshFilter meshFilter = selection.GetComponent<MeshFilter>();

                    Mesh newMesh = new Mesh();

                    newMesh.vertices = mesh.Vertices;
                    newMesh.triangles = mesh.Triangles;

                    newMesh.RecalculateNormals();

                    meshFilter.mesh = newMesh;


                    MeshRenderer renderer = selection.GetComponent<MeshRenderer>();
                    renderer.material = mat;
                    renderer.material.color = color;
                });

                return mainThreadOperation;
            }

            /// <summary>
            /// Simplifies the vertices of a MapRecord
            /// </summary>
            /// <param name="record"></param>
            /// <param name="tolerance"></param>
            /// <returns></returns>
            public static IMapRecord SimplifyRecord(IMapRecord record, float tolerance = 0.1f)
            {
                List<Vector3> points = new List<Vector3>();
                LineUtility.Simplify(record.points.ToList(), tolerance, points);
                record.points = points;
                return record;
            }
        }
    }
}
