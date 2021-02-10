using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    public class CSVLineConverterProgressModel
    {
        public bool ConvertingLines { get; set; }
        public int LinesConverted { get; set; }
        public bool Waiting { get; set; }
    }
}
