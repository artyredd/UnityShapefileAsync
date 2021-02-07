using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using PSS.Mapping.CSV;

namespace PSS.Mapping
{
    public static partial class Helpers {
        /// <summary>
        /// Common methods to assist specifcally with <see cref="File"/> functions
        /// </summary>
        public static class MapFileHelpers
        {
            /// <summary>
            /// Gets the Size of a file in <see cref="byte"/>
            /// </summary>
            /// <param name="path"></param>
            /// <returns><see cref="long"/> number of bytes the file has</returns>
            public static long GetFileSize(string path) {
                try {
                    var FileInfo = Factory.CreateFileInfo(path);
                    return FileInfo.Length;
                }
                catch (System.IO.FileNotFoundException) {
                    Factory.Log($"Failed to get file size of file, file not found, Path={path}");
                    return 0;
                }
            }
            /// <summary>
            /// Returns the number of lines in a file
            /// </summary>
            /// <returns></returns>
            public static int GetFileLineCount(string Path) {
                int lines = 0;
                using (StreamReader reader = File.OpenText(Path)) {
                    string line;
                    while ((line = reader.ReadLine())!=null) {
                        lines++;
                    }
                }
                return lines;
            }

            /// <summary>
            /// Reads lines from a file and adds them to the given stack
            /// </summary>
            /// <param name="stack"></param>
            /// <param name="progress"></param>
            /// <param name="token"></param>
            public async static Task ReadFileLinesToStackAsync(string Path, ConcurrentQueue<string> stack, IProgress<FileReadingProgressModel> progress, CancellationToken token)
            {

                FileReadingProgressModel progressReport = new FileReadingProgressModel();
                progressReport.ReadingCompleted = false;
                progressReport.TotalLines = GetFileLineCount(Path);
                progressReport.Reading = true;

                progress.Report(progressReport);
                await Task.Run(() =>
                {
                    using (StreamReader reader = File.OpenText(Path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                            stack.Enqueue(line);
                            progressReport.LineQueue = stack;
                            progressReport.LinesRead++;
                            progressReport.Reading = true;
                            progress.Report(progressReport);
                        }
                    }
                    progressReport.Reading = false;
                    progressReport.ReadingCompleted = true;
                    progress.Report(progressReport);
                    //Factory.Log($"Stack Reader task ended Lines Read: {progressReport.LinesRead}/{progressReport.TotalLines}");
                });
            }
        }
    }
}
