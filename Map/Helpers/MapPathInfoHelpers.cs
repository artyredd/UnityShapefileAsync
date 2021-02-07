using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    public static partial class Helpers { 
        /// <summary>
        /// Helper methods to assist with common <see cref="IMapPathInfo"/> necessities.
        /// </summary>
        public static class MapPathInfoHelpers
        {
            /// <summary>
            /// Verifies that all paths within an IMapPathInfo are valid
            /// </summary>
            /// <param name="PathInfo"></param>
            /// <returns><see langword="true"/> when all paths and file names inside of the PathInfo are valid and have Write Access.
            /// <para>
            /// <see langword="false"/> when any paths are invalid, or do not have write access.
            /// </para>
            /// </returns>
            public static bool VerifyPaths(IMapPathInfo PathInfo)
            {
                if (Paths.VerifyWorkingDirectory(PathInfo.ShapeFileDirectory) == false)
                {
                    Factory.Log("ShapeFileDirectory Invalid");
                    return false;
                }
                if (Paths.VerifyWorkingDirectory(PathInfo.CSVDirectory) == false)
                {
                    Factory.Log("CSVDirectory Invalid");
                    return false;
                }
                if (Paths.VerifyWorkingDirectory(PathInfo.OutputDirectory) == false)
                {
                    Factory.Log("OutputDirectory Invalid");
                    return false;
                }
                return true;
            }
        }
    }
    
}
