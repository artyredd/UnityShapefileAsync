using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping.CSV
{
    /// <summary>
    /// Defines an object that must resemble column header in a csv
    /// </summary>
    public interface IColumnHeader
    {
        /// <summary>
        /// The index this column can be found at in the CSV
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The title that this column has.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The maximum number of characters this columner can have.
        /// </summary>
        int Length { get; }
    }
}
