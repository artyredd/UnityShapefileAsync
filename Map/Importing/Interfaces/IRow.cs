using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// Defines an object that must resemble a CSV row
    /// </summary>
    public interface IRow
    {
        int Index { get; }
        /// <summary>
        /// The cells in this row
        /// </summary>
        IEnumerable<ICell> Cells { get; }
        /// <summary>
        /// Adds the given cell to <see cref="Cells"/>
        /// </summary>
        /// <param name="cell"></param>
        void AddCell(ICell cell);
    }
}
