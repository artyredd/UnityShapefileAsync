using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Model that outlines the progress of the ShapeFileReader object
    /// </summary>
    public class ShapeFileProgressModel
    {
        /// <summary>
        /// How many records have been read by the reader
        /// </summary>
        public int RecordsRead { get; set; } = 0;

        /// <summary>
        /// The total file size of the file that the reader is reading
        /// </summary>
        public long FileSize { get; set; } = 0;

        /// <summary>
        /// How many bytes so far the reader has read
        /// </summary>
        public long BytesRead { get; set; } = 0;

        /// <summary>
        /// What percentage done as an int 0-100 the reader is in reading from the file.
        /// </summary>
        public int PercentDone => (int)((BytesRead * 100) / FileSize);

        public bool FinishedReading { get; set; } = false;
    }
}
