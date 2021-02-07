using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS
{
    /// <summary>
    /// A <see cref="System.Progress"/> model used to report back information about a file being read
    /// </summary>
    public class FileReadingProgressModel
    {
        /// <summary>
        /// The number of lines read so far
        /// </summary>
        public int LinesRead { get; set; } = 0;

        /// <summary>
        /// The total amount of lines in the document
        /// </summary>
        public int TotalLines { get; set; } = 0;

        /// <summary>
        /// The percentage done as an Int(0-100) that this reader is completed
        /// </summary>
        public int PercentDone => CalcPercentage();

        /// <summary>
        /// Whether or not the reader has completed reading
        /// </summary>
        public bool ReadingCompleted { get; set; } = false;

        public bool Reading { get; set; } = false;

        public int linesConverted { get; set; } = 0;

        public bool ConvertThreadWorking { get; set; }

        public ConcurrentQueue<string> LineQueue { get; set; } = new ConcurrentQueue<string>();

        /// <summary>
        /// Calculates the percentage done without throwing divide by 0 errors
        /// </summary>
        /// <returns></returns>
        private int CalcPercentage() {
            if (LinesRead == 0) {
                return 0;
            }
            if (TotalLines == 0)
            {
                return 0;
            }
            return (LinesRead * 100) / TotalLines;
        }
    }
}
