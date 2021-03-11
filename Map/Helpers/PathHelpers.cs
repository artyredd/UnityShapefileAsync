using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PSS.Mapping
{
    /// <summary>
    /// Helps with common path functions when dealing with <see cref="IMapPathInfo"/>
    /// </summary>
    public static partial class Helpers
    {
        /// <summary>
        /// Map Helper Methods that deal specifically with p
        /// </summary>
        public static class Paths
        {

            /// <summary>
            /// Verifies that the given directory either exists, or was sucessfully created
            /// </summary>
            /// <param name="path"></param>
            public static bool VerifyWorkingDirectory(string @path)
            {
                if (Directory.Exists(path))
                {
                    return true;
                }
                DirectoryInfo info;
                try
                {
                    info = Directory.CreateDirectory(path);
                    if (info.Exists)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    if (e is UnauthorizedAccessException)
                    {
                        Factory.Log($"Failed to verify working directory -> no access to directory.,{path}");
                    }
                    if (e is ArgumentNullException)
                    {
                        Factory.Log($"Failed to verify working directory -> path is null or empty:{path}");
                    }
                    if (e is PathTooLongException)
                    {
                        Factory.Log($"Failed to verify working directory -> path was too long ({path.Length})chars:{path}");
                    }
                    if (e is DirectoryNotFoundException)
                    {
                        Factory.Log($"Failed to verify working directory -> directory not found, may reference non-existent drive {path}");
                    }
                    if (e is NotSupportedException)
                    {
                        Factory.Log($"Failed to verify working directory -> path contains colon character");
                    }
                    return false;
                }
            }

            /// <summary>
            /// Verifies that the given file path has write access
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool VerifyFileWriteAccess(string @path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                FileAttributes fileInfo = File.GetAttributes(path);
                if ((fileInfo & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return false;
                }
                if ((fileInfo & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    Factory.Log("Write Access Verification Failed, Reason: Path is a directory not a file, Path={path}");
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Verifies that a filename does not contain invalid path charcters
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool VerifyFileName(string fileName)
            {
                return VerifyStringWithErrorCoellescing(fileName, System.IO.Path.GetInvalidFileNameChars());
            }

            /// <summary>
            /// Verifies that the given path does not contain invalid characters for a path
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool VerifyDirectoryChars(string @path)
            {
                return VerifyStringWithErrorCoellescing(path, System.IO.Path.GetInvalidPathChars());
            }
            //DRY helper
            private static bool VerifyStringWithErrorCoellescing(string Value, char[] chars)
            {
                try
                {
                    return StringContainsAny(Value, chars);
                }
                catch (ArgumentNullException)
                {
                    return false;
                }
            }
            //DRY helper
            private static bool StringContainsAny(string value, char[] chars)
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("string:value");
                }
                if (chars.Length == 0)
                {
                    return false;
                }
                foreach (char c in value)
                {
                    if (chars.Contains(c))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
