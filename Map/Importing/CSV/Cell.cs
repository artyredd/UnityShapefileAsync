using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// A cell of a CSV object
    /// </summary>
    public class Cell : ICell
    {
        public Cell(int row, int column, string value)
        {
            Row = row;
            Column = column;
            Value = value;
        }

        /// <summary>
        /// The row index where this cell is located
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// The column index where this cell is located
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// The value of the cell
        /// </summary>
        public string Value { get; private set; }

        public override string ToString()
        {
            return $"Cell({Row},{Column},{Value})";
        }
    }
}
