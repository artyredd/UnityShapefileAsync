using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// Defines an object that must have a column number, a row number, and a cell
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// The Row number of the cell
        /// </summary>
        int Row { get; }

        /// <summary>
        /// The Column number of the cell
        /// </summary>
        int Column { get; }

        /// <summary>
        /// The Value of the cell
        /// </summary>
        string Value { get; }
    }
}
