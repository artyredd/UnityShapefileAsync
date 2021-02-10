using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Model that holds data about the progress of the MapImporter class
    /// </summary>
    public class MapImporterProgressModel
    {
        public bool ThreadStarted { get; set; } = false;
        public int RecordsCompleted { get; set; } = 0;
        public int TotalRecords { get; set; } = 0;
        public int PercentDone => MathHelpers.Percentages.PercentageComplete(RecordsCompleted,TotalRecords);
        public bool FinishedImporting { get; set; } = false;
    }
}
