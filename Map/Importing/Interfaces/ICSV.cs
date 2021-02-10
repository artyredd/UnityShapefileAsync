using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// Defines a CSV Document
    /// </summary>
    public interface ICSV
    {
        /// <summary>
        /// The Headers for the CSV
        /// </summary>
        IEnumerable<IColumnHeader> ColumnHeaders { get; }

        /// <summary>
        /// The Rows for the CSV
        /// </summary>
        IEnumerable<IRow> Rows { get; }

        int AddColumn(string Title, string Length);

        int AddCell(int Row, string Value);

        /// <summary>
        /// Whether this CSV has a column header matching the given string
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <returns></returns>
        bool ContainsHeader(string columnHeader);

        /// <summary>
        /// Whether this CSV contains the given value in any of its cells
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool ContainsValue(string value);

        /// <summary>
        /// Returns the value of a cell at given position
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        string GetCell(int row, int column);
    }
}
