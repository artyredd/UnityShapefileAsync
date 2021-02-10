using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// Class representing a CSV it contains no functionailty other than as a data structure representing a CSV table, adding, removing and finding values/columns inside its own <see cref="IEnumerable"/> structures.
    /// </summary>
    public class CSVModel : ICSV, IEnumerable
    {
        public IEnumerable<IColumnHeader> ColumnHeaders => _columnHeaders;
        private List<IColumnHeader> _columnHeaders { get; set; } = new List<IColumnHeader>();

        public IEnumerable<IRow> Rows => _rows;
        private List<IRow> _rows { get; set; } = new List<IRow>();

        /// <summary>
        /// Adds a new header to the ColumnHeader
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Length"></param>
        /// <returns><see cref="int"/> Column Index</returns>
        public int AddColumn(string Title, string Length)
        {

            int length;
            int.TryParse(Length, out length);

            IColumnHeader newHeader = Factory.CreateHeader(ColumnHeaders.Count(), Title, length);

            _columnHeaders.Add(newHeader);

            return newHeader.Index;
        }

        /// <summary>
        /// Adds a new cell to the CSV
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="Column"></param>
        /// <param name="Value"></param>
        /// <returns><see cref="int"/> Column Index</returns>
        public int AddCell(int Row, string Value)
        {
            if (Row < 0)
            {
                throw new IndexOutOfRangeException("int Row for Row Index out of range, must be positive integer.");
            }

            IRow row;

            //check to see if that row exists
            if (Row < Rows.Count() & Row >= 0)
            {
                row = Rows.ElementAt(Row);
            }
            else
            {
                row = Factory.CreateRow(Row);
                _rows.Add(row);
            }

            ICell newCell = Factory.CreateCell(Row, row.Cells.Count(), Value);

            row.AddCell(newCell);

            return newCell.Column;
        }

        /// <summary>
        /// Checks the CSV if the given string in any header
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <returns></returns>
        public bool ContainsHeader(string columnHeader)
        {
            foreach (var item in ColumnHeaders)
            {
                if (item.Title == columnHeader)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the CSV contains the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(string value)
        {
            foreach (var row in Rows)
            {
                foreach (var cell in row.Cells)
                {
                    if (cell.Value == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds and returns the column index of a given column header title
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <returns>
        ///     Positive <see cref="int"/>  Index where Column header was found. ( 0 - <see cref="int.MaxValue"/>)
        ///     <para>
        ///     Negative <see cref="int"/>(-1) If no column header with given title was found.
        ///     </para>
        /// </returns>
        public int GetColumnIndex(string columnHeader)
        {
            IColumnHeader header;
            if ((header = FindHeader(columnHeader)) != null)
            {
                return header.Index;
            }
            return -1;
        }
        /// <summary>
        /// Returns the value of a cell at a given location
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetCell(int row, int column)
        {
            if (row > Rows.Count() | row < 0)
            {
                throw new IndexOutOfRangeException($"Expected int row to be between 0 and {Rows.Count()}, got {row}");
            }
            if (column > ColumnHeaders.Count() | column < 0)
            {
                throw new IndexOutOfRangeException($"Expected int column to be between 0 and {ColumnHeaders.Count()}, got {row}");
            }
            return Rows.ElementAt(row).Cells.ElementAt(column).Value;
        }
        private IColumnHeader FindHeader(string title)
        {
            foreach (var item in ColumnHeaders)
            {
                if (item.Title == title)
                {
                    return item;
                }
            }
            return null;
        }

        public override string ToString()
        {
            string headers = string.Empty;
            foreach (var item in ColumnHeaders)
            {
                headers += item.ToString() + '|';
            }
            string rows = string.Empty;
            foreach (var item in Rows)
            {
                rows += item.ToString() + '\n';
            }
            return $"{headers}\n------------------------------------------------------------\n{rows}";
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Rows).GetEnumerator();
        }
    }
}
